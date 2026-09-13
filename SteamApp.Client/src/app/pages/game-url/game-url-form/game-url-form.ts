import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, EMPTY, finalize, forkJoin, of, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import {
  CreateGameUrl,
  Game,
  ItemGroup,
  PixelListItem,
  Product,
  ProductTagDetail,
  ScrapingMode,
  ScrapingModeEnum,
  UpdateGameUrl,
} from '../../../models';
import {
  GameService,
  GameUrlPixelService,
  GameUrlProductService,
  GameUrlService,
  ItemGroupService,
  PixelService,
  ProductService,
  ScrapingModeService,
} from '../../../services';

import { groupByItemGroup } from '../../../common/item-grouping';

const DEFAULT_PIXEL_X = 450;
const DEFAULT_PIXEL_Y = 50;
const DEFAULT_PIXEL_IMAGE_WIDTH = 62;
const DEFAULT_PIXEL_IMAGE_HEIGHT = 62;

@Component({
  selector: 'steam-game-url-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './game-url-form.html',
  styleUrl: './game-url-form.scss',
  host: {
    '(document:keydown)': 'onProductPreviewKey($event)',
    '(document:keyup)': 'onProductPreviewKey($event)',
    '(window:blur)': 'onProductPreviewBlur()',
  },
})
export class GameUrlForm implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  isEditMode = false;
  gameUrlId?: number;
  isSubmitting = false;
  submitError = '';
  isCreatingItemGroup = false;
  isLoadingItemGroups = false;

  readonly games = signal<readonly Game[]>([]);
  readonly itemGroups = signal<readonly ItemGroup[]>([]);
  readonly products = signal<readonly Product[]>([]);
  readonly pixels = signal<readonly PixelListItem[]>([]);
  readonly scrapingModes = signal<readonly ScrapingMode[]>([]);

  readonly selectedProductIds = signal<readonly number[]>([]);
  readonly selectedPixelIds = signal<readonly number[]>([]);
  readonly selectedScrapingModeId = signal(1);

  readonly isManualMode = computed(() => this.selectedScrapingModeId() === ScrapingModeEnum.ManualBatch);
  readonly isBatchMode = computed(() => {
    const modeId = this.selectedScrapingModeId();
    return modeId === ScrapingModeEnum.Batch || modeId === ScrapingModeEnum.PixelBatch;
  });
  readonly isPixelMode = computed(() => this.selectedScrapingModeId() === ScrapingModeEnum.PixelBatch);

  private initialProductIds: number[] = [];
  private initialPixelIds: number[] = [];
  private productSelectionAnchor: number | null = null;

  readonly productPreviewIds = new Set<number>();
  private hoveredProductId: number | null = null;
  private productPreviewShiftHeld = false;

  onProductHover(productId: number, event: MouseEvent): void {
    this.hoveredProductId = productId;
    this.productPreviewShiftHeld = event.shiftKey;
    this.updateProductPreview();
  }

  onProductPreviewKey(event: KeyboardEvent): void {
    this.productPreviewShiftHeld = event.shiftKey;
    this.updateProductPreview();
  }

  clearProductPreview(): void {
    this.hoveredProductId = null;
    this.productPreviewIds.clear();
  }

  onProductPreviewBlur(): void {
    this.productPreviewShiftHeld = false;
    this.clearProductPreview();
  }

  private updateProductPreview(): void {
    this.productPreviewIds.clear();
    if (this.productPreviewShiftHeld && this.hoveredProductId !== null && this.productSelectionAnchor !== null) {
      const visible = this.filteredProducts;
      const anchorIndex = visible.findIndex(product => product.id === this.productSelectionAnchor);
      const endpointIndex = visible.findIndex(product => product.id === this.hoveredProductId);
      if (anchorIndex >= 0 && endpointIndex >= 0) {
        for (const product of visible.slice(Math.min(anchorIndex, endpointIndex), Math.max(anchorIndex, endpointIndex) + 1)) {
          this.productPreviewIds.add(product.id);
        }
      }
    }
  }


  readonly productNameFilterControl = new FormControl('', { nonNullable: true });
  readonly productTagSelectControl = new FormControl<ProductTagDetail | null>({ value: null, disabled: true });
  readonly productTagFilters = signal<readonly string[]>([]);

  form = this.fb.nonNullable.group({
    gameId: [null as number | null, [Validators.required, Validators.min(1)]],
    itemGroupId: [null as number | null],
    name: [''],
    partialUrl: [''],
    scrapingModeId: [1, Validators.required],
    isActive: [true],
    startPage: [null as number | null],
    endPage: [null as number | null],
    pixelX: [DEFAULT_PIXEL_X as number | null],
    pixelY: [DEFAULT_PIXEL_Y as number | null],
    pixelImageWidth: [DEFAULT_PIXEL_IMAGE_WIDTH as number | null],
    pixelImageHeight: [DEFAULT_PIXEL_IMAGE_HEIGHT as number | null],
  });

  readonly newItemGroupName = this.fb.nonNullable.control(
    { value: '', disabled: true },
    [Validators.required, Validators.maxLength(255)],
  );

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly gameUrlService: GameUrlService,
    private readonly gameService: GameService,
    private readonly productService: ProductService,
    private readonly pixelService: PixelService,
    private readonly gameUrlProductService: GameUrlProductService,
    private readonly gameUrlPixelService: GameUrlPixelService,
    private readonly scrapingModeService: ScrapingModeService,
    private readonly itemGroupService: ItemGroupService,
  ) {}

  ngOnInit(): void {
    this.syncSelectedScrapingMode();

    this.productNameFilterControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.productSelectionAnchor = null;
        this.clearProductPreview();
      });

    this.productTagSelectControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(tag => {
        if (!tag?.name) {
          return;
        }
        const name = tag.name.toLowerCase();
        if (!this.productTagFilters().includes(name)) {
          this.productTagFilters.set([...this.productTagFilters(), name]);
        }
        this.productSelectionAnchor = null;
        this.clearProductPreview();
        this.productTagSelectControl.setValue(null, { emitEvent: false });
        this.syncProductTagControlState();
      });

    this.form.controls.scrapingModeId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.syncSelectedScrapingMode();
      });

    this.form.controls.gameId.valueChanges
      .pipe(
        switchMap(gameId => {
          this.isLoadingItemGroups = gameId !== null;
          this.clearProductFilters();
          this.pruneSelectedRelations();
          this.form.controls.itemGroupId.setValue(null, { emitEvent: false });
          this.itemGroups.set([]);
          this.newItemGroupName.reset('', { emitEvent: false });

          if (gameId === null) {
            this.newItemGroupName.disable({ emitEvent: false });
            return of([] as ItemGroup[]);
          }

          if (!this.isCreatingItemGroup) {
            this.newItemGroupName.enable({ emitEvent: false });
          }
          return this.itemGroupService.getByGame(gameId).pipe(
            catchError(() => of([] as ItemGroup[])),
            finalize(() => { this.isLoadingItemGroups = false; }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(itemGroups => this.itemGroups.set(itemGroups));

    this.loadLookupData();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.gameUrlId = Number(idParam);
      this.form.controls.gameId.disable({ emitEvent: false });
      this.loadGameUrl(this.gameUrlId);
    }
  }

  get availableProducts(): readonly Product[] {
    const gameId = this.form.controls.gameId.value;
    return this.products().filter((x) => x.gameId === gameId);
  }

  get filteredProducts(): readonly Product[] {
    const name = this.productNameFilterControl.value.toLowerCase();
    return this.availableProducts.filter(product =>
      (!name || (product.name ?? '').toLowerCase().includes(name)) &&
      this.productTagFilters().every(filter =>
        product.tags.some(tag => tag.toLowerCase().includes(filter)),
      ),
    );
  }

  get productTagGroups() {
    const tags = new Map<number, ProductTagDetail>();
    for (const product of this.availableProducts) {
      for (const tag of product.tagDetails ?? []) {
        if (tag.isActive && tag.name && !this.productTagFilters().includes(tag.name.toLowerCase())) {
          tags.set(tag.id, tag);
        }
      }
    }
    return groupByItemGroup([...tags.values()]);
  }

  getProductTagsTooltip(product: Product): string {
    return (product.tagDetails ?? []).map(tag => tag.name).filter(Boolean).join(', ');
  }

  clearProductFilters(): void {
    this.clearProductPreview();
    this.productNameFilterControl.setValue('', { emitEvent: false });
    this.productTagFilters.set([]);
    this.productTagSelectControl.setValue(null, { emitEvent: false });
    this.productSelectionAnchor = null;
    this.syncProductTagControlState();
  }

  removeProductTagFilter(name: string): void {
    this.clearProductPreview();
    this.productTagFilters.set(this.productTagFilters().filter(tag => tag !== name));
    this.productSelectionAnchor = null;
    this.syncProductTagControlState();
  }

  get filteredPixels(): readonly PixelListItem[] {
    const gameId = this.form.controls.gameId.value;
    return this.pixels().filter((x) => x.gameId === gameId);
  }

  get areAllFilteredProductsSelected(): boolean {
    const filtered = this.filteredProducts;
    if (filtered.length === 0) {
      return false;
    }

    const selected = new Set(this.selectedProductIds());
    return filtered.every((x) => selected.has(x.id));
  }

  get areSomeFilteredProductsSelected(): boolean {
    const filtered = this.filteredProducts;
    if (filtered.length === 0) {
      return false;
    }

    const selected = new Set(this.selectedProductIds());
    const selectedCount = filtered.filter((x) => selected.has(x.id)).length;

    return selectedCount > 0 && selectedCount < filtered.length;
  }

  get areAllFilteredPixelsSelected(): boolean {
    const filtered = this.filteredPixels;
    if (filtered.length === 0) {
      return false;
    }

    const selected = new Set(this.selectedPixelIds());
    return filtered.every((x) => selected.has(x.id));
  }

  get areSomeFilteredPixelsSelected(): boolean {
    const filtered = this.filteredPixels;
    if (filtered.length === 0) {
      return false;
    }

    const selected = new Set(this.selectedPixelIds());
    const selectedCount = filtered.filter((x) => selected.has(x.id)).length;

    return selectedCount > 0 && selectedCount < filtered.length;
  }

  isProductSelected(productId: number): boolean {
    return this.selectedProductIds().includes(productId);
  }

  isPixelSelected(pixelId: number): boolean {
    return this.selectedPixelIds().includes(pixelId);
  }

  toggleAllProducts(checked: boolean): void {
    this.clearProductPreview();
    this.productSelectionAnchor = null;
    const selected = new Set(this.selectedProductIds());

    for (const product of this.filteredProducts) {
      if (checked) {
        selected.add(product.id);
      } else {
        selected.delete(product.id);
      }
    }

    this.selectedProductIds.set([...selected]);
  }

  toggleAllPixels(checked: boolean): void {
    const selected = new Set(this.selectedPixelIds());

    for (const pixel of this.filteredPixels) {
      if (checked) {
        selected.add(pixel.id);
      } else {
        selected.delete(pixel.id);
      }
    }

    this.selectedPixelIds.set([...selected]);
  }

  onProductCardClicked(productId: number, event: MouseEvent): void {
    event.preventDefault();
    this.toggleProduct(productId, !this.isProductSelected(productId), event.shiftKey);
  }

  toggleProduct(productId: number, checked: boolean, shiftKey = false): void {
    this.clearProductPreview();
    const selected = new Set(this.selectedProductIds());
    const visible = this.filteredProducts;
    const anchorIndex = visible.findIndex(product => product.id === this.productSelectionAnchor);
    const endpointIndex = visible.findIndex(product => product.id === productId);
    const hasRange = shiftKey && anchorIndex >= 0 && endpointIndex >= 0;
    const productIds = hasRange
      ? visible.slice(Math.min(anchorIndex, endpointIndex), Math.max(anchorIndex, endpointIndex) + 1).map(product => product.id)
      : [productId];

    for (const id of productIds) {
      if (checked) {
        selected.add(id);
      } else {
        selected.delete(id);
      }
    }
    if (!hasRange) {
      this.productSelectionAnchor = productId;
    }

    this.selectedProductIds.set([...selected]);
  }

  togglePixel(pixelId: number, checked: boolean): void {
    const selected = new Set(this.selectedPixelIds());

    if (checked) {
      selected.add(pixelId);
    } else {
      selected.delete(pixelId);
    }

    this.selectedPixelIds.set([...selected]);
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting || this.isCreatingItemGroup) {
      return;
    }

    this.isSubmitting = true;
    this.submitError = '';

    const createPayload = this.buildCreatePayload();

    const request$ = this.isEditMode && this.gameUrlId
      ? this.gameUrlService
          .update(this.gameUrlId, this.buildUpdatePayload(createPayload))
          .pipe(switchMap(() => this.syncRelations(this.gameUrlId!)))
      : this.gameUrlService
          .create(createPayload)
          .pipe(switchMap((created) => {
            // A relation failure must not create a second URL when the user retries.
            this.gameUrlId = created.id;
            this.isEditMode = true;
            this.form.controls.gameId.disable({ emitEvent: false });
            return this.syncRelations(created.id);
          }));

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting = false;
        }),
      )
      .subscribe({
        next: () => { this.router.navigate(['/game-urls']); },
        error: () => {
          this.submitError = 'Could not save the relations. Product changes are all-or-nothing. Please try again.';
        },
      });
  }

  cancel(): void {
    this.router.navigate(['/game-urls']);
  }

  createItemGroup(): void {
    const gameId = this.form.controls.gameId.value;
    const name = this.newItemGroupName.value.trim();

    if (gameId === null || !name || this.newItemGroupName.invalid || this.isCreatingItemGroup || this.isLoadingItemGroups) {
      this.newItemGroupName.markAsTouched();
      return;
    }

    this.isCreatingItemGroup = true;
    this.newItemGroupName.disable({ emitEvent: false });
    this.itemGroupService
      .create({ gameId, name })
      .pipe(
        catchError(() => EMPTY),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isCreatingItemGroup = false;
          if (this.form.controls.gameId.value !== null) {
            this.newItemGroupName.enable({ emitEvent: false });
          }
        }),
      )
      .subscribe(created => {
        if (this.form.controls.gameId.value !== gameId) {
          return;
        }

        this.itemGroups.set(
          [...this.itemGroups(), created].sort((left, right) =>
            left.name.localeCompare(right.name) || left.id - right.id,
          ),
        );
        this.form.controls.itemGroupId.setValue(created.id);
        this.newItemGroupName.reset('');
      });
  }

  private pruneSelectedRelations(): void {
    const allowedProductIds = new Set(this.availableProducts.map((x) => x.id));
    const allowedPixelIds = new Set(this.filteredPixels.map((x) => x.id));

    this.selectedProductIds.set(this.selectedProductIds().filter((x) => allowedProductIds.has(x)));
    this.selectedPixelIds.set(this.selectedPixelIds().filter((x) => allowedPixelIds.has(x)));
  }

  private syncProductTagControlState(): void {
    if (this.productTagGroups.length === 0) {
      this.productTagSelectControl.disable({ emitEvent: false });
    } else {
      this.productTagSelectControl.enable({ emitEvent: false });
    }
  }

  private syncSelectedScrapingMode(): void {
    this.selectedScrapingModeId.set(this.form.controls.scrapingModeId.value ?? 1);
    this.applyPixelDefaultsIfNeeded();
  }

  private applyPixelDefaultsIfNeeded(): void {
    if (!this.isPixelMode()) {
      return;
    }

    const {
      pixelImageHeight,
      pixelImageWidth,
      pixelX,
      pixelY,
    } = this.form.getRawValue();

    const patch: {
      pixelImageHeight?: number | null;
      pixelImageWidth?: number | null;
      pixelX?: number | null;
      pixelY?: number | null;
    } = {};

    if (pixelImageWidth == null || pixelX == null) {
      patch.pixelX = DEFAULT_PIXEL_X;
      patch.pixelImageWidth = DEFAULT_PIXEL_IMAGE_WIDTH;
    }

    if (pixelImageHeight == null || pixelY == null) {
      patch.pixelY = DEFAULT_PIXEL_Y;
      patch.pixelImageHeight = DEFAULT_PIXEL_IMAGE_HEIGHT;
    }

    if (Object.keys(patch).length > 0) {
      this.form.patchValue(patch, { emitEvent: false });
    }
  }

  private loadLookupData(): void {
    this.gameService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((games) => this.games.set(games.filter((game) => game.isActive)));

    this.productService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((products) => {
        this.products.set(products.filter((product) => product.isActive));
        this.clearProductPreview();
        this.productSelectionAnchor = null;
        this.syncProductTagControlState();
      });

    this.pixelService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((pixels) => this.pixels.set(pixels.filter((pixel) => pixel.isActive)));

    this.scrapingModeService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((modes) => {
        this.scrapingModes.set([...modes].sort((a, b) => a.id - b.id));
      });
  }

  private loadGameUrl(id: number): void {
    this.gameUrlService
      .getById(id)
      .pipe(
        switchMap((gameUrl) => {
          this.form.patchValue({
            gameId: Number(gameUrl.gameId),
            itemGroupId: gameUrl.itemGroupId,
            name: gameUrl.name ?? '',
            partialUrl: gameUrl.partialUrl ?? '',
            scrapingModeId: gameUrl.scrapingModeId ?? 1,
            isActive: gameUrl.isActive,
            startPage: gameUrl.startPage,
            endPage: gameUrl.endPage,
            pixelX: gameUrl.pixelX,
            pixelY: gameUrl.pixelY,
            pixelImageWidth: gameUrl.pixelImageWidth,
            pixelImageHeight: gameUrl.pixelImageHeight,
          });

          this.syncSelectedScrapingMode();

          return forkJoin({
            products: this.gameUrlProductService.existsByGameUrl(id),
            pixels: this.gameUrlPixelService.existsByGameUrl(id),
          });
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ products, pixels }) => {
        this.initialProductIds = products.map((x) => x.productId);
        this.initialPixelIds = pixels.map((x) => x.pixelId);

        this.selectedProductIds.set([...this.initialProductIds]);
        this.selectedPixelIds.set([...this.initialPixelIds]);
      });
  }

  private buildCreatePayload(): CreateGameUrl {
    const raw = this.form.getRawValue();
    const gameId = this.requireGameId();
    const scrapingModeId = raw.scrapingModeId ?? ScrapingModeEnum.ManualBatch;
    const isBatchMode = scrapingModeId === ScrapingModeEnum.Batch || scrapingModeId === ScrapingModeEnum.PixelBatch;
    const isPixelMode = scrapingModeId === ScrapingModeEnum.PixelBatch;

    return {
      gameId,
      itemGroupId: raw.itemGroupId,
      name: raw.name,
      scrapingModeId,
      partialUrl: raw.partialUrl,
      isActive: raw.isActive,
      startPage: isBatchMode ? raw.startPage : null,
      endPage: isBatchMode ? raw.endPage : null,
      pixelX: isPixelMode ? raw.pixelX : null,
      pixelY: isPixelMode ? raw.pixelY : null,
      pixelImageWidth: isPixelMode ? raw.pixelImageWidth : null,
      pixelImageHeight: isPixelMode ? raw.pixelImageHeight : null,
    };
  }

  private requireGameId(): number {
    const gameId = this.form.controls.gameId.value;
    if (gameId === null || gameId < 1) {
      throw new Error('Game is required.');
    }

    return gameId;
  }

  private buildUpdatePayload(payload: CreateGameUrl): UpdateGameUrl {
    const { gameId: _gameId, ...updatePayload } = payload;
    return updatePayload;
  }

  private syncRelations(gameUrlId: number) {
    const selectedProducts = this.selectedProductIds();
    const selectedPixels = this.selectedPixelIds();

    const productsChanged = selectedProducts.some(id => !this.initialProductIds.includes(id)) ||
      this.initialProductIds.some(id => !selectedProducts.includes(id));

    const pixelAdds = selectedPixels
      .filter((x) => !this.initialPixelIds.includes(x))
      .map((pixelId) => this.gameUrlPixelService.create({ pixelId, gameUrlId }));

    const pixelDeletes = this.initialPixelIds
      .filter((x) => !selectedPixels.includes(x))
      .map((pixelId) => this.gameUrlPixelService.delete(pixelId, gameUrlId));

    const requests = [
      ...(productsChanged ? [this.gameUrlProductService.bulkSync(gameUrlId, selectedProducts)] : []),
      ...pixelAdds,
      ...pixelDeletes,
    ];

    if (requests.length === 0) {
      return of(void 0);
    }

    return forkJoin(requests).pipe(switchMap(() => of(void 0)));
  }
}
