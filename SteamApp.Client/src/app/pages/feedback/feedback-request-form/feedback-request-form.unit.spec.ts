import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';

import {
  FeedbackRequest,
  FeedbackRequestHistoryAction,
  FeedbackRequestStatus,
  FeedbackRequestType,
} from '../../../models/feedback-request.model';
import { FeedbackRequestService } from '../../../services/feedback-request/feedback-request.service';
import { FeedbackRequestForm } from './feedback-request-form';

describe('FeedbackRequestForm', () => {
  let fixture: ComponentFixture<FeedbackRequestForm>;
  let component: FeedbackRequestForm;
  let service: jasmine.SpyObj<FeedbackRequestService>;
  let router: jasmine.SpyObj<Router>;
  let routeId: string | null;

  const existingRequest: FeedbackRequest = {
    id: 7,
    referenceId: 'FB-000007',
    type: FeedbackRequestType.Bug,
    title: 'Export bug',
    description: 'Export should preserve filters.',
    area: 'Exports',
    status: FeedbackRequestStatus.Processed,
    createdAtUtc: '2026-06-27T10:00:00Z',
    updatedAtUtc: '2026-06-27T10:00:00Z',
    statusChangedAtUtc: '2026-06-27T10:00:00Z',
  };

  beforeEach(async () => {
    routeId = null;
    service = jasmine.createSpyObj<FeedbackRequestService>('FeedbackRequestService', [
      'create',
      'getById',
      'getHistory',
      'update',
    ]);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    service.create.and.returnValue(of(existingRequest));
    service.getById.and.returnValue(of(existingRequest));
    service.getHistory.and.returnValue(of([
      {
        id: 1,
        feedbackRequestId: 7,
        action: FeedbackRequestHistoryAction.Created,
        createdAtUtc: '2026-06-27T10:00:00Z',
        previousTitle: null,
        newTitle: 'Export bug',
        previousDescription: null,
        newDescription: 'Export should preserve filters.',
        previousStatus: null,
        newStatus: FeedbackRequestStatus.Processed,
      },
      {
        id: 2,
        feedbackRequestId: 7,
        action: FeedbackRequestHistoryAction.StatusChanged,
        createdAtUtc: '2026-06-27T11:00:00Z',
        previousStatus: FeedbackRequestStatus.Active,
        newStatus: FeedbackRequestStatus.Processed,
      },
    ]));
    service.update.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [FeedbackRequestForm],
      providers: [
        { provide: FeedbackRequestService, useValue: service },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useFactory: () => ({
            snapshot: {
              paramMap: convertToParamMap(routeId ? { id: routeId } : {}),
            },
          }),
        },
      ],
    }).compileComponents();
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(FeedbackRequestForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('creates an active request without sending a status payload', () => {
    createComponent();

    component.form.patchValue({
      type: FeedbackRequestType.Feedback,
      title: '  Improve filters  ',
      description: '  Make filters easier to scan.  ',
      area: '  Catalog  ',
    });

    component.onSubmit();

    expect(service.create).toHaveBeenCalledWith({
      type: FeedbackRequestType.Feedback,
      title: 'Improve filters',
      description: 'Make filters easier to scan.',
      area: 'Catalog',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/feedback']);
  });

  it('loads an existing request and saves details with status', () => {
    routeId = '7';
    createComponent();

    expect(component.isEditMode).toBeTrue();
    expect(component.form.controls.status.enabled).toBeTrue();
    expect(component.form.controls.title.value).toBe('Export bug');
    expect(service.getHistory).toHaveBeenCalledWith(7);

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Ticket FB-000007');
    expect(text).toContain('Ticket History');
    expect(text).toContain('Export should preserve filters.');
    expect(text).toContain('Status Changed');
    expect(text).toContain('Active');
    expect(text).toContain('Processed');

    component.form.patchValue({
      status: FeedbackRequestStatus.Closed,
      title: 'Updated export bug',
    });

    component.onSubmit();

    expect(service.update).toHaveBeenCalledWith(7, jasmine.objectContaining({
      title: 'Updated export bug',
      status: FeedbackRequestStatus.Closed,
    }));
    expect(router.navigate).toHaveBeenCalledWith(['/feedback']);
  });
});
