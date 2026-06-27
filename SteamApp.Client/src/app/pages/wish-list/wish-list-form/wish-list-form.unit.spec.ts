import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of } from 'rxjs';

import { WishListForm } from './wish-list-form';
import { GameService, WishListService } from '../../../services';

describe('WishListForm', () => {
  async function setup(id?: string) {
    const wishListService = jasmine.createSpyObj<WishListService>('WishListService', [
      'create',
      'getById',
      'update',
    ]);
    const gameService = jasmine.createSpyObj<GameService>('GameService', ['getAll']);
    const router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    gameService.getAll.and.returnValue(of([
      {
        id: 1,
        name: 'Portal',
        baseUrl: 'https://store.steampowered.com',
        pageUrl: 'https://store.steampowered.com/app/400/Portal/',
        internalId: 400,
        isActive: true,
      },
    ]));
    wishListService.getById.and.returnValue(of({
      id: Number(id ?? 1),
      gameId: 1,
      gameName: 'Portal',
      name: 'Portal under 5 EUR',
      pageUrl: 'https://store.steampowered.com/app/400/Portal/',
      price: 4.99,
      isActive: true,
    }));

    await TestBed.configureTestingModule({
      imports: [WishListForm],
      providers: [
        { provide: WishListService, useValue: wishListService },
        { provide: GameService, useValue: gameService },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap(id ? { id } : {}),
            },
          },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(WishListForm);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    return { component, fixture };
  }

  it('renders create mode Price Alert copy and helper text', async () => {
    const { fixture } = await setup();
    const text = textContent(fixture);

    expect(text).toContain('Price Alerts');
    expect(text).toContain('Create Price Alert');
    expect(text).toContain('Select a game, set the price you want to watch for');
    expect(text).toContain('Alert Name');
    expect(text).toContain('Target Price');
    expect(text).toContain("Check Price Now uses the selected game's Steam page URL");
    expect(text).toContain('Enable monitoring for this alert');
    expect(text).toContain('Create price alert');
  });

  it('renders edit mode title and save action copy', async () => {
    const { component, fixture } = await setup('7');

    expect(component.isEditMode).toBeTrue();
    expect(textContent(fixture)).toContain('Edit Price Alert');
    expect(textContent(fixture)).toContain('Save changes');
  });
});

function textContent(fixture: ComponentFixture<WishListForm>): string {
  return fixture.nativeElement.textContent ?? '';
}
