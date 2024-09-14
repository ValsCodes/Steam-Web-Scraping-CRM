import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ISellListing } from '../../models/sell.listing.model';

@Component({
  selector: 'steam-sell-listing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sell-listing.component.html',
  styleUrl: './sell-listing.component.scss'
})
export class SellListingComponent {
  @Input() sellListing!: ISellListing;
  @Output() checkIsPainted = new EventEmitter();

}
