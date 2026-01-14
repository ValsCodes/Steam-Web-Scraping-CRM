import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  Output,
  ChangeDetectionStrategy,
} from '@angular/core';

@Component({
  selector: 'app-checkbox-filter',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  template: `
    <div class="activity-filters flex gap-x-2 items-center my-2">
      <label class="mr-2">{{ CheckboxCollectionLabel }}:</label>

      <label
        *ngFor="let filter of CheckboxCollection; trackBy: trackById"
        class="flex items-center gap-1"
      >
        {{ filter.label }}
        <input
          type="checkbox"
          [checked]="filter.checked"
          (change)="toggle(filter.id, $event.target.checked)"
        />
        |
      </label>
    </div>
  `,
})
export class CheckboxesFilterComponent {
  @Input() CheckboxCollection!: {
    id: number;
    label: string;
    checked: boolean;
  }[];

  @Input() CheckboxCollectionLabel!: string;

  @Output() filterChange = new EventEmitter<
    { id: number; label: string; checked: boolean }[]
  >();

  trackById(_: number, item: { id: number }): number {
    return item.id;
  }

  toggle(id: number, checked: boolean): void {
    const next = this.CheckboxCollection.map(f =>
      f.id === id ? { ...f, checked } : f
    );

    this.filterChange.emit(next);
  }

  clearFilters(): void {
    const cleared = this.CheckboxCollection.map(f => ({
      ...f,
      checked: false,
    }));

    this.filterChange.emit(cleared);
  }
}
