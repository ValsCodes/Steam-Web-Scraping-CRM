import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { Item } from '../../../models/item.model';
import { Router } from '@angular/router';
import { ItemService } from '../../../services/item/item.service';

@Component({
  selector: 'steam-items-catalog',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSort,
    MatSortModule,
    MatPaginatorModule,
    FormsModule,
  ],
  templateUrl: './items-catalog.component.html',
  styleUrl: './items-catalog.component.scss',
})
export class ItemsCatalogComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<Item>();
  displayedColumns: string[] = [
    'id',
    'name',
    'is_active',
    'is_weapon', 
    'class_id',
    'slot_id',
    'actions',
  ];

  constructor(private router: Router, private itemService: ItemService) {}

  ngOnInit() {
    this.fetchItems();

    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  ngAfterViewInit(): void {
    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
      this.dataSource.paginator.pageSize = 10;
      this.dataSource.paginator.pageSizeOptions = [5, 10, 25];
    }
  }

  createButtonClicked(): void {
    this.router.navigate(['create-item']);
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];
  }

  refreshButtonClicked(): void {
    this.fetchItems();
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Items.xlsx`);
  }

  editButtonClicked(index: number): void {
    const itemId = this.dataSource.data.at(index)?.id!;

    if (itemId > 0) {
      this.router.navigate(['edit-item', itemId]);
    }
  }

  deleteButtonClicked(index: number): void {
    const item = this.dataSource.data[index];
    if (!item || item.id <= 0) {
      console.warn('Invalid item id:', item?.id);
      return;
    }

    console.log('Deleting item with id:', item.id);
    this.itemService.deleteItem(item.id).subscribe({
      next: (product) => {
        console.log('Item deleted:', product.id);
        this.fetchItems();
      },
      error: (err) => console.error('Error deleting item:', err),
    });
  }

  private fetchItems(): void {
    this.itemService.getItems().subscribe({
      next: (items) => {
        this.dataSource.data = items;
      },
      error: (err) => console.error('Error fetching items:', err),
    });
  }
}
