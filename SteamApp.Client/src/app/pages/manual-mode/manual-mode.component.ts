import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { FormControl, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CONSTANTS } from '../../common/constants';
import { ReactiveFormsModule } from '@angular/forms';
import { ItemService } from '../../services/item/item.service';
import {
  CheckboxesFilterComponent,
  CopyLinkComponent,
  TextFilterComponent,
} from '../../components/index';
import { Item, UpdateItem } from '../../models/index';
import {
  classFiltersCollection,
  slotFiltersCollection,
} from '../../models/enums';

@Component({
    selector: 'steam-manual-mode',
    imports: [
        FormsModule,
        CommonModule,
        ReactiveFormsModule,
        CopyLinkComponent,
        CheckboxesFilterComponent,
        TextFilterComponent,
    ],
    templateUrl: './manual-mode.component.html',
    styleUrl: './manual-mode.component.scss'
})
export class ManualModeComponent implements OnInit {
  @ViewChildren(TextFilterComponent)
  textFilters!: QueryList<TextFilterComponent>;
  @ViewChildren(CheckboxesFilterComponent)
  checkboxFilters!: QueryList<CheckboxesFilterComponent>;

  searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  classFilters = classFiltersCollection;
  slotFilters = slotFiltersCollection;

  private _constants = CONSTANTS;

  private readonly _hatUrlPartial: string =
    this._constants.SEARCH_URL_PARTIAL +
    this._constants.PRODUCT_QUERY_PARAMS_EXTENDED;

  public readonly weaponUrlPartial: string =
    this._constants.LISTING_URL_PARTIAL +
    this._constants.WEAPON_URL_QUERY_PARAMS;

  public weaponsCollection: Item[] = [];

  public hatsBatchSize: number = 7;
  public weaponsBatchSize: number = 3;

  public currentPage: number = 76;
  public currentIndex: number = 1;

  public hatStatusLabel: string = '';
  public weaponStatusLabel: string = '';

  constructor(private itemService: ItemService) {}

  ngOnInit(): void {
    this.loadWeapons();
  }

  startHatsBatch(): void {
    let toPage = this.currentPage + this.hatsBatchSize;
    let blocked = false;

    for (; this.currentPage < toPage; this.currentPage++) {
      let url = `${this._hatUrlPartial}p${this.currentPage}_price_asc`;

      let newWindow = window.open(url, '_blank');

      if (
        !newWindow ||
        newWindow.closed ||
        typeof newWindow.closed === 'undefined'
      ) {
        blocked = true;
      }
    }

    this.hatStatusLabel = blocked ? 'Bad Batch!' : 'Successful Batch!';

    if (blocked) {
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    }
  }

  startWeaponsBatch(): void {
    let toIndex = this.currentIndex + this.weaponsBatchSize;
    let blocked = false;

    for (; this.currentIndex < toIndex; this.currentIndex++) {
      let url =
        this.weaponsCollection.length >= this.currentIndex
          ? `${this.weaponUrlPartial}${
              this.weaponsCollection[this.currentIndex - 1].name
            }`
          : null;

      if (url == null) {
        this.weaponStatusLabel = 'No More Weapons!';
        return;
      }

      let newWindow = window.open(url, '_blank');

      if (
        !newWindow ||
        newWindow.closed ||
        typeof newWindow.closed === 'undefined'
      ) {
        blocked = true;
      }
    }

    this.weaponStatusLabel = blocked ? 'Bad Batch!' : 'Successful Batch!';

    if (blocked) {
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    }
  }

  showAllWeaponsButtonClicked(): void {
    let blocked = false;

    for (let index = 0; index < this.weaponsCollection.length; index++) {
      let url = `${this.weaponUrlPartial}${this.weaponsCollection[index].name}`;
      console.log(url);
      let newWindow = window.open(url, '_blank');

      if (
        !newWindow ||
        newWindow.closed ||
        typeof newWindow.closed === 'undefined'
      ) {
        blocked = true;
      }
    }

    this.weaponStatusLabel = blocked ? 'Bad Batch!' : 'Successful Batch!';

    if (blocked) {
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    }
  }

  loadWeapons = (
    nameFilter: string = this.searchByNameFilterControl.value
  ): void => {
    this.itemService
      .getItems(
        this.classFilters.filter((x) => x.checked).map((x) => x.id),
        this.slotFilters.filter((x) => x.checked).map((x) => x.id),
        nameFilter
      )
      .subscribe({
        next: (data) => {
          const filteredData = data.filter(
            (item) => item.isActive && item.isWeapon
          );

          this.weaponsCollection.length = 0;
          this.weaponsCollection.push(...filteredData);
        },
        error: (err) => {
          console.log('Error Loading Items: ' + err);
        },
      });
  };

  addStockCount(id: number, currentStock: number | null): void {
    if (Number.isNaN(id) || id <= 0) {
      console.log('Bad Id');
      return;
    }

    if (Number.isNaN(currentStock)) {
      console.log('Bad Current Stock');
      return;
    }

    if (currentStock === null) {
      currentStock = 0;
    }

    currentStock++;

    const updated: UpdateItem = {
      id: id,
      currentStock: currentStock,
    };

    this.updateItem(updated);
  }

  removeStockCount(id: number, currentStock: number | null): void {
    if (Number.isNaN(id) || id <= 0) {
      console.log('Bad Id');
      return;
    }

    if (Number.isNaN(currentStock)) {
      console.log('Bad Current Stock');
      return;
    }

    if (currentStock === null) {
      currentStock = 0;
    }

    currentStock--;

    const updated: UpdateItem = {
      id: id,
      currentStock: currentStock,
    };

    this.updateItem(updated);
  }

  updateItem = (item: UpdateItem): void => {
    this.itemService.updateItem(item).subscribe({
      next: () => {
        console.log('Updated item', item);
        this.loadWeapons();
      },
      error: (err) => console.error('Error updating item:', err),
    });
  };

  addTradesCount(id: number, tradesCount: number | null) {
    if (Number.isNaN(id) || id <= 0) {
      console.log('Bad Id');
      return;
    }

    if (Number.isNaN(tradesCount)) {
      console.log('Bad Trades Count');
      return;
    }

    if (tradesCount === null) {
      tradesCount = 0;
    }

    tradesCount++;

    const updated: UpdateItem = {
      id: id,
      tradesCount: tradesCount,
    };

    this.updateItem(updated);
  }

  resetHatsButtonClicked(): void {
    this.currentPage = 76;
    this.hatsBatchSize = 7;
    this.hatStatusLabel = '';
  }

  resetWeaponsButtonClicked(): void {
    this.currentIndex = 1;
    this.weaponsBatchSize = 3;
    this.weaponStatusLabel = '';
  }

  ClearAllFilters() {
    this.textFilters.forEach((c) => c.clearFilter());
    this.checkboxFilters.forEach((c) => c.clearFilters());
  }
}
