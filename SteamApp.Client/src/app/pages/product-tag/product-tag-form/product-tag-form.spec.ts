import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductTagForm } from './product-tag-form';

describe('ProductTagForm', () => {
  let component: ProductTagForm;
  let fixture: ComponentFixture<ProductTagForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductTagForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductTagForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
