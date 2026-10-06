import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, Observable, Subject, takeUntil } from 'rxjs';

import {
  CreateWatchList,
  UpdateWatchList,
  WatchList,
} from '../../../models';
import { WatchListService } from '../../../services';

@Component({
  selector: 'steam-watch-list-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './watch-list-form.html',
  styleUrl: './watch-list-form.scss',
})
export class WatchListForm implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  @Input() embedded = false;
  @Input() set watchList(value: WatchList | null) {
    if (value === null) {
      this.isEditMode = false;
      this.watchListId = undefined;
      this.form.reset({ url: '', name: '', isActive: true });
      return;
    }

    this.isEditMode = true;
    this.watchListId = value.id;
    this.form.reset({
      url: value.url ?? '',
      name: value.name ?? '',
      isActive: value.isActive,
    });
  }

  @Output() readonly saved = new EventEmitter<void>();
  @Output() readonly cancelled = new EventEmitter<void>();

  isEditMode = false;
  watchListId?: number;
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    url: ['', Validators.required],
    name: ['', Validators.required],
    isActive: [true],
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly watchListService: WatchListService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    if (this.embedded) {
      return;
    }

    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam !== null) {
      this.isEditMode = true;
      this.watchListId = Number(idParam);
      this.loadWatchList(this.watchListId);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadWatchList(id: number): void {
    this.watchListService
      .getById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(item => {
        this.form.patchValue({
          url: item.url ?? '',
          name: item.name ?? '',
          isActive: item.isActive,
        });

        this.cdr.markForCheck();
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const request$: Observable<unknown> = this.isEditMode && this.watchListId !== undefined
      ? this.watchListService.update(this.watchListId, this.form.getRawValue() as UpdateWatchList)
      : this.watchListService.create(this.form.getRawValue() as CreateWatchList);

    request$
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isSubmitting = false;
        }),
      )
      .subscribe(() => {
        if (this.embedded) {
          this.saved.emit();
          return;
        }

        void this.router.navigate(['/watch-list']);
      });
  }

  cancel(): void {
    if (this.embedded) {
      this.cancelled.emit();
      return;
    }

    void this.router.navigate(['/watch-list']);
  }
}
