import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { of } from 'rxjs';
import { AuthService } from '../../services/auth/auth.service';

import { AutomaticQueueBlock, ManualCheckPreset, ScrapingModeEnum } from '../../models';
import {
  GameService,
  GameUrlProductService,
  GameUrlService,
  ItemGroupService,
  ManualCheckService,
} from '../../services';
import {
  ManualCheckSetupDialogComponent,
  ManualCheckSetupDialogResult,
} from './manual-check-setup-dialog.component';

describe('ManualCheckSetupDialogComponent queue-builder mode', () => {
  async function setup(initialBlock?: AutomaticQueueBlock) {
    const manualChecks = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', ['getPresets', 'getConditionOperators']);
    const games = jasmine.createSpyObj<GameService>('GameService', ['getAll']);
    const gameUrls = jasmine.createSpyObj<GameUrlService>('GameUrlService', ['getAll']);
    const products = jasmine.createSpyObj<GameUrlProductService>('GameUrlProductService', ['existsByGameUrl']);
    const itemGroups = jasmine.createSpyObj<ItemGroupService>('ItemGroupService', ['getByGame']);
    const dialogRef = jasmine.createSpyObj<MatDialogRef<ManualCheckSetupDialogComponent, ManualCheckSetupDialogResult>>('MatDialogRef', ['close']);
    const preset: ManualCheckPreset = {
      id: 3,
      gameId: 1,
      gameUrlIds: [7],
      gameName: 'Alpha Game',
      itemGroupId: null,
      itemGroupName: null,
      name: 'Saved',
      listingLimit: 10,
      cooldownMinutes: null,
      cooldownSeconds: null,
      criteria: [{ conditionOperatorId: null, nameContains: null, valueContains: 'Unusual' }],
      createdAtUtc: '2026-09-27T00:00:00Z',
      updatedAtUtc: '2026-09-27T00:00:00Z',
    };
    games.getAll.and.returnValue(of([{ id: 1, name: 'Alpha Game', baseUrl: '', pageUrl: null, internalId: null, isActive: true }]));
    gameUrls.getAll.and.returnValue(of([{
      id: 7,
      name: 'Steam Market',
      gameId: 1,
      gameName: 'Alpha Game',
      itemGroupId: null,
      itemGroupName: null,
      scrapingModeId: ScrapingModeEnum.ManualBatch,
      isActive: true,
    }]));
    products.existsByGameUrl.and.returnValue(of([
      { productId: 10, productName: 'One', gameUrlId: 7, gameUrlName: 'Steam Market', fullUrl: '', tags: [], isActive: true, rating: null, currentStock: 0 },
      { productId: 11, productName: 'Two', gameUrlId: 7, gameUrlName: 'Steam Market', fullUrl: '', tags: [], isActive: true, rating: null, currentStock: 0 },
    ]));
    manualChecks.getPresets.and.returnValue(of([preset]));
    manualChecks.getConditionOperators.and.returnValue(of([{ id: 1, name: 'AND' }]));
    itemGroups.getByGame.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [ManualCheckSetupDialogComponent],
      providers: [
        { provide: AuthService, useValue: { currentUser$: of(null) } },
        { provide: ManualCheckService, useValue: manualChecks },
        { provide: GameService, useValue: games },
        { provide: GameUrlService, useValue: gameUrls },
        { provide: GameUrlProductService, useValue: products },
        { provide: ItemGroupService, useValue: itemGroups },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { queueBuilder: true, initialBlock } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(ManualCheckSetupDialogComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    return { fixture, component, dialogRef, manualChecks };
  }

  it('returns a selected product subset without starting a manual run', async () => {
    const { component, dialogRef } = await setup();
    expect(component.detailsExpanded()).toBeFalse();
    component.productSelectionMode = 'selected';
    component.setProductSelected(11, true);

    component.startOrSave();

    expect(dialogRef.close).toHaveBeenCalledWith({
      mode: 'queue',
      block: jasmine.objectContaining({
        type: 'ManualCheck',
        gameUrlId: 7,
        gameId: 1,
        gameName: 'Alpha Game',
        gameUrlName: 'Steam Market',
        templateMode: 'SavedPreset',
        presetId: 3,
        presetName: 'Saved',
        productIds: [11],
      }),
    });
  });

  it('reopens private drafts expanded and preserves their settings, products, and source', async () => {
    const initialBlock: AutomaticQueueBlock = {
      id: 1, sortOrder: 0, key: 'private', type: 'ManualCheck', delaySeconds: null,
      gameUrlId: 7, gameId: 1, gameName: 'Alpha Game', gameUrlName: 'Steam Market',
      templateMode: 'PrivateTemplate', presetId: null, presetName: null, bypassCache: true, productIds: [11],
      privateTemplate: { name: 'Private draft', listingLimit: 37, cooldownMinutes: 0, cooldownSeconds: 8,
        criteria: [{ conditionOperatorId: null, nameContains: null, valueContains: 'Unusual' }] },
    };
    const { component, dialogRef } = await setup(initialBlock);
    expect(component.detailsExpanded()).toBeTrue();
    expect(component.selectedGameId).toBe(1);
    expect(component.selectedGameUrlId).toBe(7);
    expect([...component.selectedProductIds]).toEqual([11]);
    component.detailsExpanded.set(false);
    component.startOrSave();
    expect(dialogRef.close).toHaveBeenCalledWith({ mode: 'queue', block: jasmine.objectContaining({
      key: 'private', templateMode: 'PrivateTemplate', privateTemplate: jasmine.objectContaining({ name: 'Private draft', listingLimit: 37 }),
      gameUrlId: 7, bypassCache: true, productIds: [11],
    }) });
    component.selectedGameUrlId = 8;
    component.loadPresets();
    expect(component.selectedPresetId).toBeNull();
    expect(component.hasCheckConfiguration).toBeFalse();
    expect(component.name).not.toBe('Private draft');
  });

  it('keeps saved-block edits private and resets the selection for a different source', async () => {
    const { component, dialogRef } = await setup();
    component.setListingLimit(37);
    component.startOrSave();
    expect(dialogRef.close).toHaveBeenCalledWith({ mode: 'queue', block: jasmine.objectContaining({
      templateMode: 'PrivateTemplate', presetId: null, privateTemplate: jasmine.objectContaining({ listingLimit: 37 }),
    }) });
    component.selectedGameUrlId = 8;
    component.loadPresets();
    expect(component.selectedPresetId).toBeNull();
    expect(component.hasCheckConfiguration).toBeFalse();
  });
});
