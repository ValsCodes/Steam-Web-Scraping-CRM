import { CommonModule } from '@angular/common';
import {
  CdkDrag,
  CdkDragDrop,
  CdkDragHandle,
  CdkDragPlaceholder,
  CdkDropList,
  CdkDropListGroup,
  moveItemInArray,
  transferArrayItem,
} from '@angular/cdk/drag-drop';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';

import { ManualCheckConditionOperator } from '../../models';
import {
  containsManualCheckGroup,
  createManualCheckCriterionNode,
  createManualCheckGroupNode,
  ManualCheckExpressionNode,
  ManualCheckGroupNode,
  normalizeManualCheckExpressionOperators,
  validateManualCheckExpression,
} from './manual-check-expression';

@Component({
  selector: 'steam-manual-check-expression-editor',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    CdkDrag,
    CdkDragHandle,
    CdkDragPlaceholder,
    CdkDropList,
    CdkDropListGroup,
  ],
  template: `
    <div class="expression-editor" cdkDropListGroup>
      <ng-container
        *ngTemplateOutlet="groupTemplate; context: { $implicit: root, depth: 0 }">
      </ng-container>
    </div>

    <ng-template #groupTemplate let-group let-depth="depth">
      <section
        class="expression-group"
        [class.expression-group--root]="group.isRoot"
        [attr.aria-label]="group.isRoot ? 'Criteria expression' : 'Criterion group'">
        <header class="expression-group__header">
          <strong>{{ group.isRoot ? 'Criteria' : 'Group' }}</strong>
          <span>{{ group.children.length }} item(s)</span>
          <span class="expression-group__spacer"></span>
          <button
            mat-stroked-button
            type="button"
            (click)="addCriterion(group)"
            [disabled]="criterionCount >= 25">
            Add criterion
          </button>
          <button
            mat-stroked-button
            type="button"
            (click)="addGroup(group)"
            [disabled]="groupCount >= 25">
            Add group
          </button>
          @if (!group.isRoot) {
            <button mat-stroked-button type="button" (click)="ungroup(group)">
              Ungroup
            </button>
          }
        </header>

        <div
          class="expression-group__drop-list"
          cdkDropList
          [cdkDropListData]="group"
          [cdkDropListEnterPredicate]="canEnterGroup"
          (cdkDropListDropped)="dropNode($event)">
          @if (group.children.length === 0) {
            <div class="expression-group__empty">Drop an item here or add one.</div>
          }

          @for (node of group.children; track node.id; let index = $index) {
            <article
              class="expression-node"
              cdkDrag
              [cdkDragData]="node">
              <div class="expression-node__row">
                <button
                  type="button"
                  class="expression-node__drag-handle"
                  cdkDragHandle
                  [attr.aria-label]="nodeLabel(node) + ': drag to move'">
                  <span aria-hidden="true">⋮⋮</span>
                </button>

                @if (index === 0) {
                  <select disabled [attr.aria-label]="nodeLabel(node) + ' starts this group'">
                    <option>Start</option>
                  </select>
                } @else {
                  <select
                    [(ngModel)]="node.conditionOperatorId"
                    (ngModelChange)="changed()"
                    [name]="'manualCheckOperator-' + node.id"
                    [attr.aria-label]="'Condition operator for ' + nodeLabel(node)">
                    @for (operator of operators; track operator.id) {
                      <option [ngValue]="operator.id">{{ operator.name }}</option>
                    }
                  </select>
                }

                @if (node.kind === 'criterion') {
                  <input
                    [(ngModel)]="node.nameContains"
                    (ngModelChange)="changed()"
                    [name]="'manualCheckName-' + node.id"
                    maxlength="200"
                    placeholder="Description name contains"
                    [attr.aria-label]="nodeLabel(node) + ': description name contains'" />
                  <input
                    [(ngModel)]="node.valueContains"
                    (ngModelChange)="changed()"
                    [name]="'manualCheckValue-' + node.id"
                    maxlength="200"
                    placeholder="Description value contains"
                    [attr.aria-label]="nodeLabel(node) + ': description value contains'" />
                } @else {
                  <span class="expression-node__group-label">Nested group</span>
                }

                <div class="expression-node__actions">
                  <button
                    type="button"
                    (click)="moveWithin(group, index, -1)"
                    [disabled]="index === 0"
                    [attr.aria-label]="'Move ' + nodeLabel(node) + ' up'">
                    ↑
                  </button>
                  <button
                    type="button"
                    (click)="moveWithin(group, index, 1)"
                    [disabled]="index === group.children.length - 1"
                    [attr.aria-label]="'Move ' + nodeLabel(node) + ' down'">
                    ↓
                  </button>
                  <button
                    type="button"
                    (click)="moveIntoAdjacent(group, index, -1)"
                    [disabled]="!isAdjacentGroup(group, index, -1)"
                    [attr.aria-label]="'Move ' + nodeLabel(node) + ' into previous group'">
                    Into previous
                  </button>
                  <button
                    type="button"
                    (click)="moveIntoAdjacent(group, index, 1)"
                    [disabled]="!isAdjacentGroup(group, index, 1)"
                    [attr.aria-label]="'Move ' + nodeLabel(node) + ' into next group'">
                    Into next
                  </button>
                  <button
                    type="button"
                    (click)="moveOut(group, index)"
                    [disabled]="group.isRoot"
                    [attr.aria-label]="'Move ' + nodeLabel(node) + ' out one level'">
                    Out
                  </button>
                  @if (node.kind === 'criterion') {
                    <button
                      type="button"
                      class="expression-node__remove"
                      (click)="removeCriterion(group, index)"
                      [disabled]="criterionCount === 1"
                      [attr.aria-label]="'Remove ' + nodeLabel(node)">
                      Remove
                    </button>
                  }
                </div>
              </div>

              @if (node.kind === 'group') {
                <ng-container
                  *ngTemplateOutlet="groupTemplate; context: { $implicit: node, depth: depth + 1 }">
                </ng-container>
              }

              <div class="expression-node__placeholder" *cdkDragPlaceholder></div>
            </article>
          }
        </div>
      </section>
    </ng-template>

    <p class="visually-hidden" aria-live="polite">{{ announcement }}</p>
  `,
  styles: [`
    .expression-editor { display: block; }
    .expression-group { display: flex; flex-direction: column; gap: .6rem; border: 1px solid #bfdbfe; border-radius: .5rem; background: #eff6ff; padding: .65rem; }
    .expression-group--root { border-color: #cbd5e1; background: #f8fafc; }
    .expression-group__header { display: flex; flex-wrap: wrap; align-items: center; gap: .5rem; }
    .expression-group__header > span:not(.expression-group__spacer) { color: #64748b; font-size: .8rem; }
    .expression-group__spacer { flex: 1; }
    .expression-group__drop-list { display: flex; min-height: 3.5rem; flex-direction: column; gap: .5rem; }
    .expression-group__empty { display: grid; min-height: 3.5rem; place-items: center; border: 2px dashed #93c5fd; border-radius: .375rem; color: #64748b; }
    .expression-node { border: 1px solid #dbe3ee; border-radius: .375rem; background: #fff; padding: .5rem; }
    .expression-node__row { display: grid; grid-template-columns: auto 8rem minmax(9rem, 1fr) minmax(9rem, 1fr) auto; align-items: center; gap: .45rem; }
    .expression-node__row select, .expression-node__row input { min-width: 0; border: 1px solid #cbd5e1; border-radius: .25rem; padding: .5rem .6rem; }
    .expression-node__drag-handle { border: 0; background: transparent; color: #64748b; cursor: grab; font-size: 1rem; }
    .expression-node__group-label { color: #334155; font-weight: 600; }
    .expression-node__actions { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: .25rem; }
    .expression-node__actions button { border: 1px solid #cbd5e1; border-radius: .25rem; background: #fff; padding: .25rem .4rem; color: #334155; }
    .expression-node__actions button:disabled { opacity: .45; }
    .expression-node__actions .expression-node__remove { color: #b91c1c; }
    .expression-node > .expression-group { margin: .6rem 0 0 1.5rem; }
    .expression-node__placeholder { min-height: 3.5rem; border: 2px dashed #2563eb; border-radius: .375rem; background: #dbeafe; }
    .cdk-drag-preview { box-sizing: border-box; border-radius: .375rem; background: #fff; box-shadow: 0 8px 24px rgb(15 23 42 / 20%); }
    .cdk-drag-animating { transition: transform 180ms ease; }
    .expression-group__drop-list.cdk-drop-list-dragging .expression-node:not(.cdk-drag-placeholder) { transition: transform 180ms ease; }
    .visually-hidden { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0 0 0 0); white-space: nowrap; }
    @media (max-width: 800px) {
      .expression-node__row { grid-template-columns: auto 7rem 1fr; }
      .expression-node__actions { grid-column: 2 / -1; justify-content: flex-start; }
      .expression-node > .expression-group { margin-left: .5rem; }
    }
  `],
})
export class ManualCheckExpressionEditorComponent {
  @Input({ required: true }) root!: ManualCheckGroupNode;
  @Input() operators: ManualCheckConditionOperator[] = [];
  @Output() readonly expressionChange = new EventEmitter<void>();

