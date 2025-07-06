import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { ProductService } from '../../../services/product/product.service';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { CONSTANTS } from '../../../common/constants';
import { Product } from '../../../models/product.model';
import { Router } from '@angular/router';

@Component({
  selector: 'steam-sell-listings-2',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSort,
    MatSortModule,
    MatPaginatorModule,
    FormsModule,
  ],
  templateUrl: './products-catalog.component.html',
  styleUrl: './products-catalog.component.scss',
  providers: [ProductService],
})
export class ProductsCatalogComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<Product>();
  displayedColumns: string[] = [
    'item',
    'listing_URL',
    'description',
    'bought-price',
    'date-bought',
    'target1',
    'target2',
    // 'target3',
    // 'target4',
    'date-sold',
    'sold-price',
    'actions',
  ];

  private _constants = CONSTANTS;
  listingUrlPartial: string = this._constants.LISTING_URL_PARTIAL;

  constructor(private router: Router, private productService: ProductService) {}

  ngOnInit() {
    this.fetchSellListings();

    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  ngAfterViewInit(): void {
    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
      this.dataSource.paginator.pageSize = 10;
      this.dataSource.paginator.pageSizeOptions = [5, 10, 25];
    }
  }

  createButtonClicked(): void {
    this.router.navigate(['create-product']);
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];
  }

  refreshButtonClicked(): void {
    this.fetchSellListings();
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Products.xlsx`);
  }

  editButtonClicked(id: number): void {
    if (!id ||  id <= 0) {
      console.warn('Invalid item id:', id);
      return;
    }

    this.router.navigate(['edit-product', id]);
  }

deleteButtonClicked(id: number): void {
    if (!id || id <= 0) {
      console.warn('Invalid item id:', id);
      return;
    }

    console.log('Deleting product with id:', id);
    this.productService.deleteProduct(id).subscribe({

      next: product => {
        console.log('Product deleted:', product.id);
        this.fetchSellListings();
      },
      error: err => console.error('Error deleting product:', err)
    }

    );
  }

  private fetchSellListings(): void {
    this.productService.getProducts().subscribe({
      next: (products) => {
        this.dataSource.data = products;
      },
      error: (err) => console.error('Error fetching sell listings:', err),
    });
  }
}
