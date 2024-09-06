import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

import { IListing } from '../home/listing.model';

@Component({
  selector: 'steam-listing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './listing.component.html',
  styleUrl: './listing.component.scss',
})
export class ListingComponent {
  @Input() listing!: IListing;
  @Output() checkIsPainted = new EventEmitter();


  isPaintedButtonClicked(name: string) {
    this.checkIsPainted.emit(name);
  }
}