  announcement = '';

  readonly canEnterGroup = (
    drag: CdkDrag<ManualCheckExpressionNode>,
    drop: CdkDropList<ManualCheckGroupNode>,
  ): boolean => {
    const node = drag.data;
    const target = drop.data;
    return node.kind !== 'group' || !containsManualCheckGroup(node, target.id);
  };

  get criterionCount(): number {
    return this.validation.criterionCount;
  }

  get groupCount(): number {
    return this.validation.groupCount;
  }

  addCriterion(group: ManualCheckGroupNode): void {
    if (this.criterionCount >= 25) {
      return;
    }

    group.children.push(createManualCheckCriterionNode(
      group.children.length === 0 ? null : this.defaultOperatorId,
    ));
    this.finishChange('Criterion added.');
  }

  addGroup(group: ManualCheckGroupNode): void {
    if (this.groupCount >= 25) {
      return;
    }

    group.children.push(createManualCheckGroupNode(
      group.children.length === 0 ? null : this.defaultOperatorId,
    ));
    this.finishChange('Empty group added. Add or move criteria into it before saving.');
  }

  removeCriterion(group: ManualCheckGroupNode, index: number): void {
    if (this.criterionCount <= 1) {
      return;
    }

    group.children.splice(index, 1);
    this.finishChange('Criterion removed.');
  }

