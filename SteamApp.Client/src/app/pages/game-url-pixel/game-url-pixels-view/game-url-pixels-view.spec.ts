import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameUrlPixelsView } from './game-url-pixels-view';

describe('GameUrlPixelsView', () => {
  let component: GameUrlPixelsView;
  let fixture: ComponentFixture<GameUrlPixelsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameUrlPixelsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameUrlPixelsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
