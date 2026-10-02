import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { ManualCheckMatchesDialogComponent, ManualCheckMatchesDialogData } from './manual-check-matches-dialog.component';

describe('ManualCheckMatchesDialogComponent', () => {
  let fixture: ComponentFixture<ManualCheckMatchesDialogComponent>;

  const data: ManualCheckMatchesDialogData = {
    productName: 'Matched Item',
    match: {
      productId: 5,
      productName: 'Matched Item',
      gameUrlId: 2,
      gameUrlName: 'Market',
      fullUrl: '',
      tags: [],
      rating: null,
      matchedAssets: [1, 2, 3].map((index) => ({
        appId: '440',
        contextId: '2',
        assetId: `asset-${index}`,
        classId: `class-${index}`,
        instanceId: '0',
        marketName: `Match ${index}`,
        iconUrl: '',
        priceMinorUnits: index * 100,
        priceCurrencyCode: 'EUR',
        priceRangeMatched: null,
        descriptions: [{
          name: 'attribute',
          value: 'Mean Green',
          color: '',
          matchedCriterionIndexes: [0],
        }],
      })),
    },
    criteria: [{ conditionOperatorId: null, nameContains: 'attribute', valueContains: 'Mean Green' }],
    trace: {
      productId: 5,
      productName: 'Matched Item',
      fullUrl: '',
      matchEvaluated: true,
      matched: true,
      matchedAssetCount: 3,
      lowestCheckedPriceMinorUnits: 75,
      priceCurrencyCode: 'EUR',
      priceRangeMatched: null,
      checkedAssets: [{
        appId: '440',
        contextId: '2',
        assetId: 'checked-unmatched',
        classId: 'class-unmatched',
        instanceId: '0',
        marketName: 'Unmatched listing',
        iconUrl: '',
        priceMinorUnits: 75,
        priceCurrencyCode: 'EUR',
        priceRangeMatched: true,
        matched: false,
        descriptions: [{
          name: 'Exterior',
          value: 'Factory New',
          color: '',
          matchedCriterionIndexes: [],
        }],
      }],
      steamApiResultJson: null,
      durationMilliseconds: 1,
    },
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManualCheckMatchesDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(ManualCheckMatchesDialogComponent);
    fixture.detectChanges();
  });

  it('shows the cheapest checked price and every matched asset in the collapsible tree', () => {
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Matched Item');
    expect(text).toContain('Matched · 3 assets');
    expect(text).toContain('Cheapest checked: €0.75');
    const details = fixture.nativeElement.querySelectorAll('steam-manual-check-match-tree details') as NodeListOf<HTMLDetailsElement>;
    expect(details.length).toBe(3);
    expect([...details].every((match) => match.open)).toBeTrue();
  });

  it('shows matched and unmatched checked-listing details in the collapsed advanced section', () => {
    const advanced = fixture.nativeElement.querySelector('.matches-dialog__advanced') as HTMLDetailsElement;
    expect(advanced.open).toBeFalse();
    expect(advanced.textContent).toContain('Unmatched listing');
    expect(advanced.textContent).toContain('No match');
    expect(advanced.textContent).toContain('Exterior');
    expect(advanced.textContent).toContain('Factory New');
  });

  it('shows a historical-data fallback when checked listings were not stored', () => {
    fixture.destroy();
    const originalTrace = data.trace;
    data.trace = {
      ...originalTrace!,
      checkedAssets: undefined,
    };
    fixture = TestBed.createComponent(ManualCheckMatchesDialogComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('may predate advanced listing inspection');
    data.trace = originalTrace;
  });
});