  ungroup(group: ManualCheckGroupNode): void {
    const location = this.findNode(this.root, group.id);
    if (!location) {
      return;
    }

    if (group.children.length > 0) {
      group.children[0].conditionOperatorId = group.conditionOperatorId;
    }
    location.parent.children.splice(location.index, 1, ...group.children);
    this.finishChange('Group removed and its contents kept in order.');
  }

  dropNode(event: CdkDragDrop<ManualCheckGroupNode>): void {
    const source = event.previousContainer.data;
    const target = event.container.data;
    const node = source.children[event.previousIndex];
    if (node?.kind === 'group' && containsManualCheckGroup(node, target.id)) {
      this.announcement = 'That move is not allowed because a group cannot contain itself.';
      return;
    }

    if (source.id === target.id) {
      moveItemInArray(target.children, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        source.children,
        target.children,
        event.previousIndex,
        event.currentIndex,
      );
    }

    this.finishChange(`${this.nodeLabel(node)} moved.`);
  }

  moveWithin(group: ManualCheckGroupNode, index: number, direction: -1 | 1): void {
    const targetIndex = index + direction;
    if (targetIndex < 0 || targetIndex >= group.children.length) {
      return;
    }

    const node = group.children[index];
    moveItemInArray(group.children, index, targetIndex);
    this.finishChange(`${this.nodeLabel(node)} moved ${direction < 0 ? 'up' : 'down'}.`);
  }

  isAdjacentGroup(
    group: ManualCheckGroupNode,
    index: number,
    direction: -1 | 1,
  ): boolean {
    return group.children[index + direction]?.kind === 'group';
  }

  moveIntoAdjacent(
    group: ManualCheckGroupNode,
    index: number,
    direction: -1 | 1,
  ): void {
    const target = group.children[index + direction];
    if (!target || target.kind !== 'group') {
      return;
    }

    const [node] = group.children.splice(index, 1);
    if (direction < 0) {
      target.children.push(node);
    } else {
      target.children.unshift(node);
    }
    this.finishChange(`${this.nodeLabel(node)} moved into the adjacent group.`);
  }

  moveOut(group: ManualCheckGroupNode, index: number): void {
    if (group.isRoot) {
      return;
    }

    const groupLocation = this.findNode(this.root, group.id);
    if (!groupLocation) {
      return;
    }

    const [node] = group.children.splice(index, 1);
    groupLocation.parent.children.splice(groupLocation.index + 1, 0, node);
    this.finishChange(`${this.nodeLabel(node)} moved out one level.`);
  }

  changed(): void {
    this.expressionChange.emit();
  }

  nodeLabel(node: ManualCheckExpressionNode | undefined): string {
    if (!node) {
      return 'Item';
    }
    if (node.kind === 'group') {
      return 'Group';
    }
    return node.nameContains?.trim() || node.valueContains?.trim() || 'Criterion';
  }

  private get validation() {
    return validateManualCheckExpression(
      this.root,
      new Set(this.operators.map((operator) => operator.id)),
    );
  }

  private get defaultOperatorId(): number | null {
    return this.operators.find((operator) => operator.name.toUpperCase() === 'AND')?.id
      ?? this.operators[0]?.id
      ?? null;
  }

  private finishChange(announcement: string): void {
    normalizeManualCheckExpressionOperators(
      this.root,
      this.defaultOperatorId,
      new Set(this.operators.map((operator) => operator.id)),
    );
    this.announcement = announcement;
    this.expressionChange.emit();
  }

  private findNode(
    group: ManualCheckGroupNode,
    nodeId: string,
  ): { parent: ManualCheckGroupNode; index: number } | null {
    for (const [index, child] of group.children.entries()) {
      if (child.id === nodeId) {
        return { parent: group, index };
      }
      if (child.kind === 'group') {
        const nested = this.findNode(child, nodeId);
        if (nested) {
          return nested;
        }
      }
    }

    return null;
  }
}
