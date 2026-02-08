import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameUrlPixelForm } from './game-url-pixel-form';

describe('GameUrlPixelForm', () => {
  let component: GameUrlPixelForm;
  let fixture: ComponentFixture<GameUrlPixelForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameUrlPixelForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameUrlPixelForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
