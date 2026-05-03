import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize, startWith, Subject, takeUntil } from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';

import { WatchList } from '../../../models/watch-list.model';
import { WatchListService } from '../../../services/watch-list/watch-list.service';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';

import * as XLSX from 'xlsx';

@Component({
  selector: 'steam-watch-lists-view',
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
  templateUrl: './watch-lists-view.html',
  styleUrl: './watch-lists-view.scss',
})
export class WatchListsView implements OnInit, OnDestroy {
  displayedColumns: string[] = [
    'name',
    'url',
    'registrationDate',
    'isActive',
    'actions',
  ];

  dataSource = new MatTableDataSource<WatchList>([]);
  private allItems: WatchList[] = [];
  private readonly deletingIds = new Set<number>();

  readonly searchByNameControl = new FormControl<string>('', { nonNullable: true });

  private readonly destroy$ = new Subject<void>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly watchListService: WatchListService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.fetchWatchLists();

    this.searchByNameControl.valueChanges
      .pipe(startWith(this.searchByNameControl.value), takeUntil(this.destroy$))
      .subscribe(name => {
        this.applyFilters(name);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private fetchWatchLists(): void {
    this.watchListService.getAll().subscribe(items => {
      this.allItems = items;

      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;

      this.applyFilters(this.searchByNameControl.value);
    });
  }

  private applyFilters(name: string): void {
    const filter = (name ?? '').trim().toLowerCase();

    this.dataSource.data = this.allItems.filter(item => {
      if (!filter) { return true; }
      return (item.name ?? '').toLowerCase().includes(filter);
    });
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameControl.setValue('');
    // subscription re-applies automatically
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data.map(x => ({
      Name: x.name ?? '',
      Url: x.url ?? '',
      RegistrationDate: x.registrationDate ?? '',
      Active: x.isActive,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'WatchList');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_WatchList.xlsx`);
  }

  refreshButtonClicked(): void {
    this.fetchWatchLists();
  }

  createButtonClicked(): void {
    this.router.navigate(['/watch-list/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/watch-list/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (this.isDeleting(id)) {
      return;
    }

    this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: {
          title: 'Delete Watch List Item',
          subtitle: 'This action cannot be undone.',
          message: 'Are you sure you want to delete this watch list item?',
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

        this.watchListService.delete(id)
          .pipe(
            finalize(() => {
              this.deletingIds.delete(id);
              this.cdr.markForCheck();
            }),
          )
          .subscribe({
            next: () => {
              this.fetchWatchLists();
            },
          });
      });
  }

  isDeleting(id: number): boolean {
    return this.deletingIds.has(id);
  }
}
