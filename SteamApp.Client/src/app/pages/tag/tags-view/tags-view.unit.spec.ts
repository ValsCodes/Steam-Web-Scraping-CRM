import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of, Subject } from 'rxjs';

import { TagsView } from './tags-view';
import { GameService, ItemGroupService, TagService } from '../../../services';
import { Tag } from '../../../models';

describe('TagsView', () => {
  it('filters a selected game to ungrouped tags and renders the Group column', async () => {
    const tags$ = new Subject<Tag[]>();
    const tagService = jasmine.createSpyObj<TagService>('TagService', ['getAll']);
    const itemGroupService = jasmine.createSpyObj<ItemGroupService>('ItemGroupService', ['getByGame']);
    const gameService = jasmine.createSpyObj<GameService>('GameService', ['getAll']);
    const router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    const dialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    tagService.getAll.and.returnValue(tags$);
    itemGroupService.getByGame.and.returnValue(of([{
      id: 10,
      gameId: 1,
      name: 'Category',
    }]));
    gameService.getAll.and.returnValue(of([{
      id: 1,
      name: 'Portal',
      baseUrl: '',
      pageUrl: null,
      internalId: 400,
      isActive: true,
    }]));

    await TestBed.configureTestingModule({
      imports: [TagsView],
      providers: [
        { provide: TagService, useValue: tagService },
        { provide: ItemGroupService, useValue: itemGroupService },
        { provide: GameService, useValue: gameService },
        { provide: Router, useValue: router },
        { provide: MatDialog, useValue: dialog },
      ],
    }).compileComponents();

    const fixture: ComponentFixture<TagsView> = TestBed.createComponent(TagsView);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    tags$.next([
      createTag(1, 'Grouped', 10, 'Category'),
      createTag(2, 'Ungrouped', null, null),
    ]);
    fixture.detectChanges();

    component.gameIdControl.setValue(1);
    component.itemGroupIdControl.setValue('ungrouped');
    fixture.detectChanges();

    expect(component.dataSource.data.map(tag => tag.name)).toEqual(['Ungrouped']);
    expect(fixture.nativeElement.textContent).toContain('Group');
    expect(fixture.nativeElement.textContent).toContain('Ungrouped');
  });
});

function createTag(
  id: number,
  name: string,
  itemGroupId: number | null,
  itemGroupName: string | null,
): Tag {
  return {
    id,
    gameId: 1,
    gameName: 'Portal',
    name,
    isActive: true,
    itemGroupId,
    itemGroupName,
  };
}
