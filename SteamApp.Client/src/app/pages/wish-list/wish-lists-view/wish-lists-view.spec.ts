import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WishListsView } from './wish-lists-view';

describe('WishListsView', () => {
  let component: WishListsView;
  let fixture: ComponentFixture<WishListsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WishListsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WishListsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
