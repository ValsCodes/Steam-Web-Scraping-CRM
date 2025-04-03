import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ProductService } from '../../services/product/product.service';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';

import { Product } from '../../models/product.model';

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
  templateUrl: './sell-listings.component.html',
  styleUrl: './sell-listings.component.scss',
  providers: [ProductService],
})
export class SellListingsComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private deletedProductIds: number[] = [];
  private tableForm: FormArray = new FormArray<any>([]);

  dataSource = new MatTableDataSource<any>();
  displayedColumns: string[] = [
    'item',
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

  constructor(private productService: ProductService, private fb: FormBuilder) {}

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
      boughtPrice: 0,
      targetSellPrice1: 0,
      targetSellPrice2: 0,
      targetSellPrice3: 0,
      targetSellPrice4: 0,
      soldPrice: null,
      isHat: false,
      isWeapon: false,
      isSold: false,
    };

    // Update dataSource
    const currentData = this.dataSource.data;
    currentData.unshift(emptySellListing);
    this.dataSource.data = currentData;

    // Also update the FormArray with the new row including soldDate control
    this.tableForm.insert(0, this.createRow(emptySellListing));
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];

    console.log(this.tableForm);
  }

  refreshButtonClicked(): void {
    this.fetchSellListings();
  }

  saveButtonClicked(): void {
    if (this.deletedProductIds.length > 0) {
      this.productService.deleteProducts(this.deletedProductIds);
    }

    const updatedProducts = this.tableForm.controls.filter((row) => row.dirty); // changed rows
    if (updatedProducts.length > 0) {
      // Map
      // this.steamService.updateProducts(updatedProducts);
    }

    const newProducts = (this.tableForm.controls as FormGroup[]).filter(
      (group) => group.get('id')?.value === 0
    );
    if (newProducts.length > 0) {
      // Map
      // this.steamService.createProducts(newProducts);
    }
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
      // Update the form control
      rowGroup.get('soldDate')?.setValue(today);

      // Update the underlying data object
      this.dataSource.data[index].dateSold = today;

      // Refresh the table to reflect changes
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
    this.productService.getProducts().subscribe(
      (listings: Product[]) => {
        this.tableForm = this.fb.array(
          listings.map((row) => this.createRow(row))
        );
        this.dataSource = new MatTableDataSource(listings); // Assign data to MatTableDataSource
      },
      (error) => {
        console.error('Error fetching sell listings:', error);
      }
    );
  }

  private createRow(data: any): FormGroup {
    const group = this.fb.group({
      id: [data.id],
      name: [data.name],
      soldDate: [data.dateSold], // include the soldDate control here
    });

    group.valueChanges.subscribe(() => {
      group.markAsDirty();
    });

    return group;
  }
}
