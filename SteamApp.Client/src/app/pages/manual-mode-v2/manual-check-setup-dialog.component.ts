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
import { finalize, forkJoin, timeout, TimeoutError } from 'rxjs';

import {
  AutomaticQueueBlock,
  AutomaticQueueBlockDraft,
  AutomaticQueueTemplateMode,
  Game,
  GameUrl,
  GameUrlProduct,
  ItemGroup,
  ManualCheckConditionOperator,
  ManualCheckPreset,
  ManualCheckPresetCombinationOperator,
  ManualCheckPresetCombinationWrite,
  ManualCheckPriceRange,
  ManualCheckPriceRangeMode,
  ManualCheckPresetWrite,
  ScrapingModeEnum,
} from '../../models';
import {
  GameService,
  GameUrlProductService,
  GameUrlService,
  ItemGroupService,
  ManualCheckService,
} from '../../services';
import { groupByItemGroup, ItemGroupSection } from '../../common/item-grouping';
import {
  createManualCheckCriterionNode,
  createManualCheckGroupNode,
  flattenManualCheckExpression,
  formatManualCheckExpression,
  MANUAL_CHECK_MAX_CRITERIA,
  MANUAL_CHECK_MAX_GROUPS,
  ManualCheckCriterionNode,
  ManualCheckGroupNode,
  normalizeManualCheckExpressionOperators,
  parseManualCheckExpression,
  validateManualCheckExpression,
} from './manual-check-expression';
import { ManualCheckExpressionEditorComponent } from './manual-check-expression-editor.component';

export interface ManualCheckSetupDialogData {
  gameId?: number;
  gameUrlId?: number;
  gameName?: string;
  gameUrlName?: string;
  gameUrls?: GameUrl[];
  preselectedPresetId?: number | null;
  queueBuilder?: boolean;
  initialBlock?: AutomaticQueueBlock | null;
}

export interface ManualCheckRunDialogResult {
  mode: 'run';
  presetId: number | null;
  presetCombination: ManualCheckPresetCombinationWrite | null;
  bypassCache: boolean;
}

export interface ManualCheckQueueDialogResult {
  mode: 'queue';
  block: AutomaticQueueBlockDraft;
}

export type ManualCheckSetupDialogResult = ManualCheckRunDialogResult | ManualCheckQueueDialogResult;

type ManualCheckPresetMode = 'Single' | 'Combination';

interface ManualCheckPresetCombinationRow {
  presetId: number | null;
  operator: ManualCheckPresetCombinationOperator | null;
}

