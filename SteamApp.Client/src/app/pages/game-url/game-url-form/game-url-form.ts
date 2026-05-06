import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, forkJoin, of, switchMap } from 'rxjs';

import {
  CreateGameUrl,
  Game,
  PixelListItem,
  Product,
  ScrapingMode,
  ScrapingModeEnum,
  UpdateGameUrl,
} from '../../../models';
import {
  GameService,
  GameUrlPixelService,
  GameUrlProductService,
  GameUrlService,
  PixelService,
  ProductService,
  ScrapingModeService,
} from '../../../services';

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
})
export class GameUrlForm implements OnInit {
  isEditMode = false;
  gameUrlId?: number;
  isSubmitting = false;

  readonly games = signal<readonly Game[]>([]);
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

  form = this.fb.nonNullable.group({
    gameId: [0, Validators.required],
    name: [''],
    partialUrl: [''],
    scrapingModeId: [1, Validators.required],
    startPage: [null as number | null],
    endPage: [null as number | null],
    pixelX: [DEFAULT_PIXEL_X as number | null],
    pixelY: [DEFAULT_PIXEL_Y as number | null],
    pixelImageWidth: [DEFAULT_PIXEL_IMAGE_WIDTH as number | null],
    pixelImageHeight: [DEFAULT_PIXEL_IMAGE_HEIGHT as number | null],
  });

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
    private readonly scrapingModeService: ScrapingModeService
  ) {}

  ngOnInit(): void {
    this.syncSelectedScrapingMode();

    this.form.controls.scrapingModeId.valueChanges.subscribe(() => {
      this.syncSelectedScrapingMode();
    });

    this.form.controls.gameId.valueChanges.subscribe(() => {
      this.pruneSelectedRelations();
    });

    this.loadLookupData();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.gameUrlId = Number(idParam);
      this.loadGameUrl(this.gameUrlId);
    }
  }

  get filteredProducts(): readonly Product[] {
    const gameId = this.form.controls.gameId.value;
    return this.products().filter((x) => x.gameId === gameId);
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

  toggleProduct(productId: number, checked: boolean): void {
    const selected = new Set(this.selectedProductIds());

    if (checked) {
      selected.add(productId);
    } else {
      selected.delete(productId);
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
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const createPayload = this.buildCreatePayload();

    const request$ = this.isEditMode && this.gameUrlId
      ? this.gameUrlService
          .update(this.gameUrlId, this.buildUpdatePayload(createPayload))
          .pipe(switchMap(() => this.syncRelations(this.gameUrlId!)))
      : this.gameUrlService
          .create(createPayload)
          .pipe(switchMap((created) => this.syncRelations(created.id)));

    request$
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe(() => {
        this.router.navigate(['/game-urls']);
      });
  }

  cancel(): void {
    this.router.navigate(['/game-urls']);
  }

  private pruneSelectedRelations(): void {
    const allowedProductIds = new Set(this.filteredProducts.map((x) => x.id));
    const allowedPixelIds = new Set(this.filteredPixels.map((x) => x.id));

    this.selectedProductIds.set(this.selectedProductIds().filter((x) => allowedProductIds.has(x)));
    this.selectedPixelIds.set(this.selectedPixelIds().filter((x) => allowedPixelIds.has(x)));
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
    this.gameService.getAll().subscribe((games) => this.games.set(games));
    this.productService.getAll().subscribe((products) => this.products.set(products));
    this.pixelService.getAll().subscribe((pixels) => this.pixels.set(pixels));
    this.scrapingModeService.getAll().subscribe((modes) => {
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
            name: gameUrl.name ?? '',
            partialUrl: gameUrl.partialUrl ?? '',
            scrapingModeId: gameUrl.scrapingModeId ?? 1,
            startPage: gameUrl.startPage,
            endPage: gameUrl.endPage,
            pixelX: gameUrl.pixelX,
            pixelY: gameUrl.pixelY,
            pixelImageWidth: gameUrl.pixelImageWidth,
            pixelImageHeight: gameUrl.pixelImageHeight,
          });

          this.syncSelectedScrapingMode();
          this.form.controls.gameId.disable();

          return forkJoin({
            products: this.gameUrlProductService.existsByGameUrl(id),
            pixels: this.gameUrlPixelService.existsByGameUrl(id),
          });
        }),
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
    const scrapingModeId = raw.scrapingModeId ?? ScrapingModeEnum.ManualBatch;
    const isBatchMode = scrapingModeId === ScrapingModeEnum.Batch || scrapingModeId === ScrapingModeEnum.PixelBatch;
    const isPixelMode = scrapingModeId === ScrapingModeEnum.PixelBatch;

    return {
      gameId: raw.gameId,
      name: raw.name,
      scrapingModeId,
      partialUrl: raw.partialUrl,
      startPage: isBatchMode ? raw.startPage : null,
      endPage: isBatchMode ? raw.endPage : null,
      pixelX: isPixelMode ? raw.pixelX : null,
      pixelY: isPixelMode ? raw.pixelY : null,
      pixelImageWidth: isPixelMode ? raw.pixelImageWidth : null,
      pixelImageHeight: isPixelMode ? raw.pixelImageHeight : null,
    };
  }

  private buildUpdatePayload(payload: CreateGameUrl): UpdateGameUrl {
    const { gameId: _gameId, ...updatePayload } = payload;
    return updatePayload;
  }

  private syncRelations(gameUrlId: number) {
    const selectedProducts = this.selectedProductIds();
    const selectedPixels = this.selectedPixelIds();

    const productAdds = selectedProducts
      .filter((x) => !this.initialProductIds.includes(x))
      .map((productId) => this.gameUrlProductService.create({ productId, gameUrlId }));

    const productDeletes = this.initialProductIds
      .filter((x) => !selectedProducts.includes(x))
      .map((productId) => this.gameUrlProductService.delete(productId, gameUrlId));

    const pixelAdds = selectedPixels
      .filter((x) => !this.initialPixelIds.includes(x))
      .map((pixelId) => this.gameUrlPixelService.create({ pixelId, gameUrlId }));

    const pixelDeletes = this.initialPixelIds
      .filter((x) => !selectedPixels.includes(x))
      .map((pixelId) => this.gameUrlPixelService.delete(pixelId, gameUrlId));

    const requests = [...productAdds, ...productDeletes, ...pixelAdds, ...pixelDeletes];

    if (requests.length === 0) {
      return of(void 0);
    }

    return forkJoin(requests).pipe(switchMap(() => of(void 0)));
  }
}
