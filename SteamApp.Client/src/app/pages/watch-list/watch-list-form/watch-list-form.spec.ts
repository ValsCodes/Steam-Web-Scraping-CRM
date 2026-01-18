import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatchListForm } from './watch-list-form';

describe('WatchListForm', () => {
  let component: WatchListForm;
  let fixture: ComponentFixture<WatchListForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WatchListForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WatchListForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
