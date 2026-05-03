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
import { Game, GameUrl } from '../../../models';
import { GameService, GameUrlService } from '../../../services';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { combineLatest, finalize, startWith, Subject, takeUntil } from 'rxjs';
import * as XLSX from 'xlsx';
import { HttpErrorResponse } from '@angular/common/http';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';
import { MatDialog } from '@angular/material/dialog';

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
  private readonly dialog = inject(MatDialog);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', { nonNullable: true });

  dataSource = new MatTableDataSource<GameUrl>([]);
  private gameUrls: GameUrl[] = [];
  private readonly deletingIds = new Set<number>();

  private readonly destroy$ = new Subject<void>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly gameUrlService: GameUrlService,
    private readonly gameService: GameService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
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
