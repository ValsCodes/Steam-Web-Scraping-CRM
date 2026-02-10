import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { WatchList } from '../../../models/watch-list.model';
import { WatchListService } from '../../../services/watch-list/watch-list.service';
import { TextFilterComponent } from '../../../components';

@Component({
  selector: 'steam-watch-lists-view',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    TextFilterComponent,
  ],
  templateUrl: './watch-lists-view.html',
  styleUrl: './watch-lists-view.scss',
})
export class WatchListsView implements OnInit {
  displayedColumns: string[] = [
    'name',
    'url',
    'registrationDate',
    'isActive',
    'actions',
  ];

  dataSource = new MatTableDataSource<WatchList>([]);
  private allItems: WatchList[] = [];

  searchByNameControl = new FormControl<string>('', { nonNullable: true });

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly watchListService: WatchListService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.fetchWatchLists();
  }

  private fetchWatchLists(): void {
    this.watchListService.getAll().subscribe((items) => {
      this.allItems = items;
      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  onNameFilterChanged(value: string): void {
    this.searchByNameControl.setValue(value, { emitEvent: false });

    const filter = value.toLowerCase();

    this.dataSource.data = this.allItems.filter(
      (item) => !filter || item.name?.toLowerCase().includes(filter),
    );
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameControl.setValue('', { emitEvent: false });
    this.dataSource.data = this.allItems;
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
