import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { BehaviorSubject, Subject, of, throwError } from 'rxjs';

import { ManualCheckPreset } from '../../models';
import { ManualCheckService } from '../../services';
import { AuthService, CurrentUser } from '../../services/auth/auth.service';
import { ManualCheckPresetPickerComponent, ManualCheckPresetPickerData } from './manual-check-preset-picker.component';

describe('ManualCheckPresetPickerComponent', () => {
  let fixture: ComponentFixture<ManualCheckPresetPickerComponent>;
  let component: ManualCheckPresetPickerComponent;
  let service: jasmine.SpyObj<ManualCheckService>;
  let dialogRef: jasmine.SpyObj<MatDialogRef<ManualCheckPresetPickerComponent, ManualCheckPreset>>;
  let user: BehaviorSubject<CurrentUser | null>;
  let data: ManualCheckPresetPickerData;

  beforeEach(async () => {
    const personal: ManualCheckPreset = {
      id: 1, gameId: 440, gameName: 'TF2', gameUrlIds: [8], itemGroupId: 2, itemGroupName: 'Rare',
      name: 'Personal', listingLimit: 20, cooldownMinutes: 0, cooldownSeconds: 9,
      priceRange: { mode: 'Above', minimumPriceMinorUnits: 100, maximumPriceMinorUnits: null },
      criteria: [{ conditionOperatorId: null, nameContains: 'Type', valueContains: 'Unusual' }],
      createdAtUtc: '', updatedAtUtc: '', canEdit: true, scope: 'personal',
    };
    data = { presets: [personal, { ...personal, id: 2, name: 'Shared', canEdit: false, scope: 'global' },
      { ...personal, id: 3, name: 'Assigned', gameUrlIds: [7, 8], itemGroupId: null, itemGroupName: null }],
      gameUrlId: 7, accessibleGameUrlIds: [7, 8], excludedPresetIds: [] };
    user = new BehaviorSubject<CurrentUser | null>(null);
    service = jasmine.createSpyObj('ManualCheckService', ['createPreset', 'updatePreset', 'updateGlobalPreset']);
    dialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    await TestBed.configureTestingModule({
      imports: [ManualCheckPresetPickerComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: ManualCheckService, useValue: service },
        { provide: AuthService, useValue: { currentUser$: user.asObservable() } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(ManualCheckPresetPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('searches names and groups and shows assignment and scope labels', () => {
    expect(fixture.nativeElement.textContent).toContain('Global');
    expect(fixture.nativeElement.textContent).toContain('Personal · Assigned');
    component.search.set(' rare ');
    expect(component.groups().flatMap((group) => group.items).map((preset) => preset.id)).toEqual([1, 2]);
    component.search.set('SHARED');
    expect(component.groups()[0].items.map((preset) => preset.id)).toEqual([2]);
    component.search.set('not found');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No matching presets.');
  });

  it('uses a global preset once without saving or assigning', () => {
    component.select(data.presets[1]);
    component.useOnce();
    expect(dialogRef.close).toHaveBeenCalledWith(data.presets[1]);
    expect(service.createPreset).not.toHaveBeenCalled();
    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(service.updateGlobalPreset).not.toHaveBeenCalled();
  });

  it('cancels without selecting or saving', () => {
    const cancel = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((button) => button.textContent?.trim() === 'Cancel')!;
    cancel.click();
    expect(dialogRef.close).toHaveBeenCalledWith();
    expect(service.updatePreset).not.toHaveBeenCalled();
  });

  it('adds the URL with one personal update while preserving saved settings and other relations', () => {
    const preset = data.presets[0];
    const saved = { ...preset, gameUrlIds: [7, 8] };
    component.select(preset);
    service.updatePreset.and.returnValue(of(saved));
    component.assign();
    expect(service.updatePreset).toHaveBeenCalledOnceWith(1, jasmine.objectContaining({
      gameUrlIds: [7, 8], listingLimit: 20, cooldownSeconds: 9, priceRange: preset.priceRange,
      criteria: preset.criteria, itemGroupId: 2, name: 'Personal',
    }));
    expect(preset.gameUrlIds).toEqual([8]);
    expect(dialogRef.close).toHaveBeenCalledWith(saved);
  });

  it('does not duplicate an existing assignment', () => {
    component.select(data.presets[2]);
    component.assign();
    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(dialogRef.close).not.toHaveBeenCalled();
  });

  it('routes admin global assignments through the protected admin update', () => {
    user.next({ isAdmin: true } as CurrentUser);
    component.select(data.presets[1]);
    service.updateGlobalPreset.and.returnValue(of({ ...data.presets[1], gameUrlIds: [7, 8] }));
    component.assign();
    expect(service.updateGlobalPreset).toHaveBeenCalledOnceWith(2, jasmine.objectContaining({ gameUrlIds: [7, 8] }));
    expect(service.createPreset).not.toHaveBeenCalled();
  });

  it('creates a personal copy for ordinary users without mutating global assignments', () => {
    data.presets[1].gameUrlIds = [8, 999];
    component.select(data.presets[1]);
    component.copyName = 'My shared check';
    service.createPreset.and.returnValue(of({ ...data.presets[1], id: 4, gameUrlIds: [7, 8], canEdit: true }));
    component.assign();
    expect(service.createPreset).toHaveBeenCalledOnceWith(jasmine.objectContaining({ name: 'My shared check', gameUrlIds: [7, 8] }));
    expect(service.updateGlobalPreset).not.toHaveBeenCalled();
    expect(data.presets[1].gameUrlIds).toEqual([8, 999]);
  });

  it('rechecks admin access at assignment time', () => {
    user.next({ isAdmin: true } as CurrentUser);
    component.select(data.presets[1]);
    user.next(null);
    service.createPreset.and.returnValue(of({ ...data.presets[1], id: 4 }));
    component.assign();
    expect(service.updateGlobalPreset).not.toHaveBeenCalled();
    expect(service.createPreset).toHaveBeenCalled();
  });

  it('keeps the picker open and releases controls after a failed assignment', () => {
    component.select(data.presets[0]);
    service.updatePreset.and.returnValue(throwError(() => new HttpErrorResponse({ status: 403, error: { detail: 'Access denied' } })));
    component.assign();
    expect(component.busy()).toBeFalse();
    expect(dialogRef.disableClose).toBeFalse();
    expect(component.errorMessage()).toBe('Access denied');
    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(data.presets[0].gameUrlIds).toEqual([8]);
  });

  it('prevents repeated requests and cancels the subscription when destroyed', () => {
    const pending = new Subject<ManualCheckPreset>();
    component.select(data.presets[0]);
    service.updatePreset.and.returnValue(pending);
    component.assign();
    component.assign();
    component.useOnce();
    expect(service.updatePreset).toHaveBeenCalledTimes(1);
    expect(dialogRef.disableClose).toBeTrue();
    fixture.destroy();
    pending.next(data.presets[0]);
    expect(dialogRef.close).not.toHaveBeenCalled();
  });

  it('rejects a blank personal name, missing selection, and excluded combination presets', () => {
    component.assign();
    component.useOnce();
    component.select(data.presets[1]);
    component.copyName = ' ';
    component.assign();
    data.excludedPresetIds.push(2);
    component.copyName = 'Valid';
    component.assign();
    component.useOnce();
    expect(service.createPreset).not.toHaveBeenCalled();
    expect(service.updateGlobalPreset).not.toHaveBeenCalled();
    expect(dialogRef.close).not.toHaveBeenCalled();
  });
});
