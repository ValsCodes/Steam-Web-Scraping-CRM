import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-checkbox-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="activity-filters flex gap-x-2 items-center">
      <label
        *ngFor="let filter of checkboxCollection"
        class="flex items-center gap-1">
        {{ filter.label }}
        <input
          type="checkbox"
          [(ngModel)]="filter.checked"
          (ngModelChange)="onFilterChange()"/>
      </label>
      <button (click)="clearActivityFilters()" class="button button-small-danger">
        Clear Filter
      </button>
    </div>
  `,
})
export class CheckboxFilterComponent implements OnInit {
  @Input() checkboxCollection!: { id:number; label: string; checked: boolean }[];

  @Output() filterChange = new EventEmitter<
    { label: string; checked: boolean }[]
  >();

  ngOnInit(): void {
    this.filterChange.emit(this.checkboxCollection);
  }

  onFilterChange(): void {
    this.filterChange.emit(this.checkboxCollection);
  }

  clearActivityFilters(): void {
    this.checkboxCollection.forEach((f) => (f.checked = false));
    this.filterChange.emit(this.checkboxCollection);
  }
}
