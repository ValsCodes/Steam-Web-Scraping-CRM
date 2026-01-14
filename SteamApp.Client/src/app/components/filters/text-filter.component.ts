import { CommonModule }                     from '@angular/common';
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Subscription }                     from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-text-filter',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="flex gap-x-2 items-center my-2">
      <label class="mr-2">{{ SearchLabel }}</label>
      <input
        type="text"
        [formControl]="SearchTextBind"
        [placeholder]="SearchLabel + '…'"
        class="px-1 py-1 w-[19rem]"
      />
    </div>
  `,
})
export class TextFilterComponent implements OnInit, OnDestroy {
  @Input() SearchLabel!: string;
  @Input() SearchTextBind!: FormControl<string | null>;

  @Output() filterChange = new EventEmitter<string>();

  private sub!: Subscription;

  ngOnInit() {
    this.sub = this.SearchTextBind.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(val => {
        const s = val ?? '';
        this.filterChange.emit(s);
      });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  clearFilter() {
    this.SearchTextBind.setValue('');
    this.filterChange.emit('');
  }
}
