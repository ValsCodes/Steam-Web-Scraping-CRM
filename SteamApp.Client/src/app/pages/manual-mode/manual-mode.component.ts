import {
  Component,
  OnInit,
  OnDestroy,
  QueryList,
  ViewChildren,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { CONSTANTS } from '../../common/constants';
import { ItemService } from '../../services/item/item.service';
import { ChangeDetectorRef } from '@angular/core';

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
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CopyLinkComponent,
    CheckboxesFilterComponent,
    TextFilterComponent,
  ],
  templateUrl: './manual-mode.component.html',
  styleUrl: './manual-mode.component.scss',
})

export class ManualModeComponent implements OnInit, OnDestroy {
  @ViewChildren(TextFilterComponent)
  textFilters!: QueryList<TextFilterComponent>;

  @ViewChildren(CheckboxesFilterComponent)
  checkboxFilters!: QueryList<CheckboxesFilterComponent>;

  private readonly destroy$ = new Subject<void>();

  searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  classFilters = classFiltersCollection;
  slotFilters = slotFiltersCollection;

  private readonly constants = CONSTANTS;

  private readonly hatUrlPartial: string =
    this.constants.SEARCH_URL_PARTIAL +
    this.constants.PRODUCT_QUERY_PARAMS_EXTENDED;

  readonly weaponUrlPartial: string =
    this.constants.LISTING_URL_PARTIAL + this.constants.WEAPON_URL_QUERY_PARAMS;

  weaponsCollection: Item[] = [];

  hatsBatchSize = 7;
  weaponsBatchSize = 3;

  currentPage = 76;
  currentIndex = 1;

  hatStatusLabel = '';
  weaponStatusLabel = '';

  constructor(
    private readonly itemService: ItemService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadWeaponsWithFilters(
      this.classFilters,
      this.slotFilters,
      this.searchByNameFilterControl.value
    );
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onTextFilterChanged(value: string): void {
    this.searchByNameFilterControl.setValue(value, { emitEvent: false });

    this.loadWeaponsWithFilters(this.classFilters, this.slotFilters, value);
  }

  trackById(index: number, item: Item): number {
    return item.id;
  }

  startHatsBatch(): void {
    const toPage = this.currentPage + this.hatsBatchSize;
    let blocked = false;

    for (; this.currentPage < toPage; this.currentPage++) {
      const url = `${this.hatUrlPartial}p${this.currentPage}_price_asc`;
      const newWindow = window.open(url, '_blank');

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
      alert('Pop-ups were blocked. Please enable pop-ups for this website.');
    }
  }

  startWeaponsBatch(): void {
    const toIndex = this.currentIndex + this.weaponsBatchSize;
    let blocked = false;

    for (; this.currentIndex < toIndex; this.currentIndex++) {
      const weapon = this.weaponsCollection[this.currentIndex - 1];
      if (!weapon) {
        this.weaponStatusLabel = 'No More Weapons!';
        return;
      }

      const newWindow = window.open(
        `${this.weaponUrlPartial}${weapon.name}`,
        '_blank'
      );

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
      alert('Pop-ups were blocked. Please enable pop-ups for this website.');
    }
  }

  showAllWeaponsButtonClicked(): void {
    let blocked = false;

    for (const weapon of this.weaponsCollection) {
      const newWindow = window.open(
        `${this.weaponUrlPartial}${weapon.name}`,
        '_blank'
      );

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
      alert('Pop-ups were blocked. Please enable pop-ups for this website.');
    }
  }

  loadWeapons(nameFilter: string = this.searchByNameFilterControl.value): void {
    const classIds = this.classFilters
      .filter((x) => x.checked)
      .map((x) => x.id);
    const slotIds = this.slotFilters.filter((x) => x.checked).map((x) => x.id);

    this.itemService
      .getItems(classIds, slotIds, nameFilter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.weaponsCollection = data.filter(
            (item) => item.isActive && item.isWeapon
          );
        },
        error: (err) => {
          console.error('Error Loading Items:', err);
        },
      });
  }

  private loadWeaponsWithFilters(
    classFilters: typeof this.classFilters,
    slotFilters: typeof this.slotFilters,
    nameFilter: string
  ): void {
    const classIds = classFilters.filter((f) => f.checked).map((f) => f.id);
    const slotIds = slotFilters.filter((f) => f.checked).map((f) => f.id);

    this.itemService
      .getItems(classIds, slotIds, nameFilter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.weaponsCollection = data
            .filter((i) => i.isActive === true && i.isWeapon === true)
            .map((i) => ({
              ...i,
              isActive: i.isActive ?? false,
              isWeapon: i.isWeapon ?? false,
            }));

          this.cdr.markForCheck();
        },
        error: (err) => console.error(err),
      });
  }

  onClassFiltersChanged(
    next: { id: number; label: string; checked: boolean }[]
  ): void {
    this.classFilters = next;

    this.loadWeaponsWithFilters(
      next,
      this.slotFilters,
      this.searchByNameFilterControl.value
    );
  }

  onSlotFiltersChanged(
    next: { id: number; label: string; checked: boolean }[]
  ): void {
    this.slotFilters = next;

    this.loadWeaponsWithFilters(
      this.classFilters,
      next,
      this.searchByNameFilterControl.value
    );
  }

addStockCount(id: number): void {
  const item = this.weaponsCollection.find(w => w.id === id);
  if (!item) { return; }

  const nextStock = (item.currentStock ?? 0) + 1;

  // ✅ optimistic UI update
  this.weaponsCollection = this.weaponsCollection.map(w =>
    w.id === id ? { ...w, currentStock: nextStock } : w
  );

  this.cdr.markForCheck();

  // ✅ sync with server
  this.updateItemRemote({ id, currentStock: nextStock });
}
removeStockCount(id: number): void {
  const item = this.weaponsCollection.find(w => w.id === id);
  if (!item) { return; }

  const nextStock = (item.currentStock ?? 0) - 1;

  this.weaponsCollection = this.weaponsCollection.map(w =>
    w.id === id ? { ...w, currentStock: nextStock } : w
  );

  this.cdr.markForCheck();

  this.updateItemRemote({ id, currentStock: nextStock });
}

addTradesCount(id: number): void {
  const item = this.weaponsCollection.find(w => w.id === id);
  if (!item) { return; }

  const nextTrades = (item.tradesCount ?? 0) + 1;

  this.weaponsCollection = this.weaponsCollection.map(w =>
    w.id === id ? { ...w, tradesCount: nextTrades } : w
  );

  this.cdr.markForCheck();

  this.updateItemRemote({ id, tradesCount: nextTrades });
}

  private updateItemRemote(update: UpdateItem): void {
  this.itemService
    .updateItem(update)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      error: err => {
        console.error(err);
        // optional rollback logic here
      }
    });
}

  private updateItemLocal(id: number, patch: Partial<UpdateItem>): void {
    if (!id || id <= 0) {
      return;
    }

    const update: UpdateItem = { id, ...patch };

    this.itemService
      .updateItem({ id, ...patch })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.weaponsCollection = this.weaponsCollection.map((item) =>
            item.id === id ? { ...item, ...patch } : item
          );
        },
        error: (err) => console.error('Error updating item:', err),
      });
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

  ClearAllFilters(): void {
    this.textFilters.forEach((c) => c.clearFilter());
    this.checkboxFilters.forEach((c) => c.clearFilters());
  }
}
