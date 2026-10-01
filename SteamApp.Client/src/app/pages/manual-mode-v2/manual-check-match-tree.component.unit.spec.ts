import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManualCheckAssetMatch, ManualCheckCriterion } from '../../models';
import { ManualCheckMatchTreeComponent } from './manual-check-match-tree.component';

describe('ManualCheckMatchTreeComponent', () => {
  let fixture: ComponentFixture<ManualCheckMatchTreeComponent>;

  const criteria: ManualCheckCriterion[] = [
    { conditionOperatorId: null, nameContains: 'attribute', valueContains: 'Mean Green' },
    { conditionOperatorId: 1, conditionOperatorName: 'AND', nameContains: 'quality', valueContains: 'Strange' },
  ];

  const asset = (assetId: string, priceMinorUnits: number | null, criterionIndexes: number[]): ManualCheckAssetMatch => ({
    appId: '440',
    contextId: '2',
    assetId,
    classId: `class-${assetId}`,
    instanceId: '0',
    marketName: `Asset ${assetId}`,
    iconUrl: '',
    priceMinorUnits,
    priceCurrencyCode: 'EUR',
    priceRangeMatched: priceMinorUnits === 175,
    descriptions: [{
      name: 'attribute',
      value: 'Mean Green Strange',
      color: '7ea9d1',
      matchedCriterionIndexes: criterionIndexes,
    }],
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManualCheckMatchTreeComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(ManualCheckMatchTreeComponent);
  });

  it('shows two collapsible matches with their prices and matched criteria', () => {
    fixture.componentRef.setInput('assets', [
      asset('one', 225, [0, 1]),
      asset('two', 175, [0]),
      asset('three', 300, [1]),
    ]);
    fixture.componentRef.setInput('criteria', criteria);
    fixture.componentRef.setInput('maxMatches', 2);
    fixture.componentRef.setInput('expandedByDefault', 'first');
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    const details = fixture.nativeElement.querySelectorAll('details') as NodeListOf<HTMLDetailsElement>;
    expect(details.length).toBe(2);
    expect(details[0].open).toBeTrue();
    expect(details[1].open).toBeFalse();
    expect(text).toContain('Match 1');
    expect(text).toContain('€2.25');
    expect(text).toContain('Criterion 1: name contains “attribute” and value contains “Mean Green”');
    expect(text).toContain('Criterion 2: name contains “quality” and value contains “Strange”');
    expect(text).toContain('+ 1 more match');
    expect(fixture.nativeElement.querySelector('.match-tree__price--failed')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.match-tree__price--matched')).not.toBeNull();
  });

  it('keeps legacy matched descriptions readable when criterion indexes and price are missing', () => {
    fixture.componentRef.setInput('assets', [asset('legacy', null, [])]);
    fixture.componentRef.setInput('criteria', criteria);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Price unavailable');
    expect(text).toContain('Matched description 1');
    expect(text).toContain('attribute: Mean Green Strange');
  });
});
