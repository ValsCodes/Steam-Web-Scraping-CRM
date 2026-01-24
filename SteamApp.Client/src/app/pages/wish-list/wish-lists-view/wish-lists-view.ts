import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { WishList } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';

@Component({
  selector: 'steam-wish-lists-view',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './wish-lists-view.html',
  styleUrl: './wish-lists-view.scss'
})
export class WishListsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    'gameName',
    'name',
    'pageUrl',
    'price',
    'isActive',
    'actions'
  ];

  dataSource = new MatTableDataSource<WishList>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private wishListService: WishListService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.fetchWishLists();
  }

  fetchWishLists(): void {
    this.wishListService.getAll().subscribe(items => {
      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/wishlist/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/wishlist/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this wish list item?')) {
      return;
    }

    this.wishListService.delete(id).subscribe(() => {
      this.fetchWishLists();
    });
  }
}
