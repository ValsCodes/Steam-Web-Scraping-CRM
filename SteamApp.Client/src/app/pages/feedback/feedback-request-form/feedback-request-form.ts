import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, Observable } from 'rxjs';

import {
  CreateFeedbackRequest,
  FEEDBACK_REQUEST_STATUS_OPTIONS,
  FEEDBACK_REQUEST_TYPE_OPTIONS,
  FeedbackRequest,
  FeedbackRequestHistory,
  FeedbackRequestStatus,
  FeedbackRequestType,
  UpdateFeedbackRequest,
  feedbackRequestHistoryActionLabel,
  feedbackRequestStatusLabel,
  feedbackRequestTypeLabel,
} from '../../../models/feedback-request.model';
import { FeedbackRequestService } from '../../../services/feedback-request/feedback-request.service';

interface FeedbackRequestHistoryFieldChange {
  field: string;
  label: string;
  previousValue: string;
  newValue: string;
}

@Component({
  selector: 'steam-feedback-request-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './feedback-request-form.html',
  styleUrl: './feedback-request-form.scss',
})
export class FeedbackRequestForm implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  readonly typeOptions = FEEDBACK_REQUEST_TYPE_OPTIONS;
  readonly statusOptions = FEEDBACK_REQUEST_STATUS_OPTIONS;

  isEditMode = false;
  feedbackRequestId?: number;
  isSubmitting = false;
  isHistoryLoading = false;
  currentRequest?: FeedbackRequest;
  history: FeedbackRequestHistory[] = [];

  readonly form = this.fb.nonNullable.group({
    type: [FeedbackRequestType.Feedback, [Validators.required]],
    title: ['', [Validators.required, Validators.maxLength(140)]],
    description: ['', [Validators.required, Validators.maxLength(4000)]],
    area: ['', [Validators.maxLength(120)]],
    status: [{ value: FeedbackRequestStatus.Active, disabled: true }, [Validators.required]],
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly feedbackRequestService: FeedbackRequestService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.feedbackRequestId = Number(idParam);
      this.form.controls.status.enable({ emitEvent: false });
      this.loadFeedbackRequest(this.feedbackRequestId);
    }
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const request$: Observable<unknown> = this.isEditMode && this.feedbackRequestId
      ? this.feedbackRequestService.update(this.feedbackRequestId, this.createUpdatePayload())
      : this.feedbackRequestService.create(this.createCreatePayload());

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting = false;
        }),
      )
      .subscribe(() => {
        this.router.navigate(['/feedback']);
      });
  }

  cancel(): void {
    this.router.navigate(['/feedback']);
  }

  private loadFeedbackRequest(id: number): void {
    this.feedbackRequestService
      .getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((request) => {
        this.currentRequest = request;
        this.form.patchValue({
          type: request.type,
          title: request.title,
          description: request.description,
          area: request.area ?? '',
          status: request.status,
        });
      });

    this.loadHistory(id);
  }

  private loadHistory(id: number): void {
    this.isHistoryLoading = true;

    this.feedbackRequestService
      .getHistory(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isHistoryLoading = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe((history) => {
        this.history = history;
      });
  }

  historyActionLabel(action: FeedbackRequestHistory['action']): string {
    return feedbackRequestHistoryActionLabel(action);
  }

  historyFieldChanges(entry: FeedbackRequestHistory): FeedbackRequestHistoryFieldChange[] {
    return [
      this.createHistoryChange('type', 'Type', entry.previousType, entry.newType),
      this.createHistoryChange('title', 'Title', entry.previousTitle, entry.newTitle),
      this.createHistoryChange('description', 'Description', entry.previousDescription, entry.newDescription),
      this.createHistoryChange('area', 'Area', entry.previousArea, entry.newArea),
      this.createHistoryChange('status', 'Status', entry.previousStatus, entry.newStatus),
    ].filter((change): change is FeedbackRequestHistoryFieldChange => change !== null);
  }

  private createHistoryChange(
    field: 'type' | 'title' | 'description' | 'area' | 'status',
    label: string,
    previousValue: string | number | null | undefined,
    newValue: string | number | null | undefined,
  ): FeedbackRequestHistoryFieldChange | null {
    if (previousValue === newValue ||
        (previousValue === undefined && newValue === undefined) ||
        (previousValue === null && newValue === null)) {
      return null;
    }

    return {
      field,
      label,
      previousValue: this.formatHistoryValue(field, previousValue),
      newValue: this.formatHistoryValue(field, newValue),
    };
  }

  private formatHistoryValue(
    field: 'type' | 'title' | 'description' | 'area' | 'status',
    value: string | number | null | undefined,
  ): string {
    if (value === null || value === undefined || value === '') {
      return 'Not set';
    }

    if (field === 'type') {
      return feedbackRequestTypeLabel(value as FeedbackRequestType);
    }

    if (field === 'status') {
      return feedbackRequestStatusLabel(value as FeedbackRequestStatus);
    }

    return String(value);
  }

  private createCreatePayload(): CreateFeedbackRequest {
    const value = this.form.getRawValue();

    return {
      type: value.type,
      title: value.title.trim(),
      description: value.description.trim(),
      area: this.normalizeOptional(value.area),
    };
  }

  private createUpdatePayload(): UpdateFeedbackRequest {
    const value = this.form.getRawValue();

    return {
      type: value.type,
      title: value.title.trim(),
      description: value.description.trim(),
      area: this.normalizeOptional(value.area),
      status: value.status,
    };
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();
    return trimmed ? trimmed : null;
  }
}
