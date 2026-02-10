import {
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import {
  CreateWatchList,
  UpdateWatchList,
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

  isEditMode = false;
  watchListId?: number;

  form = this.fb.nonNullable.group({
    url: ['', Validators.required],
    name: ['', Validators.required],
    registrationDate: [''],
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
          registrationDate: item.registrationDate,
          isActive: item.isActive,
        });

        this.cdr.markForCheck();
      });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.watchListId !== undefined) {
      const update: UpdateWatchList = this.form.getRawValue();

      this.watchListService
        .update(this.watchListId, update)
        .subscribe(() => {
          this.router.navigate(['/watch-list']);
        });

      return;
    }

    const create: CreateWatchList = this.form.getRawValue();

    this.watchListService.create(create).subscribe(() => {
      this.router.navigate(['/watch-list']);
    });
  }

  cancel(): void {
    this.router.navigate(['/watch-list']);
  }
}
