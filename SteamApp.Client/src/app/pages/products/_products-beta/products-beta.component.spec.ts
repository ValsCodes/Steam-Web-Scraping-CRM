import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductsBetaComponent } from './products-beta.component';

describe('SellListingsComponent', () => {
  let component: ProductsBetaComponent;
  let fixture: ComponentFixture<ProductsBetaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductsBetaComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductsBetaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
