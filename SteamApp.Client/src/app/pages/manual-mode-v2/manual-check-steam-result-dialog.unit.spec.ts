import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

import { ManualCheckProductTrace } from '../../models';
import { ManualCheckSteamResultDialogComponent } from './manual-check-steam-result-dialog.component';

describe('ManualCheckSteamResultDialogComponent', () => {
  let fixture: ComponentFixture<ManualCheckSteamResultDialogComponent>;

  const trace: ManualCheckProductTrace = {
    productId: 1,
    productName: 'Rocket Launcher',
    fullUrl: 'https://steamcommunity.com/market/listings/440/Rocket%20Launcher',
    matchEvaluated: true,
    matched: true,
    matchedAssetCount: 1,
    steamApiResultJson: '{"success":true,"value":"<script>alert(1)</script>"}',
    durationMilliseconds: 10,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManualCheckSteamResultDialogComponent],
      providers: [{ provide: MAT_DIALOG_DATA, useValue: trace }],
    }).compileComponents();

    fixture = TestBed.createComponent(ManualCheckSteamResultDialogComponent);
    fixture.detectChanges();
  });

  it('pretty-prints the parsed Steam response as inert text', () => {
    const pre: HTMLElement = fixture.nativeElement.querySelector('pre');

    expect(pre.textContent).toContain('"success": true');
    expect(pre.textContent).toContain('<script>alert(1)</script>');
    expect(pre.querySelector('script')).toBeNull();
  });
});
