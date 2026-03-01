import { Component, OnDestroy, OnInit, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Game, GameUrl } from '../../../models';
import { GameService, GameUrlService } from '../../../services';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { combineLatest, startWith, Subject, takeUntil } from 'rxjs';
import * as XLSX from 'xlsx';

@Component({
  selector: 'steam-game-urls-grid',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
  ],
  templateUrl: './game-urls-view.html',
  styleUrl: './game-urls-view.scss'
})
export class GameUrlsView implements OnInit, OnDestroy {

  displayedColumns: string[] = [
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

  readonly games = signal<readonly Game[]>([]);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', { nonNullable: true });

  dataSource = new MatTableDataSource<GameUrl>([]);
  private gameUrls: GameUrl[] = [];

  private readonly destroy$ = new Subject<void>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly gameUrlService: GameUrlService,
    private readonly gameService: GameService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void
  {
    this.loadGames();
    this.fetchGameUrls();

    combineLatest([
      this.gameIdControl.valueChanges.pipe(startWith(this.gameIdControl.value)),
      this.searchByNameFilterControl.valueChanges.pipe(startWith(this.searchByNameFilterControl.value)),
    ])
    .pipe(takeUntil(this.destroy$))
    .subscribe(([gameId, name]) =>
    {
      const nameFilter = (name ?? '').trim().toLowerCase();

      const filtered = this.gameUrls.filter(u =>
      {
        if (gameId !== null && u.gameId !== gameId) { return false; }
        if (nameFilter && !(u.name ?? '').toLowerCase().includes(nameFilter)) { return false; }
        return true;
      });

      this.dataSource.data = filtered;
    });
  }

  ngOnDestroy(): void
  {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onNameFilterChanged(filter: string): void
  {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: true });
  }

  exportButtonClicked(): void
  {
    const dataToExport = this.dataSource.data.map(x => ({
      gameName: x.gameName,
      name: x.name ?? '',
      partialUrl: x.partialUrl ?? '',
      isBatchUrl: x.isBatchUrl,
      startPage: x.startPage ?? '',
      endPage: x.endPage ?? '',
      isPixelScrape: x.isPixelScrape,
      isPublicApi: x.isPublicApi,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);
    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'GameUrls');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_GameUrls.xlsx`);
  }

  createButtonClicked(): void
  {
    this.router.navigate(['/game-urls/create']);
  }

  editButtonClicked(id: number): void
  {
    this.router.navigate(['/game-urls/edit', id]);
  }

  deleteButtonClicked(id: number): void
  {
    if (!confirm('Delete this Pixel?')) { return; }

    this.gameUrlService.delete(id).subscribe(() =>
    {
      this.fetchGameUrls();
    });
  }

  clearFiltersButtonClicked(): void
  {
    this.gameIdControl.setValue(null, { emitEvent: true });
    this.searchByNameFilterControl.setValue('', { emitEvent: true });
  }

  private fetchGameUrls(): void
  {
    this.gameUrlService.getAll().subscribe(urls =>
    {
      this.gameUrls = urls;
      this.dataSource.data = urls;

      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  private loadGames(): void
  {
    this.gameService.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(games =>
      {
        this.games.set(games);
      });
  }
}
