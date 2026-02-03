import {
  ChangeDetectionStrategy,
  Component,
  Input,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Observable, Subject, EMPTY, Subscription, isObservable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

type KeyOf<T> = Extract<keyof T, string>;

@Component({
  selector: 'app-combo-box',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="flex py-2 gap-2 items-center">
      <label class="w-24">{{ label }}</label>

      <select class="px-2 py-1 w-full" [formControl]="control">
        <option [ngValue]="null" disabled>{{ placeholder }}</option>

        <option
          *ngFor="let item of (items$ | async) ?? []; trackBy: trackById"
          [ngValue]="item[idKey]"
        >
          {{ item[labelKey] }}
        </option>
      </select>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComboBoxComponent<  TItem extends Record<string, any>,  TValue extends number | string | null,>  implements OnInit, OnDestroy
{
  @Input({ required: true }) control!: FormControl<TValue>;
  @Input({ required: true }) items$!: Observable<readonly TItem[]>;

  @Input({ required: true }) idKey!: KeyOf<TItem>;
  @Input({ required: true }) labelKey!: KeyOf<TItem>;

  @Input() label = '';
  @Input() placeholder = 'Select...';

  @Input() bind$?:
    | Observable<unknown>
    | ((value: TValue) => Observable<unknown>);

  private readonly destroy$ = new Subject<void>();
  private bindSub: Subscription | null = null;

  ngOnInit(): void {
    const bind = this.bind$;
    if (!bind) {
      return;
    }

    if (typeof bind !== 'function') {
      this.bindSub = bind.pipe(takeUntil(this.destroy$)).subscribe();
      return;
    }

    const factory = bind;

    const run = (value: TValue) => {
      if (this.bindSub) {
        this.bindSub.unsubscribe();
        this.bindSub = null;
      }

      this.bindSub = factory(value).pipe(takeUntil(this.destroy$)).subscribe();
    };

    run(this.control.value);

    this.control.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((value) => {
        run(value);
      });
  }

  ngOnDestroy(): void {
    this.bindSub?.unsubscribe();
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackById = (_: number, item: TItem): any => item[this.idKey];
}
