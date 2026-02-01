import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormControl,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { Game, Product, Tag, CreateProductTag } from '../../../models';
import {
  GameService,
  ProductService,
  TagService,
  ProductTagService,
} from '../../../services';

@Component({
  selector: 'steam-product-tag-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-tag-form.html',
  styleUrl: './product-tag-form.scss',
})
export class ProductTagForm implements OnInit, OnDestroy {
  isEditMode = false;

  originalProductId?: number;
  originalTagId?: number;

  form = this.fb.nonNullable.group({
    productId: [0, Validators.required],
    tagId: [0, Validators.required],
  });

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private service: ProductTagService,
    private productService: ProductService,
    private tagService: TagService,
    private gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  games: Game[] = [];
  gameIdControl = new FormControl<number | null>(null);

  tagsAll: Tag[] = [];
  tagsFiltered: Tag[] = [];
  tagIdControl = new FormControl<number | null>(null);

  productsAll: Product[] = [];
  productsFiltered: Product[] = [];
  productIdControl = new FormControl<number | null>(null);

  ngOnInit(): void {
    this.loadGames();
    this.loadTags();
    this.loadProducts();

    const productId = this.route.snapshot.paramMap.get('productId');
    const tagId = this.route.snapshot.paramMap.get('tagId');

    if (productId && tagId) {
      this.isEditMode = true;

      this.originalProductId = Number(productId);
      this.originalTagId = Number(tagId);

      this.form.patchValue({
        productId: this.originalProductId,
        tagId: this.originalTagId,
      });

      this.form.controls.productId.disable();
      this.form.controls.tagId.disable();
    }

    this.bindGameSelection();
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(games => {
        this.games = games;
        this.cdr.markForCheck();
      });
  }

  private loadTags(): void {
    this.tagService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(tags => {
        this.tagsAll = tags;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private loadProducts(): void {
    this.productService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(products => {
        this.productsAll = products;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private bindGameSelection(): void {
    this.gameIdControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(gameId => {
        this.applyGameFilter(gameId);
      });
  }

 private applyGameFilter(gameId: number | null): void {
  if (gameId === null) {
    this.tagsFiltered = [];
    this.productsFiltered = [];

    this.form.controls.tagId.reset();
    this.form.controls.productId.reset();

    this.cdr.markForCheck();
    return;
  }

  this.tagsFiltered = this.tagsAll.filter(
    tag => tag.gameId === gameId
  );

  this.productsFiltered = this.productsAll.filter(
    product => product.gameId === gameId
  );

  this.form.controls.tagId.reset();
  this.form.controls.productId.reset();

  this.cdr.markForCheck();
}

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    const payload: CreateProductTag = this.form.getRawValue();

    console.log(this.originalProductId);
    console.log(this.originalTagId);

    console.log(payload)

    if (
      this.isEditMode &&
      this.originalProductId !== undefined &&
      this.originalTagId !== undefined
    ) {
      this.service
        .delete(this.originalProductId, this.originalTagId)
        .subscribe(() => {
          this.service.create(payload).subscribe(() => {
            this.router.navigate(['/product-tags']);
          });
        });

      return;
    }

    this.service.create(payload).subscribe(() => {
      this.router.navigate(['/product-tags']);
    });
  }

  cancel(): void {
    this.router.navigate(['/product-tags']);
  }
}