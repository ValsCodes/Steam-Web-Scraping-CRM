import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, map, Observable } from 'rxjs';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';

import { GameService, ItemGroupService, TagService } from '../../../services';
import { Game, CreateTag, ItemGroup, UpdateTag } from '../../../models';

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
  isCreatingItemGroup = false;
  readonly itemGroups = signal<readonly ItemGroup[]>([]);

  form = this.fb.nonNullable.group({
    gameId: [null as number | null, [Validators.required, Validators.min(1)]],
    itemGroupId: [null as number | null],
    name: ['', Validators.required],
    isActive: [true],
  });

  readonly newItemGroupName = this.fb.nonNullable.control(
    { value: '', disabled: true },
    [Validators.required, Validators.maxLength(255)],
  );

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
    private itemGroupService: ItemGroupService,
    private gameService: GameService,
  ) {}

  ngOnInit(): void {
    this.form.controls.gameId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(gameId => {
        this.form.controls.itemGroupId.setValue(null, { emitEvent: false });
        this.itemGroups.set([]);
        this.newItemGroupName.reset('', { emitEvent: false });

        if (gameId === null) {
          this.newItemGroupName.disable({ emitEvent: false });
          return;
        }

        this.newItemGroupName.enable({ emitEvent: false });
        this.loadItemGroups(gameId);
      });

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
          itemGroupId: tag.itemGroupId,
          name: tag.name ?? '',
          isActive: tag.isActive,
        });
      });
  }

  private loadItemGroups(gameId: number): void {
    this.itemGroupService
      .getByGame(gameId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(itemGroups => this.itemGroups.set(itemGroups));
  }

  createItemGroup(): void {
    const gameId = this.form.controls.gameId.value;
    const name = this.newItemGroupName.value.trim();

    if (gameId === null || !name || this.newItemGroupName.invalid || this.isCreatingItemGroup) {
      this.newItemGroupName.markAsTouched();
      return;
    }

    this.isCreatingItemGroup = true;
    this.newItemGroupName.disable({ emitEvent: false });
    this.itemGroupService
      .create({ gameId, name })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isCreatingItemGroup = false;
          if (this.form.controls.gameId.value !== null) {
            this.newItemGroupName.enable({ emitEvent: false });
          }
        }),
      )
      .subscribe(created => {
        this.itemGroups.set(
          [...this.itemGroups(), created].sort((left, right) =>
            left.name.localeCompare(right.name) || left.id - right.id,
          ),
        );
        this.form.controls.itemGroupId.setValue(created.id);
        this.newItemGroupName.reset('');
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting || this.isCreatingItemGroup) {
      return;
    }

    this.isSubmitting = true;

    const request$: Observable<unknown> = this.isEditMode && this.tagId
      ? this.tagService.update(this.tagId, {
          name: this.form.controls.name.value,
          isActive: this.form.controls.isActive.value,
          itemGroupId: this.form.controls.itemGroupId.value,
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
