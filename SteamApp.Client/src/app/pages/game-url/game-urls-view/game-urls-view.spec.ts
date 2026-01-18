import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameUrlsView } from './game-urls-view';

describe('GameUrlsView', () => {
  let component: GameUrlsView;
  let fixture: ComponentFixture<GameUrlsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameUrlsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameUrlsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
