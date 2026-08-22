import {
  ManualCheckConditionOperator,
  ManualCheckCriterion,
} from '../../models/manual-check.model';

export type ManualCheckExpressionNode =
  | ManualCheckCriterionNode
  | ManualCheckGroupNode;

export interface ManualCheckCriterionNode {
  kind: 'criterion';
  id: string;
  conditionOperatorId: number | null;
  conditionOperatorName?: string | null;
  nameContains: string | null;
  valueContains: string | null;
}

export interface ManualCheckGroupNode {
  kind: 'group';
  id: string;
  conditionOperatorId: number | null;
  conditionOperatorName?: string | null;
  children: ManualCheckExpressionNode[];
  isRoot?: boolean;
}

export interface ManualCheckExpressionParseResult {
  root: ManualCheckGroupNode;
  isValid: boolean;
}

export interface ManualCheckExpressionValidationResult {
  isValid: boolean;
  criterionCount: number;
  groupCount: number;
}

let nextNodeId = 0;

export function createManualCheckCriterionNode(
  conditionOperatorId: number | null = null,
): ManualCheckCriterionNode {
  return {
    kind: 'criterion',
    id: createNodeId('criterion'),
    conditionOperatorId,
    nameContains: '',
    valueContains: '',
  };
}

export function createManualCheckGroupNode(
  conditionOperatorId: number | null = null,
  isRoot = false,
): ManualCheckGroupNode {
  return {
    kind: 'group',
    id: createNodeId(isRoot ? 'root' : 'group'),
    conditionOperatorId,
    children: [],
    isRoot,
  };
}

export function parseManualCheckExpression(
  criteria: ManualCheckCriterion[],
): ManualCheckExpressionParseResult {
  const root = createManualCheckGroupNode(null, true);
  const groupStack: ManualCheckGroupNode[] = [root];
  let totalGroups = 0;
  let isValid = criteria.length > 0;

  for (const [index, criterion] of criteria.entries()) {
    const openCount = criterion.openGroupCount ?? 0;
    const closeCount = criterion.closeGroupCount ?? 0;

    if (!isValidGroupCount(openCount) || !isValidGroupCount(closeCount)) {
      isValid = false;
      break;
    }

    totalGroups += openCount;
    if (totalGroups > 25 || groupStack.length - 1 + openCount > 25) {
      isValid = false;
      break;
    }

    let currentGroup = groupStack[groupStack.length - 1];
    for (let groupIndex = 0; groupIndex < openCount; groupIndex += 1) {
      const group = createManualCheckGroupNode(
        groupIndex === 0 ? criterion.conditionOperatorId : null,
      );
      if (groupIndex === 0) {
        group.conditionOperatorName = criterion.conditionOperatorName;
      }
      currentGroup.children.push(group);
      groupStack.push(group);
      currentGroup = group;
    }

    currentGroup.children.push({
      kind: 'criterion',
      id: createNodeId('criterion'),
      conditionOperatorId:
        openCount > 0 ? null : criterion.conditionOperatorId,
      conditionOperatorName:
        openCount > 0 ? null : criterion.conditionOperatorName,
      nameContains: criterion.nameContains,
      valueContains: criterion.valueContains,
    });

    if (closeCount >= groupStack.length) {
      isValid = false;
      break;
    }

    for (let closeIndex = 0; closeIndex < closeCount; closeIndex += 1) {
      groupStack.pop();
    }

    if (index === 0 && criterion.conditionOperatorId !== null) {
      isValid = false;
    }
  }

  if (!isValid || groupStack.length !== 1) {
    return {
      root: createFlatFallback(criteria),
      isValid: false,
    };
  }

  return { root, isValid: true };
}

export function flattenManualCheckExpression(
  root: ManualCheckGroupNode,
): ManualCheckCriterion[] {
  return flattenChildren(root.children);
}

export function normalizeManualCheckExpressionOperators(
  group: ManualCheckGroupNode,
  defaultOperatorId: number | null,
  validOperatorIds?: Set<number>,
): void {
  group.children.forEach((child, index) => {
    if (index === 0) {
      child.conditionOperatorId = null;
    } else if (
      child.conditionOperatorId === null ||
      (validOperatorIds && !validOperatorIds.has(child.conditionOperatorId))
    ) {
      child.conditionOperatorId = defaultOperatorId;
    }

    if (child.kind === 'group') {
      normalizeManualCheckExpressionOperators(child, defaultOperatorId, validOperatorIds);
    }
  });
}

