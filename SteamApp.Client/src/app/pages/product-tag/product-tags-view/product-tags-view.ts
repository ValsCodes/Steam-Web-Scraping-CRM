import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { ProductTag } from '../../../models';
import { ProductTagService } from '../../../services';

@Component({
  selector: 'app-product-tags',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
  ],
  templateUrl: './product-tags-view.html',
})
export class ProductTagsView implements AfterViewInit {
  displayedColumns: string[] = [
    'productName',
    'tagName',
    'actions',
  ];

  dataSource = new MatTableDataSource<ProductTag>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productTagService: ProductTagService,
    private router: Router,
  ) {}

  ngAfterViewInit(): void {
    this.fetchProductTags();
  }

  fetchProductTags(): void {
    this.productTagService.getAll().subscribe(items => {
      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/product-tags/create']);
  }

  deleteButtonClicked(productId: number, tagId: number): void {
    if (!confirm('Remove tag from product?')) {
      return;
    }

    this.productTagService.delete(productId, tagId).subscribe(() => {
      this.fetchProductTags();
    });
  }
}