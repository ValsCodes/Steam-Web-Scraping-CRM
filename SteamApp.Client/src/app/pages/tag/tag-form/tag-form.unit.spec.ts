import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of } from 'rxjs';

import { TagForm } from './tag-form';
import { GameService, ItemGroupService, TagService } from '../../../services';

describe('TagForm', () => {
  async function setup(id?: string) {
    const tagService = jasmine.createSpyObj<TagService>('TagService', [
      'create',
      'getById',
      'update',
    ]);
    const itemGroupService = jasmine.createSpyObj<ItemGroupService>('ItemGroupService', [
      'create',
      'getByGame',
    ]);
    const gameService = jasmine.createSpyObj<GameService>('GameService', ['getAll']);
    const router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    gameService.getAll.and.returnValue(of([{
      id: 1,
      name: 'Portal',
      baseUrl: '',
      pageUrl: null,
      internalId: 400,
      isActive: true,
    }]));
    itemGroupService.getByGame.and.returnValue(of([{
      id: 10,
      gameId: 1,
      name: 'Category',
    }]));
    itemGroupService.create.and.returnValue(of({
      id: 11,
      gameId: 1,
      name: 'Priority',
    }));
    tagService.getById.and.returnValue(of({
      id: Number(id ?? 1),
      gameId: 1,
      gameName: 'Portal',
      name: 'Featured',
      isActive: true,
      itemGroupId: 10,
      itemGroupName: 'Category',
    }));
    tagService.create.and.returnValue(of({
      id: 20,
      gameId: 1,
      gameName: 'Portal',
      name: 'Featured',
      isActive: true,
      itemGroupId: null,
      itemGroupName: null,
    }));
    tagService.update.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [TagForm],
      providers: [
        { provide: TagService, useValue: tagService },
        { provide: ItemGroupService, useValue: itemGroupService },
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

    const fixture = TestBed.createComponent(TagForm);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    return { component, fixture, tagService, itemGroupService, router };
  }

  it('submits a tag without requiring a type', async () => {
    const { component, tagService, router } = await setup();
    component.form.setValue({
      gameId: 1,
      itemGroupId: null,
      name: 'Featured',
      isActive: true,
    });

    component.onSubmit();

    expect(tagService.create).toHaveBeenCalledWith({
      gameId: 1,
      itemGroupId: null,
      name: 'Featured',
      isActive: true,
    });
    expect(router.navigate).toHaveBeenCalledWith(['/tags']);
  });

  it('creates a type inline and selects it', async () => {
    const { component, itemGroupService } = await setup();
    component.form.controls.gameId.setValue(1);
    component.newItemGroupName.setValue(' Priority ');

    component.createItemGroup();

    expect(itemGroupService.create).toHaveBeenCalledWith({ gameId: 1, name: 'Priority' });
    expect(component.form.controls.itemGroupId.value).toBe(11);
    expect(component.itemGroups().map(itemGroup => itemGroup.name)).toEqual(['Category', 'Priority']);
    expect(component.newItemGroupName.value).toBe('');
  });

  it('loads the selected type in edit mode and clears it when the game changes', async () => {
    const { component, itemGroupService } = await setup('7');

    expect(component.isEditMode).toBeTrue();
    expect(component.form.controls.itemGroupId.value).toBe(10);
    expect(itemGroupService.getByGame).toHaveBeenCalledWith(1);

    component.form.controls.gameId.enable({ emitEvent: false });
    component.form.controls.gameId.setValue(null);

    expect(component.form.controls.itemGroupId.value).toBeNull();
    expect(component.itemGroups()).toEqual([]);
  });
});
