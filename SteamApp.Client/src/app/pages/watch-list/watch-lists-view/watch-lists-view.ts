import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { WatchList } from '../../../models/watch-list.model';
import { WatchListService } from '../../../services/watch-list/watch-list.service';

@Component({
  selector: 'steam-watch-lists-view',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './watch-lists-view.html',
  styleUrl: './watch-lists-view.scss'
})
export class WatchListsView implements OnInit {
  displayedColumns: string[] = [
    'gameName',
    'gameUrlName',
    'name',
    'rating',
    'releaseDate',
    'description',
    'isActive',
    'actions'
  ];

  dataSource = new MatTableDataSource<WatchList>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private watchListService: WatchListService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.fetchWatchLists();
  }

  fetchWatchLists(): void {
    this.watchListService.getAll().subscribe(items => {
      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
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
