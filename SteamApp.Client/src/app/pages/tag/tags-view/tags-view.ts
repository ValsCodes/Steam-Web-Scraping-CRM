import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';

import { GameService, TagService } from '../../../services';
import { Game, Tag } from '../../../models';
import { BehaviorSubject, combineLatest, finalize, startWith, Subject, takeUntil } from 'rxjs';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';

import * as XLSX from 'xlsx';

@Component({
  selector: 'steam-tags-grid',
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
  templateUrl: './tags-view.html',
  styleUrl: './tags-view.scss',
})
export class TagsView implements OnInit, OnDestroy {
  displayedColumns: string[] = ['gameName', 'name', 'actions'];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  private readonly destroy$ = new Subject<void>();

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', { nonNullable: true });

  dataSource = new MatTableDataSource<Tag>([]);
  private tags: Tag[] = [];
  private readonly deletingIds = new Set<number>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly tagService: TagService,
    private readonly router: Router,
    private readonly gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.fetchTags();

    combineLatest([
      this.gameIdControl.valueChanges.pipe(startWith(this.gameIdControl.value)),
      this.searchByNameFilterControl.valueChanges.pipe(startWith(this.searchByNameFilterControl.value)),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([gameId, name]) => {
        this.applyFilters(gameId, name);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private applyFilters(gameId: number | null, name: string): void {
    const nameFilter = (name ?? '').trim().toLowerCase();

    const filtered = this.tags.filter(t => {
      if (gameId !== null && t.gameId !== gameId) { return false; }
      if (nameFilter && !(t.name ?? '').toLowerCase().includes(nameFilter)) { return false; }
      return true;
    });

    this.dataSource.data = filtered;
    this.cdr.markForCheck();
  }

  fetchTags(): void {
    this.tagService.getAll().subscribe(tags => {
      this.tags = tags;

      this.dataSource.data = tags;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;

      this.applyFilters(this.gameIdControl.value, this.searchByNameFilterControl.value);
    });
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(games => {
        this.games$.next(games);
        this.cdr.markForCheck();
      });
  }

  createButtonClicked(): void {
    this.router.navigate(['/tags/create']);
  }

  refreshButtonClicked(): void {
    this.fetchTags();
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/tags/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (this.isDeleting(id)) {
      return;
    }

    this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: {
          title: 'Delete Tag',
          subtitle: 'This action cannot be undone.',
          message: 'Are you sure you want to delete this Tag?',
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

        this.tagService.delete(id)
          .pipe(
            finalize(() => {
              this.deletingIds.delete(id);
              this.cdr.markForCheck();
            }),
          )
          .subscribe({
            next: () => {
              this.fetchTags();
            },
          });
      });
  }

  isDeleting(id: number): boolean {
    return this.deletingIds.has(id);
  }

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.searchByNameFilterControl.setValue('');

  }

  exportButtonClicked(): void
{
  const dataToExport = this.dataSource.data.map(x => ({
    Game: x.gameName,
    Name: x.name ?? '',
  }));

  const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

  const workbook: XLSX.WorkBook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Tags');

  const today = new Date();
  XLSX.writeFile(workbook, `Export_${today.toDateString()}_Tags.xlsx`);
}
}
