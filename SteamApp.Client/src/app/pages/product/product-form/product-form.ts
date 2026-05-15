import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, forkJoin, of, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { GameService, ProductService } from '../../../services';
import { CreateProduct, Game, Tag, UpdateProduct } from '../../../models';
import { TagService } from '../../../services/tag/tag.service';
import { ProductTagService } from '../../../services/product-tag/product-tag.service';

@Component({
  selector: 'steam-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
})
export class ProductForm implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  isEditMode = false;
  productId?: number;
  isSubmitting = false;
  readonly ratingOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] as const;

  readonly games = signal<readonly Game[]>([]);
  readonly tags = signal<readonly Tag[]>([]);
  readonly selectedTagIds = signal<readonly number[]>([]);

  private initialTagIds: number[] = [];

  form = this.fb.nonNullable.group({
    gameId: [null as number | null, [Validators.required, Validators.min(1)]],
    name: ['', Validators.required],
    rating: [null as number | null],
    isActive: [true],
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly productService: ProductService,
    private readonly gameService: GameService,
    private readonly tagService: TagService,
    private readonly productTagService: ProductTagService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    this.form.controls.gameId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.pruneSelectedTags();
      });

    this.loadLookupData();

    if (idParam) {
      this.isEditMode = true;
      this.productId = Number(idParam);
      this.form.controls.gameId.disable({ emitEvent: false });
      this.loadProduct(this.productId);
    }
  }

  get filteredTags(): readonly Tag[] {
    const gameId = this.form.controls.gameId.value;
    return this.tags().filter(x => x.gameId === gameId);
  }

  get areAllFilteredTagsSelected(): boolean {
    const filtered = this.filteredTags;
    if (filtered.length === 0) {
      return false;
    }

    const selected = new Set(this.selectedTagIds());
    return filtered.every(x => selected.has(x.id));
  }

  get areSomeFilteredTagsSelected(): boolean {
    const filtered = this.filteredTags;
    if (filtered.length === 0) {
      return false;
    }

    const selected = new Set(this.selectedTagIds());
    const selectedCount = filtered.filter(x => selected.has(x.id)).length;

    return selectedCount > 0 && selectedCount < filtered.length;
  }

  isTagSelected(tagId: number): boolean {
    return this.selectedTagIds().includes(tagId);
  }

  toggleTag(tagId: number, checked: boolean): void {
    const selected = new Set(this.selectedTagIds());

    if (checked) {
      selected.add(tagId);
    } else {
      selected.delete(tagId);
    }

    this.selectedTagIds.set([...selected]);
  }

  toggleAllTags(checked: boolean): void {
    const selected = new Set(this.selectedTagIds());

    for (const tag of this.filteredTags) {
      if (checked) {
        selected.add(tag.id);
      } else {
        selected.delete(tag.id);
      }
    }

    this.selectedTagIds.set([...selected]);
  }

  private pruneSelectedTags(): void {
    const allowedTagIds = new Set(this.filteredTags.map(x => x.id));
    this.selectedTagIds.set(this.selectedTagIds().filter(x => allowedTagIds.has(x)));
  }

  private loadLookupData(): void {
    this.gameService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(games => this.games.set(games.filter(game => game.isActive)));

    this.tagService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(tags => this.tags.set(tags.filter(tag => tag.isActive)));
  }

  private loadProduct(id: number): void {
    this.productService
      .getById(id)
      .pipe(
        switchMap(product => {
          this.form.patchValue({
            gameId: product.gameId,
            name: product.name ?? '',
            rating: product.rating ?? null,
            isActive: product.isActive ?? false,
          });

          return this.productTagService.getByProduct(id);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(productTags => {
        this.initialTagIds = productTags.map(x => x.tagId);
        this.selectedTagIds.set([...this.initialTagIds]);
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const request$ = this.isEditMode && this.productId
      ? this.productService
          .update(this.productId, {
            name: this.form.controls.name.value,
            isActive: this.form.controls.isActive.value,
            rating: this.form.controls.rating.value,
          } as UpdateProduct)
          .pipe(switchMap(() => this.syncProductTags(this.productId!)))
      : this.productService
          .create(this.form.getRawValue() as CreateProduct)
          .pipe(switchMap(created => this.syncProductTags(created.id)));

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting = false;
        }),
      )
      .subscribe(() => {
        this.router.navigate(['/products']);
      });
  }

  private syncProductTags(productId: number) {
    const selectedTags = this.selectedTagIds();

    const adds = selectedTags
      .filter(x => !this.initialTagIds.includes(x))
      .map(tagId => this.productTagService.create({ productId, tagId }));

    const deletes = this.initialTagIds
      .filter(x => !selectedTags.includes(x))
      .map(tagId => this.productTagService.delete(productId, tagId));

    const requests = [...adds, ...deletes];

    if (requests.length === 0) {
      return of(void 0);
    }

    return forkJoin(requests).pipe(switchMap(() => of(void 0)));
  }

  cancel(): void {
    this.router.navigate(['/products']);
  }
}
