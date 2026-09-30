import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Observable, Subject, catchError, finalize, of, switchMap, takeUntil, takeWhile, throwError, timer } from 'rxjs';

import {
  AutomaticQueueBlock,
  AutomaticQueueBlockRunStatus,
  AutomaticQueueBlockWrite,
  AutomaticQueueDefinition,
  AutomaticQueueRun,
  AutomaticQueueRunAccepted,
  AutomaticQueueRunBlock,
  AutomaticQueueWrite,
} from '../../models';
import { AutomaticQueueService } from '../../services';
import {
  ManualCheckSetupDialogComponent,
  ManualCheckSetupDialogData,
  ManualCheckSetupDialogResult,
} from '../manual-mode-v2/manual-check-setup-dialog.component';
import {
  ManualCheckTraceDialogComponent,
  ManualCheckTraceDialogData,
} from '../manual-mode-v2/manual-check-trace-dialog.component';
import { AutomaticQueueDelayDialogComponent } from './automatic-queue-delay-dialog.component';
import { AutomaticQueueConfigurationDialogComponent } from './automatic-queue-configuration-dialog.component';

type EditorBlock = AutomaticQueueBlockWrite & Partial<AutomaticQueueBlock>;

@Component({
  selector: 'steam-automatic-queue-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, DragDropModule, MatButtonModule, MatDialogModule, MatIconModule, MatMenuModule],
  templateUrl: './automatic-queue-page.html',
  styleUrl: './automatic-queue-page.scss',
})
export class AutomaticQueuePage implements OnInit, OnDestroy {
  private readonly service = inject(AutomaticQueueService);
  private readonly dialog = inject(MatDialog);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();
  private readonly pollStop$ = new Subject<void>();

  readonly definitions = signal<AutomaticQueueDefinition[]>([]);
  readonly runs = signal<AutomaticQueueRun[]>([]);
  readonly selectedDefinitionId = signal<number | null>(null);
  readonly selectedRun = signal<AutomaticQueueRun | null>(null);
  readonly selectedDefinition = computed(() =>
    this.definitions().find((definition) => definition.id === this.selectedDefinitionId()) ?? null);
  readonly locked = computed(() => (this.selectedDefinition()?.activeRunId ?? 0) > 0);
  readonly canPause = computed(() =>
    this.selectedRun()?.status === 'Queued' || this.selectedRun()?.status === 'Running');
  readonly canResume = computed(() => this.selectedRun()?.status === 'Paused');
  readonly activeRun = computed(() => {
    const run = this.selectedRun();
    return !!run && this.isActiveStatus(run.status);
  });
  name = '';
  blocks: EditorBlock[] = [];
  loading = true;
  busy = false;
  errorMessage = '';
  successMessage = '';
  readonly now = signal(Date.now());

