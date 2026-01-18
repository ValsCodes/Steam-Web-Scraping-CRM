import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { GameUrlService } from '../../../services/game-url/game-url.service';
import { GameUrl } from '../../../models/game-url.model';

@Component({
  selector: 'steam-game-urls-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './game-urls-view.html',
  styleUrl: './game-urls-view.scss'
})
export class GameUrlsView implements OnInit {
  displayedColumns: string[] = [
    'id',
    'gameId',
    'partialUrl',
    'isBatchUrl',
    'startPage',
    'endPage',
    'isPixelScrape',
    'actions'
  ];

  dataSource = new MatTableDataSource<GameUrl>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private gameUrlService: GameUrlService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.fetchGameUrls();
  }

  fetchGameUrls(): void {
    this.gameUrlService.getAll().subscribe(urls => {
      this.dataSource.data = urls;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/game-urls/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/game-urls/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this Game URL?')) {
      return;
    }

    this.gameUrlService.delete(id).subscribe(() => {
      this.fetchGameUrls();
    });
  }
}
