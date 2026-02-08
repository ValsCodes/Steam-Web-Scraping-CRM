import { CommonModule }                     from '@angular/common';
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Subscription }                     from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-number-filter',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="flex gap-x-2 items-center my-2">
      <label class="mr-2">{{ SearchLabel }}</label>
      <input
        type="number"
        [formControl]="SearchNumberBind"
        [placeholder]="SearchLabel + '…'"
        class="px-1 py-1 w-[19rem]"
      />
    </div>
  `,
})
export class NumberFilterComponent implements OnInit, OnDestroy {
  @Input() SearchLabel!: string;
  @Input() SearchNumberBind!: FormControl<number | null>;

  @Output() filterChange = new EventEmitter<number>();

  private sub!: Subscription;

  ngOnInit() {
    this.sub = this.SearchNumberBind.valueChanges
      .pipe(debounceTime(500), distinctUntilChanged())
      .subscribe((val) => {
        const s = val ?? 0;
        this.filterChange.emit(s);
      });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  clearFilter() {
    this.SearchNumberBind.setValue(null);
    this.filterChange.emit(0);
  }
}
