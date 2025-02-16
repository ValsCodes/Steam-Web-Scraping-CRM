import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { ListingComponent } from '../listing/listing.component';
import { SteamService } from '../../services/steam/steam.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';


import { Listing } from '../../models/listing.model';

@Component({
  selector: 'web-scraper',
  standalone: true,
  imports: [ListingComponent, FormsModule, CommonModule, MatTableModule,
    MatSort,
    MatSortModule,
    MatPaginatorModule,],
  templateUrl: './web-scraper.component.html',
  styleUrl: './web-scraper.component.scss',
  providers: [
    SteamService,    
  ],
})
export class WebScraperComponent implements OnInit, AfterViewInit{
  pageNumber: number = 0;
  displayedColumns: string[] = [
    'item',
    'quantity',
    'color',
    'price',
    'actions'
  ];

  dataSource = new MatTableDataSource<Listing>([]);

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }

    this.dataSource.data = this.listings; 
  }

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  public listings: Listing[] = [{
    name: 'Red Jacket',
    price: 59.99,
    imageUrl: 'https://example.com/images/red-jacket.png',
    quantity: 10,
    color: 'Red',
    linkUrl: "https://examplelink.com"
  },
  {
    name: 'Blue Shoes',
    price: 89.99,
    imageUrl: 'https://example.com/images/blue-shoes.png',
    quantity: 5,
    color: 'Blue',
    linkUrl: "https://examplelink.com"
  },
  {
    name: 'Green Hat',
    price: 25.00,
    imageUrl: 'https://example.com/images/green-hat.png',
    quantity: 15,
    color: 'Green',
    linkUrl: "https://examplelink.com"
  },
  {
    name: 'Yellow Scarf',
    price: 19.99,
    imageUrl: 'https://example.com/images/yellow-scarf.png',
    quantity: 8,
    color: 'Yellow',
    linkUrl: "https://examplelink.com"
  },
  {
    name: 'Black Watch',
    price: 150.00,
    imageUrl: 'https://example.com/images/black-watch.png',
    quantity: 3,
    color: 'Black',
    linkUrl: "https://examplelink.com"
  }]

  constructor(private steamService: SteamService) {}

  getListingsButtonClicked(pageNum: number) {
      this.pageNumber += 1;
  }

  isPaintedButtonClicked(name: string) {
      this.pageNumber += 1;
  }

  onPageNumberChange(value: number) {
    
    if (value < 0 || value > 100000) {
      this.pageNumber = 0;
    } else {
      this.pageNumber = value;
    }
  }
}
