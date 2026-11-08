import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { finalize } from 'rxjs';

import { ManualCheckPreset, ManualCheckPresetWrite } from '../../models';
import { ManualCheckService } from '../../services';
import { AuthService } from '../../services/auth/auth.service';
import { groupByItemGroup } from '../../common/item-grouping';

export interface ManualCheckPresetPickerData {
  presets: ManualCheckPreset[];
  gameUrlId: number;
  accessibleGameUrlIds: number[];
  excludedPresetIds: number[];
}

@Component({
  selector: 'steam-manual-check-preset-picker',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>Choose a preset</h2>
    <mat-dialog-content>
      <label class="picker-search">Search presets
        <input name="presetSearch" [ngModel]="search()" (ngModelChange)="search.set($event)" placeholder="Name or group" [disabled]="busy()" />
      </label>
      @for (group of groups(); track group.itemGroupId) {
        <fieldset [disabled]="busy()">
          <legend>{{ group.name }}</legend>
          @for (preset of group.items; track preset.id) {
            <label class="picker-option">
              <input type="radio" name="pickerPreset" [value]="preset.id" [ngModel]="selectedId()" (ngModelChange)="select(preset)" />
              <span><strong>{{ preset.name }}</strong><small>{{ preset.canEdit === false ? 'Global' : 'Personal' }}{{ isAssigned(preset) ? ' · Assigned' : '' }}</small></span>
            </label>
          }
        </fieldset>
      } @empty {
        <p>No matching presets.</p>
      }
      @if (needsPersonalCopy()) {
        <label class="picker-search">Personal preset name
          <input name="personalCopyName" maxlength="100" [(ngModel)]="copyName" [disabled]="busy()" />
        </label>
        <p>Global assignments are managed by admins. You can use this preset once or save your own copy.</p>
      } @else if (selected()?.canEdit === false) {
        <p>Assigning this global preset changes its shared assignments for everyone.</p>
      }
      @if (errorMessage()) { <p role="alert">{{ errorMessage() }}</p> }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="dialogRef.close()" [disabled]="busy()">Cancel</button>
      <button mat-stroked-button type="button" (click)="useOnce()" [disabled]="!selected() || busy()">Use once</button>
      <button mat-flat-button type="button" (click)="assign()"
        [disabled]="!selected() || busy() || (selected() && isAssigned(selected()!)) || (needsPersonalCopy() && !copyName.trim())">
        {{ busy() ? 'Saving…' : needsPersonalCopy() ? 'Save personal copy and assign' : 'Assign to this URL' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host { display: block; }
    mat-dialog-content { display: flex; flex-direction: column; gap: 1rem; }
    fieldset { border: 1px solid #cbd5e1; border-radius: .4rem; }
    .picker-search { display: flex; flex-direction: column; gap: .4rem; }
    .picker-search input { padding: .6rem; }
    .picker-option { display: flex; align-items: center; gap: .6rem; padding: .7rem .3rem; cursor: pointer; }
    .picker-option small { display: block; margin-top: .2rem; color: #64748b; }
  `],
})
export class ManualCheckPresetPickerComponent {
  readonly data = inject<ManualCheckPresetPickerData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<ManualCheckPresetPickerComponent, ManualCheckPreset>>(MatDialogRef);
  private readonly service = inject(ManualCheckService);
  private readonly user = toSignal(inject(AuthService).currentUser$, { initialValue: null });
  private readonly untilDestroyed = takeUntilDestroyed<ManualCheckPreset>();
  readonly search = signal('');
  readonly selectedId = signal<number | null>(null);
  readonly busy = signal(false);
  readonly errorMessage = signal('');
  copyName = '';
  readonly groups = computed(() => {
    const search = this.search().trim().toLowerCase();
    return groupByItemGroup(this.data.presets.filter((preset) =>
      !this.data.excludedPresetIds.includes(preset.id)
      && `${preset.name} ${preset.itemGroupName ?? ''}`.toLowerCase().includes(search)));
  });
  readonly selected = computed(() => this.data.presets.find((preset) => preset.id === this.selectedId()) ?? null);
  readonly needsPersonalCopy = computed(() => this.selected()?.canEdit === false && !this.user()?.isAdmin);

  isAssigned(preset: ManualCheckPreset): boolean {
    return preset.gameUrlIds.includes(this.data.gameUrlId);
  }

  select(preset: ManualCheckPreset): void {
    this.selectedId.set(preset.id);
    this.copyName = preset.name;
    this.errorMessage.set('');
  }

  useOnce(): void {
    const preset = this.selected();
    if (preset && !this.busy() && !this.data.excludedPresetIds.includes(preset.id)) this.dialogRef.close(preset);
  }

  assign(): void {
    const preset = this.selected();
    if (!preset || this.busy() || this.isAssigned(preset) || this.data.excludedPresetIds.includes(preset.id)) return;
    const personalCopy = this.needsPersonalCopy();
    if (personalCopy && (!this.copyName.trim() || this.copyName.trim().length > 100)) return;
    const gameUrlIds = personalCopy
      ? preset.gameUrlIds.filter((id) => this.data.accessibleGameUrlIds.includes(id))
      : preset.gameUrlIds;
    const input: ManualCheckPresetWrite = {
      gameId: preset.gameId,
      gameUrlIds: [...new Set([...gameUrlIds, this.data.gameUrlId])].sort((a, b) => a - b),
      itemGroupId: preset.itemGroupId,
      name: personalCopy ? this.copyName.trim() : preset.name,
      listingLimit: preset.listingLimit,
      priceRange: preset.priceRange ? { ...preset.priceRange } : null,
      cooldownMinutes: preset.cooldownMinutes,
      cooldownSeconds: preset.cooldownSeconds,
      criteria: preset.criteria.map((criterion) => ({ ...criterion })),
    };
    const request = personalCopy ? this.service.createPreset(input)
      : preset.canEdit === false ? this.service.updateGlobalPreset(preset.id, input)
      : this.service.updatePreset(preset.id, input);
    this.busy.set(true);
    this.errorMessage.set('');
    this.dialogRef.disableClose = true;
    request.pipe(this.untilDestroyed, finalize(() => {
      this.busy.set(false);
      this.dialogRef.disableClose = false;
    })).subscribe({
      next: (saved) => this.dialogRef.close(saved),
      error: (error: unknown) => this.errorMessage.set(error instanceof HttpErrorResponse
        ? error.error?.detail ?? 'Unable to assign the preset. Try again.'
        : 'Unable to assign the preset. Try again.'),
    });
  }
}