@Component({
  selector: 'steam-manual-check-setup-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    ManualCheckExpressionEditorComponent,
  ],
  template: `
    <h2 mat-dialog-title>{{ data.queueBuilder ? 'Manual check block' : 'Automated check' }}</h2>
    <mat-dialog-content class="manual-check-dialog">
      <p class="manual-check-dialog__source">
        {{ selectedGameName }} · {{ selectedGameUrlName }}
      </p>
      <p class="manual-check-dialog__intro">
        {{ data.queueBuilder
          ? 'Choose the source, products, and a saved or private check template for this block.'
          : 'Choose a saved preset or create one, review its criteria, then start the check.' }}
      </p>

      @if (data.queueBuilder) {
        <section class="manual-check-dialog__queue-source">
          <label>
            <span>Game</span>
            <select name="queueGame" [(ngModel)]="selectedGameId" (ngModelChange)="queueGameChanged()">
              @for (game of games; track game.id) {
                <option [ngValue]="game.id">{{ game.name || ('Game #' + game.id) }}</option>
              }
            </select>
          </label>
          <label>
            <span>Game URL</span>
            <select name="queueGameUrl" [(ngModel)]="selectedGameUrlId" (ngModelChange)="queueGameUrlChanged()">
              @for (gameUrl of eligibleGameUrls; track gameUrl.id) {
                <option [ngValue]="gameUrl.id">{{ gameUrl.name || ('Game URL #' + gameUrl.id) }}</option>
              }
            </select>
          </label>
        </section>
      }

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

        <fieldset class="manual-check-dialog__mode">
          <legend>Preset mode</legend>
          <label class="manual-check-dialog__radio">
            <input type="radio" name="manualCheckPresetMode" value="Single" [ngModel]="presetMode" (ngModelChange)="setPresetMode($event)" />
            <span>Single preset</span>
          </label>
          <label class="manual-check-dialog__radio">
            <input type="radio" name="manualCheckPresetMode" value="Combination" [ngModel]="presetMode" (ngModelChange)="setPresetMode($event)" [disabled]="presets.length < 2" />
            <span>Combine presets</span>
          </label>
        </fieldset>

        @if (presetMode === 'Single') {
          <label>
            <span>Saved preset</span>
            <select
              name="manualCheckPreset"
              [(ngModel)]="selectedPresetId"
              (ngModelChange)="selectPreset($event)">
              <option [ngValue]="null">{{ data.queueBuilder ? 'Private template' : 'Create new preset' }}</option>
              @for (group of presetGroups; track group.itemGroupId) {
                <optgroup [label]="group.name">
                  @for (preset of group.items; track preset.id) {
                    <option [ngValue]="preset.id">{{ preset.name }}</option>
                  }
                </optgroup>
              }
            </select>
          </label>

          @if (isSelectedPresetReadOnly) {
            <div class="manual-check-dialog__callout" role="status">
              <strong>Global preset</strong>
              <span>This preset is read-only. Run it as-is or clone it to make personal changes.</span>
              @if (selectedPreset?.canClone) {
                <button mat-stroked-button type="button" (click)="cloneSelectedPreset()" [disabled]="busy">
                  Clone to my presets
                </button>
              }
            </div>
          }

          <div
            [class.manual-check-dialog__read-only]="isSelectedPresetReadOnly"
            [attr.inert]="isSelectedPresetReadOnly ? '' : null"
            [attr.aria-disabled]="isSelectedPresetReadOnly">

          <label>
            <span>{{ data.queueBuilder && templateMode === 'PrivateTemplate' ? 'Private template name' : 'Preset name' }}</span>
            <input
              name="manualCheckPresetName"
              maxlength="100"
              [(ngModel)]="name"
              (ngModelChange)="markDirty()"
              placeholder="Preset name" />
          </label>

          @if (!data.queueBuilder) {
            <label>
              <span>Preset group</span>
              <select
                name="manualCheckPresetItemGroup"
                [(ngModel)]="itemGroupId"
                (ngModelChange)="markDirty()">
                <option [ngValue]="null">No group</option>
                @for (itemGroup of itemGroups; track itemGroup.id) {
                  <option [ngValue]="itemGroup.id">{{ itemGroup.name }}</option>
                }
              </select>
            </label>

            <section class="manual-check-dialog__item-group-create">
              <label>
                <span>Create a new group</span>
                <input
                  name="manualCheckNewItemGroupName"
                  maxlength="255"
                  [(ngModel)]="newItemGroupName"
                  placeholder="Group name" />
              </label>
              <button
                mat-stroked-button
                type="button"
                (click)="createItemGroup()"
                [disabled]="!newItemGroupName.trim() || creatingItemGroup || busy">
                {{ creatingItemGroup ? 'Creating...' : 'Create group' }}
              </button>
            </section>

            <details class="manual-check-dialog__assignments">
              <summary>
                <strong>Available for Game URLs</strong>
                <span class="manual-check-dialog__assignment-summary">
                  @for (gameUrl of selectedPresetAssignmentGameUrls; track gameUrl.id) {
                    <span class="manual-check-dialog__assignment-chip">
                      {{ gameUrl.name || ('Game URL #' + gameUrl.id) }}{{ gameUrl.isActive ? '' : ' (inactive)' }}
                    </span>
                  } @empty {
                    <span class="manual-check-dialog__assignment-empty">No URLs selected</span>
                  }
                </span>
              </summary>
              <fieldset class="manual-check-dialog__assignment-options">
                <legend>Select Game URLs</legend>
                @for (gameUrl of presetAssignmentGameUrls; track gameUrl.id) {
                  <label class="manual-check-dialog__radio">
                    <input
                      type="checkbox"
                      [checked]="selectedPresetGameUrlIds.has(gameUrl.id)"
                      (change)="setPresetGameUrlAssigned(gameUrl.id, $any($event.target).checked)" />
                    <span>{{ gameUrl.name || ('Game URL #' + gameUrl.id) }}{{ gameUrl.isActive ? '' : ' (inactive)' }}</span>
                  </label>
                }
                @if (selectedPresetGameUrlIds.size === 0) {
                  <p class="manual-check-dialog__validation" role="alert">Assign the preset to at least one Manual Batch URL.</p>
                }
              </fieldset>
            </details>
          }
          </div>
        } @else {
          <section class="manual-check-dialog__combination" aria-labelledby="manualCheckCombinationLabel">
            <div>
              <strong id="manualCheckCombinationLabel">Preset combination</strong>
              <p>Each preset keeps its own criteria. The combined result is evaluated against the same listing asset.</p>
            </div>
            @for (row of combinationRows; track $index; let index = $index) {
              <div class="manual-check-dialog__combination-row">
                @if (index > 0) {
                  <select [name]="'combinationOperator' + index" [(ngModel)]="row.operator" (ngModelChange)="combinationChanged()" aria-label="Combination operator">
                    <option value="And">AND</option>
                    <option value="Or">OR</option>
                  </select>
                } @else {
                  <strong>Start with</strong>
                }
                <select [name]="'combinationPreset' + index" [(ngModel)]="row.presetId" (ngModelChange)="combinationChanged()" aria-label="Combined preset">
                  @for (group of presetGroups; track group.itemGroupId) {
                    <optgroup [label]="group.name">
                      @for (preset of group.items; track preset.id) {
                        <option [ngValue]="preset.id">{{ preset.name }}</option>
                      }
                    </optgroup>
                  }
                </select>
                <button mat-button type="button" (click)="moveCombinationRow(index, -1)" [disabled]="index === 0">Up</button>
                <button mat-button type="button" (click)="moveCombinationRow(index, 1)" [disabled]="index === combinationRows.length - 1">Down</button>
                <button mat-button type="button" color="warn" (click)="removeCombinationRow(index)" [disabled]="combinationRows.length <= 2">Remove</button>
              </div>
            }
            <button mat-stroked-button type="button" (click)="addCombinationRow()" [disabled]="combinationRows.length >= combinationLimit || combinationRows.length >= presets.length">Add preset</button>
            <div class="manual-check-dialog__expression-preview" aria-live="polite">
              <strong>Combination preview</strong>
              <code>{{ combinationPreview }}</code>
              <small>Evaluation is strictly left-to-right. Top-level parentheses are not supported.</small>
            </div>
            @if (!combinationValidation.isValid) {
              <p class="manual-check-dialog__validation" role="alert">{{ combinationValidation.message }}</p>
            }
          </section>
        }

        <div
          [class.manual-check-dialog__read-only]="isSelectedPresetReadOnly"
          [attr.inert]="isSelectedPresetReadOnly ? '' : null"
          [attr.aria-disabled]="isSelectedPresetReadOnly">
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

        <section class="manual-check-dialog__price-check" aria-labelledby="manualCheckPriceCheckLabel">
          <div>
            <strong id="manualCheckPriceCheckLabel">Price check</strong>
            <p>Compare the cheapest checked Steam listing, including fees, in EUR.</p>
          </div>
          <div class="manual-check-dialog__price-check-controls">
            <label>
              <span>Range</span>
              <select
                name="manualCheckPriceRangeMode"
                [(ngModel)]="priceRangeMode"
                (ngModelChange)="priceRangeChanged()">
                <option value="Any">Any price</option>
                <option value="Above">Above (&gt;)</option>
                <option value="Between">Between (inclusive)</option>
                <option value="Below">Below (&lt;)</option>
              </select>
            </label>
            @if (priceRangeMode === 'Above' || priceRangeMode === 'Between') {
              <label>
                <span>{{ priceRangeMode === 'Above' ? 'Above €' : 'Minimum €' }}</span>
                <input
                  type="number"
                  name="manualCheckMinimumPrice"
                  min="0"
                  step="0.01"
                  [(ngModel)]="minimumPriceEuros"
                  (ngModelChange)="priceRangeChanged()" />
              </label>
            }
            @if (priceRangeMode === 'Below' || priceRangeMode === 'Between') {
              <label>
                <span>{{ priceRangeMode === 'Below' ? 'Below €' : 'Maximum €' }}</span>
                <input
                  type="number"
                  name="manualCheckMaximumPrice"
                  min="0"
                  step="0.01"
                  [(ngModel)]="maximumPriceEuros"
                  (ngModelChange)="priceRangeChanged()" />
              </label>
            }
          </div>
          @if (!isPriceRangeValid) {
            <p class="manual-check-dialog__validation" role="alert">
              Enter non-negative EUR values with no more than two decimals, and keep the minimum at or below the maximum.
            </p>
          }
        </section>

        <section class="manual-check-dialog__cooldown" aria-labelledby="manualCheckCooldownLabel">
          <label class="manual-check-dialog__radio">
            <input
              type="checkbox"
              name="manualCheckCustomCooldown"
              [ngModel]="customCooldown"
              (ngModelChange)="setCustomCooldown($event)" />
            <span>
              <strong id="manualCheckCooldownLabel">Custom cooldown between product checks</strong><br />
              <small>When disabled, the server default is used (currently 3 seconds).</small>
            </span>
          </label>
          @if (customCooldown) {
            <div class="manual-check-dialog__cooldown-controls">
              <label>
                <span>Minutes</span>
                <input
                  type="number"
                  name="manualCheckCooldownMinutes"
                  min="0"
                  max="59"
                  step="1"
                  [(ngModel)]="cooldownMinutes"
                  (ngModelChange)="markDirty()" />
              </label>
              <label>
                <span>Seconds</span>
                <input
                  type="number"
                  name="manualCheckCooldownSeconds"
                  min="0"
                  max="59"
                  step="1"
                  [(ngModel)]="cooldownSeconds"
                  (ngModelChange)="markDirty()" />
              </label>
            </div>
            @if (!isCooldownValid) {
              <p class="manual-check-dialog__validation" role="alert">
                Enter whole minutes and seconds between 0 and 59.
              </p>
            }
          }
        </section>
        </div>

        <fieldset>
          <legend>Steam response cache</legend>
          <label class="manual-check-dialog__radio">
            <input
              type="checkbox"
              name="manualCheckBypassCache"
              [(ngModel)]="bypassCache"
              aria-describedby="manualCheckBypassCacheDescription" />
            <span>
              <strong>Refresh Steam data</strong><br />
              <small id="manualCheckBypassCacheDescription">
                Bypass existing cached responses and replace them. Otherwise responses are reused for up to 20 minutes.
              </small>
            </span>
          </label>
        </fieldset>

        @if (data.queueBuilder) {
          <fieldset class="manual-check-dialog__products">
            <legend>Products</legend>
            <label class="manual-check-dialog__radio">
              <input type="radio" name="queueProducts" value="all" [(ngModel)]="productSelectionMode" />
              <span>All active products (default)</span>
            </label>
            <label class="manual-check-dialog__radio">
              <input type="radio" name="queueProducts" value="selected" [(ngModel)]="productSelectionMode" />
              <span>Select products</span>
            </label>
            @if (productSelectionMode === 'selected') {
              <input name="queueProductSearch" [(ngModel)]="productSearch" placeholder="Filter products" />
              <div class="manual-check-dialog__product-list">
                @for (product of filteredProducts; track product.productId) {
                  <label class="manual-check-dialog__radio">
                    <input
                      type="checkbox"
                      [checked]="selectedProductIds.has(product.productId)"
                      (change)="setProductSelected(product.productId, $any($event.target).checked)" />
                    <span>{{ product.productName || ('Product #' + product.productId) }}</span>
                  </label>
                }
              </div>
              <small>{{ selectedProductIds.size }} product(s) selected</small>
            }
          </fieldset>
        }

        @if (presetMode === 'Single') {
          <div
            [class.manual-check-dialog__read-only]="isSelectedPresetReadOnly"
            [attr.inert]="isSelectedPresetReadOnly ? '' : null"
            [attr.aria-disabled]="isSelectedPresetReadOnly">
          <steam-manual-check-expression-editor
            [root]="expressionRoot"
            [operators]="conditionOperators"
            (expressionChange)="markDirty()">
          </steam-manual-check-expression-editor>

          <section class="manual-check-dialog__expression-preview" aria-live="polite">
            <strong>Expression preview</strong>
            <code>{{ expressionPreview }}</code>
            <small>Parentheses define groups. Evaluation is strictly left-to-right inside each group.</small>
          </section>

          @if (!expressionValidation.isValid) {
            <p class="manual-check-dialog__validation" role="alert">
              Complete every criterion and operator, populate empty groups, and keep the expression within {{ criterionLimit }} criteria and {{ groupLimit }} groups.
            </p>
          }
          </div>
        }

        @if (errorMessage) {
          <div class="manual-check-dialog__callout manual-check-dialog__callout--error" role="alert">
            <strong>{{ presetMode === 'Combination' ? 'Could not use the combination' : 'Could not save the preset' }}</strong>
            <span>{{ errorMessage }}</span>
          </div>
        }
        @if (successMessage) {
          <div class="manual-check-dialog__callout manual-check-dialog__callout--success" role="status">
            {{ successMessage }}
          </div>
        }
        @if (presetMode === 'Single' && dirty && selectedPresetId !== null) {
          <p class="manual-check-dialog__hint">Your changes will be saved when you start the check.</p>
        }
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      @if (!loading && !loadError && !data.queueBuilder && presetMode === 'Single') {
        <button
          mat-button
          type="button"
          color="warn"
          (click)="deletePreset()"
          [disabled]="selectedPresetId === null || isSelectedPresetReadOnly || busy || creatingItemGroup">
          Delete
        </button>
        <button mat-stroked-button type="button" (click)="newPreset()" [disabled]="busy || creatingItemGroup">
          Create new
        </button>
        <button
          mat-stroked-button
          type="button"
          (click)="savePreset()"
          [disabled]="isSelectedPresetReadOnly || !isDraftValid || !dirty || busy || creatingItemGroup">
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
          [disabled]="!isDraftValid || !isQueueSelectionValid || busy || creatingItemGroup">
          {{ primaryActionLabel }}
        </button>
      }
    </mat-dialog-actions>
  `,
  styles: [`
    .manual-check-dialog { display: flex; min-width: 0; flex-direction: column; gap: 1rem; }
    .manual-check-dialog.mat-mdc-dialog-content { min-height: 0; max-height: none; overflow-x: hidden; overflow-y: auto; }
    .manual-check-dialog steam-manual-check-expression-editor { display: block; min-width: 0; }
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
    .manual-check-dialog__mode { flex-wrap: wrap; }
    .manual-check-dialog__combination { display: flex; flex-direction: column; gap: .65rem; border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-dialog__combination p { margin: .2rem 0 0; color: #64748b; }
    .manual-check-dialog__combination-row { display: grid; grid-template-columns: 7rem minmax(12rem, 1fr) auto auto auto; align-items: center; gap: .45rem; }
    .manual-check-dialog__item-group-create { display: flex; flex-wrap: wrap; align-items: end; gap: .6rem; }
    .manual-check-dialog__item-group-create label { flex: 1; min-width: 12rem; }
    .manual-check-dialog__listing-limit { display: flex; flex-direction: column; gap: .65rem; border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-dialog__listing-limit p { margin: .2rem 0 0; color: #64748b; }
    .manual-check-dialog__listing-limit-controls { display: flex; flex-wrap: wrap; align-items: end; gap: .6rem; }
    .manual-check-dialog__listing-limit-controls label { min-width: 9rem; }
    .manual-check-dialog__price-check { display: flex; flex-direction: column; gap: .65rem; border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-dialog__price-check p { margin: .2rem 0 0; color: #64748b; }
    .manual-check-dialog__price-check-controls { display: flex; flex-wrap: wrap; align-items: end; gap: .6rem; }
    .manual-check-dialog__price-check-controls label { min-width: 9rem; }
    .manual-check-dialog__cooldown { display: flex; flex-direction: column; gap: .65rem; border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-dialog__cooldown-controls { display: flex; flex-wrap: wrap; gap: .6rem; }
    .manual-check-dialog__cooldown-controls label { width: 8rem; }
    .manual-check-dialog__quick-option--active { border-color: #2563eb; background: #eff6ff; color: #1d4ed8; }
    .manual-check-dialog__validation { color: #b91c1c !important; }
    .manual-check-dialog__expression-preview { display: flex; flex-direction: column; gap: .35rem; border-left: 4px solid #60a5fa; border-radius: .25rem; background: #eff6ff; padding: .75rem; }
    .manual-check-dialog__expression-preview code { overflow-wrap: anywhere; color: #1e3a8a; white-space: normal; }
    .manual-check-dialog__expression-preview small { color: #475569; }
    .manual-check-dialog__hint { color: #92400e; margin: 0; }
    .manual-check-dialog__read-only { opacity: .72; }
    .manual-check-dialog__spacer { flex: 1; }
    .manual-check-dialog__queue-source { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .75rem; }
    .manual-check-dialog__products { flex-direction: column !important; }
    .manual-check-dialog__assignments { border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-dialog__assignments summary { cursor: pointer; }
    .manual-check-dialog__assignments summary strong { margin-right: .5rem; }
    .manual-check-dialog__assignment-summary { display: inline-flex; flex-wrap: wrap; gap: .35rem; vertical-align: middle; }
    .manual-check-dialog__assignment-chip { border-radius: 999px; background: #e2e8f0; padding: .2rem .55rem; color: #334155; font-size: .8rem; }
    .manual-check-dialog__assignment-empty { color: #64748b; font-size: .85rem; }
    .manual-check-dialog__assignment-options { flex-direction: column !important; margin-top: .75rem; }
    .manual-check-dialog__product-list { display: grid; max-height: 14rem; overflow: auto; gap: .35rem; border: 1px solid #e2e8f0; border-radius: .375rem; padding: .6rem; }
    @media (max-width: 700px) { .manual-check-dialog__queue-source, .manual-check-dialog__combination-row { grid-template-columns: 1fr; } }
    @keyframes manual-check-spin { to { transform: rotate(360deg); } }
  `],
})
export class ManualCheckSetupDialogComponent implements OnInit {
  readonly data = inject<ManualCheckSetupDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<ManualCheckSetupDialogComponent, ManualCheckSetupDialogResult>>(MatDialogRef);
  readonly criterionLimit = MANUAL_CHECK_MAX_CRITERIA;
  readonly groupLimit = MANUAL_CHECK_MAX_GROUPS;
  readonly combinationLimit = 10;
  private readonly manualCheckService = inject(ManualCheckService);
  private readonly itemGroupService = inject(ItemGroupService);
  private readonly gameService = inject(GameService);
  private readonly gameUrlService = inject(GameUrlService);
  private readonly gameUrlProductService = inject(GameUrlProductService);
  private readonly cdr = inject(ChangeDetectorRef);

