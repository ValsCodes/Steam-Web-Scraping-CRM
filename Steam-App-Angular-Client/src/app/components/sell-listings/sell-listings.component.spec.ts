import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellListingsComponent } from './sell-listings.component';

describe('SellListingsComponent', () => {
  let component: SellListingsComponent;
  let fixture: ComponentFixture<SellListingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellListingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SellListingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
