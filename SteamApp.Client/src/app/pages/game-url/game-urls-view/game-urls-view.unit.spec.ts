import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of, Subject } from 'rxjs';
import * as XLSX from 'xlsx';

import { GameUrlsView } from './game-urls-view';
import { GameUrl } from '../../../models';
import { GameService, GameUrlService, ScrapingModeService } from '../../../services';

describe('GameUrlsView item groups', () => {
  const urls: GameUrl[] = [
    { id: 1, gameId: 1, gameName: 'Portal', name: 'Grouped', itemGroupId: 10, itemGroupName: 'Category', isActive: true },
    { id: 2, gameId: 1, gameName: 'Portal', name: 'Legacy', itemGroupId: null, itemGroupName: null, isActive: true },
  ];

  async function setup() {
    const source = new Subject<GameUrl[]>();
    await TestBed.configureTestingModule({
      imports: [GameUrlsView],
      providers: [
        { provide: GameUrlService, useValue: { getAll: () => source } },
        { provide: GameService, useValue: { getAll: () => of([]) } },
        { provide: ScrapingModeService, useValue: { getAll: () => of([]) } },
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } },
        { provide: MatDialog, useValue: {} },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(GameUrlsView);
    fixture.detectChanges();
    source.next(urls);
    fixture.detectChanges();
    return { component: fixture.componentInstance, fixture };
  }

  it('displays group names and Ungrouped for legacy URLs', async () => {
    const { component, fixture } = await setup();
    expect(component.displayedColumns).toContain('itemGroupName');
    const cells: NodeListOf<HTMLElement> = fixture.nativeElement.querySelectorAll('td.mat-column-itemGroupName');
    expect(Array.from(cells).map(cell => cell.textContent?.trim())).toEqual(['Category', 'Ungrouped']);
  });

  it('includes groups in exported rows', async () => {
    const { component } = await setup();
    const toSheet = spyOn(XLSX.utils, 'json_to_sheet').and.returnValue({});
    spyOn(XLSX.utils, 'book_new').and.returnValue({ SheetNames: [], Sheets: {} });
    spyOn(XLSX.utils, 'book_append_sheet').and.throwError('Stop before browser download.');

    expect(() => component.exportButtonClicked()).toThrowError('Stop before browser download.');
    expect(toSheet).toHaveBeenCalledWith([
      jasmine.objectContaining({ group: 'Category' }),
      jasmine.objectContaining({ group: 'Ungrouped' }),
    ]);
  });
});
