import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PixelForm } from './pixel-form';

describe('PixelForm', () => {
  let component: PixelForm;
  let fixture: ComponentFixture<PixelForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PixelForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PixelForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
