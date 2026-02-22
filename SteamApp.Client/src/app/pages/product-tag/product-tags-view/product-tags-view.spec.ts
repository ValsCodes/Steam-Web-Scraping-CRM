import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductTagsView } from './product-tags-view';

describe('ProductTagsView', () => {
  let component: ProductTagsView;
  let fixture: ComponentFixture<ProductTagsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductTagsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductTagsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
