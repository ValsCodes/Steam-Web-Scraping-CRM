// Not done
import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';

@Component({
  selector: 'app-radio-buttons-filter',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="flex gap-x-2 items-center">
      <label class="mr-2">{{ SearchLabel }}</label>
      <input
        type="radio"
        [formControl]="SearchTextBind"
        (input)="onFilterChange()"
        [placeholder]="SearchLabel + '…'"
        class="px-1 py-1 w-[19rem]"
      />
      <!-- <button (click)="clearFilter()" class="button-small-danger">
        Clear Filter
      </button> -->
    </div>
  `,
})
export class RadioButtonsFilterComponent {
  @Input() SearchLabel!: string;
  @Input() SearchTextBind!: FormControl<string | null>;

  @Output() filterChange = new EventEmitter<string>();

  onFilterChange() {
    const val = this.SearchTextBind.value ?? '';
    this.filterChange.emit(val);
  }

  clearFilter() {
    // clear the FormControl
    this.SearchTextBind.setValue('');

    // emit the cleared value
    this.filterChange.emit('');
  }
}
