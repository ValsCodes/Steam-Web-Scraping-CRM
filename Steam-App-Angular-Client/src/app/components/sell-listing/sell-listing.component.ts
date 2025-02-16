import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Product } from '../../models/sell.listing.model';

@Component({
  selector: 'steam-sell-listing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sell-listing.component.html',
  styleUrl: './sell-listing.component.scss',

})
export class SellListingComponent {

  private previousSellListing!: Product;

  @Input() sellListing!: Product;
  @Input() isModified?: boolean = true;
  @Output() deleteSellListing = new EventEmitter<number>();

  ngDoCheck(): void {
    if(this.isModified)
    {
      return;
    }

    if (this.sellListing && this.previousSellListing) {
      if (this.haveSellListingPropertiesChanged()) {

        this.isModified = true;
        console.log('Sell listing properties have changed!');
      }
    }

    this.previousSellListing = { ...this.sellListing };
  }

  private haveSellListingPropertiesChanged(): boolean {
    return (
      this.sellListing.name !== this.previousSellListing.name ||
      this.sellListing.qualityId !== this.previousSellListing.qualityId ||
      this.sellListing.description !== this.previousSellListing.description ||
      this.sellListing.dateBought !== this.previousSellListing.dateBought ||
      this.sellListing.dateSold !== this.previousSellListing.dateSold ||
      this.sellListing.boughtPrice !== this.previousSellListing.boughtPrice ||
      this.sellListing.targetSellPrice1 !== this.previousSellListing.targetSellPrice1 ||
      this.sellListing.targetSellPrice2 !== this.previousSellListing.targetSellPrice2 ||
      this.sellListing.targetSellPrice3 !== this.previousSellListing.targetSellPrice3 ||
      this.sellListing.targetSellPrice4 !== this.previousSellListing.targetSellPrice4 ||
      this.sellListing.soldPrice !== this.previousSellListing.soldPrice ||
      this.sellListing.isHat !== this.previousSellListing.isHat ||
      this.sellListing.isWeapon !== this.previousSellListing.isWeapon ||
      this.sellListing.isSold !== this.previousSellListing.isSold
    );
  }

  soldButtonClicked() {
    const today = new Date();
    const formattedDate = today.toISOString().split('T')[0];
    this.sellListing.dateSold = new Date(formattedDate);

    if(!this.isModified)
    {
      this.isModified = true;
      console.log(`IsModified: ${this.isModified}`);
    }
  }

  deleteButtonClicked(): void {
    this.deleteSellListing.emit(this.sellListing.id);
  }
}
