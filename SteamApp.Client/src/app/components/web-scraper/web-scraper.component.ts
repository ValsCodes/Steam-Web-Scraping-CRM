import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { SteamService } from '../../services/steam/steam.service';
import { FormArray, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import * as XLSX from 'xlsx';

@Component({
  selector: 'web-scraper',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    MatTableModule,
    MatSort,
    MatSortModule,
    MatPaginatorModule,
  ],
  templateUrl: './web-scraper.component.html',
  styleUrl: './web-scraper.component.scss',
  providers: [SteamService],
})
export class WebScraperComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  dataSource = new MatTableDataSource<any>([]);

  private tableForm: FormArray = new FormArray<any>([]);

  pageNumber: number = 0;
  displayedColumns: string[] = [
    'item',
    'quantity',
    'color',
    'price',
    'actions',
  ];

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  constructor(private steamService: SteamService) {}

  public readonly hatURL: string =
  'https://steamcommunity.com/market/listings/440/';

  getListingsButtonClicked() {
    this.steamService.getScrapedPage(this.pageNumber).subscribe(
      (response) => {
        if (response.length === 0) {
          console.log('No results found');
        }

        this.dataSource.data = response;
        console.log([response]);
      },
      (error) => {
        console.error('Error fetching data:', error);
      }
    );

    this.pageNumber += 1;
  }

  getPaintedListingsButtonClicked() {
    this.steamService.getScrapedPagePaintedOnly(this.pageNumber).subscribe(
      (response) => {
        if (response.length === 0) {
          console.log('No results found');
        }

        this.dataSource.data = response;
        console.log([response]);
      },
      (error) => {
        console.error('Error fetching data:', error);
      }
    );
  }

  getBulkListingsButtonClicked() {
    this.steamService.getBulkPage(this.pageNumber).subscribe(
      (response) => {
        if (response.length === 0) {
          console.log('No results found');
        }

        this.dataSource.data = response;
        console.table(response);
      },
      (error) => {
        console.error('Error fetching data:', error);
      }
    );
  }

  checkPaintButtonClicked(name:string, index:number) {

    this.steamService.getIsHatPainted(name).subscribe(
      (response) => {

        console.log([response]);

        if(response.isPainted === false)
        {
          this.dataSource.data[index].color = "Not Painted";
        }
        else
        {
          this.dataSource.data[index].color = response.paintText;
        }  
      },
      (error) => {
        console.error('Error fetching data:', error);
      }
    );
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Listings.xlsx`);
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];

    console.log(this.tableForm);
  }

  onPageNumberChange(value: number) {
    if (value < 0 || value > 100000) {
      this.pageNumber = 0;
    } else {
      this.pageNumber = value;
    }
  }
}
