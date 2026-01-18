import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WishListForm } from './wish-list-form';

describe('WishListForm', () => {
  let component: WishListForm;
  let fixture: ComponentFixture<WishListForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WishListForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WishListForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
