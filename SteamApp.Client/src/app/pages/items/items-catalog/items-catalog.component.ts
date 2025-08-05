import { AfterViewInit, Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { Item } from '../../../models/index';
import { Router } from '@angular/router';
import { ItemService } from '../../../services/item/item.service';
import {
  Slot,
  slotsMap,
  Class,
  classesMap,
  ActivityFilters,
  activityFiltersCollection,
  classFiltersCollection, slotFiltersCollection
} from '../../../models/enums/index';
import { CONSTANTS } from '../../../common/constants';
import { TextFilterComponent, CheckboxesFilterComponent } from "../../../components/index";

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
    ReactiveFormsModule,
    CheckboxesFilterComponent,
    TextFilterComponent,
  ],
  templateUrl: './items-catalog.component.html',
  styleUrl: './items-catalog.component.scss',
})
export class ItemsCatalogComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  @ViewChildren(TextFilterComponent) textFilters!: QueryList<TextFilterComponent>;
  @ViewChildren(CheckboxesFilterComponent) checkboxFilters!: QueryList<CheckboxesFilterComponent>;

  private _constants = CONSTANTS;

  readonly weaponUrlPartial: string =
    this._constants.LISTING_URL_PARTIAL +
    this._constants.WEAPON_URL_QUERY_PARAMS;

  dataSource = new MatTableDataSource<Item>();
  displayedColumns: string[] = [
    'id',
    'name',
    'currentStock',
    'tradesCount',
    'rating',
    'classId',
    'slotId',
    'isActive',
    'isWeapon',
    'actions',
  ];

  searchByItemNameFilter = new FormControl<string>('', { nonNullable: true });
  activityFilters = activityFiltersCollection;
  classFilters = classFiltersCollection;
  slotFilters = slotFiltersCollection;

  ClearAllFilters() {
     this.textFilters.forEach(c => c.clearFilter());
     this.checkboxFilters.forEach(c => c.clearFilters());
  }

  constructor(private router: Router, private itemService: ItemService) {}

  ngOnInit() {
    this.fetchItems();
  }

  ngAfterViewInit(): void {
    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }

    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
      this.dataSource.paginator.pageSize = 10;
      this.dataSource.paginator.pageSizeOptions = [5, 10, 25];
    }
  }

  getClassLabel(id: Class) {
    return classesMap[id];
  }

  getSlotLabel(id: Slot) {
    return slotsMap[id];
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

  clearActivityFilters(): void {
    this.activityFilters.forEach((filter) => (filter.checked = false));
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

  repeatStars = (count: number): string => {
    if (count < 0) {
      if (count < -5) {
        count = -5;
      }

      return '💩'.repeat(Math.abs(count));
    }

    if (count > 5) {
      count = 5;
    }

    return '⭐'.repeat(count);
  };

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

  fetchItems = (
    nameFilter: string = this.searchByItemNameFilter.value
  ): void => {
    this.itemService
      .getItems(
        this.classFilters.filter((x) => x.checked === true).map((x) => x.id),
        this.slotFilters.filter((x) => x.checked === true).map((x) => x.id),
        nameFilter
      )
      .subscribe({
        next: (items) => {
          const filteredData = this.applyFilters(items);
          this.dataSource.data = filteredData;
        },
        error: (err) => console.error('Error fetching items:', err),
      });
  };

  private applyFilters(items: Item[]) {
    return items.filter((item) => {
      if (this.isChecked(ActivityFilters.Active) && !item.isActive)
        return false;
      if (this.isChecked(ActivityFilters.Inactive) && item.isActive)
        return false;
      if (this.isChecked(ActivityFilters.Weapon) && !item.isWeapon)
        return false;
      if (this.isChecked(ActivityFilters.NonWeapon) && item.isWeapon)
        return false;
      return true;
    });
  }

  private isChecked(id: ActivityFilters): boolean {
    return !!this.activityFilters.find((f) => f.id === id)?.checked;
  }
}
