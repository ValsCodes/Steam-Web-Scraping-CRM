import { Component, OnInit } from '@angular/core';
import { FormControl, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CONSTANTS } from '../../common/constants';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ReactiveFormsModule } from '@angular/forms';
import { ItemService } from '../../services/item/item.service';
import { ItemSearchComponent } from '../../components/item-search/item-search.component';

@Component({
  selector: 'steam-manual-mode',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    ReactiveFormsModule,
    ItemSearchComponent,
  ],
  templateUrl: './manual-mode.component.html',
  styleUrl: './manual-mode.component.scss',
})
export class ManualModeComponent implements OnInit {
  searchControl = new FormControl<string>('', { nonNullable: true });
  searchByItemName: string = '';

  selectedClasses: number[] = [];

  selectedSlots: number[] = [];

  private _constants = CONSTANTS;

  private readonly _hatUrlPartial: string =
    this._constants.SEARCH_URL_PARTIAL +
    this._constants.PRODUCT_QUERY_PARAMS_EXTENDED;

  private currentUrl: string = this._hatUrlPartial;

  public readonly weaponUrlPartial: string =
    this._constants.LISTING_URL_PARTIAL +
    this._constants.WEAPON_URL_QUERY_PARAMS;

  public weaponNames: string[] = [];

  public isHatsSearch: boolean = true;
  public batchSize: number = 7;
  public currentPage: number = 76;
  public statusLabel: string = '';

  constructor(private itemService: ItemService) {}

  ngOnInit(): void {
    this.loadWeapons();

    this.searchControl.valueChanges
      .pipe(debounceTime(500), distinctUntilChanged())
      .subscribe((value) => {
        this.searchByItemName = value;
        this.loadWeapons();
      });
  }

  onClassesChanged(newClasses: number[]) {
    this.selectedClasses = newClasses;
  }

  onSlotsChanged(newSlots: number[]) {
    this.selectedSlots = newSlots;
  }

  onSearchByNameChange(value: string) {
    this.searchByItemName = value;

    this.loadWeapons();
  }

  startButtonClicked(): void {
    let toPage = this.currentPage + this.batchSize;
    let blocked = false;

    for (; this.currentPage < toPage; this.currentPage++) {
      let url = this.isHatsSearch
        ? `${this.currentUrl}p${this.currentPage}_price_asc`
        : this.weaponNames.length >= this.currentPage
        ? `${this.currentUrl}${this.weaponNames[this.currentPage - 1]}`
        : null;

      if (url == null) {
        this.statusLabel = 'No More Weapons!';
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

    this.statusLabel = blocked ? 'Bad Batch!' : 'Successful Batch!';

    if (blocked) {
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    }
  }

  onClassCheckboxChange(event: Event, classId: number) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      if (!this.selectedClasses.includes(classId)) {
        this.selectedClasses.push(classId);
      }
    } else {
      this.selectedClasses = this.selectedClasses.filter(
        (id) => id !== classId
      );
    }

    this.loadWeapons();
  }

  onSlotCheckboxChange(event: Event, slotId: number) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      if (!this.selectedSlots.includes(slotId)) {
        this.selectedSlots.push(slotId);
      }
    } else {
      this.selectedSlots = this.selectedSlots.filter((id) => id !== slotId);
    }

    this.loadWeapons();
  }

  loadWeapons = (nameFilter: string = this.searchByItemName): void => {
    this.itemService
      .getItems(this.selectedClasses, this.selectedSlots, nameFilter)
      .subscribe({
        next: (data) => {
          const filteredData = data
            .filter((item) => item.isActive && item.isWeapon)
            .map((item) => item.name);

          this.weaponNames.length = 0;
          this.weaponNames.push(...filteredData);
        },
        error: (err) => {
          console.log('Error Loading Items: ' + err);
        },
      });
  };

  hatsButtonClicked(): void {
    if (!this.isHatsSearch) {
      this.currentUrl = this._hatUrlPartial;
      this.isHatsSearch = true;

      this.resetButtonClicked();
    }
  }

  weaponsButtonClicked(): void {
    if (this.isHatsSearch) {
      this.currentUrl = this.weaponUrlPartial;
      this.isHatsSearch = false;
      this.resetButtonClicked();
    }
  }

  showAllWeaponsButtonClicked(): void {
    let blocked = false;
    let weaponNamesLength = this.weaponNames.length;

    for (let index = 0; index < weaponNamesLength; index++) {
      let url = `${this.currentUrl}${this.weaponNames[index]}`;
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

    this.statusLabel = blocked ? 'Bad Batch!' : 'Successful Batch!';

    if (blocked) {
      alert(
        'Pop-ups were blocked. Please enable pop-ups for this website to open all pages.'
      );
    }
  }

  resetButtonClicked(): void {
    if (this.isHatsSearch) {
      this.currentPage = 76;
      this.batchSize = 7;
    } else {
      this.currentPage = 1;
      this.batchSize = 3;
    }

    this.statusLabel = '';
  }
}
