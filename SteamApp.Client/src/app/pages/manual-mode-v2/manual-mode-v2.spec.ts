import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManualModeV2 } from './manual-mode-v2';

describe('ManualModeV2', () => {
  let component: ManualModeV2;
  let fixture: ComponentFixture<ManualModeV2>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManualModeV2]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManualModeV2);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
