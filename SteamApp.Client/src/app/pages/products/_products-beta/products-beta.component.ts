import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ProductService } from '../../../services/product/product.service';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { CONSTANTS } from '../../../common/constants';

import { Product, CreateProduct } from '../../../models/product.model';

@Component({
  selector: 'steam-sell-listings',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSort,
    MatSortModule,
    MatPaginatorModule,
    FormsModule,
  ],
  templateUrl: './products-beta.component.html',
  styleUrl: './products-beta.component.scss',
  providers: [ProductService],
})
export class ProductsBetaComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private deletedProductIds: number[] = [];
  private tableForm: FormArray = new FormArray<any>([]);

  dataSource = new MatTableDataSource<any>();
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

  constructor(
    private productService: ProductService,
    private fb: FormBuilder
  ) {}

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

  addButtonClicked(): void {
    const emptySellListing: Product = {
      id: 0,
      name: '',
      qualityId: null,
      description: '',
      dateBought: new Date(),
      dateSold: null,
      costPrice: 0,
      targetSellPrice1: 0,
      targetSellPrice2: 0,
      targetSellPrice3: 0,
      targetSellPrice4: 0,
      soldPrice: null,
      isHat: false,
      isWeapon: false,
      isSold: false,
      isStrange: null,
      paintId: null,
      sheenId: null
    };

    const currentData = this.dataSource.data;
    currentData.unshift(emptySellListing);
    this.dataSource.data = currentData;

    this.tableForm.insert(0, this.createRow(emptySellListing));
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];
  }

  refreshButtonClicked(): void {
    this.fetchSellListings();
  }

  saveButtonClicked(): void {
    if (this.deletedProductIds.length > 0) {
      this.productService.deleteProducts(this.deletedProductIds).subscribe({
        error: (err: any) => {
          console.error('Error Deleting sell listings:', err);
        },
        complete: () => {
          this.deletedProductIds.length = 0;
        },
      });
    }

    const array = Object.values(this.tableForm.controls) as FormGroup[];
    const updatedProducts = array
      .filter((g) => g.dirty)
      .map((g) => g.value as Product);

    if (updatedProducts.length > 0) {
      this.productService.updateProducts(updatedProducts).subscribe({
        error: (err: any) => {
          console.error('Error Updating sell listings:', err);
        },
        complete: () => {},
      });
    }

    const newProducts = (this.tableForm.controls as FormGroup[])
      .filter((group) => group.get('id')?.value === 0)
      .map((row) => {
        const base = row.value;
        const product: CreateProduct = {
          name: 'Test CLient',
          description: 'Test',
          dateBought: base.dateBought ?? new Date(),
          qualityId: 0,
          dateSold: new Date(),
          costPrice: null,
          targetSellPrice1: null,
          targetSellPrice2: null,
          targetSellPrice3: null,
          targetSellPrice4: null,
          soldPrice: null,
          isHat: true,
          isWeapon: false,
          isSold: false,
        };

        return product;
      });

    if (newProducts.length > 0) {
      this.productService.createProducts(newProducts).subscribe({
        error: (err: any) => {
          console.error('Error Creating sell listings:', err);
        },
      });
    }

    console.info('Save Complete!');
  }

  importButtonClicked(): void {
    throw new Error('Method not implemented.');
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Products.xlsx`);
  }

  soldButtonClicked(index: number): void {
    const rowGroup = this.tableForm.at(index) as FormGroup;
    if (rowGroup && rowGroup.get('soldDate')) {
      const today = new Date().toISOString().substring(0, 10);
      rowGroup.get('soldDate')?.setValue(today);

      this.dataSource.data[index].dateSold = today;

      this.dataSource._updateChangeSubscription();
    } else {
      console.error('Control "soldDate" not found on row at index', index);
    }
  }

  deleteButtonClicked(index: number): void {
    const itemId = (this.tableForm.at(index) as FormGroup).get('id')?.value;
    if (itemId > 0) {
      this.deletedProductIds.push(itemId);
    }

    this.tableForm.removeAt(index);
    const data = this.dataSource.data;
    data.splice(index, 1);
    this.dataSource.data = data;
  }

  private fetchSellListings(): void {
    this.productService.getProducts().subscribe({
      next: (value: Product[]) => {
        this.tableForm = this.fb.array(value.map((row) => this.createRow(row)));
        this.dataSource = new MatTableDataSource(value);
      },
      error: (err: any) => {
        console.error('Error fetching sell listings:', err);
      },
    });
  }

  private createRow(data: Product): FormGroup {
    const group = this.fb.group({
      id: [data.id],
      name: [data.name],
      soldDate: [data.dateSold], // include the soldDate control here
    });

    group.valueChanges.subscribe(() => {
      group.markAsDirty();
      console.log('Marked as dirty');
    });

    return group;
  }
}
