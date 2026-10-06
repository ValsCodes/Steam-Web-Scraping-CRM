import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { AutomaticQueueBlockWrite, AutomaticQueueDefinition } from '../../models';
import {
  AutomaticQueueCombinationDialogComponent,
  AutomaticQueueCombinationDialogData,
  AutomaticQueueCombinationDialogResult,
} from './automatic-queue-combination-dialog.component';

describe('AutomaticQueueCombinationDialogComponent', () => {
  let data: AutomaticQueueCombinationDialogData;
  let dialogRef: jasmine.SpyObj<MatDialogRef<AutomaticQueueCombinationDialogComponent, AutomaticQueueCombinationDialogResult>>;

  beforeEach(async () => {
    data = { definitions: [definition(1, 'First'), definition(2, 'Second'), definition(3, 'Third')], preferredQueueId: 2 };
    dialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [AutomaticQueueCombinationDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useFactory: () => data },
        { provide: MatDialogRef, useValue: dialogRef },
      ],
    }).compileComponents();
  });

  it('starts with the preferred queue followed by the first available alternative', () => {
    const component = createComponent();

    expect(component.queueIds()).toEqual([2, 1]);
    expect(component.combinationPreview()).toBe('Second → First');
    expect(component.validation().isValid).toBeTrue();
  });

  it('adds, reorders, removes, and confirms queues in visible order', () => {
    const component = createComponent();

    component.moveQueue(1, -1);
    component.addQueue();
    component.removeQueue(1);
    component.confirm();

    expect(component.queueIds()).toEqual([1, 3]);
    expect(dialogRef.close).toHaveBeenCalledOnceWith({ queueIds: [1, 3] });
  });

  it('rejects duplicate and unavailable queue selections', () => {
    const component = createComponent();

    component.updateQueue(1, 2);
    expect(component.validation()).toEqual({ isValid: false, message: 'Each queue can appear only once.' });

    component.updateQueue(1, 999);
    expect(component.validation()).toEqual({ isValid: false, message: 'One or more selected queues are no longer available.' });
  });

  it('enforces the queue-count and combined-block boundaries', () => {
    data.definitions = [definition(1, 'Only')];
    data.preferredQueueId = 1;
    let component = createComponent();
    expect(component.validation().message).toContain('between 2 and 10');

    data.definitions = Array.from({ length: 11 }, (_, index) => definition(index + 1, `Queue ${index + 1}`));
    data.preferredQueueId = 1;
    component = createComponent();
    for (let index = 0; index < 9; index++) component.addQueue();
    expect(component.queueIds().length).toBe(10);
    component.addQueue();
    expect(component.queueIds().length).toBe(10);

    data.definitions = [definition(1, 'Large one', 51), definition(2, 'Large two', 50)];
    data.preferredQueueId = 1;
    component = createComponent();
    expect(component.combinedBlockCount()).toBe(101);
    expect(component.validation()).toEqual({ isValid: false, message: 'Combined queues cannot exceed 100 blocks.' });
  });

  function createComponent(): AutomaticQueueCombinationDialogComponent {
    return TestBed.createComponent(AutomaticQueueCombinationDialogComponent).componentInstance;
  }

  function definition(id: number, name: string, blockCount = 1): AutomaticQueueDefinition {
    return {
      id,
      name,
      activeRunId: null,
      createdAtUtc: '2026-10-06T00:00:00Z',
      updatedAtUtc: '2026-10-06T00:00:00Z',
      blocks: Array.from({ length: blockCount }, (_, index) => ({
        ...delayBlock(index + 1),
        id: id * 1000 + index,
        sortOrder: index,
        gameId: null,
        gameName: null,
        gameUrlName: null,
        presetName: null,
      })),
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
});
