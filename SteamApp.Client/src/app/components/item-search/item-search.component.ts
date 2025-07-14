import { CommonModule } from '@angular/common';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { classesCollection } from '../../models/enums/class.enum';
import { slotsCollection } from '../../models/enums/slot.enum';

@Component({
  selector: 'steam-item-search',
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule],
  templateUrl: './item-search.component.html',
  styleUrl: './item-search.component.scss',
})
export class ItemSearchComponent {
  @Input() getItems!: () => void;

  @Output() selectedClasses = new EventEmitter<number[]>();
  @Output() selectedSlots = new EventEmitter<number[]>();
  @Output() searchByNameValue = new EventEmitter<string>();

  _selectedSlots: number[] = [];
  _selectedClasses: number[] = [];
  _searchByItemName: string = '';

  classes = classesCollection;
  slots = slotsCollection;

  searchControl = new FormControl<string>('', { nonNullable: true });

  ngOnInit(): void {
    this.getItems();

    this.searchControl.valueChanges
      .pipe(debounceTime(500), distinctUntilChanged())
      .subscribe((value: string) => {
        this._searchByItemName = value;
        this.searchByNameValue.emit(value);
        this.getItems();
      });
  }

  onClassChange(evt: Event, clsId: number) {
    const checked = (evt.target as HTMLInputElement).checked;
    if (checked) this._selectedClasses.push(clsId);
    else
      this._selectedClasses = this._selectedClasses.filter(
        (id) => id !== clsId
      );
    this.selectedClasses.emit(this._selectedClasses);
    this.getItems();
  }

  onSlotChange(evt: Event, slotId: number) {
    const checked = (evt.target as HTMLInputElement).checked;
    if (checked) this._selectedSlots.push(slotId);
    else
      this._selectedSlots = this._selectedSlots.filter((id) => id !== slotId);
    this.selectedSlots.emit(this._selectedSlots);
    this.getItems();
  }

  clearSearchFilter(): void {
    this.searchByNameValue.emit('');
    this.searchControl.reset();
  }

  clearSlotFilters(): void {
    this._selectedSlots.length = 0;

    document
      .querySelectorAll('.slot-filters input[type="checkbox"]')
      .forEach((el: Element) => ((el as HTMLInputElement).checked = false));

    this.selectedSlots.emit([]);
    this.getItems();
  }

  clearClassFilters(): void {
    this._selectedClasses.length = 0;

    document
      .querySelectorAll('.class-filters input[type="checkbox"]')
      .forEach((el: Element) => ((el as HTMLInputElement).checked = false));

    this.selectedClasses.emit([]);
    this.getItems();
  }

  clearAllFilter(): void {
    this._selectedClasses.length = 0;

    document
      .querySelectorAll('.class-filters input[type="checkbox"]')
      .forEach((el: Element) => ((el as HTMLInputElement).checked = false));
    this.selectedClasses.emit([]);

    this._selectedSlots.length = 0;
    document
      .querySelectorAll('.slot-filters input[type="checkbox"]')
      .forEach((el: Element) => ((el as HTMLInputElement).checked = false));

    this.selectedSlots.emit([]);

    this.clearSearchFilter();
  }
}
