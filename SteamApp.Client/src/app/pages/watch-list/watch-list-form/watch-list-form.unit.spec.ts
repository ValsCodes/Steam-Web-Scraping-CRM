import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of } from 'rxjs';

import { WatchListService } from '../../../services';
import { WatchListForm } from './watch-list-form';

describe('WatchListForm', () => {
  let component: WatchListForm;
  let fixture: ComponentFixture<WatchListForm>;
  let watchListService: jasmine.SpyObj<WatchListService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    watchListService = jasmine.createSpyObj<WatchListService>('WatchListService', [
      'create',
      'getById',
      'update',
    ]);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [WatchListForm],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({}) } },
        },
        { provide: Router, useValue: router },
        { provide: WatchListService, useValue: watchListService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WatchListForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('submits a new watch item without a registration date', () => {
    watchListService.create.and.returnValue(of({
      id: 3,
      name: 'Watch Gamma',
      url: 'https://steam.example/watch/3',
      registrationDate: '2026-10-02',
      isActive: true,
    }));
    component.form.setValue({
      name: 'Watch Gamma',
      url: 'https://steam.example/watch/3',
      isActive: true,
    });

    component.onSubmit();

    expect(watchListService.create).toHaveBeenCalledOnceWith({
      name: 'Watch Gamma',
      url: 'https://steam.example/watch/3',
      isActive: true,
    });
    expect(router.navigate).toHaveBeenCalledOnceWith(['/watch-list']);
  });
});