  presets: ManualCheckPreset[] = [];
  games: Game[] = [];
  gameUrls: GameUrl[] = [];
  products: GameUrlProduct[] = [];
  selectedGameId: number | null = null;
  selectedGameUrlId: number | null = null;
  selectedPresetGameUrlIds = new Set<number>();
  templateMode: AutomaticQueueTemplateMode = 'SavedPreset';
  productSelectionMode: 'all' | 'selected' = 'all';
  productSearch = '';
  selectedProductIds = new Set<number>();
  itemGroups: ItemGroup[] = [];
  conditionOperators: ManualCheckConditionOperator[] = [];
  presetMode: ManualCheckPresetMode = 'Single';
  combinationRows: ManualCheckPresetCombinationRow[] = [];
  selectedPresetId: number | null = null;
  itemGroupId: number | null = null;
  newItemGroupName = '';
  creatingItemGroup = false;
  name = '';
  listingLimit: number | null = 10;
  priceRangeMode: ManualCheckPriceRangeMode | 'Any' = 'Any';
  minimumPriceEuros: number | null = null;
  maximumPriceEuros: number | null = null;
  customCooldown = false;
  cooldownMinutes: number | null = 0;
  cooldownSeconds: number | null = 0;
  bypassCache = false;
  expressionRoot = this.createEmptyExpression();
  loading = true;
  loadError = false;
  busy = false;
  dirty = true;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    if (this.data.queueBuilder) {
      this.loadQueueBuilder();
    } else {
      this.selectedGameId = this.data.gameId ?? null;
      this.selectedGameUrlId = this.data.gameUrlId ?? null;
      this.gameUrls = this.data.gameUrls ?? [];
      this.loadPresets();
    }
  }

  get eligibleGameUrls(): GameUrl[] {
    return this.gameUrls.filter((x) =>
      x.gameId === this.selectedGameId && x.isActive && x.scrapingModeId === ScrapingModeEnum.ManualBatch);
  }

  get filteredProducts(): GameUrlProduct[] {
    const search = this.productSearch.trim().toLowerCase();
    return search ? this.products.filter((x) => (x.productName ?? '').toLowerCase().includes(search)) : this.products;
  }

  get presetAssignmentGameUrls(): GameUrl[] {
    return this.gameUrls.filter((x) =>
      x.gameId === this.selectedGameId && x.scrapingModeId === ScrapingModeEnum.ManualBatch);
  }

  get selectedPresetAssignmentGameUrls(): GameUrl[] {
    return this.presetAssignmentGameUrls.filter((x) => this.selectedPresetGameUrlIds.has(x.id));
  }

  get selectedGameName(): string {
    return this.games.find((x) => x.id === this.selectedGameId)?.name
      ?? this.data.gameName
      ?? (this.selectedGameId ? `Game #${this.selectedGameId}` : 'Select a game');
  }

  get selectedGameUrlName(): string {
    return this.gameUrls.find((x) => x.id === this.selectedGameUrlId)?.name
      ?? this.data.gameUrlName
      ?? (this.selectedGameUrlId ? `Game URL #${this.selectedGameUrlId}` : 'Select a Game URL');
  }

  get presetGroups(): readonly ItemGroupSection<ManualCheckPreset>[] {
    return groupByItemGroup(this.presets);
  }

  get selectedPreset(): ManualCheckPreset | null {
    return this.selectedPresetId === null
      ? null
      : this.presets.find((preset) => preset.id === this.selectedPresetId) ?? null;
  }

  get isSelectedPresetReadOnly(): boolean {
    return this.presetMode === 'Single' && this.selectedPreset?.canEdit === false;
  }

  loadPresets(): void {
    this.loading = true;
    this.loadError = false;
    this.errorMessage = '';
    this.successMessage = '';

    forkJoin({
      presets: this.manualCheckService.getPresets(this.selectedGameId!, this.selectedGameUrlId ?? undefined),
      itemGroups: this.itemGroupService.getByGame(this.selectedGameId!),
      conditionOperators: this.manualCheckService.getConditionOperators(),
    }).pipe(
      timeout({ first: 15000 }),
      finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }),
    ).subscribe({
      next: ({ presets, itemGroups, conditionOperators }) => {
        this.presets = presets;
        this.itemGroups = itemGroups;
        this.conditionOperators = conditionOperators;
        const preferredPresetId = this.data.initialBlock?.presetId ?? this.data.preselectedPresetId;
        const preferred = presets.find((x) => x.id === preferredPresetId)
          ?? (preferredPresetId === null || preferredPresetId === undefined ? presets[0] : undefined);
        if (
          this.data.queueBuilder &&
          this.data.initialBlock?.templateMode === 'PresetCombination' &&
          this.data.initialBlock.gameId === this.selectedGameId
        ) {
          this.applyPresetCombination(this.data.initialBlock);
        } else if (this.data.queueBuilder && this.data.initialBlock?.templateMode === 'PrivateTemplate') {
          this.applyPrivateTemplate(this.data.initialBlock);
        } else if (preferred) {
          this.selectedPresetId = preferred.id;
          this.selectPreset(preferred.id);
        } else {
          this.newPreset();
          if (preferredPresetId !== null && preferredPresetId !== undefined) {
            this.errorMessage = 'The selected preset is no longer assigned to this Game URL.';
          }
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
    if (this.presetMode === 'Combination') {
      return this.combinationValidation.isValid
        && this.isListingLimitValid
        && this.isPriceRangeValid
        && this.isCooldownValid;
    }

    return this.name.trim().length >= 1
      && this.name.trim().length <= 100
      && this.isListingLimitValid
      && this.isPriceRangeValid
      && this.isCooldownValid
      && (this.data.queueBuilder || this.selectedPresetGameUrlIds.size > 0)
      && this.expressionValidation.isValid;
  }

  get isQueueSelectionValid(): boolean {
    if (!this.data.queueBuilder) {
      return this.presetMode === 'Combination'
        || (this.selectedGameUrlId !== null && this.selectedPresetGameUrlIds.has(this.selectedGameUrlId));
    }
    return this.selectedGameId !== null
      && this.selectedGameUrlId !== null
      && (this.productSelectionMode === 'all' || this.selectedProductIds.size > 0)
      && (this.templateMode === 'PrivateTemplate' ||
        (this.templateMode === 'PresetCombination' && this.combinationValidation.isValid) ||
        this.selectedPresetId !== null);
  }

  get expressionValidation() {
    return validateManualCheckExpression(
      this.expressionRoot,
      new Set(this.conditionOperators.map((operator) => operator.id)),
    );
  }

  get expressionPreview(): string {
    return formatManualCheckExpression(this.expressionRoot, this.conditionOperators);
  }

  get combinationPreview(): string {
    return this.combinationRows.map((row, index) => {
      const name = this.presets.find((preset) => preset.id === row.presetId)?.name ?? 'Select preset';
      return index === 0 ? `(${name})` : `${row.operator?.toUpperCase() ?? 'AND'} (${name})`;
    }).join(' ');
  }

  get combinationValidation(): { isValid: boolean; message: string } {
    if (this.combinationRows.length < 2 || this.combinationRows.length > this.combinationLimit) {
      return { isValid: false, message: `Choose between 2 and ${this.combinationLimit} presets.` };
    }

    const ids = this.combinationRows.map((row) => row.presetId);
    if (ids.some((id) => id === null)) {
      return { isValid: false, message: 'Choose a preset in every row.' };
    }
    if (new Set(ids).size !== ids.length) {
      return { isValid: false, message: 'Each preset can appear only once.' };
    }
    if (this.combinationRows[0].operator !== null || this.combinationRows.slice(1).some((row) => !row.operator)) {
      return { isValid: false, message: 'Choose AND or OR between every preset.' };
    }

    const selected = ids.map((id) => this.presets.find((preset) => preset.id === id));
    if (selected.some((preset) => !preset)) {
      return { isValid: false, message: 'One or more selected presets are no longer available.' };
    }
    const criterionCount = selected.reduce((total, preset) => total + preset!.criteria.length, 0);
    const groupCount = selected.reduce((total, preset) => total + preset!.criteria.reduce(
      (presetTotal, criterion) => presetTotal + (criterion.openGroupCount ?? 0),
      1,
    ), 0);
    if (criterionCount > this.criterionLimit || groupCount > this.groupLimit) {
      return {
        isValid: false,
        message: `Combined presets must stay within ${this.criterionLimit} criteria and ${this.groupLimit} groups.`,
      };
    }

    return { isValid: true, message: '' };
  }

  get criteria(): ManualCheckCriterionNode[] {
    return this.collectCriteria(this.expressionRoot);
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

  get isCooldownValid(): boolean {
    if (!this.customCooldown) {
      return true;
    }

    return this.isCooldownPartValid(this.cooldownMinutes)
      && this.isCooldownPartValid(this.cooldownSeconds);
  }

  get primaryActionLabel(): string {
    if (this.data.queueBuilder) return 'Use block';
    if (this.presetMode === 'Combination') return 'Start combined check';
    if (this.selectedPresetId === null) {
      return 'Create & start';
    }
    return this.dirty ? 'Save & start' : 'Start check';
  }

  get isPriceRangeValid(): boolean {
    if (this.priceRangeMode === 'Any') {
      return true;
    }

    if (this.priceRangeMode === 'Above') {
      return this.isEuroAmountValid(this.minimumPriceEuros);
    }

    if (this.priceRangeMode === 'Below') {
      return this.isEuroAmountValid(this.maximumPriceEuros);
    }

    return this.isEuroAmountValid(this.minimumPriceEuros)
      && this.isEuroAmountValid(this.maximumPriceEuros)
      && this.minimumPriceEuros! <= this.maximumPriceEuros!;
  }

  setPresetMode(mode: ManualCheckPresetMode): void {
    if (mode === 'Combination') {
      if (this.presets.length < 2) return;
      this.presetMode = 'Combination';
      this.templateMode = 'PresetCombination';
      if (this.combinationRows.length < 2) {
        const first = this.presets.find((preset) => preset.id === this.selectedPresetId) ?? this.presets[0];
        const second = this.presets.find((preset) => preset.id !== first.id)!;
        this.combinationRows = [
          { presetId: first.id, operator: null },
          { presetId: second.id, operator: 'And' },
        ];
        this.applyCombinationSettings(first);
      }
    } else {
      this.presetMode = 'Single';
      this.templateMode = this.data.queueBuilder && this.selectedPresetId === null
        ? 'PrivateTemplate'
        : 'SavedPreset';
    }
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  addCombinationRow(): void {
    if (this.combinationRows.length >= this.combinationLimit) return;
    const selectedIds = new Set(this.combinationRows.map((row) => row.presetId));
    const next = this.presets.find((preset) => !selectedIds.has(preset.id));
    if (!next) return;
    this.combinationRows = [...this.combinationRows, { presetId: next.id, operator: 'And' }];
    this.combinationChanged();
  }

  removeCombinationRow(index: number): void {
    if (this.combinationRows.length <= 2) return;
    this.combinationRows = this.combinationRows.filter((_, rowIndex) => rowIndex !== index);
    this.normalizeCombinationOperators();
    this.combinationChanged();
  }

  moveCombinationRow(index: number, direction: -1 | 1): void {
    const target = index + direction;
    if (target < 0 || target >= this.combinationRows.length) return;
    const rows = [...this.combinationRows];
    [rows[index], rows[target]] = [rows[target], rows[index]];
    this.combinationRows = rows;
    this.normalizeCombinationOperators();
    this.combinationChanged();
  }

  combinationChanged(): void {
    this.templateMode = 'PresetCombination';
    this.errorMessage = '';
    this.successMessage = '';
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
    this.presetMode = 'Single';
    if (this.data.queueBuilder) this.templateMode = 'SavedPreset';
    this.itemGroupId = preset.itemGroupId;
    this.selectedPresetGameUrlIds = new Set(preset.gameUrlIds);
    this.name = preset.name;
    this.listingLimit = preset.listingLimit;
    this.applyPriceRange(preset.priceRange);
    this.customCooldown = preset.cooldownMinutes !== null && preset.cooldownSeconds !== null;
    this.cooldownMinutes = preset.cooldownMinutes ?? 0;
    this.cooldownSeconds = preset.cooldownSeconds ?? 0;
    this.expressionRoot = parseManualCheckExpression(preset.criteria).root;
    this.dirty = false;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  newPreset(): void {
    this.presetMode = 'Single';
    this.selectedPresetId = null;
    if (this.data.queueBuilder) this.templateMode = 'PrivateTemplate';
    this.itemGroupId = null;
    this.selectedPresetGameUrlIds = new Set(
      this.selectedGameUrlId === null ? [] : [this.selectedGameUrlId],
    );
    this.name = '';
    this.listingLimit = 10;
    this.applyPriceRange(null);
    this.customCooldown = false;
    this.cooldownMinutes = 0;
    this.cooldownSeconds = 0;
    this.expressionRoot = this.createEmptyExpression();
    this.dirty = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  markDirty(): void {
    if (this.isSelectedPresetReadOnly) return;

    if (this.data.queueBuilder && this.templateMode === 'SavedPreset') {
      this.templateMode = 'PrivateTemplate';
      this.selectedPresetId = null;
    }
    this.dirty = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  setPresetGameUrlAssigned(gameUrlId: number, assigned: boolean): void {
    if (assigned) {
      this.selectedPresetGameUrlIds.add(gameUrlId);
    } else {
      this.selectedPresetGameUrlIds.delete(gameUrlId);
    }
    this.markDirty();
  }

  priceRangeChanged(): void {
    this.markDirty();
  }

  createItemGroup(): void {
    const name = this.newItemGroupName.trim();
    if (this.isSelectedPresetReadOnly || !name || name.length > 255 || this.creatingItemGroup || this.busy) {
      return;
    }

    this.creatingItemGroup = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.itemGroupService.create({ gameId: this.selectedGameId!, name })
      .pipe(finalize(() => {
        this.creatingItemGroup = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (created) => {
          this.itemGroups = [...this.itemGroups, created].sort((left, right) =>
            left.name.localeCompare(right.name) || left.id - right.id);
          this.itemGroupId = created.id;
          this.newItemGroupName = '';
          this.markDirty();
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.errorMessage = this.getError(error, 'Unable to create the group.');
          this.cdr.markForCheck();
        },
      });
  }

  setListingLimit(listingLimit: number): void {
    if (this.isSelectedPresetReadOnly) return;
    this.listingLimit = listingLimit;
    this.markDirty();
  }

  setCustomCooldown(enabled: boolean): void {
    if (this.isSelectedPresetReadOnly) return;
    this.customCooldown = enabled;
    this.markDirty();
  }

  addCriterion(): void {
    if (this.criteria.length < this.criterionLimit) {
      this.expressionRoot.children.push(this.emptyCriterion(false));
      normalizeManualCheckExpressionOperators(
        this.expressionRoot,
        this.defaultOperatorId,
        new Set(this.conditionOperators.map((operator) => operator.id)),
      );
      this.markDirty();
    }
  }

  removeCriterion(index: number): void {
    const criterion = this.criteria[index];
    const location = criterion ? this.findCriterion(this.expressionRoot, criterion.id) : null;
    if (this.criteria.length > 1 && location) {
      location.parent.children.splice(location.index, 1);
      normalizeManualCheckExpressionOperators(
        this.expressionRoot,
        this.defaultOperatorId,
        new Set(this.conditionOperators.map((operator) => operator.id)),
      );
      this.markDirty();
    }
  }

  savePreset(startAfterSave = false): void {
    if (this.isSelectedPresetReadOnly || !this.isDraftValid || this.busy || this.creatingItemGroup) {
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
        const remainsAvailable = this.selectedGameUrlId !== null
          && saved.gameUrlIds.includes(this.selectedGameUrlId);
        const index = this.presets.findIndex((x) => x.id === saved.id);
        if (!remainsAvailable) {
          this.presets = this.presets.filter((preset) => preset.id !== saved.id);
        } else if (index >= 0) {
          this.presets = this.presets.map((preset) => preset.id === saved.id ? saved : preset);
        } else {
          this.presets = [...this.presets, saved];
        }
        this.selectedPresetId = saved.id;
        if (remainsAvailable) {
          this.selectPreset(saved.id);
        } else {
          this.newPreset();
        }
        if (startAfterSave) {
          this.closeForStart(saved.id, null);
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
    if (id === null || this.isSelectedPresetReadOnly || this.busy || this.creatingItemGroup || !confirm(`Delete preset “${this.name}”? Historical runs will be retained.`)) {
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

  cloneSelectedPreset(): void {
    const preset = this.selectedPreset;
    if (!preset?.canClone || this.busy) return;

    this.busy = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.manualCheckService.clonePreset(preset.id)
      .pipe(finalize(() => {
        this.busy = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (clone) => {
          this.presets = [...this.presets.filter((item) => item.id !== clone.id), clone];
          this.selectPreset(clone.id);
          this.successMessage = `Preset “${clone.name}” cloned to your presets.`;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.errorMessage = this.getError(error, 'Unable to clone the preset.');
          this.cdr.markForCheck();
        },
      });
  }

  startOrSave(): void {
    if (!this.isDraftValid || !this.isQueueSelectionValid || this.busy || this.creatingItemGroup) {
      return;
    }

    if (this.data.queueBuilder) {
      this.closeForQueue();
      return;
    }

    if (this.presetMode === 'Combination') {
      this.closeForStart(null, this.toPresetCombination());
      return;
    }

    if (this.selectedPresetId === null || this.dirty) {
      this.savePreset(true);
      return;
    }

    this.closeForStart(this.selectedPresetId, null);
  }

  private closeForStart(
    presetId: number | null,
    presetCombination: ManualCheckPresetCombinationWrite | null,
  ): void {
    this.dialogRef.close({ mode: 'run', presetId, presetCombination, bypassCache: this.bypassCache });
  }

  private toWriteModel(): ManualCheckPresetWrite {
    return {
      gameId: this.selectedGameId!,
      gameUrlIds: [...this.selectedPresetGameUrlIds].sort((left, right) => left - right),
      itemGroupId: this.itemGroupId,
      name: this.name.trim(),
      listingLimit: this.listingLimit!,
      priceRange: this.toPriceRange(),
      cooldownMinutes: this.customCooldown ? this.cooldownMinutes! : null,
      cooldownSeconds: this.customCooldown ? this.cooldownSeconds! : null,
      criteria: flattenManualCheckExpression(this.expressionRoot).map((criterion) => ({
        ...criterion,
        nameContains: criterion.nameContains?.trim() || null,
        valueContains: criterion.valueContains?.trim() || null,
      })),
    };
  }

  queueGameChanged(): void {
    const first = this.eligibleGameUrls[0];
    this.selectedGameUrlId = first?.id ?? null;
    this.selectedProductIds.clear();
    this.productSelectionMode = 'all';
    this.queueGameUrlChanged();
  }

  queueGameUrlChanged(): void {
    const gameUrlId = this.selectedGameUrlId;
    this.products = [];
    this.selectedProductIds.clear();
    if (gameUrlId === null) {
      this.cdr.markForCheck();
      return;
    }
    this.loadPresets();
    this.gameUrlProductService.existsByGameUrl(gameUrlId).subscribe({
      next: (products) => {
        this.products = products.filter((x) => x.isActive === true);
        const requested = this.data.initialBlock?.gameUrlId === gameUrlId
          ? this.data.initialBlock.productIds
          : null;
        this.productSelectionMode = requested === null ? 'all' : 'selected';
        this.selectedProductIds = new Set(requested ?? []);
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.errorMessage = this.getError(error, 'Unable to load products for this Game URL.');
        this.cdr.markForCheck();
      },
    });
  }

  setProductSelected(productId: number, selected: boolean): void {
    if (selected) this.selectedProductIds.add(productId);
    else this.selectedProductIds.delete(productId);
  }

  private loadQueueBuilder(): void {
    this.loading = true;
    forkJoin({
      games: this.gameService.getAll(),
      gameUrls: this.gameUrlService.getAll(),
    }).pipe(finalize(() => this.cdr.markForCheck())).subscribe({
      next: ({ games, gameUrls }) => {
        this.games = games.filter((x) => x.isActive === true);
        this.gameUrls = gameUrls;
        this.selectedGameId = this.data.initialBlock?.gameId ?? this.games[0]?.id ?? null;
        const preferredUrl = this.eligibleGameUrls.find((x) => x.id === this.data.initialBlock?.gameUrlId)
          ?? this.eligibleGameUrls[0];
        this.selectedGameUrlId = preferredUrl?.id ?? null;
        this.bypassCache = this.data.initialBlock?.bypassCache ?? false;
        this.loadPresets();
        this.queueGameUrlChanged();
      },
      error: (error) => {
        this.loading = false;
        this.loadError = true;
        this.errorMessage = this.getError(error, 'Unable to load games and Game URLs.');
        this.cdr.markForCheck();
      },
    });
  }

  private applyPrivateTemplate(block: AutomaticQueueBlock): void {
    const template = block.privateTemplate;
    if (!template) {
      this.newPreset();
      return;
    }
    this.templateMode = 'PrivateTemplate';
    this.selectedPresetId = null;
    this.name = template.name;
    this.listingLimit = template.listingLimit;
    this.applyPriceRange(template.priceRange);
    this.customCooldown = template.cooldownMinutes !== null && template.cooldownSeconds !== null;
    this.cooldownMinutes = template.cooldownMinutes ?? 0;
    this.cooldownSeconds = template.cooldownSeconds ?? 0;
    this.expressionRoot = parseManualCheckExpression(template.criteria).root;
    this.bypassCache = block.bypassCache;
    this.dirty = true;
  }

  private applyPresetCombination(block: AutomaticQueueBlock): void {
    const combination = block.presetCombination;
    if (!combination) {
      this.newPreset();
      return;
    }

    this.presetMode = 'Combination';
    this.templateMode = 'PresetCombination';
    this.selectedPresetId = null;
    this.combinationRows = combination.terms.map((term, index) => ({
      presetId: term.presetId,
      operator: index === 0 ? null : term.operator ?? 'And',
    }));
    this.listingLimit = combination.listingLimit;
    this.applyPriceRange(combination.priceRange);
    this.customCooldown = combination.cooldownMinutes !== null && combination.cooldownSeconds !== null;
    this.cooldownMinutes = combination.cooldownMinutes ?? 0;
    this.cooldownSeconds = combination.cooldownSeconds ?? 0;
    this.bypassCache = block.bypassCache;
    this.dirty = false;
  }

  private closeForQueue(): void {
    if (!this.isQueueSelectionValid || this.selectedGameUrlId === null) return;
    const write = this.presetMode === 'Single' ? this.toWriteModel() : null;
    this.dialogRef.close({
      mode: 'queue',
      block: {
        key: this.data.initialBlock?.key ?? crypto.randomUUID(),
        type: 'ManualCheck',
        delaySeconds: null,
        gameUrlId: this.selectedGameUrlId,
        gameId: this.selectedGameId,
        gameName: this.selectedGameName,
        gameUrlName: this.selectedGameUrlName,
        templateMode: this.templateMode,
        presetId: this.templateMode === 'SavedPreset' ? this.selectedPresetId : null,
        presetCombination: this.templateMode === 'PresetCombination' ? this.toPresetCombination() : null,
        presetName: this.templateMode === 'SavedPreset'
          ? this.presets.find((preset) => preset.id === this.selectedPresetId)?.name ?? null
          : this.templateMode === 'PresetCombination' ? this.combinationPreview : null,
        privateTemplate: this.templateMode === 'PrivateTemplate' ? {
          name: write!.name,
          listingLimit: write!.listingLimit,
          priceRange: write!.priceRange,
          cooldownMinutes: write!.cooldownMinutes,
          cooldownSeconds: write!.cooldownSeconds,
          criteria: write!.criteria,
        } : null,
        bypassCache: this.bypassCache,
        productIds: this.productSelectionMode === 'all'
          ? null
          : this.products.filter((x) => this.selectedProductIds.has(x.productId)).map((x) => x.productId),
      },
    });
  }

  private toPresetCombination(): ManualCheckPresetCombinationWrite {
    return {
      listingLimit: this.listingLimit!,
      priceRange: this.toPriceRange(),
      cooldownMinutes: this.customCooldown ? this.cooldownMinutes! : null,
      cooldownSeconds: this.customCooldown ? this.cooldownSeconds! : null,
      terms: this.combinationRows.map((row, index) => ({
        presetId: row.presetId!,
        operator: index === 0 ? null : row.operator,
      })),
    };
  }

  private applyCombinationSettings(preset: ManualCheckPreset): void {
    this.listingLimit = preset.listingLimit;
    this.applyPriceRange(preset.priceRange);
    this.customCooldown = preset.cooldownMinutes !== null && preset.cooldownSeconds !== null;
    this.cooldownMinutes = preset.cooldownMinutes ?? 0;
    this.cooldownSeconds = preset.cooldownSeconds ?? 0;
  }

  private toPriceRange(): ManualCheckPriceRange | null {
    if (this.priceRangeMode === 'Any') {
      return null;
    }

    return {
      mode: this.priceRangeMode,
      minimumPriceMinorUnits: this.priceRangeMode === 'Above' || this.priceRangeMode === 'Between'
        ? Math.round(this.minimumPriceEuros! * 100)
        : null,
      maximumPriceMinorUnits: this.priceRangeMode === 'Below' || this.priceRangeMode === 'Between'
        ? Math.round(this.maximumPriceEuros! * 100)
        : null,
    };
  }

  private applyPriceRange(priceRange: ManualCheckPriceRange | null | undefined): void {
    this.priceRangeMode = priceRange?.mode ?? 'Any';
    this.minimumPriceEuros = priceRange?.minimumPriceMinorUnits == null
      ? null
      : priceRange.minimumPriceMinorUnits / 100;
    this.maximumPriceEuros = priceRange?.maximumPriceMinorUnits == null
      ? null
      : priceRange.maximumPriceMinorUnits / 100;
  }

  private isEuroAmountValid(value: number | null): boolean {
    return value !== null
      && Number.isFinite(value)
      && value >= 0
      && Math.abs(value * 100 - Math.round(value * 100)) < Number.EPSILON * 100;
  }

  private normalizeCombinationOperators(): void {
    this.combinationRows = this.combinationRows.map((row, index) => ({
      ...row,
      operator: index === 0 ? null : row.operator ?? 'And',
    }));
  }

  private emptyCriterion(first = true): ManualCheckCriterionNode {
    return createManualCheckCriterionNode(first ? null : this.defaultOperatorId);
  }

  private createEmptyExpression(): ManualCheckGroupNode {
    const root = createManualCheckGroupNode(null, true);
    root.children.push(createManualCheckCriterionNode());
    return root;
  }

  private get defaultOperatorId(): number | null {
    return this.conditionOperators.find((operator) => operator.name.toUpperCase() === 'AND')?.id
      ?? this.conditionOperators[0]?.id
      ?? null;
  }

  private collectCriteria(group: ManualCheckGroupNode): ManualCheckCriterionNode[] {
    return group.children.flatMap((child) =>
      child.kind === 'criterion' ? [child] : this.collectCriteria(child));
  }

  private findCriterion(
    group: ManualCheckGroupNode,
    criterionId: string,
  ): { parent: ManualCheckGroupNode; index: number } | null {
    for (const [index, child] of group.children.entries()) {
      if (child.kind === 'criterion' && child.id === criterionId) {
        return { parent: group, index };
      }
      if (child.kind === 'group') {
        const nested = this.findCriterion(child, criterionId);
        if (nested) {
          return nested;
        }
      }
    }
    return null;
  }

  private isCooldownPartValid(value: number | null): boolean {
    return value !== null && Number.isInteger(value) && value >= 0 && value <= 59;
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
