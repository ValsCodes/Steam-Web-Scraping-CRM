import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import {
  FEEDBACK_REQUEST_STATUS_OPTIONS,
  FEEDBACK_REQUEST_TYPE_OPTIONS,
  FeedbackRequest,
  FeedbackRequestStatus,
  FeedbackRequestType,
  feedbackRequestStatusLabel,
  feedbackRequestTypeLabel,
} from '../../../models/feedback-request.model';
import { FeedbackRequestService } from '../../../services/feedback-request/feedback-request.service';

@Component({
  selector: 'steam-feedback-requests-view',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
  ],
  templateUrl: './feedback-requests-view.html',
  styleUrl: './feedback-requests-view.scss',
})
export class FeedbackRequestsView implements OnInit {
  readonly typeOptions = FEEDBACK_REQUEST_TYPE_OPTIONS;
  readonly statusOptions = FEEDBACK_REQUEST_STATUS_OPTIONS;
  readonly FeedbackRequestStatus = FeedbackRequestStatus;

  readonly displayedColumns = [
    'referenceId',
    'title',
    'type',
    'area',
    'status',
    'createdAtUtc',
    'updatedAtUtc',
    'actions',
  ];

  readonly textFilterControl = new FormControl<string>('', { nonNullable: true });
  readonly typeFilterControl = new FormControl<FeedbackRequestType | null>(null);
  readonly statusFilterControl = new FormControl<FeedbackRequestStatus | null>(null);

  readonly dataSource = new MatTableDataSource<FeedbackRequest>([]);
  isGridLoading = false;
  pageSize = 25;
  readonly pageSizeOptions = [10, 25, 50, 100];

  private feedbackRequests: FeedbackRequest[] = [];
  private readonly statusUpdatingIds = new Set<number>();

  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  constructor(
    private readonly feedbackRequestService: FeedbackRequestService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.textFilterControl.valueChanges.subscribe(() => this.applyFilters());
    this.typeFilterControl.valueChanges.subscribe(() => this.applyFilters());
    this.statusFilterControl.valueChanges.subscribe(() => this.applyFilters());

    this.fetchFeedbackRequests();
  }

  fetchFeedbackRequests(): void {
    this.isGridLoading = true;
    this.cdr.markForCheck();

    this.feedbackRequestService
      .getAll()
      .pipe(
        finalize(() => {
          this.isGridLoading = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe((items) => {
        this.feedbackRequests = items;
        this.attachTableControls();
        this.applyFilters();
      });
  }

  clearFiltersButtonClicked(): void {
    this.textFilterControl.setValue('');
    this.typeFilterControl.setValue(null);
    this.statusFilterControl.setValue(null);
  }

  refreshButtonClicked(): void {
    this.fetchFeedbackRequests();
  }

  createButtonClicked(): void {
    this.router.navigate(['/feedback/create']);
  }

  sendButtonClicked(): void {
    this.router.navigate(['/feedback/send']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/feedback/edit', id]);
  }

  pageSizeChanged(value: string | number): void {
    const pageSize = Number(value);
    if (Number.isNaN(pageSize) || pageSize <= 0 || this.pageSize === pageSize) {
      return;
    }

    this.pageSize = pageSize;

    if (this.paginator) {
      this.paginator.pageSize = pageSize;
      this.paginator.firstPage();
      this.dataSource.data = [...this.dataSource.data];
      this.cdr.markForCheck();
    }
  }

  statusButtonClicked(request: FeedbackRequest, status: FeedbackRequestStatus): void {
    if (request.status === status || this.isStatusUpdating(request.id)) {
      return;
    }

    this.statusUpdatingIds.add(request.id);

    this.feedbackRequestService
      .updateStatus(request.id, { status })
      .pipe(
        finalize(() => {
          this.statusUpdatingIds.delete(request.id);
          this.cdr.markForCheck();
        }),
      )
      .subscribe(() => {
        const now = new Date().toISOString();
        this.feedbackRequests = this.feedbackRequests.map((item) =>
          item.id === request.id
            ? {
                ...item,
                status,
                updatedAtUtc: now,
                statusChangedAtUtc: now,
              }
            : item,
        );

        this.applyFilters();
      });
  }

  isStatusUpdating(id: number): boolean {
    return this.statusUpdatingIds.has(id);
  }

  get showNoItemsEmptyState(): boolean {
    return !this.isGridLoading && this.feedbackRequests.length === 0;
  }

  get showFilteredEmptyState(): boolean {
    return !this.isGridLoading &&
      this.feedbackRequests.length > 0 &&
      this.dataSource.data.length === 0;
  }

  typeLabel(value: FeedbackRequestType): string {
    return feedbackRequestTypeLabel(value);
  }

  statusLabel(value: FeedbackRequestStatus): string {
    return feedbackRequestStatusLabel(value);
  }

  private applyFilters(): void {
    const textFilter = this.textFilterControl.value.trim().toLowerCase();
    const typeFilter = this.typeFilterControl.value;
    const statusFilter = this.statusFilterControl.value;

    const filtered = this.feedbackRequests.filter((request) => {
      if (typeFilter !== null && request.type !== typeFilter) {
        return false;
      }

      if (statusFilter !== null && request.status !== statusFilter) {
        return false;
      }

      if (!textFilter) {
        return true;
      }

      const searchable = [
        request.referenceId,
        request.title,
        request.description,
        request.area ?? '',
      ].join(' ').toLowerCase();

      return searchable.includes(textFilter);
    });

    this.dataSource.data = filtered;
    this.paginator?.firstPage();
  }

  private attachTableControls(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
      this.paginator.pageSize = this.pageSize;
    }

    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }
}
