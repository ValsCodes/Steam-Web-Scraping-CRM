import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { SteamService } from '../../services/steam/steam.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';


@Component({
  selector: 'home',
  standalone: true,
  imports: [FormsModule, CommonModule,     MatTableModule,
    MatSortModule,
    MatPaginatorModule,],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  providers: [
    SteamService,    
  ],
})
export class HomeComponent{

}
