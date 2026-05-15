import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { defer, of } from 'rxjs';

import { Game } from '../models';
import { GamesView } from '../pages/game/games-view/games-view';
import { GameService } from '../services/game/game.service';

describe('GamesView integration tests', () => {
  const games: Game[] = [
    {
      id: 1,
      name: 'Alpha Game',
      baseUrl: 'https://steamcommunity.com',
      pageUrl: 'https://steamcommunity.com/app/440',
      internalId: 440,
      isActive: true,
    },
    {
      id: 2,
      name: 'Beta Game',
      baseUrl: 'https://steamcommunity.com',
      pageUrl: 'https://steamcommunity.com/app/570',
      internalId: 570,
      isActive: false,
    },
  ];

  let fixture: ComponentFixture<GamesView>;
  let component: GamesView;
  let gameService: {
    getAll: jest.Mock;
    updateStatus: jest.Mock;
    delete: jest.Mock;
  };
  let router: {
    navigate: jest.Mock;
  };
  let dialog: {
    open: jest.Mock;
  };

  beforeEach(async () => {
    gameService = {
      getAll: jest.fn(() => defer(() => Promise.resolve(games))),
      updateStatus: jest.fn(() => of(undefined)),
      delete: jest.fn(() => of(undefined)),
    };
    router = {
      navigate: jest.fn(),
    };
    dialog = {
      open: jest.fn(() => ({ afterClosed: () => of(true) })),
    };

    await TestBed.configureTestingModule({
      imports: [GamesView, NoopAnimationsModule],
      providers: [
        { provide: GameService, useValue: gameService },
        { provide: Router, useValue: router },
        { provide: MatDialog, useValue: dialog },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(GamesView);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('loads games into the table and renders active status', () => {
    const text = fixture.nativeElement.textContent as string;

    expect(gameService.getAll).toHaveBeenCalledTimes(1);
    expect(component.dataSource.data).toHaveLength(2);
    expect(text).toContain('Alpha Game');
    expect(text).toContain('Beta Game');
  });

  it('filters the visible game rows by name', () => {
    component.onNameFilterChanged('beta');
    fixture.detectChanges();

    expect(component.dataSource.data.map((game) => game.name)).toEqual(['Beta Game']);
  });

  it('routes to create and edit screens from table actions', () => {
    component.createButtonClicked();
    component.editButtonClicked(2);

    expect(router.navigate).toHaveBeenCalledWith(['/games/create']);
    expect(router.navigate).toHaveBeenCalledWith(['/games/edit', 2]);
  });

  it('updates active status in-place after the service confirms', () => {
    component.activeButtonClicked(games[0]);

    expect(gameService.updateStatus).toHaveBeenCalledWith({ id: 1, isActive: false });
    expect(component.games().find((game) => game.id === 1)?.isActive).toBe(false);
  });
});
