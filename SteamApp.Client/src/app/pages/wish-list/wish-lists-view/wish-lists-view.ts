import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  BehaviorSubject,
  combineLatest,
  finalize,
  startWith,
  Subject,
  takeUntil,
} from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';

import { UpdateWishListStatus, WishList } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';
import { GameService, SteamService } from '../../../services';
import { Game, WhishListResponse } from '../../../models';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';
import { StatusDialogComponent } from '../../../components/status-dialog.component';
import { safeExternalUrl } from '../../../common';

import * as XLSX from 'xlsx';

@Component({
  selector: 'steam-wish-lists-view',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
  ],
  templateUrl: './wish-lists-view.html',
  styleUrl: './wish-lists-view.scss',
})
export class WishListsView implements OnInit, OnDestroy {
  readonly safeExternalUrl = safeExternalUrl;
  readonly priceAlertSteps = [
    'Choose a game',
    'Set a target price',
    'Check the current Steam price',
  ];

  displayedColumns: string[] = [
    'gameName',
    'name',
    'price',
    'isActive',
    'actions',
  ];

  dataSource = new MatTableDataSource<WishList>([]);
  isGridLoading = false;
  pageSize = 25;
  readonly pageSizeOptions = [10, 25, 50, 100];
  private wishLists: WishList[] = [];

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  isChecking: boolean = false;
  checkingId: number | null = null;
  private readonly deletingIds = new Set<number>();
  private readonly statusUpdatingIds = new Set<number>();

  private readonly destroy$ = new Subject<void>();
  private readonly cancelCheck$ = new Subject<void>();
  private readonly euroPriceFormatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'EUR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  constructor(
    private readonly wishListService: WishListService,
    private readonly gameService: GameService,
    private readonly steamService: SteamService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.fetchWishLists();

    // Apply filters whenever either control changes
    combineLatest([
      this.gameIdControl.valueChanges.pipe(startWith(this.gameIdControl.value)),
      this.searchByNameFilterControl.valueChanges.pipe(
        startWith(this.searchByNameFilterControl.value),
      ),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([gameId, name]) => {
        this.applyFilters(gameId, name);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    this.cancelCheck$.next();
    this.cancelCheck$.complete();
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games$.next(games);
      });
  }

  fetchWishLists(): void {
    this.isGridLoading = true;
    this.cdr.markForCheck();

    this.wishListService
      .getAll()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isGridLoading = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe((items) => {
        this.wishLists = items;

        this.dataSource.data = items;
        this.dataSource.paginator = this.paginator;
        this.paginator.pageSize = this.pageSize;
        this.dataSource.sort = this.sort;

        // ensure current filter state is applied after load
        this.applyFilters(
          this.gameIdControl.value,
          this.searchByNameFilterControl.value,
        );
      });
  }

