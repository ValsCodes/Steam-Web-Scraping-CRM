import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Output,
  input
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

type NamedTag = { name: string | null };

@Component({
  selector: 'app-tag-filter-select',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex gap-x-2 items-start">
      <label class="pt-2">{{ label() }}</label>

      <select
        class="px-2 py-2 w-[15rem] ml-2 rounded-md border border-black focus:border-gray-600 focus:outline-none"
        [formControl]="control()"
        [disabled]="disabled() ?? (options()?.length ?? 0) === 0"
        (change)="onSelectChanged()"
      >
        <option [ngValue]="null">{{ placeholder() }}</option>

        <option *ngFor="let tag of options()" [ngValue]="tag">
          {{ display(tag) }}
        </option>
      </select>

      <div class="flex flex-wrap gap-2 ml-2">
        <div
          *ngFor="let f of filters()"
          class="flex items-center gap-1 px-2 py-1 rounded-md border border-black"
        >
          <span>{{ f }}</span>
          <button type="button" (click)="removeClicked.emit(f)">×</button>
        </div>
      </div>
    </div>
  `,
})
export class TagFilterSelectComponent<TTag extends NamedTag> {
  readonly control = input.required<FormControl<TTag | null>>();
  readonly options = input.required<readonly TTag[]>();
  readonly filters = input.required<readonly string[]>();

  readonly label = input('Tags');
  readonly placeholder = input('-- select tag --');
  readonly disabled = input<boolean | null>(null);

  readonly displayWith = input<((tag: TTag) => string) | null>(null);

  @Output() tagSelected = new EventEmitter<TTag>();
  @Output() removeClicked = new EventEmitter<string>();

  onSelectChanged(): void {
    const tag = this.control().value;
    if (!tag) {
      return;
    }

    this.tagSelected.emit(tag);

    // Match your current UX: clear selection after picking
    this.control().setValue(null, { emitEvent: false });
  }

  display(tag: TTag): string {
    const displayWith = this.displayWith();
    if (displayWith) {
      return displayWith(tag);
    }

    return tag.name ?? '';
  }
}