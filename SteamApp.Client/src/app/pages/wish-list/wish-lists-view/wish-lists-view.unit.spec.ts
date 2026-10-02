import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { from, of, throwError } from 'rxjs';

import { WishListsView } from './wish-lists-view';
import { WishList } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';
import { GameService } from '../../../services';
import { StatusDialogComponent } from '../../../components/status-dialog.component';
import { WishListCheckHistoryDialogComponent } from './wish-list-check-history-dialog.component';

describe('WishListsView', () => {
  async function setup(items: WishList[] = []) {
    const wishListService = jasmine.createSpyObj<WishListService>('WishListService', [
      'delete',
      'check',
      'getAll',
      'getCheckHistory',
      'updateStatus',
    ]);
    const gameService = jasmine.createSpyObj<GameService>('GameService', ['getAll']);
    const router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    const dialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    wishListService.getAll.and.returnValue(from(Promise.resolve(items)));
    gameService.getAll.and.returnValue(of([]));
    wishListService.check.and.returnValue(of(createCheckTrace()));
    dialog.open.and.returnValue({ afterClosed: () => of(false) } as never);

    await TestBed.configureTestingModule({
      imports: [WishListsView],
      providers: [
        provideNoopAnimations(),
        { provide: WishListService, useValue: wishListService },
        { provide: GameService, useValue: gameService },
        { provide: Router, useValue: router },
        { provide: MatDialog, useValue: dialog },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(WishListsView);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    return { component, dialog, fixture, wishListService };
  }

  it('renders the Price Alerts intro and clarified table headings', async () => {
    const { fixture } = await setup([createWishList()]);
    const text = textContent(fixture);

    expect(text).toContain('Price Alerts');
    expect(text).toContain('Track Steam game prices against your target thresholds.');
    expect(text).toContain('Choose a game');
    expect(text).toContain('Set a target price');
    expect(text).toContain('Check the current Steam price');
    expect(text).toContain('Alert Name');
    expect(text).toContain('Target Price');
    expect(text).toContain('Monitoring');
    expect(text).toContain('Enabled');
  });

  it('shows empty state copy when there are no price alerts', async () => {
    const { fixture } = await setup([]);

    expect(textContent(fixture)).toContain(
      'No price alerts yet. Create one to track a Steam game against a target price.',
    );
  });

  it('shows filtered empty state copy when filters hide all alerts', async () => {
    const { component, fixture } = await setup([createWishList()]);

    component.searchByNameFilterControl.setValue('missing alert');
    fixture.detectChanges();

    expect(textContent(fixture)).toContain(
      'No price alerts match the current filters. Clear filters or create a new alert.',
    );
  });

  it('opens clarified success, info, and error dialogs for price checks', async () => {
    const { component, dialog, wishListService } = await setup([createWishList()]);

    wishListService.check.and.returnValue(of(createCheckTrace({
      isPriceReached: true,
      currentPrice: 0,
    })));
    component.checkButtonClicked(1);

    expect(dialog.open).toHaveBeenCalledWith(StatusDialogComponent, jasmine.objectContaining({
      data: jasmine.objectContaining({
        title: 'Target Price Reached',
        message: 'Current Steam price: Free',
        variant: 'success',
      }),
    }));

    wishListService.check.and.returnValue(of(createCheckTrace({
      isPriceReached: false,
      currentPrice: 12.5,
    })));
    component.checkButtonClicked(1);

    expect(dialog.open).toHaveBeenCalledWith(StatusDialogComponent, jasmine.objectContaining({
      data: jasmine.objectContaining({
        title: 'Target Price Not Reached',
        message: jasmine.stringMatching(/^Current Steam price: .*12\.50$/),
        variant: 'info',
      }),
    }));

    wishListService.check.and.returnValue(throwError(() => new Error('Steam failed')));
    component.checkButtonClicked(1);

    expect(dialog.open).toHaveBeenCalledWith(StatusDialogComponent, jasmine.objectContaining({
      data: jasmine.objectContaining({
        title: 'Price Check Failed',
        subtitle: 'Steam may be unavailable or the page may not expose a readable price.',
        variant: 'error',
      }),
    }));
  });

  it('opens check history for the selected price alert', async () => {
    const item = createWishList();
    const { component, dialog } = await setup([item]);

    component.viewCheckHistoryButtonClicked(item);

    expect(dialog.open).toHaveBeenCalledWith(
      WishListCheckHistoryDialogComponent,
      jasmine.objectContaining({
        data: {
          wishListId: item.id,
          alertName: item.name,
          gameName: item.gameName,
        },
      }),
    );
  });
});

function createWishList(): WishList {
  return {
    id: 1,
    gameId: 10,
    gameName: 'Portal',
    name: 'Portal under 5 EUR',
    pageUrl: 'https://store.steampowered.com/app/400/Portal/',
    price: 4.99,
    isActive: true,
  };
}

function createCheckTrace(
  overrides: Partial<ReturnType<typeof createCheckTraceBase>> = {},
) {
  return { ...createCheckTraceBase(), ...overrides };
}

function createCheckTraceBase() {
  return {
    id: 1,
    wishListId: 1,
    gameName: 'Portal',
    source: 'Manual' as const,
    status: 'Succeeded' as const,
    targetPrice: 4.99,
    currentPrice: 4.99,
    isPriceReached: true,
    requestedAtUtc: '2026-10-02T12:00:00Z',
    startedAtUtc: '2026-10-02T12:00:00Z',
    completedAtUtc: '2026-10-02T12:00:01Z',
    durationMilliseconds: 1000,
    correlationId: 'trace-1',
    errorCode: null,
    errorText: null,
  };
}

function textContent(fixture: ComponentFixture<WishListsView>): string {
  return fixture.nativeElement.textContent ?? '';
}
