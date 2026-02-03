import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort'
import { GameService, GameUrlService, ProductService } from '../../../services';
import { Game, Product } from '../../../models';
import { encode } from '../../../common';
import { FormControl } from '@angular/forms';
import { TextFilterComponent } from "../../../components";
import { NumberFilterComponent } from "../../../components/filters/number-filter.component";

@Component({
  selector: 'steam-products-grid',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, TextFilterComponent, NumberFilterComponent],
  templateUrl: './products-view.html',
  styleUrl: './products-view.scss',
})
export class ProductsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    //'gameId',
    'gameName',
    //'fullUrl',
    'name',
    'tags',
    'rating',
    'isActive',
    'actions',
  ];

  dataSource = new MatTableDataSource<Product>([]);

  products: Product[] = [];
  productsFiltered: Product[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productService: ProductService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.fetchProducts();
  }

  fetchProducts(): void {
    this.productService.getAll().subscribe((products) => {
      this.dataSource.data = products;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;

      this.products = products;
      this.productsFiltered = products;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/products/create']);
  }

  encodeText(value: string): string {
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

  searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  searchByRatingFilterControl = new FormControl<number | null>(null, {
    nonNullable: true,
  });

  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredProducts();
  }

   onRatingFilterChanged(filter: number): void {
    this.searchByRatingFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredProducts();
  }

  private loadFilteredProducts(): void {

    const nameFilter = this.searchByNameFilterControl.value?.toLowerCase() ?? '';

      const ratingFilter = this.searchByRatingFilterControl.value;
      
    this.productsFiltered = this.products.filter((product) => {
      const matchesName = (!nameFilter || product.name?.toLowerCase().includes(nameFilter)) && 
      (!ratingFilter || product.rating == ratingFilter);

      return matchesName;
    });

    this.dataSource.data = this.productsFiltered;
  }

    clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.searchByRatingFilterControl.setValue(null, { emitEvent: false });

    this.loadFilteredProducts();
  }
}
