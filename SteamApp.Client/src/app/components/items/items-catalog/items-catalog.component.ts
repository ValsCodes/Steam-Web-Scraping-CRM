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
import { Class, classesCollection, classesMap } from '../../../models/enums/class.enum';
import { Slot, slotsCollection, slotsMap } from '../../../models/enums/slot.enum';

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

      classes = classesCollection;
      getClassLabel(id: Class) {
        console.log(classesMap[id])
        return classesMap[id];
      }
    
      slots = slotsCollection;
    
      getSlotLabel(id: Slot) {
        return slotsMap[id];
      }

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

  editButtonClicked(id: number): void {
    if (!id || id <= 0) {
      console.warn('Invalid item id:', id);
      return;
    }

    this.router.navigate(['edit-item', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!id || id <= 0) {
      console.warn('Invalid item id:', id);
      return;
    }

    console.log('Deleting item with id:', id);
    this.itemService.deleteItem(id).subscribe({
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
