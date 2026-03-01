import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
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

import { WishList } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';
import { GameService, SteamService } from '../../../services';
import { Game, WhishListResponse } from '../../../models';

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
  ],
  templateUrl: './wish-lists-view.html',
  styleUrl: './wish-lists-view.scss',
})
export class WishListsView implements OnInit, OnDestroy {
  displayedColumns: string[] = [
    'gameName',
    'name',
    'pageUrl',
    'price',
    'isActive',
    'actions',
  ];

  dataSource = new MatTableDataSource<WishList>([]);
  private wishLists: WishList[] = [];

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  checkLabel: string | null = null;
  isChecking: boolean = false;

  private readonly destroy$ = new Subject<void>();
  private readonly cancelCheck$ = new Subject<void>();

  constructor(
    private readonly wishListService: WishListService,
    private readonly gameService: GameService,
    private readonly steamService: SteamService,
    private readonly router: Router,
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
    this.wishListService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((items) => {
        this.wishLists = items;

        this.dataSource.data = items;
        this.dataSource.paginator = this.paginator;
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
    this.checkLabel = '';
    // subscriptions re-apply filters automatically
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data.map((x) => ({
      Game: x.gameName ?? '',
      Name: x.name ?? '',
      PageUrl: x.pageUrl ?? '',
      Price: x.price ?? '',
      Active: x.isActive,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'WishList');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_WishList.xlsx`);
  }

  createButtonClicked(): void {
    this.router.navigate(['/wishlist/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/wishlist/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this wish list item?')) {
      return;
    }

    this.wishListService
      .delete(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.fetchWishLists();
      });
  }

  checkButtonClicked(whishListItemId: number): void {
    this.cancelCheck$.next();

    this.checkLabel = 'Checking...';
    this.isChecking = true;

    this.steamService
      .checkWishlistItem(whishListItemId)
      .pipe(
        takeUntil(this.cancelCheck$),
        finalize(() => {
          this.isChecking = false;
        }),
      )
      .subscribe({
        next: (response: WhishListResponse) => {
          if (response.isPriceReached === true) {
            this.checkLabel = `Price goal has been reached for Game ${response.gameName}! Current Price: ${
              response.currentPrice === 0 ? 'Free' : response.currentPrice
            }`;
          } else {
            this.checkLabel = `Price goal has not been reached for Game ${response.gameName}! Current Price: ${response.currentPrice}`;
          }
        },
        error: () => {
          this.checkLabel = 'Error Checking Wishlist Item';
        },
      });
  }

  cancelButtonClicked(): void {
    this.cancelCheck$.next();
    this.checkLabel = 'Operation Cancelled';
  }
}