export function validateManualCheckExpression(
  root: ManualCheckGroupNode,
  validOperatorIds: Set<number>,
): ManualCheckExpressionValidationResult {
  let criterionCount = 0;
  let groupCount = 0;
  let isValid = true;

  const visitGroup = (group: ManualCheckGroupNode): void => {
    if (!group.isRoot) {
      groupCount += 1;
      if (group.children.length === 0) {
        isValid = false;
      }
    }

    group.children.forEach((child, index) => {
      if (
        (index === 0 && child.conditionOperatorId !== null) ||
        (index > 0 &&
          (child.conditionOperatorId === null ||
            !validOperatorIds.has(child.conditionOperatorId)))
      ) {
        isValid = false;
      }

      if (child.kind === 'criterion') {
        criterionCount += 1;
        if (!child.nameContains?.trim() && !child.valueContains?.trim()) {
          isValid = false;
        }
      } else {
        visitGroup(child);
      }
    });
  };

  visitGroup(root);
  if (
    root.children.length === 0 ||
    criterionCount === 0 ||
    criterionCount > 25 ||
    groupCount > 25
  ) {
    isValid = false;
  }

  return { isValid, criterionCount, groupCount };
}

export function formatManualCheckExpression(
  root: ManualCheckGroupNode,
  operators: ManualCheckConditionOperator[],
): string {
  const operatorNames = new Map(
    operators.map((operator) => [operator.id, operator.name]),
  );

  return formatChildren(root.children, operatorNames);
}

export function containsManualCheckGroup(
  group: ManualCheckGroupNode,
  targetGroupId: string,
): boolean {
  if (group.id === targetGroupId) {
    return true;
  }

  return group.children.some(
    (child) =>
      child.kind === 'group' &&
      containsManualCheckGroup(child, targetGroupId),
  );
}

function flattenChildren(
  children: ManualCheckExpressionNode[],
): ManualCheckCriterion[] {
  const flattened: ManualCheckCriterion[] = [];

  for (const child of children) {
    if (child.kind === 'criterion') {
      flattened.push({
        conditionOperatorId: child.conditionOperatorId,
        conditionOperatorName: child.conditionOperatorName,
        openGroupCount: 0,
        closeGroupCount: 0,
        nameContains: child.nameContains,
        valueContains: child.valueContains,
      });
      continue;
    }

    const groupCriteria = flattenChildren(child.children);
    if (groupCriteria.length === 0) {
      continue;
    }

    groupCriteria[0].conditionOperatorId = child.conditionOperatorId;
    groupCriteria[0].conditionOperatorName = child.conditionOperatorName;
    groupCriteria[0].openGroupCount = (groupCriteria[0].openGroupCount ?? 0) + 1;
    const lastCriterion = groupCriteria[groupCriteria.length - 1];
    lastCriterion.closeGroupCount = (lastCriterion.closeGroupCount ?? 0) + 1;
    flattened.push(...groupCriteria);
  }

  return flattened;
}

function createFlatFallback(
  criteria: ManualCheckCriterion[],
): ManualCheckGroupNode {
  const root = createManualCheckGroupNode(null, true);
  root.children = criteria.map((criterion, index) => ({
    kind: 'criterion',
    id: createNodeId('criterion'),
    conditionOperatorId:
      index === 0 ? null : criterion.conditionOperatorId,
    conditionOperatorName:
      index === 0 ? null : criterion.conditionOperatorName,
    nameContains: criterion.nameContains,
    valueContains: criterion.valueContains,
  }));
  return root;
}

function formatChildren(
  children: ManualCheckExpressionNode[],
  operatorNames: Map<number, string>,
): string {
  return children
    .map((child, index) => {
      const operand =
        child.kind === 'group'
          ? `(${formatChildren(child.children, operatorNames)})`
          : formatCriterion(child);
      if (index === 0) {
        return operand;
      }

      const operator =
        child.conditionOperatorId === null
          ? 'AND'
          : operatorNames.get(child.conditionOperatorId) ??
            child.conditionOperatorName ??
            'AND';
      return `${operator} ${operand}`;
    })
    .join(' ');
}

function formatCriterion(criterion: ManualCheckCriterionNode): string {
  const name = criterion.nameContains?.trim() || '…';
  const value = criterion.valueContains?.trim() || '…';
  return `[${name}: ${value}]`;
}

function isValidGroupCount(value: number): boolean {
  return Number.isInteger(value) && value >= 0 && value <= 25;
}

function createNodeId(prefix: string): string {
  nextNodeId += 1;
  return `${prefix}-${nextNodeId}`;
}
