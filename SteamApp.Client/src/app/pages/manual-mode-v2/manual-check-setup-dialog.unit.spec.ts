import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';

import { ManualCheckPreset } from '../../models';
import { ManualCheckService } from '../../services';
import {
  ManualCheckSetupDialogComponent,
  ManualCheckSetupDialogResult,
} from './manual-check-setup-dialog.component';

describe('ManualCheckSetupDialogComponent', () => {
  let fixture: ComponentFixture<ManualCheckSetupDialogComponent>;
  let component: ManualCheckSetupDialogComponent;
  let service: jasmine.SpyObj<ManualCheckService>;
  let dialogRef: jasmine.SpyObj<MatDialogRef<ManualCheckSetupDialogComponent, ManualCheckSetupDialogResult>>;

  const presets: ManualCheckPreset[] = [
    preset(1, 'First'),
    preset(2, 'Preferred'),
  ];

  beforeEach(async () => {
    service = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', [
      'getPresets',
      'getConditionOperators',
      'createPreset',
      'updatePreset',
      'deletePreset',
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

    await TestBed.configureTestingModule({
      imports: [ManualCheckSetupDialogComponent],
      providers: [
        { provide: ManualCheckService, useValue: service },
        { provide: MatDialogRef, useValue: dialogRef },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            gameId: 440,
            gameName: 'Team Fortress 2',
            gameUrlName: 'Steam Market',
            preselectedPresetId: 2,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ManualCheckSetupDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('preselects and prefills the requested game preset', () => {
    expect(service.getPresets).toHaveBeenCalledWith(440);
    expect(component.selectedPresetId).toBe(2);
    expect(component.name).toBe('Preferred');
    expect(component.listingLimit).toBe(10);
    expect(component.customCooldown).toBeFalse();
    expect(component.bypassCache).toBeFalse();
    expect(component.criteria[0].valueContains).toBe('Mean Green');
    expect(component.canStart).toBeTrue();
    expect(component.loading).toBeFalse();
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
      listingLimit: 10,
      cooldownMinutes: null,
      cooldownSeconds: null,
    }));
    expect(component.canStart).toBeTrue();
  });

  it('allows one to twenty-five valid criterion rows', () => {
    for (let i = 1; i < 25; i++) {
      component.addCriterion();
      component.criteria[i].valueContains = `Value ${i}`;
    }
    component.criteria[0].valueContains = 'Value 0';
    component.addCriterion();

    expect(component.criteria.length).toBe(25);
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

  it('creates a new preset and starts it in one action', () => {
    const saved = preset(3, 'One click');
    service.createPreset.and.returnValue(of(saved));
    component.newPreset();
    component.name = saved.name;
    component.criteria[0].valueContains = 'Mean Green';
    component.bypassCache = true;

    component.startOrSave();

    expect(service.createPreset).toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith({ presetId: 3, bypassCache: true });
  });

  it('starts an existing preset with the selected cache behavior without marking it dirty', () => {
    component.bypassCache = true;

    component.startOrSave();

    expect(service.updatePreset).not.toHaveBeenCalled();
    expect(dialogRef.close).toHaveBeenCalledWith({ presetId: 2, bypassCache: true });
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
  ): ManualCheckPreset {
    return {
      id,
      gameId: 440,
      gameName: 'Team Fortress 2',
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
