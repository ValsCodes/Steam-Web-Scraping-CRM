import { ChangeDetectorRef } from '@angular/core';
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';

import {
  AutomaticQueueBlockWrite,
  AutomaticQueueDefinition,
  AutomaticQueueRun,
} from '../../models';
import { AutomaticQueueService } from '../../services';
import { ManualCheckTraceDialogComponent } from '../manual-mode-v2/manual-check-trace-dialog.component';
import { AutomaticQueueDelayDialogComponent } from './automatic-queue-delay-dialog.component';
import { AutomaticQueuePage } from './automatic-queue-page';

describe('AutomaticQueuePage', () => {
  let component: AutomaticQueuePage;
  let service: jasmine.SpyObj<AutomaticQueueService>;
  let dialog: jasmine.SpyObj<MatDialog>;

  beforeEach(() => {
    service = jasmine.createSpyObj<AutomaticQueueService>('AutomaticQueueService', [
      'getDefinitions',
      'createDefinition',
      'updateDefinition',
      'deleteDefinition',
      'startRun',
      'getRuns',
      'getRun',
      'pauseRun',
      'continueRun',
      'cancelRun',
    ]);
    dialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    dialog.open.and.returnValue({ afterClosed: () => of(undefined) } as never);

    TestBed.configureTestingModule({
      providers: [
        { provide: AutomaticQueueService, useValue: service },
        { provide: MatDialog, useValue: dialog },
        { provide: ChangeDetectorRef, useValue: jasmine.createSpyObj('ChangeDetectorRef', ['markForCheck']) },
      ],
    });
    component = TestBed.runInInjectionContext(() => new AutomaticQueuePage());
  });

  afterEach(() => component.ngOnDestroy());

  it('adds, duplicates, reorders, and removes blocks with stable keys', () => {
    component.addDelayBlock();
    const firstKey = component.blocks[0].key;
    component.duplicateBlock(0);

    expect(component.blocks.length).toBe(2);
    expect(component.blocks[1].key).not.toBe(firstKey);

    component.drop({ previousIndex: 1, currentIndex: 0 } as never);
    expect(component.blocks[0].key).not.toBe(firstKey);

    component.removeBlock(0);
    expect(component.blocks.map((block) => block.key)).toEqual([firstKey]);
  });

  it('saves the visible definition before starting a run', () => {
    const block = manualBlock();
    const definition = savedDefinition(block);
    const run = queueRun('Running', block);
    component.name = definition.name;
    component.blocks = [block];
    service.createDefinition.and.returnValue(of(definition));
    service.startRun.and.returnValue(of({ runId: run.id, run }));
    service.getRun.and.returnValue(of(run));

    component.run();

    expect(service.createDefinition).toHaveBeenCalledBefore(service.startRun);
    expect(service.createDefinition).toHaveBeenCalledWith(jasmine.objectContaining({ name: 'Nightly checks' }));
    expect(service.startRun).toHaveBeenCalledOnceWith(definition.id);
    expect(component.selectedRun()?.id).toBe(run.id);
    expect(component.successMessage).toBe('Queue started.');
  });

  it('saves without starting when the Save action is used', () => {
    const block = manualBlock();
    const definition = savedDefinition(block);
    component.name = definition.name;
    component.blocks = [block];
    service.createDefinition.and.returnValue(of(definition));

    component.save();

    expect(service.createDefinition).toHaveBeenCalledOnceWith(jasmine.objectContaining({ name: 'Nightly checks' }));
    expect(service.startRun).not.toHaveBeenCalled();
    expect(component.successMessage).toBe('Queue saved.');
  });

  it('maps block states, warning counts, and local delay countdowns from the selected run', () => {
    const delay = delayBlock(60);
    const run = queueRun('Running', delay);
    run.blocks[0].status = 'CompletedWithWarnings';
    run.blocks[0].warningCount = 2;
    component.blocks = [delay];
    component.selectedRun.set(run);

    expect(component.blockStatus(delay)).toBe('CompletedWithWarnings');
    expect(component.statusIcon('CompletedWithWarnings')).toBe('check_circle');
    expect(component.runBlock(delay)?.warningCount).toBe(2);

    run.blocks[0].status = 'Running';
    run.blocks[0].waitUntilUtc = new Date(component.now() + 5000).toISOString();
    expect(component.countdown(delay)).toBe('00:00:05');
  });

  it('updates a running timeout countdown every second', fakeAsync(() => {
    service.getDefinitions.and.returnValue(of([]));
    component.ngOnInit();
    tick(0);
    const delay = delayBlock(10);
    const run = queueRun('Running', delay);
    run.blocks[0].waitUntilUtc = new Date(component.now() + 5000).toISOString();
    component.blocks = [delay];
    component.selectedRun.set(run);

    expect(component.countdown(delay)).toBe('00:00:05');

    tick(1000);

    expect(component.countdown(delay)).toBe('00:00:04');
  }));

  it('reuses the trace dialog for manual results and the timing dialog for delays', () => {
    const manual = manualBlock();
    const delay = delayBlock(10);
    const run = queueRun('Succeeded', manual, delay);
    run.blocks[0].manualCheckRunId = 44;
    component.selectedRun.set(run);

    component.viewBlock(manual);
    component.viewBlock(delay);

    expect(dialog.open.calls.argsFor(0)[0]).toBe(ManualCheckTraceDialogComponent);
    expect(dialog.open.calls.argsFor(0)[1]?.data).toEqual({ runId: 44, initialView: 'results' });
    expect(dialog.open.calls.argsFor(1)[0]).toBe(AutomaticQueueDelayDialogComponent);
  });

  it('stops two-second polling when destroyed', fakeAsync(() => {
    const block = manualBlock();
    const run = queueRun('Running', block);
    service.getRun.and.returnValue(of(run));

    (component as unknown as { pollRun(id: number): void }).pollRun(run.id);
    tick(0);
    expect(service.getRun).toHaveBeenCalledTimes(1);

    component.ngOnDestroy();
    tick(2500);
    expect(service.getRun).toHaveBeenCalledTimes(1);
  }));

  it('releases the definition lock when polling receives a terminal run', fakeAsync(() => {
    const block = manualBlock();
    const run = queueRun('Succeeded', block);
    const definition = { ...savedDefinition(block), activeRunId: run.id };
    component.definitions.set([definition]);
    component.selectedDefinitionId.set(definition.id);
    service.getRun.and.returnValue(of(run));

    (component as unknown as { pollRun(id: number): void }).pollRun(run.id);
    tick(0);

    expect(component.locked()).toBeFalse();
    expect(component.selectedDefinition()?.activeRunId).toBeNull();
  }));

  it('cancels the active run and manually unlocks the definition', () => {
    const block = manualBlock();
    const running = queueRun('Running', block);
    const canceled = { ...running, status: 'Canceled' as const };
    const definition = { ...savedDefinition(block), activeRunId: running.id };
    component.definitions.set([definition]);
    component.selectedDefinitionId.set(definition.id);
    service.getRun.and.returnValue(of(running));
    service.cancelRun.and.returnValue(of(canceled));

    component.cancelAndUnlock();

    expect(service.getRun).toHaveBeenCalledOnceWith(running.id);
    expect(service.cancelRun).toHaveBeenCalledOnceWith(running.id);
    expect(component.locked()).toBeFalse();
    expect(component.successMessage).toBe('The queue was canceled and unlocked.');
  });

  it('refreshes a stale lock when the active run already finished', () => {
    const block = manualBlock();
    const finished = queueRun('Succeeded', block);
    const definition = { ...savedDefinition(block), activeRunId: finished.id };
    component.definitions.set([definition]);
    component.selectedDefinitionId.set(definition.id);
    service.getRun.and.returnValue(of(finished));

    component.cancelAndUnlock();

    expect(service.getRun).toHaveBeenCalledOnceWith(finished.id);
    expect(service.cancelRun).not.toHaveBeenCalled();
    expect(component.locked()).toBeFalse();
    expect(component.successMessage).toBe('The queue was already finished and is now unlocked.');
  });

  it('does not lock or request run zero for an invalid active-run value', () => {
    const block = manualBlock();
    const definition = { ...savedDefinition(block), activeRunId: 0 };
    component.definitions.set([definition]);
    component.selectedDefinitionId.set(definition.id);

    component.cancelAndUnlock();

    expect(component.locked()).toBeFalse();
    expect(service.getRun).not.toHaveBeenCalled();
    expect(service.cancelRun).not.toHaveBeenCalled();
  });

  function manualBlock(): AutomaticQueueBlockWrite {
    return {
      key: crypto.randomUUID(),
      type: 'ManualCheck',
      delaySeconds: null,
      gameUrlId: 7,
      templateMode: 'SavedPreset',
      presetId: 3,
      privateTemplate: null,
      bypassCache: false,
      productIds: null,
    };
  }

  function delayBlock(seconds: number): AutomaticQueueBlockWrite {
    return {
      key: crypto.randomUUID(),
      type: 'Delay',
      delaySeconds: seconds,
      gameUrlId: null,
      templateMode: null,
      presetId: null,
      privateTemplate: null,
      bypassCache: false,
      productIds: null,
    };
  }

  function savedDefinition(block: AutomaticQueueBlockWrite): AutomaticQueueDefinition {
    return {
      id: 11,
      name: 'Nightly checks',
      createdAtUtc: '2026-09-27T08:00:00Z',
      updatedAtUtc: '2026-09-27T08:00:00Z',
      activeRunId: null,
      blocks: [{
        ...block,
        id: 101,
        sortOrder: 0,
        gameId: 1,
        gameName: 'Alpha Game',
        gameUrlName: 'Steam Market',
        presetName: 'Default',
      }],
    };
  }

  function queueRun(
    status: AutomaticQueueRun['status'],
    ...blocks: AutomaticQueueBlockWrite[]
  ): AutomaticQueueRun {
    return {
      id: 21,
      queueId: 11,
      queueName: 'Nightly checks',
      status,
      currentBlockIndex: 0,
      totalBlocks: blocks.length,
      completedBlocks: status === 'Succeeded' ? blocks.length : 0,
      date: '2026-09-27T08:00:00Z',
      startedAtUtc: '2026-09-27T08:00:00Z',
      completedAtUtc: status === 'Succeeded' ? '2026-09-27T08:01:00Z' : null,
      errorText: null,
      correlationId: 'trace-21',
      blocks: blocks.map((block, index) => ({
        id: 200 + index,
        key: block.key,
        sortOrder: index,
        type: block.type,
        status: status === 'Succeeded' ? 'Succeeded' : 'Running',
        configuration: {
          ...block,
          id: 100 + index,
          sortOrder: index,
          gameId: block.type === 'ManualCheck' ? 1 : null,
          gameName: block.type === 'ManualCheck' ? 'Alpha Game' : null,
          gameUrlName: block.type === 'ManualCheck' ? 'Steam Market' : null,
          presetName: block.type === 'ManualCheck' ? 'Default' : null,
        },
        manualCheckRunId: null,
        warningCount: 0,
        waitUntilUtc: null,
        remainingDelaySeconds: block.delaySeconds,
        startedAtUtc: null,
        completedAtUtc: null,
        errorText: null,
      })),
    };
  }
});
