import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort'
import { GameService, GameUrlService, ProductService } from '../../../services';
import { Game, Product } from '../../../models';
import { encode } from '../../../common';

@Component({
  selector: 'steam-products-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './products-view.html',
  styleUrl: './products-view.scss'
})
export class ProductsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    //'gameId',
    'gameName',
    //'fullUrl',
    'name',
    'isActive',
    'actions'
  ];

  dataSource = new MatTableDataSource<Product>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productService: ProductService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.fetchProducts();
  }

  fetchProducts(): void {
    this.productService.getAll().subscribe(products => {
      this.dataSource.data = products;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }
  
  createButtonClicked(): void {
    this.router.navigate(['/products/create']);
  }

    encodeText(value:string): string {
    return encode(value);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/products/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this product?')) {
      return;
    }

    this.productService.delete(id).subscribe(() => {
      this.fetchProducts();
    });
  }
}
