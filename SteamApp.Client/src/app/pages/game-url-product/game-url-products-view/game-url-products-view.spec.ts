import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameUrlProductsView } from './game-url-products-view';

describe('GameUrlProductsView', () => {
  let component: GameUrlProductsView;
  let fixture: ComponentFixture<GameUrlProductsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameUrlProductsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameUrlProductsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
