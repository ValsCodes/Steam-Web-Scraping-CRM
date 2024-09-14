import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellListingComponent } from './sell-listing.component';

describe('SellListingComponent', () => {
  let component: SellListingComponent;
  let fixture: ComponentFixture<SellListingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellListingComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SellListingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
