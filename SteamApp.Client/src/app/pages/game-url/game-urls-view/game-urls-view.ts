import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Game, GameUrl, ScrapingMode, UpdateGameUrlStatus } from '../../../models';
import { GameService, GameUrlService, ScrapingModeService } from '../../../services';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { combineLatest, finalize, startWith, Subject, takeUntil } from 'rxjs';
import * as XLSX from 'xlsx';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { ExternalLinkDirective, openableExternalUrl } from '../../../common';

@Component({
  selector: 'steam-game-urls-grid',
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
    ExternalLinkDirective,
  ],
  templateUrl: './game-urls-view.html',
  styleUrl: './game-urls-view.scss'
})
export class GameUrlsView implements OnInit, OnDestroy {
  readonly openableExternalUrl = openableExternalUrl;

  displayedColumns: string[] = [
    'gameName',
    'name',
    'partialUrl',
    'scrapingModeName',
    'startPage',
    'endPage',
    'isActive',
    'actions'
  ];

  readonly games = signal<readonly Game[]>([]);
  readonly scrapingModes = signal<readonly ScrapingMode[]>([]);
  private readonly dialog = inject(MatDialog);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly scrapingModeIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', { nonNullable: true });

  dataSource = new MatTableDataSource<GameUrl>([]);
  isGridLoading = false;
  pageSize = 25;
  readonly pageSizeOptions = [10, 25, 50, 100];
  private gameUrls: GameUrl[] = [];
  private readonly deletingIds = new Set<number>();
  private readonly statusUpdatingIds = new Set<number>();

  private readonly destroy$ = new Subject<void>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly gameUrlService: GameUrlService,
    private readonly gameService: GameService,
    private readonly scrapingModeService: ScrapingModeService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void
  {
    this.loadGames();
    this.loadScrapingModes();
    this.fetchGameUrls();

    combineLatest([
      this.gameIdControl.valueChanges.pipe(startWith(this.gameIdControl.value)),
      this.scrapingModeIdControl.valueChanges.pipe(startWith(this.scrapingModeIdControl.value)),
      this.searchByNameFilterControl.valueChanges.pipe(startWith(this.searchByNameFilterControl.value)),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([gameId, scrapingModeId, name]) => {
        this.applyFilters(gameId, scrapingModeId, name);
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
      scrapingMode: x.scrapingModeName ?? '',
      startPage: x.startPage ?? '',
      endPage: x.endPage ?? '',
      isActive: x.isActive,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);
    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'GameUrls');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_GameUrls.xlsx`);
  }

  refreshButtonClicked(): void
  {
    this.fetchGameUrls();
  }

  createButtonClicked(): void
  {
    this.router.navigate(['/game-urls/create']);
  }

  editButtonClicked(id: number): void
  {
    this.router.navigate(['/game-urls/edit', id]);
  }

  activeButtonClicked(gameUrl: GameUrl): void {
    if (this.isStatusUpdating(gameUrl.id)) {
      return;
    }

    const nextIsActive = !gameUrl.isActive;

    const input: UpdateGameUrlStatus = {
      id: gameUrl.id,
      isActive: nextIsActive,
    };

    this.statusUpdatingIds.add(gameUrl.id);

    this.gameUrlService
      .updateStatus(input)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.statusUpdatingIds.delete(gameUrl.id);
          this.cdr.markForCheck();
        }),
      )
      .subscribe(() => {
        this.gameUrls = this.gameUrls.map((x) =>
          x.id === gameUrl.id ? { ...x, isActive: nextIsActive } : x,
        );

        this.applyFilters(
          this.gameIdControl.value,
          this.scrapingModeIdControl.value,
          this.searchByNameFilterControl.value,
        );
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
        title: 'Delete Game URL',
        subtitle: 'This action cannot be undone.',
        message: 'Are you sure you want to delete this Game URL?',
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

      this.gameUrlService
        .delete(id)
        .pipe(
          finalize(() => {
            this.deletingIds.delete(id);
            this.cdr.markForCheck();
          }),
        )
        .subscribe({
          next: () => {
            this.fetchGameUrls();
          },
        });
    });
}

  isDeleting(id: number): boolean
  {
    return this.deletingIds.has(id);
  }

  isStatusUpdating(id: number): boolean
  {
    return this.statusUpdatingIds.has(id);
  }

  clearFiltersButtonClicked(): void
  {
    this.gameIdControl.setValue(null, { emitEvent: true });
    this.scrapingModeIdControl.setValue(null, { emitEvent: true });
    this.searchByNameFilterControl.setValue('', { emitEvent: true });
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

  private fetchGameUrls(): void
  {
    this.isGridLoading = true;
    this.cdr.markForCheck();

    this.gameUrlService.getAll()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isGridLoading = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe(urls =>
      {
        this.gameUrls = urls;

        this.dataSource.paginator = this.paginator;
        this.paginator.pageSize = this.pageSize;
        this.dataSource.sort = this.sort;

        this.applyFilters(
          this.gameIdControl.value,
          this.scrapingModeIdControl.value,
          this.searchByNameFilterControl.value,
        );
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

  private loadScrapingModes(): void
  {
    this.scrapingModeService.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(scrapingModes =>
      {
        this.scrapingModes.set([...scrapingModes].sort((a, b) => a.id - b.id));
      });
  }

  private applyFilters(
    gameId: number | null,
    scrapingModeId: number | null,
    name: string,
  ): void {
    const nameFilter = (name ?? '').trim().toLowerCase();

    const filtered = this.gameUrls.filter((u) => {
      if (gameId !== null && u.gameId !== gameId) { return false; }
      if (scrapingModeId !== null && u.scrapingModeId !== scrapingModeId) { return false; }
      if (nameFilter && !(u.name ?? '').toLowerCase().includes(nameFilter)) { return false; }
      return true;
    });

    this.dataSource.data = filtered;
    this.cdr.markForCheck();
  }
}
