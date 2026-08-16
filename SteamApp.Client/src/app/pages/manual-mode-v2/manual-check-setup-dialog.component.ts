import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { finalize, timeout, TimeoutError } from 'rxjs';

import {
  ManualCheckCriterion,
  ManualCheckMatchMode,
  ManualCheckPreset,
  ManualCheckPresetWrite,
} from '../../models';
import { ManualCheckService } from '../../services';

export interface ManualCheckSetupDialogData {
  gameId: number;
  gameName: string;
  gameUrlName: string;
  preselectedPresetId?: number | null;
}

@Component({
  selector: 'steam-manual-check-setup-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Automated check</h2>
    <mat-dialog-content class="manual-check-dialog">
      <p class="manual-check-dialog__source">
        {{ data.gameName }} · {{ data.gameUrlName }}
      </p>
      <p class="manual-check-dialog__intro">
        Choose a saved preset or create one, review its criteria, then start the check.
      </p>

      @if (loading) {
        <div class="manual-check-dialog__loading" role="status" aria-live="polite">
          <span class="manual-check-dialog__spinner" aria-hidden="true"></span>
          <span>Loading presets…</span>
        </div>
      } @else if (loadError) {
        <div class="manual-check-dialog__callout manual-check-dialog__callout--error" role="alert">
          <strong>Presets could not be loaded</strong>
          <span>{{ errorMessage }}</span>
          <button mat-stroked-button type="button" (click)="loadPresets()">Try again</button>
        </div>
      } @else {
        @if (presets.length === 0) {
          <div class="manual-check-dialog__callout">
            <strong>No presets for this game yet</strong>
            <span>Complete the fields below to create the first preset.</span>
          </div>
        }

        <label>
          <span>Saved preset</span>
          <select
            name="manualCheckPreset"
            [(ngModel)]="selectedPresetId"
            (ngModelChange)="selectPreset($event)">
            <option [ngValue]="null">Create new preset</option>
            @for (preset of presets; track preset.id) {
              <option [ngValue]="preset.id">{{ preset.name }}</option>
            }
          </select>
        </label>

        <label>
          <span>Preset name</span>
          <input
            name="manualCheckPresetName"
            maxlength="100"
            [(ngModel)]="name"
            (ngModelChange)="markDirty()"
            placeholder="Preset name" />
        </label>

        <fieldset>
          <legend>Match mode</legend>
          <label class="manual-check-dialog__radio">
            <input
              type="radio"
              name="manualCheckMatchMode"
              value="Any"
              [(ngModel)]="matchMode"
              (ngModelChange)="markDirty()" />
            Any criterion
          </label>
          <label class="manual-check-dialog__radio">
            <input
              type="radio"
              name="manualCheckMatchMode"
              value="All"
              [(ngModel)]="matchMode"
              (ngModelChange)="markDirty()" />
            All criteria in the same asset
          </label>
        </fieldset>

        <section class="manual-check-dialog__listing-limit" aria-labelledby="manualCheckListingLimitLabel">
          <div>
            <strong id="manualCheckListingLimitLabel">Listings to check</strong>
            <p>Cheapest listings are checked first after Steam results are loaded.</p>
          </div>
          <div class="manual-check-dialog__listing-limit-controls">
            <button
              mat-stroked-button
              type="button"
              [class.manual-check-dialog__quick-option--active]="listingLimit === 10"
              (click)="setListingLimit(10)">
              Top 10
            </button>
            <button
              mat-stroked-button
              type="button"
              [class.manual-check-dialog__quick-option--active]="listingLimit === 20"
              (click)="setListingLimit(20)">
              Top 20
            </button>
            <label>
              <span>Custom Top X</span>
              <input
                type="number"
                name="manualCheckListingLimit"
                min="1"
                max="2147483647"
                step="1"
                [(ngModel)]="listingLimit"
                (ngModelChange)="markDirty()"
                [attr.aria-invalid]="!isListingLimitValid" />
            </label>
          </div>
          @if (!isListingLimitValid) {
            <p class="manual-check-dialog__validation" role="alert">Enter a positive whole number.</p>
          }
        </section>

        <div class="manual-check-dialog__criteria-heading">
          <strong>Criteria</strong>
          <button
            mat-stroked-button
            type="button"
            (click)="addCriterion()"
            [disabled]="criteria.length >= 25">
            Add criterion
          </button>
        </div>

        @for (criterion of criteria; track $index; let i = $index) {
          <div class="manual-check-dialog__criterion">
            <span class="manual-check-dialog__criterion-number">{{ i + 1 }}</span>
            <input
              [name]="'manualCheckName' + i"
              maxlength="200"
              [(ngModel)]="criterion.nameContains"
              (ngModelChange)="markDirty()"
              placeholder="Description name contains" />
            <input
              [name]="'manualCheckValue' + i"
              maxlength="200"
              [(ngModel)]="criterion.valueContains"
              (ngModelChange)="markDirty()"
              placeholder="Description value contains" />
            <button
              mat-button
              type="button"
              color="warn"
              (click)="removeCriterion(i)"
              [disabled]="criteria.length === 1">
              Remove
            </button>
          </div>
        }

        @if (errorMessage) {
          <div class="manual-check-dialog__callout manual-check-dialog__callout--error" role="alert">
            <strong>Could not save the preset</strong>
            <span>{{ errorMessage }}</span>
          </div>
        }
        @if (successMessage) {
          <div class="manual-check-dialog__callout manual-check-dialog__callout--success" role="status">
            {{ successMessage }}
          </div>
        }
        @if (dirty && selectedPresetId !== null) {
          <p class="manual-check-dialog__hint">Your changes will be saved when you start the check.</p>
        }
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      @if (!loading && !loadError) {
        <button
          mat-button
          type="button"
          color="warn"
          (click)="deletePreset()"
          [disabled]="selectedPresetId === null || busy">
          Delete
        </button>
        <button mat-stroked-button type="button" (click)="newPreset()" [disabled]="busy">
          Create new
        </button>
        <button
          mat-stroked-button
          type="button"
          (click)="savePreset()"
          [disabled]="!isDraftValid || !dirty || busy">
          Save only
        </button>
      }
      <span class="manual-check-dialog__spacer"></span>
      <button mat-button type="button" (click)="dialogRef.close()">Cancel</button>
      @if (!loading && !loadError) {
        <button
          mat-flat-button
          type="button"
          (click)="startOrSave()"
          [disabled]="!isDraftValid || busy">
          {{ primaryActionLabel }}
        </button>
      }
    </mat-dialog-actions>
  `,
  styles: [`
    .manual-check-dialog { display: flex; flex-direction: column; gap: 1rem; min-width: min(46rem, 82vw); }
    .manual-check-dialog__source { margin: 0; color: #475569; font-weight: 600; }
    .manual-check-dialog__intro { margin: -.5rem 0 0; color: #64748b; }
    .manual-check-dialog__loading { display: flex; min-height: 10rem; align-items: center; justify-content: center; gap: .75rem; color: #475569; }
    .manual-check-dialog__spinner { width: 1.25rem; height: 1.25rem; border: 2px solid #cbd5e1; border-top-color: #2563eb; border-radius: 999px; animation: manual-check-spin .8s linear infinite; }
    .manual-check-dialog__callout { display: flex; flex-direction: column; align-items: flex-start; gap: .4rem; border: 1px solid #cbd5e1; border-radius: .375rem; background: #f8fafc; padding: .75rem; color: #334155; }
    .manual-check-dialog__callout--error { border-color: #fecaca; background: #fef2f2; color: #991b1b; }
    .manual-check-dialog__callout--success { border-color: #bbf7d0; background: #f0fdf4; color: #166534; }
    .manual-check-dialog label:not(.manual-check-dialog__radio) { display: flex; flex-direction: column; gap: .35rem; }
    .manual-check-dialog select, .manual-check-dialog input { border: 1px solid #cbd5e1; border-radius: .25rem; padding: .55rem .7rem; }
    .manual-check-dialog fieldset { display: flex; gap: 1.25rem; border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-dialog__radio { display: inline-flex; align-items: center; gap: .4rem; }
    .manual-check-dialog__listing-limit { display: flex; flex-direction: column; gap: .65rem; border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-dialog__listing-limit p { margin: .2rem 0 0; color: #64748b; }
    .manual-check-dialog__listing-limit-controls { display: flex; flex-wrap: wrap; align-items: end; gap: .6rem; }
    .manual-check-dialog__listing-limit-controls label { min-width: 9rem; }
    .manual-check-dialog__quick-option--active { border-color: #2563eb; background: #eff6ff; color: #1d4ed8; }
    .manual-check-dialog__validation { color: #b91c1c !important; }
    .manual-check-dialog__criteria-heading { display: flex; align-items: center; justify-content: space-between; }
    .manual-check-dialog__criterion { display: grid; grid-template-columns: 2rem 1fr 1fr auto; gap: .5rem; align-items: center; }
    .manual-check-dialog__criterion-number { color: #64748b; text-align: center; }
    .manual-check-dialog__hint { color: #92400e; margin: 0; }
    .manual-check-dialog__spacer { flex: 1; }
    @keyframes manual-check-spin { to { transform: rotate(360deg); } }
    @media (max-width: 700px) { .manual-check-dialog__criterion { grid-template-columns: 2rem 1fr; } }
  `],
})
export class ManualCheckSetupDialogComponent implements OnInit {
  readonly data = inject<ManualCheckSetupDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<ManualCheckSetupDialogComponent, number>>(MatDialogRef);
  private readonly manualCheckService = inject(ManualCheckService);
  private readonly cdr = inject(ChangeDetectorRef);

  presets: ManualCheckPreset[] = [];
  selectedPresetId: number | null = null;
  name = '';
  matchMode: ManualCheckMatchMode = 'Any';
  listingLimit: number | null = 10;
  criteria: ManualCheckCriterion[] = [this.emptyCriterion()];
  loading = true;
  loadError = false;
  busy = false;
  dirty = true;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadPresets();
  }

  loadPresets(): void {
    this.loading = true;
    this.loadError = false;
    this.errorMessage = '';
    this.successMessage = '';

    this.manualCheckService.getPresets(this.data.gameId).pipe(
      timeout({ first: 15000 }),
      finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }),
    ).subscribe({
      next: (presets) => {
        this.presets = presets;
        const preferred = presets.find((x) => x.id === this.data.preselectedPresetId)
          ?? presets[0];
        if (preferred) {
          this.selectedPresetId = preferred.id;
          this.selectPreset(preferred.id);
        } else {
          this.newPreset();
        }
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.loadError = true;
        this.errorMessage = this.getError(error, 'Unable to load presets.');
        this.cdr.markForCheck();
      },
    });
  }

  get isDraftValid(): boolean {
    return this.name.trim().length >= 1
      && this.name.trim().length <= 100
      && this.isListingLimitValid
      && this.criteria.length >= 1
      && this.criteria.length <= 25
      && this.criteria.every((x) =>
        !!x.nameContains?.trim() || !!x.valueContains?.trim());
  }

  get canStart(): boolean {
    return this.selectedPresetId !== null && !this.dirty && this.isDraftValid;
  }

  get isListingLimitValid(): boolean {
    return this.listingLimit !== null
      && Number.isInteger(this.listingLimit)
      && this.listingLimit >= 1
      && this.listingLimit <= 2147483647;
  }

  get primaryActionLabel(): string {
    if (this.selectedPresetId === null) {
      return 'Create & start';
    }
    return this.dirty ? 'Save & start' : 'Start check';
  }

  selectPreset(id: number | null): void {
    if (id === null) {
      this.newPreset();
      return;
    }

    const preset = this.presets.find((x) => x.id === id);
    if (!preset) {
      return;
    }

    this.selectedPresetId = preset.id;
    this.name = preset.name;
    this.matchMode = preset.matchMode;
    this.listingLimit = preset.listingLimit;
    this.criteria = preset.criteria.map((x) => ({ ...x }));
    this.dirty = false;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  newPreset(): void {
    this.selectedPresetId = null;
    this.name = '';
    this.matchMode = 'Any';
    this.listingLimit = 10;
    this.criteria = [this.emptyCriterion()];
    this.dirty = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  markDirty(): void {
    this.dirty = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  setListingLimit(listingLimit: number): void {
    this.listingLimit = listingLimit;
    this.markDirty();
  }

  addCriterion(): void {
    if (this.criteria.length < 25) {
      this.criteria.push(this.emptyCriterion());
      this.markDirty();
    }
  }

  removeCriterion(index: number): void {
    if (this.criteria.length > 1) {
      this.criteria.splice(index, 1);
      this.markDirty();
    }
  }

  savePreset(startAfterSave = false): void {
    if (!this.isDraftValid || this.busy) {
      return;
    }

    this.busy = true;
    this.errorMessage = '';
    this.successMessage = '';
    const input = this.toWriteModel();
    const request = this.selectedPresetId === null
      ? this.manualCheckService.createPreset(input)
      : this.manualCheckService.updatePreset(this.selectedPresetId, input);

    request.pipe(finalize(() => {
      this.busy = false;
      this.cdr.markForCheck();
    })).subscribe({
      next: (saved) => {
        const index = this.presets.findIndex((x) => x.id === saved.id);
        if (index >= 0) {
          this.presets[index] = saved;
        } else {
          this.presets = [...this.presets, saved].sort((a, b) => a.name.localeCompare(b.name));
        }
        this.selectedPresetId = saved.id;
        this.selectPreset(saved.id);
        if (startAfterSave) {
          this.dialogRef.close(saved.id);
          return;
        }
        this.successMessage = `Preset “${saved.name}” saved.`;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.errorMessage = this.getError(error, 'Unable to save the preset.');
        this.cdr.markForCheck();
      },
    });
  }

  deletePreset(): void {
    const id = this.selectedPresetId;
    if (id === null || this.busy || !confirm(`Delete preset “${this.name}”? Historical runs will be retained.`)) {
      return;
    }

    this.busy = true;
    this.manualCheckService.deletePreset(id)
      .pipe(finalize(() => {
        this.busy = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: () => {
          this.presets = this.presets.filter((x) => x.id !== id);
          this.newPreset();
          this.successMessage = 'Preset deleted.';
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.errorMessage = this.getError(error, 'Unable to delete the preset.');
          this.cdr.markForCheck();
        },
      });
  }

  startOrSave(): void {
    if (!this.isDraftValid || this.busy) {
      return;
    }

    if (this.selectedPresetId === null || this.dirty) {
      this.savePreset(true);
      return;
    }

    this.dialogRef.close(this.selectedPresetId);
  }

  private toWriteModel(): ManualCheckPresetWrite {
    return {
      gameId: this.data.gameId,
      name: this.name.trim(),
      matchMode: this.matchMode,
      listingLimit: this.listingLimit!,
      criteria: this.criteria.map((x) => ({
        nameContains: x.nameContains?.trim() || null,
        valueContains: x.valueContains?.trim() || null,
      })),
    };
  }

  private emptyCriterion(): ManualCheckCriterion {
    return { nameContains: null, valueContains: null };
  }

  private getError(error: unknown, fallback: string): string {
    if (error instanceof TimeoutError) {
      return 'Preset loading took too long. Check the API connection and try again.';
    }
    if (error instanceof HttpErrorResponse) {
      const detail = error.error?.detail ?? error.error?.message;
      if (error.status === 429) {
        const retryAfter = error.headers.get('Retry-After');
        return detail ?? (retryAfter
          ? `Too many requests. Try again in ${retryAfter} seconds.`
          : 'Too many requests. Wait briefly and try again.');
      }
      return detail ?? fallback;
    }
    return error instanceof Error ? error.message : fallback;
  }
}
