import { Component, OnInit } from '@angular/core';
import { ListingComponent } from '../listing/listing.component';
import { Observable, BehaviorSubject } from 'rxjs';
import { SteamService } from './steam.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { IListing } from '../home/listing.model';

@Component({
  selector: 'steam-home',
  standalone: true,
  imports: [ListingComponent, FormsModule, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  providers: [SteamService],
})
export class HomeComponent {
  pageNumber: number = 0;

  //private listingsCollection: BehaviorSubject<IListing[]> = new BehaviorSubject<IListing[]>([]);
  public listings: IListing[] = [{
    name: 'Red Jacket',
    price: 59.99,
    imageUrl: 'https://example.com/images/red-jacket.png',
    quantity: 10,
    color: 'Red'
  },
  {
    name: 'Blue Shoes',
    price: 89.99,
    imageUrl: 'https://example.com/images/blue-shoes.png',
    quantity: 5,
    color: 'Blue'
  },
  {
    name: 'Green Hat',
    price: 25.00,
    imageUrl: 'https://example.com/images/green-hat.png',
    quantity: 15,
    color: 'Green'
  },
  {
    name: 'Yellow Scarf',
    price: 19.99,
    imageUrl: 'https://example.com/images/yellow-scarf.png',
    quantity: 8,
    color: 'Yellow'
  },
  {
    name: 'Black Watch',
    price: 150.00,
    imageUrl: 'https://example.com/images/black-watch.png',
    quantity: 3,
    color: 'Black'
  }]

  //injecting the service makes the page go white for some reason
  //constructor(private steamSrv: SteamService) {}

  getListingsButtonClicked(pageNum: number) {
    //test binding
      this.pageNumber += 1;
    // Get Listings Steam from Service
  }

  isPaintedButtonClicked(name: string) {
    //test binding
      this.pageNumber += 1;
    // Get Listings Steam from Service
  }

  onPageNumberChange(value: number) {
    
    if (value < 0 || value > 100000) {
      this.pageNumber = 0;
    } else {
      this.pageNumber = value;
    }
  }
}
