import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, map, Observable } from 'rxjs';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';

import { GameService, TagService } from '../../../services';
import { Game, CreateTag, UpdateTag } from '../../../models';

@Component({
  selector: 'steam-tag-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tag-form.html',
  styleUrl: './tag-form.scss',
})
export class TagForm implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  isEditMode = false;
  tagId?: number;
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    gameId: [null as number | null, [Validators.required, Validators.min(1)]],
    name: ['', Validators.required],
    isActive: [true],
  });

  readonly games = toSignal(
    this.gameService.getAll().pipe(
      map(games => games.filter(game => game.isActive)),
    ),
    { initialValue: [] as Game[] },
  );

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private tagService: TagService,
    private gameService: GameService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.tagId = Number(idParam);
      this.form.controls.gameId.disable({ emitEvent: false });
      this.loadTag(this.tagId);
    }
  }

  private loadTag(id: number): void {
    this.tagService
      .getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(tag => {
        this.form.patchValue({
          gameId: tag.gameId,
          name: tag.name ?? '',
          isActive: tag.isActive,
        });
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const request$: Observable<unknown> = this.isEditMode && this.tagId
      ? this.tagService.update(this.tagId, {
          name: this.form.controls.name.value,
          isActive: this.form.controls.isActive.value,
        } as UpdateTag)
      : this.tagService.create(this.form.getRawValue() as CreateTag);

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting = false;
        }),
      )
      .subscribe(() => {
        this.router.navigate(['/tags']);
      });
  }

  cancel(): void {
    this.router.navigate(['/tags']);
  }
}
