import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import {
  CreateFeedbackRequest,
  FEEDBACK_REQUEST_TYPE_OPTIONS,
  FeedbackRequestType,
} from '../../../models/feedback-request.model';
import { FeedbackRequestService } from '../../../services/feedback-request/feedback-request.service';

@Component({
  selector: 'steam-feedback-send-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './feedback-send-page.html',
  styleUrl: './feedback-send-page.scss',
})
export class FeedbackSendPage {
  private readonly destroyRef = inject(DestroyRef);

  readonly typeOptions = FEEDBACK_REQUEST_TYPE_OPTIONS;
  isSubmitting = false;

  readonly form = this.fb.nonNullable.group({
    type: [FeedbackRequestType.Feedback, [Validators.required]],
    title: ['', [Validators.required, Validators.maxLength(140)]],
    description: ['', [Validators.required, Validators.maxLength(4000)]],
    area: ['', [Validators.maxLength(120)]],
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly feedbackRequestService: FeedbackRequestService,
  ) {}

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    this.feedbackRequestService
      .create(this.createPayload())
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

  private createPayload(): CreateFeedbackRequest {
    const value = this.form.getRawValue();

    return {
      type: value.type,
      title: value.title.trim(),
      description: value.description.trim(),
      area: this.normalizeOptional(value.area),
    };
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();
    return trimmed ? trimmed : null;
  }
}
