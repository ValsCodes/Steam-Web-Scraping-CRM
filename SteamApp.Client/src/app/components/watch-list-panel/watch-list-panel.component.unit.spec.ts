import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BehaviorSubject, of } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';

import { WatchList } from '../../models';
import {
  AuthService,
  EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY,
  WatchListService,
} from '../../services';
import { WatchListPanelComponent } from './watch-list-panel.component';

describe('WatchListPanelComponent', () => {
  const item: WatchList = {
    id: 11,
    name: 'Daily prices',
    url: 'https://steam.example/watch/11',
    registrationDate: '2026-10-06',
    isActive: true,
  };

  let fixture: ComponentFixture<WatchListPanelComponent>;
  let component: WatchListPanelComponent;
  let loggedIn$: BehaviorSubject<boolean>;
  let watchListService: jasmine.SpyObj<WatchListService>;
  let dialog: jasmine.SpyObj<MatDialog>;

  beforeEach(async () => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);
    loggedIn$ = new BehaviorSubject(true);
    watchListService = jasmine.createSpyObj<WatchListService>('WatchListService', [
      'getAll',
      'updateStatus',
      'delete',
    ]);
    dialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    watchListService.getAll.and.returnValue(of([item]));
    watchListService.updateStatus.and.returnValue(of(void 0));
    watchListService.delete.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [WatchListPanelComponent],
      providers: [
        { provide: AuthService, useValue: { loggedIn$: loggedIn$.asObservable() } },
        { provide: WatchListService, useValue: watchListService },
        { provide: MatDialog, useValue: dialog },
        { provide: Router, useValue: { url: '/home', navigateByUrl: jasmine.createSpy('navigateByUrl') } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WatchListPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);
  });

  it('loads on first open and clears authenticated data on logout', () => {
    component.open();

    expect(watchListService.getAll).toHaveBeenCalledTimes(1);
    expect(component.items()).toEqual([item]);
    expect(component.isOpen()).toBeTrue();

    loggedIn$.next(false);

    expect(component.isOpen()).toBeFalse();
    expect(component.items()).toEqual([]);
  });

  it('toggles an item using the existing status endpoint', () => {
    component.open();

    component.toggleActive(item);

    expect(watchListService.updateStatus).toHaveBeenCalledOnceWith({
      id: item.id,
      isActive: false,
    });
    expect(component.items()[0].isActive).toBeFalse();
  });

  it('renders accepted item URLs as protected new-window links', () => {
    localStorage.setItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY, 'accepted');
    component.open();
    fixture.detectChanges();

    const anchor = fixture.nativeElement.querySelector('.watch-list-item__link') as HTMLAnchorElement;

    expect(anchor.getAttribute('href')).toBe('https://steam.example/watch/11');
    expect(anchor.getAttribute('target')).toBe('_blank');
    expect(anchor.getAttribute('rel')).toBe('noopener noreferrer');
    expect(anchor.getAttribute('referrerpolicy')).toBe('no-referrer');
  });

  it('removes a confirmed item from the panel', () => {
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as never);
    component.open();

    component.delete(item);

    expect(watchListService.delete).toHaveBeenCalledOnceWith(item.id);
    expect(component.items()).toEqual([]);
  });
});
