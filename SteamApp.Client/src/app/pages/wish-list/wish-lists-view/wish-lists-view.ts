import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, finalize, startWith, tap } from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { WishList } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';
import { GameService, SteamService } from '../../../services';
import { ComboBoxComponent } from '../../../components/filter-components/combo-box-filter.component';
import { TextFilterComponent } from '../../../components';
import { Game, WhishListResponse } from '../../../models';

@Component({
  selector: 'steam-wish-lists-view',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    ComboBoxComponent,
    TextFilterComponent,
  ],
  templateUrl: './wish-lists-view.html',
  styleUrl: './wish-lists-view.scss',
})
export class WishListsView implements OnInit {
  displayedColumns: string[] = [
    'gameName',
    'name',
    'pageUrl',
    'price',
    'isActive',
    'actions',
  ];

  dataSource = new MatTableDataSource<WishList>([]);
  wishLists: WishList[] = [];
  filteredWishLists: WishList[] = [];

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  private games: Game[] = [];
  readonly games$ = new BehaviorSubject<readonly Game[]>([]);

  readonly bindGameToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap(() => this.loadFilteredWishLists()),
  );

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  checkLabel: string | null = null;
  isChecking: boolean = false;

  constructor(
    private readonly wishListService: WishListService,
    private readonly gameService: GameService,
    private readonly steamService: SteamService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.fetchWishLists();
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe((games) => {
      this.games = games;
      this.games$.next(games);
    });
  }

  fetchWishLists(): void {
    this.wishListService.getAll().subscribe((items) => {
      this.wishLists = items;
      this.filteredWishLists = items;

      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredWishLists();
  }

  private loadFilteredWishLists(): void {
    const gameId = this.gameIdControl.value;
    const nameFilter = this.searchByNameFilterControl.value.toLowerCase();

    this.filteredWishLists = this.wishLists.filter((x) => {
      const matchesGame = gameId === null || x.gameId === gameId;

      const matchesName =
        !nameFilter || x.name?.toLowerCase().includes(nameFilter);

      return matchesGame && matchesName;
    });

    this.dataSource.data = this.filteredWishLists;
  }

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.checkLabel = '';
    this.loadFilteredWishLists();
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

    this.wishListService.delete(id).subscribe(() => {
      this.fetchWishLists();
    });
  }

  checkButtonClicked(whishListItemId: number) {
    this.checkLabel = 'Checking...';
    this.isChecking = true;

    this.steamService
      .checkWishlistItem(whishListItemId)
      .pipe(
        finalize(() => {
          this.isChecking = false;
        }),
      )
      .subscribe({
        next: (response: WhishListResponse) => {
          this.checkLabel = `Price goal has been reached for Game ${response.gameName}! Current Price: ${response.currentPrice === 0 ? 'Free' : response.currentPrice}`;
        },
        error: (err: any) => {
          this.checkLabel = 'Error Checking Wishlist Item';
        },
      });
  }
}
