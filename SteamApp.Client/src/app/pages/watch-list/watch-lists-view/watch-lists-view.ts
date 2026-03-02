import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { startWith, Subject, takeUntil } from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { WatchList } from '../../../models/watch-list.model';
import { WatchListService } from '../../../services/watch-list/watch-list.service';

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

  readonly searchByNameControl = new FormControl<string>('', { nonNullable: true });

  private readonly destroy$ = new Subject<void>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly watchListService: WatchListService,
    private readonly router: Router,
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

  createButtonClicked(): void {
    this.router.navigate(['/watch-list/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/watch-list/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this watch list item?')) {
      return;
    }

    this.watchListService.delete(id).subscribe(() => {
      this.fetchWatchLists();
    });
  }
}