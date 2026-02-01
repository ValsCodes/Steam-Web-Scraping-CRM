import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { TagService } from '../../../services';
import { Tag } from '../../../models';

@Component({
  selector: 'steam-tags-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
  ],
  templateUrl: './tags-view.html',
  styleUrl: './tags-view.scss',
})
export class TagsView implements OnInit {
  displayedColumns: string[] = [
    'gameName',
    'name',
    'actions',
  ];

  dataSource = new MatTableDataSource<Tag>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private tagService: TagService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.fetchTags();
  }

  fetchTags(): void {
    this.tagService.getAll().subscribe(tags => {
      this.dataSource.data = tags;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/tags/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/tags/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this tag?')) {
      return;
    }

    this.tagService.delete(id).subscribe(() => {
      this.fetchTags();
    });
  }
}