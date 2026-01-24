import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameUrlProductForm } from './game-url-product-form';

describe('GameUrlProductForm', () => {
  let component: GameUrlProductForm;
  let fixture: ComponentFixture<GameUrlProductForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameUrlProductForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameUrlProductForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
