import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-checkbox-filter',
    imports: [CommonModule, FormsModule],
    template: `
    <div class="activity-filters flex gap-x-2 items-center my-2">
      <label class="mr-2">{{ CheckboxCollectionLabel }}:</label>
      <label
        *ngFor="let filter of CheckboxCollection"
        class="flex items-center gap-1"
      >
        {{ filter.label }}
        <input
          type="checkbox"
          [(ngModel)]="filter.checked"
          (ngModelChange)="onFilterChange()"
        /> |
      </label>
    </div>
  `
})
export class CheckboxesFilterComponent implements OnInit {
  @Input() CheckboxCollectionLabel!: string;
  @Input() CheckboxCollection!: {
    id: number;
    label: string;
    checked: boolean;
  }[];

  @Output() filterChange = new EventEmitter<
    { label: string; checked: boolean }[]
  >();

  ngOnInit(): void {
    this.filterChange.emit(this.CheckboxCollection);
  }

  onFilterChange(): void {
    this.filterChange.emit(this.CheckboxCollection);
  }

  clearFilters(): void {
    this.CheckboxCollection.forEach((f) => (f.checked = false));
    this.filterChange.emit(this.CheckboxCollection);
  }
}