  ngOnInit(): void {
    this.loadDefinitions();
    timer(0, 1000).pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.now.set(Date.now());
    });
  }

  ngOnDestroy(): void {
    this.pollStop$.next();
    this.pollStop$.complete();
    this.destroy$.next();
    this.destroy$.complete();
  }

  get canSave(): boolean {
    return !this.locked() && this.name.trim().length > 0 && this.blocks.length > 0
      && this.blocks.some((x) => x.type === 'ManualCheck');
  }

  loadDefinitions(): void {
    this.loading = true;
    this.errorMessage = '';
    this.service.getDefinitions().pipe(finalize(() => {
      this.loading = false;
      this.cdr.markForCheck();
    })).subscribe({
      next: (definitions) => {
        this.definitions.set(definitions);
        if (definitions.length > 0) this.selectDefinition(definitions[0]);
        else this.createNew();
      },
      error: (error) => this.errorMessage = this.getError(error, 'Unable to load automatic queues.'),
    });
  }

  createNew(): void {
    this.pollStop$.next();
    this.selectedDefinitionId.set(null);
    this.selectedRun.set(null);
    this.runs.set([]);
    this.name = '';
    this.blocks = [];
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  duplicateDefinition(): void {
    const definition = this.selectedDefinition();
    if (!definition) return;
    this.pollStop$.next();
    this.selectedDefinitionId.set(null);
    this.selectedRun.set(null);
    this.runs.set([]);
    this.name = `${definition.name} copy`.slice(0, 100);
    this.blocks = structuredClone(definition.blocks).map((block) => ({ ...block, key: crypto.randomUUID() }));
    this.successMessage = 'Queue copied. Save it to create the new definition.';
    this.errorMessage = '';
    this.cdr.markForCheck();
  }

  selectDefinition(definition: AutomaticQueueDefinition): void {
    this.pollStop$.next();
    this.selectedDefinitionId.set(definition.id);
    this.name = definition.name;
    this.blocks = definition.blocks.map((x) => ({ ...x, productIds: x.productIds ? [...x.productIds] : null }));
    this.selectedRun.set(null);
    this.loadRuns(definition.id, definition.activeRunId);
  }

  addManualBlock(): void {
    if (this.locked()) return;
    this.openManualBlockDialog(null);
  }

  editManualBlock(block: EditorBlock): void {
    if (this.locked() || block.type !== 'ManualCheck') return;
    this.openManualBlockDialog(block as AutomaticQueueBlock);
  }

  addDelayBlock(): void {
    if (this.locked()) return;
    this.blocks = [...this.blocks, {
      key: crypto.randomUUID(),
      type: 'Delay',
      delaySeconds: 60,
      gameUrlId: null,
      templateMode: null,
      presetId: null,
      presetCombination: null,
      privateTemplate: null,
      bypassCache: false,
      productIds: null,
    }];
  }

  updateDelay(block: EditorBlock, value: number): void {
    block.delaySeconds = Math.max(1, Math.min(604800, Math.trunc(value || 1)));
  }

  duplicateBlock(index: number): void {
    if (this.locked()) return;
    const clone = structuredClone(this.blocks[index]);
    clone.key = crypto.randomUUID();
    this.blocks.splice(index + 1, 0, clone);
    this.blocks = [...this.blocks];
  }

  removeBlock(index: number): void {
    if (this.locked()) return;
    this.blocks = this.blocks.filter((_, blockIndex) => blockIndex !== index);
  }

  drop(event: CdkDragDrop<EditorBlock[]>): void {
    if (this.locked()) return;
    moveItemInArray(this.blocks, event.previousIndex, event.currentIndex);
    this.blocks = [...this.blocks];
  }

  save(): void {
    this.persist(false);
  }

  run(): void {
    this.persist(true);
  }

  private persist(runAfterSave: boolean): void {
    if (!this.canSave || this.busy) return;
    this.busy = true;
    this.errorMessage = '';
    const input = this.toWriteModel();
    const definitionId = this.selectedDefinitionId();
    const saveRequest = definitionId === null
      ? this.service.createDefinition(input)
      : this.service.updateDefinition(definitionId, input);
    const request: Observable<AutomaticQueueDefinition | AutomaticQueueRunAccepted> = runAfterSave
      ? saveRequest.pipe(switchMap((saved) => {
          this.applySavedDefinition(saved);
          return this.service.startRun(saved.id);
        }))
      : saveRequest;

    request.pipe(finalize(() => {
      this.busy = false;
      this.cdr.markForCheck();
    })).subscribe({
      next: (result) => {
        if ('runId' in result) {
          this.selectedRun.set(result.run);
          this.updateActiveRunReference(result.run);
          this.pollRun(result.runId);
          this.successMessage = 'Queue started.';
        } else {
          this.applySavedDefinition(result);
          this.successMessage = 'Queue saved.';
        }
      },
      error: (error) => this.errorMessage = this.getError(error, 'Unable to save the queue.'),
    });
  }

  deleteDefinition(): void {
    const definition = this.selectedDefinition();
    if (!definition || this.locked() || this.busy || !confirm(`Delete queue “${definition.name}”? Run history will be retained.`)) return;
    this.busy = true;
    this.service.deleteDefinition(definition.id).pipe(finalize(() => {
      this.busy = false;
      this.cdr.markForCheck();
    })).subscribe({
      next: () => {
        this.definitions.update((definitions) => definitions.filter((x) => x.id !== definition.id));
        const next = this.definitions()[0];
        if (next) this.selectDefinition(next); else this.createNew();
      },
      error: (error) => this.errorMessage = this.getError(error, 'Unable to delete the queue.'),
    });
  }

  pause(): void { this.changeRun('pause'); }
  resume(): void { this.changeRun('continue'); }
  cancel(): void { this.changeRun('cancel'); }

  cancelAndUnlock(): void {
    const definition = this.selectedDefinition();
    const activeRunId = definition?.activeRunId;
    if (!definition || activeRunId == null || activeRunId <= 0 || this.busy) return;

    this.busy = true;
    this.errorMessage = '';
    let alreadyFinished = false;
    this.service.getRun(activeRunId).pipe(
      switchMap((run) => {
        if (!this.isActiveStatus(run.status)) {
          alreadyFinished = true;
          return of(run);
        }
        return this.service.cancelRun(activeRunId).pipe(catchError((error: unknown) => {
          if (error instanceof HttpErrorResponse && error.status === 409) {
            alreadyFinished = true;
            return this.service.getRun(activeRunId);
          }
          return throwError(() => error);
        }));
      }),
      finalize(() => {
        this.busy = false;
        this.cdr.markForCheck();
      }),
    ).subscribe({
      next: (run) => {
        this.selectedRun.set(run);
        this.runs.update((runs) => [run, ...runs.filter((item) => item.id !== run.id)]);
        this.updateActiveRunReference(run);
        this.successMessage = alreadyFinished
          ? 'The queue was already finished and is now unlocked.'
          : 'The queue was canceled and unlocked.';
      },
      error: (error) => this.errorMessage = this.getError(error, 'Unable to cancel and unlock the queue.'),
    });
  }

  selectHistoryRun(runId: number | string): void {
    const id = Number(runId);
    const run = this.runs().find((x) => x.id === id);
    if (!run) return;
    this.selectedRun.set(run);
    if (this.isActiveStatus(run.status)) this.pollRun(run.id);
    else this.cdr.markForCheck();
  }

  blockStatus(block: EditorBlock): AutomaticQueueBlockRunStatus {
    return this.runBlock(block)?.status ?? 'Pending';
  }

  runBlock(block: EditorBlock): AutomaticQueueRunBlock | undefined {
    return this.selectedRun()?.blocks.find((x) => x.key === block.key);
  }

  statusIcon(status: AutomaticQueueBlockRunStatus): string {
    if (status === 'Succeeded' || status === 'CompletedWithWarnings') return 'check_circle';
    if (status === 'Failed' || status === 'Canceled') return 'cancel';
    if (status === 'Running') return 'schedule';
    if (status === 'Paused') return 'pause_circle';
    if (status === 'Skipped') return 'skip_next';
    return 'radio_button_unchecked';
  }

  countdown(block: EditorBlock): string | null {
    const runBlock = this.selectedRun()?.blocks.find((x) => x.key === block.key);
    if (!runBlock?.waitUntilUtc || runBlock.status !== 'Running') return null;
    const seconds = Math.max(0, Math.ceil((new Date(runBlock.waitUntilUtc).getTime() - this.now()) / 1000));
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${(seconds % 60).toString().padStart(2, '0')}`;
  }

  combinationOverrides(block: EditorBlock): string {
    const combination = block.presetCombination;
    if (!combination) return '';
    const cooldown = combination.cooldownMinutes === null || combination.cooldownSeconds === null
      ? 'server-default cooldown'
      : `${combination.cooldownMinutes}m ${combination.cooldownSeconds}s cooldown`;
    return `Top ${combination.listingLimit} listings · ${cooldown}`;
  }

  viewBlock(block: EditorBlock): void {
    const runBlock = this.selectedRun()?.blocks.find((x) => x.key === block.key);
    if (!runBlock) return;
    if (runBlock.type === 'ManualCheck' && runBlock.manualCheckRunId) {
      const data: ManualCheckTraceDialogData = { runId: runBlock.manualCheckRunId, initialView: 'results' };
      this.dialog.open<ManualCheckTraceDialogComponent, ManualCheckTraceDialogData>(ManualCheckTraceDialogComponent, {
        width: 'min(68rem, 96vw)', maxWidth: '96vw', maxHeight: '92vh', data,
      });
    } else if (runBlock.type === 'Delay') {
      this.dialog.open(AutomaticQueueDelayDialogComponent, { data: runBlock, width: '30rem', maxWidth: '96vw' });
    }
  }

  viewConfiguration(block: EditorBlock): void {
    this.dialog.open(AutomaticQueueConfigurationDialogComponent, {
      data: block,
      width: '32rem',
      maxWidth: '96vw',
    });
  }

  private openManualBlockDialog(initialBlock: AutomaticQueueBlock | null): void {
    const data: ManualCheckSetupDialogData = { queueBuilder: true, initialBlock };
    this.dialog.open<ManualCheckSetupDialogComponent, ManualCheckSetupDialogData, ManualCheckSetupDialogResult>(
      ManualCheckSetupDialogComponent,
      { data, width: 'min(76rem, 96vw)', maxWidth: '96vw', maxHeight: '92vh', disableClose: true },
    ).afterClosed().pipe(takeUntil(this.destroy$)).subscribe((result) => {
      if (!result || result.mode !== 'queue') return;
      const index = this.blocks.findIndex((x) => x.key === result.block.key);
      if (index >= 0) this.blocks[index] = { ...this.blocks[index], ...result.block };
      else this.blocks.push(result.block);
      this.blocks = [...this.blocks];
      this.cdr.markForCheck();
    });
  }

  private loadRuns(queueId: number, preferredRunId: number | null): void {
    this.service.getRuns(queueId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (runs) => {
        this.runs.set(runs);
        const preferred = runs.find((x) => x.id === preferredRunId) ?? runs[0] ?? null;
        this.selectedRun.set(preferred);
        if (preferred && this.isActiveStatus(preferred.status)) this.pollRun(preferred.id);
        this.cdr.markForCheck();
      },
      error: (error) => this.errorMessage = this.getError(error, 'Unable to load queue history.'),
    });
  }

  private pollRun(runId: number): void {
    this.pollStop$.next();
    timer(0, 2000).pipe(
      switchMap(() => this.service.getRun(runId)),
      takeUntil(this.pollStop$),
      takeUntil(this.destroy$),
      takeWhile((run) => this.isActiveStatus(run.status), true),
    ).subscribe({
      next: (run) => {
        this.selectedRun.set(run);
        this.runs.update((runs) => [run, ...runs.filter((item) => item.id !== run.id)]);
        this.updateActiveRunReference(run);
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.errorMessage = this.getError(error, 'Unable to retrieve queue progress.');
        this.cdr.markForCheck();
      },
    });
  }

  private changeRun(operation: 'pause' | 'continue' | 'cancel'): void {
    const selectedRun = this.selectedRun();
    if (!selectedRun || this.busy) return;
    this.busy = true;
    const request = operation === 'pause'
      ? this.service.pauseRun(selectedRun.id)
      : operation === 'continue'
        ? this.service.continueRun(selectedRun.id)
        : this.service.cancelRun(selectedRun.id);
    request.pipe(finalize(() => {
      this.busy = false;
      this.cdr.markForCheck();
    })).subscribe({
      next: (run) => {
        this.selectedRun.set(run);
        this.updateActiveRunReference(run);
        if (this.isActiveStatus(run.status) && run.status !== 'Paused') this.pollRun(run.id);
      },
      error: (error) => this.errorMessage = this.getError(error, `Unable to ${operation} the queue.`),
    });
  }

  private applySavedDefinition(saved: AutomaticQueueDefinition): void {
    const definitions = this.definitions();
    const index = definitions.findIndex((x) => x.id === saved.id);
    this.definitions.set(index >= 0
      ? definitions.map((x) => x.id === saved.id ? saved : x)
      : [...definitions, saved].sort((a, b) => a.name.localeCompare(b.name)));
    this.selectedDefinitionId.set(saved.id);
    this.name = saved.name;
    this.blocks = saved.blocks.map((x) => ({ ...x, productIds: x.productIds ? [...x.productIds] : null }));
  }

  private updateActiveRunReference(run: AutomaticQueueRun): void {
    if (!run.queueId) return;
    this.definitions.update((definitions) => definitions.map((definition) => definition.id === run.queueId
      ? { ...definition, activeRunId: this.isActiveStatus(run.status) ? run.id : null }
      : definition));
  }

  private toWriteModel(): AutomaticQueueWrite {
    return {
      name: this.name.trim(),
      blocks: this.blocks.map((block) => ({
        key: block.key,
        type: block.type,
        delaySeconds: block.type === 'Delay' ? block.delaySeconds : null,
        gameUrlId: block.type === 'ManualCheck' ? block.gameUrlId : null,
        templateMode: block.type === 'ManualCheck' ? block.templateMode : null,
        presetId: block.type === 'ManualCheck' ? block.presetId : null,
        presetCombination: block.type === 'ManualCheck' ? block.presetCombination : null,
        privateTemplate: block.type === 'ManualCheck' ? block.privateTemplate : null,
        bypassCache: block.type === 'ManualCheck' && block.bypassCache,
        productIds: block.type === 'ManualCheck' ? block.productIds : null,
      })),
    };
  }

  private isActiveStatus(status: AutomaticQueueRun['status']): boolean {
    return ['Queued', 'Running', 'PauseRequested', 'Paused'].includes(status);
  }

  private getError(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) return error.error?.detail ?? error.error?.message ?? fallback;
    return error instanceof Error ? error.message : fallback;
  }
}
