import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { SellListingComponent } from '../sell-listing/sell-listing.component';
import { SteamService } from '../../services/steam/steam.service';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormsModule } from '@angular/forms';
import { QueryList, ViewChildren } from '@angular/core';

import { Product } from '../../models/sell.listing.model';

@Component({
  selector: 'steam-sell-listings',
  standalone: true,
  imports: [
    SellListingComponent,
    CommonModule,
    MatTableModule,
    MatSort,
    MatSortModule,
    MatPaginatorModule,
    FormsModule,
  ],
  templateUrl: './sell-listings.component.html',
  styleUrl: './sell-listings.component.scss',
  providers: [SteamService],
})
export class SellListingsComponent implements OnInit, AfterViewInit {
  private deletedListings: number[] = [];

  constructor(private steamService: SteamService) {
    this.fetchSellListings();
  }

  ngOnInit() {

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

  //#region Decorators

  @ViewChildren(SellListingComponent)
  sellListingComponents!: QueryList<SellListingComponent>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  //#endregion

  //#region Public Properties
  dataSource = new MatTableDataSource<Product>([]);

  loading = true;

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
//#endregion Public Properties
modifiedStates: { [key: number]: boolean } = {};
  //#region Button Commands
  addButtonClicked() {
    //TODO set Child Component IsModified to true
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
      isSold: false
    };

    this.modifiedStates[emptySellListing.id] = true;

    const currentData = this.dataSource.data;
    currentData.unshift(emptySellListing);

    this.dataSource.data = currentData;
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];
  }

  refreshButtonClicked(): void {
    this.fetchSellListings();
  }

  saveButtonClicked(): void {
    if (this.deletedListings.length > 0) {
     // this.steamService.deleteBulkListings(this.deletedListings);
    }

    var updatedProducts = this.sellListingComponents
      .filter((x) => {
        return x.isModified === true;
      }
    )
      .map((x) => x.sellListing);

    this.steamService.updateProducts(updatedProducts);
  }

  onDeleteSellListing(id: number) {
    if (id != 0) {
      this.deletedListings.push(id);
    }

    const index = this.dataSource.data.findIndex((item) => item.id === id);
    this.dataSource.data.splice(index, 1);
    this.dataSource.data = [...this.dataSource.data];
  }

  importButtonClicked() {
    throw new Error('Method not implemented.');
  }

  exportButtonClicked() {
    throw new Error('Method not implemented.');
  }
  //#endregion Button Commands

  //#region Private Methods

  private fetchSellListings(): void {
    this.steamService.getProducts().subscribe(
      (listings: Product[]) => {
        this.dataSource.data = listings; // Assign data to MatTableDataSource
        this.loading = false; // Set loading to false when data has been loaded
      },
      (error) => {
        console.error('Error fetching sell listings:', error);
        this.loading = false; // Set loading to false in case of error
      }
    );
  }

  onDateSoldChange(sellListing: Product, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    sellListing.dateSold = value ? new Date(value) : null;
  }

  //#endregion Private Methods
}
