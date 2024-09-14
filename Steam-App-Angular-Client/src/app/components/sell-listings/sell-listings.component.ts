import { Component } from '@angular/core';
import { SellListingComponent } from '../sell-listing/sell-listing.component';
import { Observable, BehaviorSubject } from 'rxjs';
import { SteamService } from '../../services/steam/steam.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { ISellListing } from '../../models/sell.listing.model';

@Component({
  selector: 'steam-sell-listings',
  standalone: true,
  imports: [SellListingComponent, FormsModule, CommonModule],
  templateUrl: './sell-listings.component.html',
  styleUrl: './sell-listings.component.scss'
})
export class SellListingsComponent {

  public sellListings: ISellListing[] = [{
    id: 1,
    name: 'Vintage Fedora Hat',
    description: 'A classic fedora hat from the 1950s.',
    dateBought: new Date('2022-01-15'),
    costPrice: 50,
    targetSellPrice1: 100,
    targetSellPrice2: 120,
    isHat: true,
    isWeapon: false,
    isSold: false,
  },
  {
    id: 2,
    name: 'Antique Sword',
    description: 'A rare 17th-century sword.',
    dateBought: new Date('2021-11-10'),
    dateSold: new Date('2022-02-01'),
    costPrice: 500,
    targetSellPrice1: 800,
    targetSellPrice2: 850,
    targetSellPrice3: 900,
    soldPrice: 850,
    isHat: false,
    isWeapon: true,
    isSold: true,
  },
  {
    id: 3,
    name: 'Leather Cowboy Hat',
    description: 'A rugged leather cowboy hat perfect for outdoors.',
    dateBought: new Date('2022-03-25'),
    costPrice: 70,
    targetSellPrice1: 150,
    targetSellPrice2: 175,
    isHat: true,
    isWeapon: false,
    isSold: false,
  }];


}
