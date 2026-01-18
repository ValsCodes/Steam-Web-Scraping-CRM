import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameUrlForm } from './game-url-form';

describe('GameUrlForm', () => {
  let component: GameUrlForm;
  let fixture: ComponentFixture<GameUrlForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameUrlForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameUrlForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
