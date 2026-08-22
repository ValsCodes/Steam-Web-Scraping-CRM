import { ManualCheckCriterion } from '../../models';
import {
  createManualCheckCriterionNode,
  createManualCheckGroupNode,
  flattenManualCheckExpression,
  formatManualCheckExpression,
  parseManualCheckExpression,
  validateManualCheckExpression,
} from './manual-check-expression';
import { ManualCheckExpressionEditorComponent } from './manual-check-expression-editor.component';

describe('manual check expressions', () => {
  const operators = [
    { id: 1, name: 'AND' },
    { id: 2, name: 'OR' },
  ];

  it('round-trips nested groups through the flat API markers', () => {
    const criteria: ManualCheckCriterion[] = [
      criterion('A'),
      criterion('B', 1, 1),
      criterion('C', 2, 0, 1),
      criterion('D', 1, 1),
      criterion('E', 2, 1),
      criterion('Y', 1, 0, 2),
    ];

    const parsed = parseManualCheckExpression(criteria);

    expect(parsed.isValid).toBeTrue();
    expect(flattenManualCheckExpression(parsed.root)).toEqual(criteria);
    expect(formatManualCheckExpression(parsed.root, operators)).toBe(
      '[A: A] AND ([B: B] OR [C: C]) AND ([D: D] OR ([E: E] AND [Y: Y]))',
    );
  });

  it('falls back to a readable flat expression for malformed historical markers', () => {
    const parsed = parseManualCheckExpression([
      criterion('A', null, 0, 1),
      criterion('B', 1),
    ]);

    expect(parsed.isValid).toBeFalse();
    expect(parsed.root.children.length).toBe(2);
    expect(formatManualCheckExpression(parsed.root, operators)).toBe(
      '[A: A] AND [B: B]',
    );
  });

  it('rejects empty draft groups until they are populated', () => {
    const root = createManualCheckGroupNode(null, true);
    root.children.push(createManualCheckCriterionNode());
    root.children[0].kind === 'criterion' && (root.children[0].valueContains = 'A');
    root.children.push(createManualCheckGroupNode(1));

    const validation = validateManualCheckExpression(root, new Set([1, 2]));

    expect(validation.isValid).toBeFalse();
    expect(validation.criterionCount).toBe(1);
    expect(validation.groupCount).toBe(1);
  });

  it('ungroups in place and transfers the group operator to its first child', () => {
    const root = createManualCheckGroupNode(null, true);
    const first = createManualCheckCriterionNode();
    first.valueContains = 'A';
    const group = createManualCheckGroupNode(2);
    const childB = createManualCheckCriterionNode();
    childB.valueContains = 'B';
    const childC = createManualCheckCriterionNode(1);
    childC.valueContains = 'C';
    group.children.push(childB, childC);
    root.children.push(first, group);
    const editor = new ManualCheckExpressionEditorComponent();
    editor.root = root;
    editor.operators = operators;

    editor.ungroup(group);

    expect(root.children.map((node) => node.kind === 'criterion' ? node.valueContains : 'group'))
      .toEqual(['A', 'B', 'C']);
    expect(root.children[1].conditionOperatorId).toBe(2);
    expect(root.children[2].conditionOperatorId).toBe(1);
  });

  it('normalizes Start and AND after keyboard reordering', () => {
    const root = createManualCheckGroupNode(null, true);
    const first = createManualCheckCriterionNode();
    first.valueContains = 'A';
    const second = createManualCheckCriterionNode(2);
    second.valueContains = 'B';
    root.children.push(first, second);
    const editor = new ManualCheckExpressionEditorComponent();
    editor.root = root;
    editor.operators = operators;

    editor.moveWithin(root, 1, -1);

    expect(root.children[0].conditionOperatorId).toBeNull();
    expect(root.children[1].conditionOperatorId).toBe(1);
  });

  it('transfers a dragged criterion between groups and normalizes both groups', () => {
    const root = createManualCheckGroupNode(null, true);
    const source = createManualCheckGroupNode();
    const target = createManualCheckGroupNode(1);
    const a = createManualCheckCriterionNode();
    a.valueContains = 'A';
    const b = createManualCheckCriterionNode();
    b.valueContains = 'B';
    source.children.push(a);
    target.children.push(b);
    root.children.push(source, target);
    const editor = new ManualCheckExpressionEditorComponent();
    editor.root = root;
    editor.operators = operators;

    editor.dropNode({
      previousContainer: { data: source },
      container: { data: target },
      previousIndex: 0,
      currentIndex: 1,
    } as never);

    expect(source.children).toEqual([]);
    expect(target.children.map((node) => node.kind === 'criterion' ? node.valueContains : 'group'))
      .toEqual(['B', 'A']);
    expect(target.children[0].conditionOperatorId).toBeNull();
    expect(target.children[1].conditionOperatorId).toBe(1);
  });

  it('rejects dropping a group into one of its descendants', () => {
    const outer = createManualCheckGroupNode();
    const inner = createManualCheckGroupNode();
    outer.children.push(inner);
    const editor = new ManualCheckExpressionEditorComponent();

    const allowed = editor.canEnterGroup(
      { data: outer } as never,
      { data: inner } as never,
    );

    expect(allowed).toBeFalse();
  });

  function criterion(
    term: string,
    operatorId: number | null = null,
    openGroupCount = 0,
    closeGroupCount = 0,
  ): ManualCheckCriterion {
    return {
      conditionOperatorId: operatorId,
      conditionOperatorName: operatorId === 1 ? 'AND' : operatorId === 2 ? 'OR' : null,
      openGroupCount,
      closeGroupCount,
      nameContains: term,
      valueContains: term,
    };
  }
});
