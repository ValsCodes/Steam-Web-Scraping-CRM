import { Component } from '@angular/core';
import { ListingComponent } from "../listing/listing.component";

@Component({
  selector: 'steam-home',
  standalone: true,
  imports: [ListingComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {

}
