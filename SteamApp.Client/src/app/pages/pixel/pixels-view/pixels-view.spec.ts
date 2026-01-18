import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PixelsView } from './pixels-view';

describe('PixelsView', () => {
  let component: PixelsView;
  let fixture: ComponentFixture<PixelsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PixelsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PixelsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
