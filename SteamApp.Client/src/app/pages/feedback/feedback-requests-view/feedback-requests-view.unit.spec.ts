import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import {
  FeedbackRequest,
  FeedbackRequestStatus,
  FeedbackRequestType,
} from '../../../models/feedback-request.model';
import { FeedbackRequestService } from '../../../services/feedback-request/feedback-request.service';
import { FeedbackRequestsView } from './feedback-requests-view';

describe('FeedbackRequestsView', () => {
  let fixture: ComponentFixture<FeedbackRequestsView>;
  let component: FeedbackRequestsView;
  let service: jasmine.SpyObj<FeedbackRequestService>;
  let router: jasmine.SpyObj<Router>;

  const requests: FeedbackRequest[] = [
    {
      id: 1,
      referenceId: 'FB-000001',
      type: FeedbackRequestType.Feedback,
      title: 'Improve filters',
      description: 'Make filters easier to scan.',
      area: 'Catalog',
      status: FeedbackRequestStatus.Active,
      createdAtUtc: '2026-06-27T10:00:00Z',
      updatedAtUtc: '2026-06-27T10:00:00Z',
      statusChangedAtUtc: '2026-06-27T10:00:00Z',
    },
    {
      id: 2,
      referenceId: 'FB-000002',
      type: FeedbackRequestType.Bug,
      title: 'Export bug',
      description: 'Export should preserve filters.',
      area: 'Exports',
      status: FeedbackRequestStatus.Processed,
      createdAtUtc: '2026-06-27T11:00:00Z',
      updatedAtUtc: '2026-06-27T11:00:00Z',
      statusChangedAtUtc: '2026-06-27T11:00:00Z',
    },
  ];

  beforeEach(async () => {
    service = jasmine.createSpyObj<FeedbackRequestService>('FeedbackRequestService', [
      'getAll',
      'updateStatus',
    ]);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    service.getAll.and.returnValue(of(requests));
    service.updateStatus.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [FeedbackRequestsView, NoopAnimationsModule],
      providers: [
        { provide: FeedbackRequestService, useValue: service },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FeedbackRequestsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders feedback requests and filters them by status', () => {
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('Feedback Requests');
    expect(text).toContain('FB-000001');
    expect(text).toContain('Improve filters');
    expect(text).toContain('Export bug');

    component.textFilterControl.setValue('FB-000001');
    component.statusFilterControl.setValue(FeedbackRequestStatus.Active);
    fixture.detectChanges();

    expect(component.dataSource.data.map((request) => request.title)).toEqual(['Improve filters']);
  });

  it('updates request status from the row action handler', () => {
    component.statusButtonClicked(requests[0], FeedbackRequestStatus.Closed);

    expect(service.updateStatus).toHaveBeenCalledWith(1, {
      status: FeedbackRequestStatus.Closed,
    });
    expect(component.dataSource.data.find((request) => request.id === 1)?.status)
      .toBe(FeedbackRequestStatus.Closed);
  });

  it('navigates to the create, send, and edit routes', () => {
    component.createButtonClicked();
    component.sendButtonClicked();
    component.editButtonClicked(2);

    expect(router.navigate).toHaveBeenCalledWith(['/feedback/create']);
    expect(router.navigate).toHaveBeenCalledWith(['/feedback/send']);
    expect(router.navigate).toHaveBeenCalledWith(['/feedback/edit', 2]);
  });
});