  private applyFilters(gameId: number | null, name: string): void {
    const nameFilter = (name ?? '').trim().toLowerCase();

    const filtered = this.wishLists.filter((x) => {
      if (gameId !== null && x.gameId !== gameId) {
        return false;
      }
      if (nameFilter && !(x.name ?? '').toLowerCase().includes(nameFilter)) {
        return false;
      }
      return true;
    });

    this.dataSource.data = filtered;
  }

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.searchByNameFilterControl.setValue('');
    // subscriptions re-apply filters automatically
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data.map((x) => ({
      Game: x.gameName ?? '',
      AlertName: x.name ?? '',
      PageUrl: x.pageUrl ?? '',
      TargetPrice: x.price ?? '',
      Monitoring: x.isActive ? 'Enabled' : 'Paused',
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'WishList');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_PriceAlerts.xlsx`);
  }

  refreshButtonClicked(): void {
    this.fetchWishLists();
  }

  pageSizeChanged(value: string | number): void {
    const pageSize = Number(value);
    if (Number.isNaN(pageSize) || pageSize <= 0 || this.pageSize === pageSize) {
      return;
    }

    this.pageSize = pageSize;

    if (this.paginator) {
      this.paginator.pageSize = pageSize;
      this.paginator.firstPage();
      this.dataSource.data = [...this.dataSource.data];
      this.cdr.markForCheck();
    }
  }

  createButtonClicked(): void {
    this.router.navigate(['/wishlist/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/wishlist/edit', id]);
  }

  activeButtonClicked(item: WishList): void {
    if (this.isStatusUpdating(item.id)) {
      return;
    }

    const nextIsActive = !item.isActive;

    const input: UpdateWishListStatus = {
      id: item.id,
      isActive: nextIsActive,
    };

    this.statusUpdatingIds.add(item.id);

    this.wishListService
      .updateStatus(input)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.statusUpdatingIds.delete(item.id);
          this.cdr.markForCheck();
        }),
      )
      .subscribe(() => {
        this.wishLists = this.wishLists.map((x) =>
          x.id === item.id ? { ...x, isActive: nextIsActive } : x,
        );

        this.applyFilters(this.gameIdControl.value, this.searchByNameFilterControl.value);
      });
  }

  deleteButtonClicked(id: number): void {
    if (this.isDeleting(id)) {
      return;
    }

    this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: {
          title: 'Delete Price Alert',
          subtitle: 'This action cannot be undone.',
          message: 'Are you sure you want to delete this price alert?',
          confirmText: 'Delete',
          cancelText: 'Cancel',
        },
      })
      .afterClosed()
      .subscribe((confirmed: boolean) => {
        if (!confirmed) {
          return;
        }

        this.deletingIds.add(id);
        this.cdr.markForCheck();

        this.wishListService
          .delete(id)
          .pipe(
            takeUntil(this.destroy$),
            finalize(() => {
              this.deletingIds.delete(id);
              this.cdr.markForCheck();
            }),
          )
          .subscribe({
            next: () => {
              this.fetchWishLists();
            },
          });
      });
  }

  checkButtonClicked(whishListItemId: number): void {
    this.cancelCheck$.next();

    this.isChecking = true;
    this.checkingId = whishListItemId;

    this.steamService
      .checkWishlistItem(whishListItemId)
      .pipe(
        takeUntil(this.cancelCheck$),
        finalize(() => {
          this.isChecking = false;
          if (this.checkingId === whishListItemId) {
            this.checkingId = null;
          }

          this.cdr.markForCheck();
        }),
      )
      .subscribe({
        next: (response: WhishListResponse) => {
          if (response.isPriceReached === true) {
            this.openStatusDialog({
              title: 'Target Price Reached',
              subtitle: `${response.gameName} is at or below your target price.`,
              message: `Current Steam price: ${this.formatCurrentPrice(response.currentPrice)}`,
              variant: 'success',
            });
          } else {
            this.openStatusDialog({
              title: 'Target Price Not Reached',
              subtitle: `${response.gameName} is still above your target price.`,
              message: `Current Steam price: ${this.formatCurrentPrice(response.currentPrice)}`,
              variant: 'info',
            });
          }
        },
        error: () => {
          this.openStatusDialog({
            title: 'Price Check Failed',
            subtitle: 'Steam may be unavailable or the page may not expose a readable price.',
            message: 'Please try again in a moment.',
            variant: 'error',
          });
        },
      });
  }

  cancelButtonClicked(): void {
    this.cancelCheck$.next();
    this.openStatusDialog({
      title: 'Check Cancelled',
      subtitle: 'The price check was stopped before completion.',
      message: 'No changes were made.',
      variant: 'warn',
    });
  }

  isDeleting(id: number): boolean {
    return this.deletingIds.has(id);
  }

  isStatusUpdating(id: number): boolean {
    return this.statusUpdatingIds.has(id);
  }

  get showNoItemsEmptyState(): boolean {
    return !this.isGridLoading && this.wishLists.length === 0;
  }

  get showFilteredEmptyState(): boolean {
    return !this.isGridLoading &&
      this.wishLists.length > 0 &&
      this.dataSource.data.length === 0;
  }

  private formatCurrentPrice(price: number): string {
    return price === 0 ? 'Free' : this.euroPriceFormatter.format(price);
  }

  private openStatusDialog(data: {
    title: string;
    subtitle: string;
    message: string;
    variant: 'success' | 'info' | 'warn' | 'error';
  }): void {
    this.dialog.open(StatusDialogComponent, {
      width: '420px',
      data,
    });
  }
}
