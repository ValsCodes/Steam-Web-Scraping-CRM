import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import { AutomaticQueueDefinition } from '../../models';

export interface AutomaticQueueCombinationDialogData {
  definitions: readonly AutomaticQueueDefinition[];
  preferredQueueId: number | null;
}

export interface AutomaticQueueCombinationDialogResult {
  queueIds: number[];
}

@Component({
  selector: 'steam-automatic-queue-combination-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>Combine queues</h2>
    <mat-dialog-content class="queue-combination-dialog">
      <p>Select the saved queues to copy. Their blocks will run sequentially in the order shown.</p>

      <div class="queue-combination-dialog__rows">
        @for (queueId of queueIds(); track $index; let index = $index) {
          <div class="queue-combination-dialog__row">
            <strong>{{ index === 0 ? 'Start with' : 'Then' }}</strong>
            <select
              [ngModel]="queueId"
              (ngModelChange)="updateQueue(index, $event)"
              [attr.aria-label]="'Combined queue ' + (index + 1)">
              <option [ngValue]="null">Select queue</option>
              @for (definition of data.definitions; track definition.id) {
                <option
                  [ngValue]="definition.id"
                  [disabled]="isSelectedElsewhere(definition.id, index)">
                  {{ definition.name }} ({{ definition.blocks.length }} blocks)
                </option>
              }
            </select>
            <button mat-button type="button" (click)="moveQueue(index, -1)" [disabled]="index === 0">Up</button>
            <button mat-button type="button" (click)="moveQueue(index, 1)" [disabled]="index === queueIds().length - 1">Down</button>
            <button mat-button type="button" color="warn" (click)="removeQueue(index)" [disabled]="queueIds().length <= 2">Remove</button>
          </div>
        }
      </div>

      <button
        mat-stroked-button
        type="button"
        (click)="addQueue()"
        [disabled]="queueIds().length >= maximumQueues || queueIds().length >= data.definitions.length">
        Add queue
      </button>

      <section class="queue-combination-dialog__preview" aria-live="polite">
        <strong>Execution order</strong>
        <span>{{ combinationPreview() }}</span>
        <small>{{ combinedBlockCount() }} total blocks</small>
      </section>

      @if (!validation().isValid) {
        <p class="queue-combination-dialog__validation" role="alert">{{ validation().message }}</p>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Cancel</button>
      <button mat-flat-button type="button" (click)="confirm()" [disabled]="!validation().isValid">Create draft</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .queue-combination-dialog { display: flex; min-width: min(42rem, 78vw); flex-direction: column; gap: .8rem; }
    .queue-combination-dialog > p { margin: 0; color: #64748b; }
    .queue-combination-dialog__rows { display: flex; flex-direction: column; gap: .55rem; }
    .queue-combination-dialog__row { display: grid; grid-template-columns: 6rem minmax(13rem, 1fr) auto auto auto; align-items: center; gap: .45rem; }
    .queue-combination-dialog__row select { border: 1px solid #cbd5e1; border-radius: .25rem; padding: .55rem .7rem; }
    .queue-combination-dialog__preview { display: flex; flex-direction: column; gap: .25rem; border: 1px solid #e2e8f0; border-radius: .375rem; background: #f8fafc; padding: .75rem; }
    .queue-combination-dialog__preview span { overflow-wrap: anywhere; }
    .queue-combination-dialog__preview small { color: #64748b; }
    .queue-combination-dialog__validation { margin: 0; color: #b91c1c; }
    @media (max-width: 700px) {
      .queue-combination-dialog { min-width: 0; }
      .queue-combination-dialog__row { grid-template-columns: 1fr; }
    }
  `],
})
export class AutomaticQueueCombinationDialogComponent {
  readonly data = inject<AutomaticQueueCombinationDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<AutomaticQueueCombinationDialogComponent, AutomaticQueueCombinationDialogResult>>(MatDialogRef);
  readonly minimumQueues = 2;
  readonly maximumQueues = 10;
  readonly maximumBlocks = 100;
  readonly queueIds = signal<(number | null)[]>(this.initialQueueIds());

  readonly selectedDefinitions = computed(() => this.queueIds().map((id) =>
    this.data.definitions.find((definition) => definition.id === id) ?? null));
  readonly combinedBlockCount = computed(() => this.selectedDefinitions().reduce(
    (total, definition) => total + (definition?.blocks.length ?? 0),
    0,
  ));
  readonly combinationPreview = computed(() => this.selectedDefinitions()
    .map((definition) => definition?.name ?? 'Select queue')
    .join(' → '));
  readonly validation = computed((): { isValid: boolean; message: string } => {
    const queueIds = this.queueIds();
    if (queueIds.length < this.minimumQueues || queueIds.length > this.maximumQueues) {
      return { isValid: false, message: `Choose between ${this.minimumQueues} and ${this.maximumQueues} queues.` };
    }
    if (queueIds.some((id) => id === null)) {
      return { isValid: false, message: 'Choose a queue in every row.' };
    }
    if (new Set(queueIds).size !== queueIds.length) {
      return { isValid: false, message: 'Each queue can appear only once.' };
    }
    if (this.selectedDefinitions().some((definition) => definition === null)) {
      return { isValid: false, message: 'One or more selected queues are no longer available.' };
    }
    if (this.combinedBlockCount() > this.maximumBlocks) {
      return { isValid: false, message: `Combined queues cannot exceed ${this.maximumBlocks} blocks.` };
    }
    return { isValid: true, message: '' };
  });

  updateQueue(index: number, queueId: number | null): void {
    this.queueIds.update((queueIds) => queueIds.map((currentId, currentIndex) =>
      currentIndex === index ? queueId : currentId));
  }

  addQueue(): void {
    const queueIds = this.queueIds();
    if (queueIds.length >= this.maximumQueues || queueIds.length >= this.data.definitions.length) return;
    const selectedIds = new Set(queueIds);
    const next = this.data.definitions.find((definition) => !selectedIds.has(definition.id));
    if (next) this.queueIds.set([...queueIds, next.id]);
  }

  removeQueue(index: number): void {
    if (this.queueIds().length <= this.minimumQueues) return;
    this.queueIds.update((queueIds) => queueIds.filter((_, currentIndex) => currentIndex !== index));
  }

  moveQueue(index: number, direction: -1 | 1): void {
    const target = index + direction;
    if (target < 0 || target >= this.queueIds().length) return;
    const queueIds = [...this.queueIds()];
    [queueIds[index], queueIds[target]] = [queueIds[target], queueIds[index]];
    this.queueIds.set(queueIds);
  }

  isSelectedElsewhere(queueId: number, rowIndex: number): boolean {
    return this.queueIds().some((selectedId, index) => index !== rowIndex && selectedId === queueId);
  }

  confirm(): void {
    if (!this.validation().isValid) return;
    this.dialogRef.close({ queueIds: this.queueIds() as number[] });
  }

  private initialQueueIds(): number[] {
    const preferred = this.data.definitions.find((definition) => definition.id === this.data.preferredQueueId)
      ?? this.data.definitions[0];
    if (!preferred) return [];
    const alternative = this.data.definitions.find((definition) => definition.id !== preferred.id);
    return alternative ? [preferred.id, alternative.id] : [preferred.id];
  }
}
