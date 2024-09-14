import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManualModeComponent } from './manual-mode.component';

describe('ManualModeComponent', () => {
  let component: ManualModeComponent;
  let fixture: ComponentFixture<ManualModeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManualModeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManualModeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
