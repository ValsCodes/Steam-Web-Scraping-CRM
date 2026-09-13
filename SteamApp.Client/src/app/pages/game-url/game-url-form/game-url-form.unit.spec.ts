import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { GameUrlForm } from './game-url-form';
import { ItemGroup } from '../../../models';
import {
  GameService,
  GameUrlPixelService,
  GameUrlProductService,
  GameUrlService,
  ItemGroupService,
  PixelService,
  ProductService,
  ScrapingModeService,
} from '../../../services';

describe('GameUrlForm item groups', () => {
  async function setup(id?: string) {
    const gameUrlService = jasmine.createSpyObj<GameUrlService>('GameUrlService', ['create', 'getById', 'update']);
    const itemGroupService = jasmine.createSpyObj<ItemGroupService>('ItemGroupService', ['create', 'getByGame']);
    const router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    const lookupService = { getAll: () => of([]) };
    const relationService = { existsByGameUrl: () => of([]) };

    itemGroupService.getByGame.and.returnValue(of([{ id: 10, gameId: 1, name: 'Category' }]));
    itemGroupService.create.and.returnValue(of({ id: 11, gameId: 1, name: 'Priority' }));
    gameUrlService.getById.and.returnValue(of({
      id: Number(id ?? 1),
      name: 'Market',
      gameId: 1,
      gameName: 'Portal',
      itemGroupId: 10,
      itemGroupName: 'Category',
      scrapingModeId: 1,
      isActive: true,
    }));
    gameUrlService.create.and.returnValue(of({
      id: 20,
      name: 'Market',
      gameId: 1,
      gameName: 'Portal',
      itemGroupId: null,
      itemGroupName: null,
      isActive: true,
    }));
    gameUrlService.update.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [GameUrlForm],
      providers: [
        { provide: GameUrlService, useValue: gameUrlService },
        { provide: ItemGroupService, useValue: itemGroupService },
        { provide: GameService, useValue: lookupService },
        { provide: ProductService, useValue: lookupService },
        { provide: PixelService, useValue: lookupService },
        { provide: ScrapingModeService, useValue: lookupService },
        { provide: GameUrlProductService, useValue: relationService },
        { provide: GameUrlPixelService, useValue: relationService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(id ? { id } : {}) } } },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(GameUrlForm);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    return { component, fixture, gameUrlService, itemGroupService, router };
  }

  it('loads groups for the selected game and clears them when no game is selected', async () => {
    const { component, itemGroupService } = await setup();
    expect(component.newItemGroupName.disabled).toBeTrue();
    component.form.controls.gameId.setValue(1);
    component.form.controls.itemGroupId.setValue(10);

    expect(itemGroupService.getByGame).toHaveBeenCalledWith(1);
    expect(component.itemGroups().map(group => group.name)).toEqual(['Category']);
    expect(component.newItemGroupName.enabled).toBeTrue();

    component.form.controls.gameId.setValue(null);
    expect(component.form.controls.itemGroupId.value).toBeNull();
    expect(component.itemGroups()).toEqual([]);
    expect(component.newItemGroupName.disabled).toBeTrue();
  });

  it('ignores a stale group lookup after switching games', async () => {
    const { component, itemGroupService } = await setup();
    const oldGroups = new Subject<ItemGroup[]>();
    itemGroupService.getByGame.and.returnValues(oldGroups, of([{ id: 12, gameId: 2, name: 'Other game' }]));
    component.form.controls.gameId.setValue(1);
    component.form.controls.gameId.setValue(2);
    oldGroups.next([{ id: 10, gameId: 1, name: 'Category' }]);

    expect(component.itemGroups().map(group => group.id)).toEqual([12]);
    expect(component.form.controls.itemGroupId.value).toBeNull();
  });

  it('creates a trimmed group inline and selects it in the form', async () => {
    const { component, fixture, itemGroupService } = await setup();
    component.form.controls.gameId.setValue(1);
    component.newItemGroupName.setValue(' Priority ');
    component.createItemGroup();
    fixture.detectChanges();

    expect(itemGroupService.create).toHaveBeenCalledWith({ gameId: 1, name: 'Priority' });
    expect(component.form.controls.itemGroupId.value).toBe(11);
    expect(component.itemGroups().map(group => group.name)).toEqual(['Category', 'Priority']);
    expect(component.newItemGroupName.value).toBe('');
    expect(component.isCreatingItemGroup).toBeFalse();
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('#game-url-item-group-id');
    expect(Array.from(select.options).map(option => option.textContent?.trim())).toEqual(['No group', 'Category', 'Priority']);
    expect(select.selectedOptions[0].textContent?.trim()).toBe('Priority');
  });

  it('rejects blank and over-length group names without sending a request', async () => {
    const { component, itemGroupService } = await setup();
    component.form.controls.gameId.setValue(1);
    for (const name of ['   ', 'x'.repeat(256)]) {
      component.newItemGroupName.setValue(name);
      component.createItemGroup();
    }

    expect(itemGroupService.create).not.toHaveBeenCalled();
  });

  it('waits for the group lookup before allowing inline creation', async () => {
    const { component, itemGroupService } = await setup();
    const lookup = new Subject<ItemGroup[]>();
    itemGroupService.getByGame.and.returnValue(lookup);
    component.form.controls.gameId.setValue(1);
    component.newItemGroupName.setValue('Priority');
    component.createItemGroup();
    expect(component.isLoadingItemGroups).toBeTrue();
    expect(itemGroupService.create).not.toHaveBeenCalled();

    lookup.next([]);
    lookup.complete();
    component.createItemGroup();
    expect(component.form.controls.itemGroupId.value).toBe(11);
  });

  it('recovers from a failed lookup and a failed group creation', async () => {
    const { component, itemGroupService } = await setup();
    itemGroupService.getByGame.and.returnValues(throwError(() => new Error('Lookup failed')), of([]));
    component.form.controls.gameId.setValue(1);
    expect(component.isLoadingItemGroups).toBeFalse();
    expect(component.itemGroups()).toEqual([]);
    component.form.controls.gameId.setValue(2);
    expect(itemGroupService.getByGame).toHaveBeenCalledWith(2);

    itemGroupService.create.and.returnValue(throwError(() => new Error('Creation failed')));
    component.newItemGroupName.setValue('Priority');
    component.createItemGroup();
    expect(component.isCreatingItemGroup).toBeFalse();
    expect(component.newItemGroupName.enabled).toBeTrue();
    expect(component.form.controls.itemGroupId.value).toBeNull();
  });

  it('loads the assigned group in edit mode and sends null to clear it', async () => {
    const { component, itemGroupService, gameUrlService, router } = await setup('7');
    expect(component.isEditMode).toBeTrue();
    expect(component.form.controls.gameId.disabled).toBeTrue();
    expect(itemGroupService.getByGame).toHaveBeenCalledWith(1);
    expect(component.form.controls.itemGroupId.value).toBe(10);

    component.form.controls.itemGroupId.setValue(null);
    component.onSubmit();
    expect(gameUrlService.update).toHaveBeenCalledWith(7, jasmine.objectContaining({ itemGroupId: null }));
    expect(gameUrlService.create).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/game-urls']);
  });

  for (const itemGroupId of [null, 10]) {
    it(`submits the optional group ${itemGroupId} in the create payload`, async () => {
      const { component, gameUrlService, router } = await setup();
      component.form.patchValue({ gameId: 1, name: 'Market', itemGroupId });
      component.onSubmit();

      expect(gameUrlService.create).toHaveBeenCalledWith(jasmine.objectContaining({ gameId: 1, itemGroupId }));
      expect(router.navigate).toHaveBeenCalledWith(['/game-urls']);
    });
  }

  it('blocks submission while creating and does not assign a late result to another game', async () => {
    const { component, itemGroupService, gameUrlService } = await setup();
    const created = new Subject<ItemGroup>();
    itemGroupService.create.and.returnValue(created);
    component.form.controls.gameId.setValue(1);
    component.newItemGroupName.setValue('Priority');
    component.createItemGroup();
    component.onSubmit();
    expect(gameUrlService.create).not.toHaveBeenCalled();

    component.form.controls.gameId.setValue(2);
    created.next({ id: 11, gameId: 1, name: 'Priority' });
    created.complete();
    expect(component.form.controls.itemGroupId.value).toBeNull();
    expect(component.isCreatingItemGroup).toBeFalse();
    expect(component.newItemGroupName.enabled).toBeTrue();
  });
});
