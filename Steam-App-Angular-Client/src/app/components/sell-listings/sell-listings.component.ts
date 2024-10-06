import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { SellListingComponent } from '../sell-listing/sell-listing.component';
import { SteamService } from '../../services/steam/steam.service';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { ISellListing } from '../../models/sell.listing.model';

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
  ],
  templateUrl: './sell-listings.component.html',
  styleUrl: './sell-listings.component.scss',
  providers: [SteamService],
})
export class SellListingsComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = [
    'item',
    'description',
    'bought-price',
    'date-bought',
    'target1',
    'target2',
    'target3',
    'target4',
    'date-sold',
    'actions',
  ];

  constructor(private steamService: SteamService) {
    this.fetchSellListings();
  }

  dataSource = new MatTableDataSource<ISellListing>([]);
  loading = true;

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  fetchSellListings(): void {
    this.steamService.getSellListings().subscribe(
      (listings: ISellListing[]) => {
        this.dataSource.data = listings; // Assign data to MatTableDataSource
        this.loading = false; // Set loading to false when data has been loaded
      },
      (error) => {
        console.error('Error fetching sell listings:', error);
        this.loading = false; // Set loading to false in case of error
      }
    );
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];
  }

  refreshButtonClicked(): void {
    this.fetchSellListings();
  }
}
