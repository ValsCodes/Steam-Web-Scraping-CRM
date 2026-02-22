import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatchListsView } from './watch-lists-view';

describe('WatchListsView', () => {
  let component: WatchListsView;
  let fixture: ComponentFixture<WatchListsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WatchListsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WatchListsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
