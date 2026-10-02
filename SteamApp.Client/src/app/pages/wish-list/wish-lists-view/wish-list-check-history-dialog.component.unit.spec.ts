import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { WishListCheckHistoryPage } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';
import { WishListCheckHistoryDialogComponent } from './wish-list-check-history-dialog.component';

describe('WishListCheckHistoryDialogComponent', () => {
  async function setup(response: WishListCheckHistoryPage | Error) {
    const wishListService = jasmine.createSpyObj<WishListService>('WishListService', ['getCheckHistory']);
    wishListService.getCheckHistory.and.returnValue(
      response instanceof Error ? throwError(() => response) : of(response),
    );
    const dialogRef = jasmine.createSpyObj<MatDialogRef<WishListCheckHistoryDialogComponent>>(
      'MatDialogRef',
      ['close'],
    );

    await TestBed.configureTestingModule({
      imports: [WishListCheckHistoryDialogComponent],
      providers: [
        provideNoopAnimations(),
        { provide: WishListService, useValue: wishListService },
        { provide: MatDialogRef, useValue: dialogRef },
        {
          provide: MAT_DIALOG_DATA,
          useValue: { wishListId: 7, alertName: 'Portal under 5', gameName: 'Portal' },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(WishListCheckHistoryDialogComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    return { component: fixture.componentInstance, fixture, wishListService };
  }

  it('renders successful and failed traces with prices and redacted errors', async () => {
    const page = createPage();
    page.items.push({
      ...page.items[0],
      id: 2,
      status: 'Failed',
      currentPrice: null,
      isPriceReached: null,
      errorCode: 'WishlistCheck.PriceUnavailable',
      errorText: 'Steam did not provide a readable price for this game.',
    });

    const { fixture } = await setup(page);
    const text = fixture.nativeElement.textContent ?? '';

    expect(text).toContain('Price Check History');
    expect(text).toContain('Portal under 5');
    expect(text).toContain('Target reached');
    expect(text).toContain('Failed');
    expect(text).toContain('WishlistCheck.PriceUnavailable');
    expect(text).toContain('Steam did not provide a readable price for this game.');
    expect(text).toContain('trace-1');
  });

  it('renders empty and error states', async () => {
    const empty = await setup({ ...createPage(), items: [], totalCount: 0 });
    expect(empty.fixture.nativeElement.textContent).toContain(
      'No checks have been recorded for this price alert yet.',
    );

    TestBed.resetTestingModule();
    const failed = await setup(new Error('network'));
    expect(failed.fixture.nativeElement.textContent).toContain(
      'Unable to load price-check history. Please try again.',
    );
  });

  it('requests a new server page and formats boundary values', async () => {
    const { component, wishListService } = await setup(createPage());

    component.pageChanged({ pageIndex: 1, pageSize: 50, length: 60 });

    expect(wishListService.getCheckHistory).toHaveBeenCalledWith(7, 2, 50);
    expect(component.formatPrice(0)).toBe('Free');
    expect(component.formatPrice(null)).toBe('Unavailable');
    expect(component.formatDuration(null)).toBe('Unavailable');
  });
});

function createPage(): WishListCheckHistoryPage {
  return {
    items: [{
      id: 1,
      wishListId: 7,
      gameName: 'Portal',
      source: 'Manual',
      status: 'Succeeded',
      targetPrice: 5,
      currentPrice: 4,
      isPriceReached: true,
      requestedAtUtc: '2026-10-02T12:00:00Z',
      startedAtUtc: '2026-10-02T12:00:00Z',
      completedAtUtc: '2026-10-02T12:00:01Z',
      durationMilliseconds: 1000,
      correlationId: 'trace-1',
      errorCode: null,
      errorText: null,
    }],
    pageNumber: 1,
    pageSize: 25,
    totalCount: 1,
    totalPages: 1,
  };
}
