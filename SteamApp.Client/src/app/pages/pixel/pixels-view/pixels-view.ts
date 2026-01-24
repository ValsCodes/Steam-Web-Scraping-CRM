import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { PixelListItem } from '../../../models';
import { PixelService } from '../../../services';

@Component({
  selector: 'steam-pixels-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './pixels-view.html',
  styleUrl: './pixels-view.scss'
})
export class PixelsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    'gameName',
    'name',
    'rgb',
    'actions'
  ];

  dataSource = new MatTableDataSource<PixelListItem>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private pixelService: PixelService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.fetchPixels();
  }

  fetchPixels(): void {
    this.pixelService.getAll().subscribe(pixels => {
      this.dataSource.data = pixels;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/pixels/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/pixels/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this pixel?')) {
      return;
    }

    this.pixelService.delete(id).subscribe(() => {
      this.fetchPixels();
    });
  }

  rgb(pixel: PixelListItem): string {
    return `rgb(${pixel.redValue}, ${pixel.greenValue}, ${pixel.blueValue})`;
  }
}
