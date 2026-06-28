import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import {
  FeedbackRequestStatus,
  FeedbackRequestType,
} from '../../../models/feedback-request.model';
import { FeedbackRequestService } from '../../../services/feedback-request/feedback-request.service';
import { FeedbackSendPage } from './feedback-send-page';

describe('FeedbackSendPage', () => {
  let fixture: ComponentFixture<FeedbackSendPage>;
  let component: FeedbackSendPage;
  let service: jasmine.SpyObj<FeedbackRequestService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    service = jasmine.createSpyObj<FeedbackRequestService>('FeedbackRequestService', ['create']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    service.create.and.returnValue(of({
      id: 1,
      referenceId: 'FB-000001',
      type: FeedbackRequestType.Bug,
      title: 'Broken export',
      description: 'Export should preserve filters.',
      area: 'Exports',
      status: FeedbackRequestStatus.Active,
      createdAtUtc: '2026-06-27T10:00:00Z',
      updatedAtUtc: '2026-06-27T10:00:00Z',
      statusChangedAtUtc: '2026-06-27T10:00:00Z',
    }));

    await TestBed.configureTestingModule({
      imports: [FeedbackSendPage],
      providers: [
        { provide: FeedbackRequestService, useValue: service },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FeedbackSendPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('submits a simplified active request without status editing', () => {
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('Tell us what needs attention');
    expect(text).not.toContain('Status');

    component.form.patchValue({
      type: FeedbackRequestType.Bug,
      title: '  Broken export  ',
      description: '  Export should preserve filters.  ',
      area: '  Exports  ',
    });

    component.onSubmit();

    expect(service.create).toHaveBeenCalledWith({
      type: FeedbackRequestType.Bug,
      title: 'Broken export',
      description: 'Export should preserve filters.',
      area: 'Exports',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/feedback']);
  });
});
