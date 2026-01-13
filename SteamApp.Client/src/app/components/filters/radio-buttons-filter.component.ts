import { CommonModule } from '@angular/common';
import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Subscription, debounceTime, distinctUntilChanged } from 'rxjs';

export interface RadioOption {
  id: string;
  label: string;
}

@Component({
    selector: 'app-radio-buttons-filter',
    imports: [CommonModule, ReactiveFormsModule],
    template: `
    <div class="flex items-center gap-x-4">
      <span class="font-medium">{{ label }}:</span>
      <ng-container *ngFor="let opt of options; trackBy: trackById">
        <label class="flex items-center gap-1">
          <input type="radio" [value]="opt.id" [formControl]="control" />
          {{ opt.label }}
        </label>
      </ng-container>
    </div>
  `
})
export class RadioButtonsFilterComponent implements OnInit, OnDestroy {
  @Input() label = '';

  @Input() options: RadioOption[] = [];

  @Input() control!: FormControl<string | null>;

  @Output() selectionChange = new EventEmitter<string>();

  private sub!: Subscription;

  ngOnInit() {
    this.sub = this.control.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((v) => this.selectionChange.emit(v ?? ''));
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  clear() {
    this.control.setValue(null);
    this.selectionChange.emit('');
  }

  trackById(_: number, opt: RadioOption) {
    return opt.id;
  }
}
