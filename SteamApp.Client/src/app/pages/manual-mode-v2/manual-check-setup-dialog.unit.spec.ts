import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { BehaviorSubject, Subject, of, throwError } from 'rxjs';
import { AuthService, CurrentUser } from '../../services/auth/auth.service';

import { ManualCheckPreset } from '../../models';
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
import { parseManualCheckExpression } from './manual-check-expression';
import { ManualCheckPresetPickerComponent, ManualCheckPresetPickerData } from './manual-check-preset-picker.component';

describe('ManualCheckSetupDialogComponent', () => {
  let fixture: ComponentFixture<ManualCheckSetupDialogComponent>;
  let component: ManualCheckSetupDialogComponent;
  let service: jasmine.SpyObj<ManualCheckService>;
  let currentUser: BehaviorSubject<CurrentUser | null>;
  let dialogs: jasmine.SpyObj<MatDialog>;
  let pickerResult: Subject<ManualCheckPreset | undefined>;
  let itemGroupService: jasmine.SpyObj<ItemGroupService>;
  let dialogRef: jasmine.SpyObj<MatDialogRef<ManualCheckSetupDialogComponent, ManualCheckSetupDialogResult>>;

  const presets: ManualCheckPreset[] = [
    preset(1, 'First', null, null),
    preset(2, 'Preferred', 10, 'Priority'),
  ];

  beforeEach(async () => {
    currentUser = new BehaviorSubject<CurrentUser | null>(null);
    pickerResult = new Subject<ManualCheckPreset | undefined>();
    dialogs = jasmine.createSpyObj('MatDialog', ['open']);
    dialogs.open.and.returnValue({ afterClosed: () => pickerResult.asObservable() } as MatDialogRef<ManualCheckPresetPickerComponent>);
    service = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', [
      'getPresets',
      'getConditionOperators',
      'createPreset',
      'updatePreset',
      'deletePreset',
      'clonePreset',
      'updateGlobalPreset',
      'deleteGlobalPreset',
    ]);
    itemGroupService = jasmine.createSpyObj<ItemGroupService>('ItemGroupService', [
      'getByGame',
      'create',
    ]);
    dialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    service.getPresets.and.returnValue(of(presets.map((item) => ({
      ...item,
      criteria: item.criteria.map((criterion) => ({ ...criterion })),
    }))));
    service.getConditionOperators.and.returnValue(of([
      { id: 1, name: 'AND' },
      { id: 2, name: 'OR' },
      { id: 3, name: 'AND NOT' },
      { id: 4, name: 'OR NOT' },
      { id: 5, name: 'XOR' },
      { id: 6, name: 'NAND' },
      { id: 7, name: 'NOR' },
    ]));
    itemGroupService.getByGame.and.returnValue(of([
      { id: 10, gameId: 440, name: 'Priority' },
    ]));
    itemGroupService.create.and.returnValue(of({ id: 11, gameId: 440, name: 'Category' }));

    await TestBed.configureTestingModule({
      imports: [ManualCheckSetupDialogComponent],
      providers: [
        { provide: AuthService, useValue: { currentUser$: currentUser.asObservable() } },
        { provide: MatDialog, useValue: dialogs },
        { provide: ManualCheckService, useValue: service },
        { provide: ItemGroupService, useValue: itemGroupService },
        { provide: GameService, useValue: jasmine.createSpyObj('GameService', ['getAll']) },
        { provide: GameUrlService, useValue: jasmine.createSpyObj('GameUrlService', ['getAll']) },
        { provide: GameUrlProductService, useValue: jasmine.createSpyObj('GameUrlProductService', ['existsByGameUrl']) },
        { provide: MatDialogRef, useValue: dialogRef },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            gameId: 440,
            gameUrlId: 7,
            gameName: 'Team Fortress 2',
            gameUrlName: 'Steam Market',
            gameUrls: [{
              id: 7,
              gameId: 440,
              gameName: 'Team Fortress 2',
              name: 'Steam Market',
              itemGroupId: null,
              itemGroupName: null,
              scrapingModeId: 1,
              isActive: true,
            }, {
              id: 8,
              gameId: 440,
              gameName: 'Team Fortress 2',
              name: 'Archived Market',
              itemGroupId: null,
              itemGroupName: null,
              scrapingModeId: 1,
              isActive: false,
            }],
            preselectedPresetId: 2,
          },
        },
      ],
    }).overrideProvider(MatDialog, { useValue: dialogs }).compileComponents();

    fixture = TestBed.createComponent(ManualCheckSetupDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('preselects and prefills the requested game preset', () => {
    expect(service.getPresets).toHaveBeenCalledWith(440);
    expect(component.selectedPresetId).toBe(2);
    expect(component.itemGroupId).toBe(10);
    expect(component.name).toBe('Preferred');
    expect(component.listingLimit).toBe(10);
    expect(component.customCooldown).toBeFalse();
    expect(component.bypassCache).toBeFalse();
    expect(component.criteria[0].valueContains).toBe('Mean Green');
    expect(component.canStart).toBeTrue();
    expect(component.loading).toBeFalse();
  });

  it('opens with collapsed details, a settings summary, and only Cancel and Start in the footer', () => {
    const details = fixture.nativeElement.querySelector('.manual-check-dialog__details') as HTMLDetailsElement;
    expect(details.open).toBeFalse();
    expect(component.settingsSummary).toContain('10 listings');
    expect(component.settingsSummary).toContain('Any price');
    expect(component.settingsSummary).toContain('Default cooldown');
    expect(component.settingsSummary).toContain('1 criterion');
    const footer = fixture.nativeElement.querySelector('mat-dialog-actions');
    expect(Array.from(footer.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .map((button) => button.textContent?.trim())).toEqual(['Cancel', 'Start check']);
    expect(fixture.nativeElement.querySelector('input[name="manualCheckNewItemGroupName"]')).toBeNull();
  });

  it('filters selectors to assigned presets while retaining an explicitly selected one-off preset', () => {
    component.presets = [presets[0], { ...presets[1], gameUrlIds: [8] }];
    component.selectPreset(1);
    expect(component.presetGroups.flatMap((group) => group.items).map((item) => item.id)).toEqual([1]);
    component.openPresetPicker();
    pickerResult.next(component.presets[1]);
    expect(component.selectedPresetId).toBe(2);
    expect(component.isSelectedPresetUnassigned).toBeTrue();
    expect(component.detailsExpanded()).toBeFalse();
    expect(component.presetGroups.flatMap((group) => group.items).map((item) => item.id)).toEqual([2, 1]);
    component.startOrSave();
    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(service.createPreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith(jasmine.objectContaining({ presetId: 2, privateTemplate: null }));
  });

  it('offers Top X outside collapsed details and applies it once without saving the preset', async () => {
    const section = fixture.nativeElement.querySelector('.manual-check-dialog__listing-limit') as HTMLElement;
    expect(section.closest('details')).toBeNull();
    expect(fixture.nativeElement.querySelectorAll('.manual-check-dialog__listing-limit').length).toBe(1);
    const top20 = Array.from(section.querySelectorAll('button')).find((button) => button.textContent?.trim() === 'Top 20')!;
    top20.click();
    fixture.detectChanges();
    expect(component.listingLimit).toBe(20);
    expect(component.detailsExpanded()).toBeFalse();

    const custom = section.querySelector('input') as HTMLInputElement;
    custom.value = '37';
    custom.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();
    component.startOrSave();

    expect(component.selectedPreset?.listingLimit).toBe(10);
    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(service.createPreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith(jasmine.objectContaining({ presetId: null,
      privateTemplate: jasmine.objectContaining({ listingLimit: 37 }) }));
  });

  it('does not select an unassigned preset automatically when the requested preset is unavailable', () => {
    service.getPresets.and.returnValue(of([{ ...presets[0], gameUrlIds: [8] }]));
    component.loadPresets();
    expect(component.selectedPresetId).toBeNull();
    expect(component.hasCheckConfiguration).toBeFalse();
    expect(component.detailsExpanded()).toBeFalse();
    component.startOrSave();
    expect(dialogRef.close).not.toHaveBeenCalled();
  });

  it('falls back to an assigned preset when the requested preset is unavailable', () => {
    service.getPresets.and.returnValue(of([presets[0]]));
    component.loadPresets();
    expect(component.selectedPresetId).toBe(1);
  });

  it('honors an explicitly requested unassigned preset without assigning it', () => {
    service.getPresets.and.returnValue(of([presets[0], { ...presets[1], gameUrlIds: [8] }]));
    component.loadPresets();
    expect(component.selectedPresetId).toBe(2);
    expect(component.isSelectedPresetUnassigned).toBeTrue();
    expect(service.updatePreset).not.toHaveBeenCalled();
  });

  it('preserves a draft when the picker is cancelled and confirms before replacing unsaved work', () => {
    component.setListingLimit(37);
    const confirm = spyOn(window, 'confirm').and.returnValue(false);
    component.openPresetPicker();
    expect(dialogs.open).not.toHaveBeenCalled();
    confirm.and.returnValue(true);
    component.openPresetPicker();
    pickerResult.next(undefined);
    expect(component.listingLimit).toBe(37);
    expect(component.dirty).toBeTrue();
    expect(component.selectedPresetId).toBe(2);
  });

  it('keeps edits when details are collapsed and expands invalid settings when starting', () => {
    component.detailsExpanded.set(true);
    component.setListingLimit(0);
    component.detailsExpanded.set(false);
    component.startOrSave();
    expect(component.detailsExpanded()).toBeTrue();
    expect(component.listingLimit).toBe(0);
    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.errorMessage).toContain('Complete');
  });

  it('runs a new one-off check without any URL assignments but requires assignments to save', () => {
    component.createNewFromUi();
    component.name = 'One-off';
    component.criteria[0].valueContains = 'Unusual';
    component.selectedPresetGameUrlIds.clear();
    expect(component.detailsExpanded()).toBeTrue();
    expect(component.isDraftValid).toBeTrue();
    expect(component.canSavePreset).toBeFalse();
    component.savePreset();
    component.startOrSave();
    expect(service.createPreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith(jasmine.objectContaining({ presetId: null,
      privateTemplate: jasmine.objectContaining({ name: 'One-off' }) }));
  });

  it('updates only the chosen combination row from the picker and preserves combination settings', () => {
    component.presets.push({ ...preset(3, 'Third'), gameUrlIds: [8] });
    component.setPresetMode('Combination');
    component.setListingLimit(37);
    const otherRow = { ...component.combinationRows[0] };
    component.openPresetPicker(1);
    const options = dialogs.open.calls.mostRecent().args[1]!;
    expect((options.data as ManualCheckPresetPickerData).excludedPresetIds).toEqual([otherRow.presetId!]);
    pickerResult.next(component.presets.find((item) => item.id === 3)!);
    expect(component.presetMode).toBe('Combination');
    expect(component.combinationRows[0]).toEqual(otherRow);
    expect(component.combinationRows[1].presetId).toBe(3);
    expect(component.listingLimit).toBe(37);
    expect(component.combinationPresetGroups(0).flatMap((group) => group.items).map((item) => item.id)).not.toContain(3);
    expect(component.combinationPresetGroups(1).flatMap((group) => group.items).map((item) => item.id)).toContain(3);
  });

  it('starts combination rows empty instead of silently using unassigned presets', () => {
    component.presets = component.presets.map((item) => ({ ...item, gameUrlIds: [8] }));
    component.selectedPresetId = null;
    component.setPresetMode('Combination');
    expect(component.combinationRows.map((row) => row.presetId)).toEqual([null, null]);
    expect(component.combinationValidation.isValid).toBeFalse();
  });

  it('keeps mode and draft when discarding changes is cancelled', () => {
    component.setListingLimit(37);
    spyOn(window, 'confirm').and.returnValue(false);
    component.setPresetMode('Combination');
    component.selectPresetFromUi(1);
    component.createNewFromUi();
    expect(component.presetMode).toBe('Single');
    expect(component.selectedPresetId).toBe(2);
    expect(component.listingLimit).toBe(37);
  });

  it('groups saved presets by item group and keeps ungrouped presets last', () => {
    expect(component.presetGroups.map((group) => group.name)).toEqual(['Priority', 'Ungrouped']);
    expect(component.presetGroups[0].items.map((item) => item.name)).toEqual(['Preferred']);
    expect(component.presetGroups[1].items.map((item) => item.name)).toEqual(['First']);
  });

  it('creates and selects a shared item group for the preset', () => {
    component.newItemGroupName = ' Category ';

    component.createItemGroup();

    expect(itemGroupService.create).toHaveBeenCalledWith({ gameId: 440, name: 'Category' });
    expect(component.itemGroupId).toBe(11);
    expect(component.itemGroups.map((itemGroup) => itemGroup.name)).toEqual(['Category', 'Priority']);
    expect(component.dirty).toBeTrue();
  });

  it('requires edited presets to be saved before starting', () => {
    component.criteria[0].valueContains = 'Hot Rod';
    component.markDirty();
    expect(component.canStart).toBeFalse();

    const saved = {
      ...presets[1],
      criteria: [{ conditionOperatorId: null, nameContains: 'attribute', valueContains: 'Hot Rod' }],
    };
    service.updatePreset.and.returnValue(of(saved));
    component.savePreset();

    expect(service.updatePreset).toHaveBeenCalledWith(2, jasmine.objectContaining({
      gameId: 440,
      itemGroupId: 10,
      listingLimit: 10,
      cooldownMinutes: null,
      cooldownSeconds: null,
    }));
    expect(component.canStart).toBeTrue();
  });

  it('allows one to fifty valid criterion rows', () => {
    for (let i = 1; i < 50; i++) {
      component.addCriterion();
      component.criteria[i].valueContains = `Value ${i}`;
    }
    component.criteria[0].valueContains = 'Value 0';
    component.addCriterion();

    expect(component.criteria.length).toBe(50);
    expect(component.isDraftValid).toBeTrue();
  });

  it('supports Top 10, Top 20, and a positive custom listing count', () => {
    component.setListingLimit(20);
    expect(component.listingLimit).toBe(20);
    expect(component.dirty).toBeTrue();

    component.listingLimit = 37;
    expect(component.isListingLimitValid).toBeTrue();
    expect(component.isDraftValid).toBeTrue();

    component.listingLimit = 0;
    expect(component.isListingLimitValid).toBeFalse();
    expect(component.isDraftValid).toBeFalse();

    component.listingLimit = 1.5;
    expect(component.isListingLimitValid).toBeFalse();
  });

  it('validates and serializes an explicit custom cooldown', () => {
    component.setCustomCooldown(true);
    component.cooldownMinutes = 1;
    component.cooldownSeconds = 9;
    component.criteria[0].valueContains = 'Mean Green';
    service.updatePreset.and.returnValue(of({
      ...presets[1],
      cooldownMinutes: 1,
      cooldownSeconds: 9,
    }));

    component.savePreset();

    expect(service.updatePreset).toHaveBeenCalledWith(2, jasmine.objectContaining({
      cooldownMinutes: 1,
      cooldownSeconds: 9,
    }));
    component.cooldownSeconds = 60;
    expect(component.isCooldownValid).toBeFalse();
  });

  it('defaults new presets to the current URL and saves additional URL assignments', () => {
    component.newPreset();
    expect([...component.selectedPresetGameUrlIds]).toEqual([7]);
    component.name = 'Multi URL';
    component.criteria[0].valueContains = 'Mean Green';
    component.setPresetGameUrlAssigned(8, true);
    const saved = { ...preset(3, 'Multi URL'), gameUrlIds: [7, 8] };
    service.createPreset.and.returnValue(of(saved));

    component.savePreset();

    expect(service.createPreset).toHaveBeenCalledWith(jasmine.objectContaining({
      gameUrlIds: [7, 8],
    }));
  });

  it('collapses URL assignments by default and summarizes only selected URLs', () => {
    const assignments = fixture.nativeElement.querySelector(
      '.manual-check-dialog__assignments',
    ) as HTMLDetailsElement;
    const summary = assignments.querySelector('summary') as HTMLElement;
    const options = assignments.querySelector('.manual-check-dialog__assignment-options') as HTMLElement;

    expect(assignments.open).toBeFalse();
    expect(summary.textContent).toContain('Steam Market');
    expect(summary.textContent).not.toContain('Archived Market');
    expect(options.textContent).toContain('Steam Market');
    expect(options.textContent).toContain('Archived Market');
  });

  it('validates and serializes EUR price ranges as minor units', () => {
    component.priceRangeMode = 'Between';
    component.minimumPriceEuros = 1.25;
    component.maximumPriceEuros = 3.5;
    component.priceRangeChanged();
    service.updatePreset.and.returnValue(of({
      ...presets[1],
      priceRange: {
        mode: 'Between',
        minimumPriceMinorUnits: 125,
        maximumPriceMinorUnits: 350,
      },
    }));

    component.savePreset();

    expect(service.updatePreset).toHaveBeenCalledWith(2, jasmine.objectContaining({
      priceRange: {
        mode: 'Between',
        minimumPriceMinorUnits: 125,
        maximumPriceMinorUnits: 350,
      },
    }));
    component.minimumPriceEuros = 3.501;
    expect(component.isPriceRangeValid).toBeFalse();
    component.minimumPriceEuros = 4;
    component.maximumPriceEuros = 3;
    expect(component.isPriceRangeValid).toBeFalse();
  });

  it('assigns and requires an operator after the first criterion', () => {
    component.addCriterion();
    component.criteria[0].valueContains = 'Mean Green';
    component.criteria[1].valueContains = 'Tradable';

    expect(component.criteria[0].conditionOperatorId).toBeNull();
    expect(component.criteria[1].conditionOperatorId).toBe(1);
    expect(component.isDraftValid).toBeTrue();

    component.criteria[1].conditionOperatorId = null;
    expect(component.isDraftValid).toBeFalse();
  });

  it('renders, previews, and serializes nested groups', () => {
    const criteria = [
      { conditionOperatorId: null, nameContains: 'A', valueContains: 'A', openGroupCount: 0, closeGroupCount: 0 },
      { conditionOperatorId: 1, nameContains: 'B', valueContains: 'B', openGroupCount: 1, closeGroupCount: 0 },
      { conditionOperatorId: 2, nameContains: 'C', valueContains: 'C', openGroupCount: 0, closeGroupCount: 1 },
    ];
    component.expressionRoot = parseManualCheckExpression(criteria).root;
    component.markDirty();
    service.updatePreset.and.returnValue(of({ ...presets[1], criteria }));
    fixture.detectChanges();

    component.savePreset();

    const write = service.updatePreset.calls.mostRecent().args[1];
    expect(write.criteria.map((criterion) => ({
      operator: criterion.conditionOperatorId,
      opens: criterion.openGroupCount,
      closes: criterion.closeGroupCount,
    }))).toEqual([
      { operator: null, opens: 0, closes: 0 },
      { operator: 1, opens: 1, closes: 0 },
      { operator: 2, opens: 0, closes: 1 },
    ]);
    expect(fixture.nativeElement.textContent).toContain('([B: B] OR [C: C])');
    expect(fixture.nativeElement.querySelectorAll('.expression-group').length).toBeGreaterThan(1);
  });

  it('starts a new draft without saving it', () => {
    const saved = preset(3, 'One click');
    service.createPreset.and.returnValue(of(saved));
    component.newPreset();
    component.name = saved.name;
    component.criteria[0].valueContains = 'Mean Green';
    component.bypassCache = true;

    component.startOrSave();

    expect(service.createPreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith(jasmine.objectContaining({
      mode: 'run', presetId: null, sourcePresetId: null, bypassCache: true,
      privateTemplate: jasmine.objectContaining({ name: saved.name }),
    }));
  });

  it('starts an existing preset with the selected cache behavior without marking it dirty', () => {
    component.bypassCache = true;

    component.startOrSave();

    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith({ mode: 'run', presetId: 2, presetCombination: null,
      privateTemplate: null, sourcePresetId: 2, bypassCache: true });
  });

  it('keeps global presets read-only and clones them into a personal preset', () => {
    const globalPreset = {
      ...component.presets.find((item) => item.id === 2)!,
      scope: 'global' as const,
      canEdit: false,
      canClone: true,
    };
    const personalClone = {
      ...globalPreset,
      id: 3,
      name: 'Preferred (copy)',
      scope: 'personal' as const,
      canEdit: true,
    };
    component.presets = component.presets.map((item) => item.id === 2 ? globalPreset : item);
    component.selectPreset(2);
    service.clonePreset.and.returnValue(of(personalClone));

    component.markDirty();
    component.cloneSelectedPreset();

    expect(component.isSelectedPresetReadOnly).toBeFalse();
    expect(component.selectedPresetId).toBe(3);
    expect(component.dirty).toBeFalse();
    expect(service.clonePreset).toHaveBeenCalledWith(2);
    expect(component.successMessage).toContain('cloned');
  });

  it('runs edits to a global preset as a one-off draft', () => {
    component.presets = component.presets.map((item) => item.id === 2
      ? { ...item, scope: 'global', canEdit: false, canClone: true } : item);
    component.selectPreset(2);
    component.setListingLimit(20);

    component.startOrSave();

    expect(service.createPreset).not.toHaveBeenCalled();
    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith(jasmine.objectContaining({
      mode: 'run', presetId: null, sourcePresetId: 2,
      privateTemplate: jasmine.objectContaining({ listingLimit: 20 }),
    }));
  });

  it('lets an unchanged global preset be saved personally with the current URL', () => {
    selectGlobalPreset();
    service.createPreset.and.returnValue(of({ ...presets[1], id: 3, gameUrlIds: [7, 8], canEdit: true }));
    fixture.detectChanges();
    const saveButton = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((button) => button.textContent?.trim() === 'Save as personal preset')!;

    expect(component.dirty).toBeFalse();
    expect(saveButton.disabled).toBeFalse();
    expect(fixture.nativeElement.querySelector('[inert]')).toBeNull();
    saveButton.click();

    expect(service.createPreset).toHaveBeenCalledWith(jasmine.objectContaining({ gameUrlIds: [8, 7] }));
    expect(service.updateGlobalPreset).not.toHaveBeenCalled();
    expect(component.selectedPresetId).toBe(3);
  });

  it('lets admins save global URL assignments and delete using the admin endpoints', () => {
    selectGlobalPreset();
    currentUser.next({ id: 'admin', displayName: 'Admin', firstName: null, lastName: null,
      userName: null, email: null, phone: null, clientId: null, scope: 'user', roles: ['Admin'], isAdmin: true });
    component.setPresetGameUrlAssigned(7, true);
    service.updateGlobalPreset.and.returnValue(of({ ...component.selectedPreset!, gameUrlIds: [7, 8] }));
    fixture.detectChanges();
    const buttons = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>);
    const saveButton = buttons.find((button) => button.textContent?.trim() === 'Save global changes')!;
    expect(saveButton.disabled).toBeFalse();
    saveButton.click();

    expect(service.updateGlobalPreset).toHaveBeenCalledWith(2, jasmine.objectContaining({ gameUrlIds: [7, 8] }));
    expect(service.createPreset).not.toHaveBeenCalled();
    service.deleteGlobalPreset.and.returnValue(of(undefined));
    spyOn(window, 'confirm').and.returnValue(true);
    component.deletePreset();
    expect(service.deleteGlobalPreset).toHaveBeenCalledWith(2);
    expect(service.deletePreset).not.toHaveBeenCalled();
  });

  it('blocks global mutations for non-admin users and after admin access is removed', () => {
    selectGlobalPreset();
    component.markDirty();
    component.savePreset(true);
    component.deletePreset();
    expect(service.updateGlobalPreset).not.toHaveBeenCalled();
    expect(service.deleteGlobalPreset).not.toHaveBeenCalled();
    currentUser.next({ isAdmin: true } as CurrentUser);
    expect(component.canManageGlobalPreset).toBeTrue();
    currentUser.next(null);
    expect(component.canManageGlobalPreset).toBeFalse();
  });

  it('retains a global preset when deletion is blocked by an automatic queue', () => {
    selectGlobalPreset();
    currentUser.next({ isAdmin: true } as CurrentUser);
    spyOn(window, 'confirm').and.returnValue(true);
    service.deleteGlobalPreset.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 409, error: { detail: 'The global preset is referenced by an automatic queue.' },
    })));
    component.deletePreset();
    expect(component.selectedPresetId).toBe(2);
    expect(component.presets.some((item) => item.id === 2)).toBeTrue();
    expect(component.busy).toBeFalse();
    expect(component.errorMessage).toContain('referenced by an automatic queue');
  });

  function selectGlobalPreset(): void {
    component.presets = component.presets.map((item) => item.id === 2
      ? { ...item, gameUrlIds: [8], scope: 'global', canEdit: false, canClone: true } : item);
    component.selectPreset(2);
  }

  it('builds and starts a left-to-right preset combination with independent settings', () => {
    component.setPresetMode('Combination');
    component.combinationRows[1].operator = 'Or';
    component.setListingLimit(20);
    component.setCustomCooldown(true);
    component.cooldownMinutes = 1;
    component.cooldownSeconds = 5;

    component.startOrSave();

    expect(service.createPreset).not.toHaveBeenCalled();
    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith({
      mode: 'run',
      presetId: null,
      privateTemplate: null,
      sourcePresetId: 2,
      presetCombination: {
        listingLimit: 20,
        priceRange: null,
        cooldownMinutes: 1,
        cooldownSeconds: 5,
        terms: [
          { presetId: 2, operator: null },
          { presetId: 1, operator: 'Or' },
        ],
      },
      bypassCache: false,
    });
  });

  it('rejects duplicate combined presets and aggregate group overflow', () => {
    component.setPresetMode('Combination');
    component.combinationRows[1].presetId = component.combinationRows[0].presetId;

    expect(component.combinationValidation.isValid).toBeFalse();
    expect(component.combinationValidation.message).toContain('only once');

    component.combinationRows[1].presetId = 1;
    component.presets = component.presets.map((preset, presetIndex) => presetIndex === 0 ? {
      ...preset,
      criteria: Array.from({ length: 25 }, (_, index) => ({
        conditionOperatorId: index === 0 ? null : 1,
        openGroupCount: 1,
        closeGroupCount: 1,
        nameContains: null,
        valueContains: `Value ${index}`,
      })),
    } : preset);

    expect(component.combinationValidation.isValid).toBeFalse();
    expect(component.combinationValidation.message).toContain('groups');
  });

  it('shows a useful retry message for rate-limited preset requests', () => {
    service.getPresets.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 429,
      error: { detail: 'The request limit was reached. Try again in 12 seconds.' },
      headers: new HttpHeaders({ 'Retry-After': '12' }),
    })));

    component.loadPresets();

    expect(component.loading).toBeFalse();
    expect(component.loadError).toBeTrue();
    expect(component.errorMessage).toContain('12 seconds');
  });

  function preset(
    id: number,
    name: string,
    itemGroupId: number | null = null,
    itemGroupName: string | null = null,
  ): ManualCheckPreset {
    return {
      id,
      gameId: 440,
      gameUrlIds: [7],
      gameName: 'Team Fortress 2',
      itemGroupId,
      itemGroupName,
      name,
      listingLimit: 10,
      cooldownMinutes: null,
      cooldownSeconds: null,
      criteria: [{ conditionOperatorId: null, nameContains: 'attribute', valueContains: 'Mean Green' }],
      createdAtUtc: '2026-08-14T00:00:00Z',
      updatedAtUtc: '2026-08-14T00:00:00Z',
    };
  }
});
