import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Game, GameUrl } from '../../../models';
import { GameService, GameUrlService } from '../../../services';
import { ComboBoxComponent } from "../../../components/filter-components/combo-box-filter.component";
import { TextFilterComponent } from "../../../components";
import { FormControl } from '@angular/forms';
import { BehaviorSubject, startWith, Subject, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'steam-game-urls-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    ComboBoxComponent,
    TextFilterComponent
],
  templateUrl: './game-urls-view.html',
  styleUrl: './game-urls-view.scss'
})
export class GameUrlsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    //'gameId',
    'gameName',
    'name',
    'partialUrl',
    'isBatchUrl',
    'startPage',
    'endPage',
    'isPixelScrape',
    'isPublicApi',
    'actions'
  ];

  private games: Game[] = [];
  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  gameNameById = new Map<number, string>();

  private readonly destroy$ = new Subject<void>();
  
  readonly gameIdControl = new FormControl<number | null>(null);

    searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  dataSource = new MatTableDataSource<GameUrl>([]);
  gameUrls: GameUrl[] = [];
   gameUrlsFiltered: GameUrl[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private gameUrlService: GameUrlService,
    private gameService: GameService,
    private router: Router,
     private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();

    this.fetchGameUrls();
  }

  fetchGameUrls(): void {
    this.gameUrlService.getAll().subscribe(urls => {
      this.gameUrls = urls;
      this.gameUrlsFiltered = urls;
      this.dataSource.data = urls;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  
  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredGameUrls();
  }

  createButtonClicked(): void {
    this.router.navigate(['/game-urls/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/game-urls/edit', id]);
  }


  private loadFilteredGameUrls(): void {
    const nameFilter =
    this.searchByNameFilterControl.value?.toLowerCase() ?? '';

    this.gameUrlsFiltered = this.gameUrls.filter((gameUrl) => {
      const matchesName =
        !nameFilter || gameUrl.name?.toLowerCase().includes(nameFilter);


      return matchesName;
    });

    this.dataSource.data = this.gameUrlsFiltered;
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games = games;
        this.games$.next(games);
        this.cdr.markForCheck();
      });
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this Pixel?')) {
      return;
    }

    this.gameUrlService.delete(id).subscribe(() => {
      this.fetchGameUrls();
    });
  }

    readonly bindToExternal$ = this.gameIdControl.valueChanges.pipe(
      startWith(this.gameIdControl.value),
      tap((gameId) => {
        this.applyGameFilter(gameId);
      }),
    );

  private applyGameFilter(gameId: number | null): void {

    this.gameUrlsFiltered = this.gameUrls.filter(x=> x.gameId === gameId);
    this.dataSource.data=this.gameUrlsFiltered ;
    
    this.cdr.markForCheck();
  }


    clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });

    this.gameIdControl.setValue(null);

    this.searchByNameFilterControl.setValue('', { emitEvent: false });

    const gameId = this.gameIdControl.value;

    this.loadFilteredGameUrls();
  }
}
