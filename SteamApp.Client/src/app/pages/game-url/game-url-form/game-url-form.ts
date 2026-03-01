import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, of, switchMap } from 'rxjs';

import { GameUrlService } from '../../../services/game-url/game-url.service';
import { CreateGameUrl, UpdateGameUrl } from '../../../models/game-url.model';
import { GameService } from '../../../services/game/game.service';
import { Game, PixelListItem, Product } from '../../../models';
import { ProductService } from '../../../services/product/product.service';
import { PixelService } from '../../../services/pixel/pixel.service';
import { GameUrlProductService } from '../../../services/game-url-product/game-url-product.service';
import { GameUrlPixelService } from '../../../services/game-url-pixel/game-url-pixel.service';

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

  readonly games = signal<readonly Game[]>([]);
  readonly products = signal<readonly Product[]>([]);
  readonly pixels = signal<readonly PixelListItem[]>([]);

  readonly selectedProductIds = signal<readonly number[]>([]);
  readonly selectedPixelIds = signal<readonly number[]>([]);

  private initialProductIds: number[] = [];
  private initialPixelIds: number[] = [];

  readonly isBatchUrl = signal(false);
  readonly isPixelScrape = signal(false);
  readonly isPublicApi = signal(false);

  form = this.fb.nonNullable.group({
    gameId: [0, Validators.required],
    name: [''],
    partialUrl: [''],
    isBatchUrl: [false],
    startPage: [null as number | null],
    endPage: [null as number | null],
    isPixelScrape: [false],

    pixelX: [null as number | null],
    pixelY: [null as number | null],
    pixelImageWidth: [null as number | null],
    pixelImageHeight: [null as number | null],
    isPublicApi: [false],
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
  ) {}

  ngOnInit(): void {

    this.syncSignalsFromForm();

    this.form.controls.isPublicApi.valueChanges.subscribe(v =>
    {
      const enabled = v === true;

      if (enabled)
      {
        this.form.controls.isBatchUrl.setValue(false, { emitEvent: false });
        this.form.controls.isPixelScrape.setValue(false, { emitEvent: false });

        this.form.controls.isBatchUrl.disable({ emitEvent: false });
        this.form.controls.isPixelScrape.disable({ emitEvent: false });
      }
      else
      {
        this.form.controls.isBatchUrl.enable({ emitEvent: false });
        this.form.controls.isPixelScrape.enable({ emitEvent: false });
      }

      this.syncSignalsFromForm();
    });

    this.form.controls.isBatchUrl.valueChanges.subscribe(() =>
    {
      this.syncSignalsFromForm();
    });

    this.form.controls.isPixelScrape.valueChanges.subscribe(() =>
    {
      this.syncSignalsFromForm();
    });

    this.loadLookupData();

    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam)
    {
      this.isEditMode = true;
      this.gameUrlId = Number(idParam);
      this.loadGameUrl(this.gameUrlId);
    }
  }

  get filteredProducts(): readonly Product[]
  {
    const gameId = this.form.controls.gameId.value;
    return this.products().filter(x => x.gameId === gameId);
  }

  get filteredPixels(): readonly PixelListItem[]
  {
    const gameId = this.form.controls.gameId.value;
    return this.pixels().filter(x => x.gameId === gameId);
  }

  isProductSelected(productId: number): boolean
  {
    return this.selectedProductIds().includes(productId);
  }

  isPixelSelected(pixelId: number): boolean
  {
    return this.selectedPixelIds().includes(pixelId);
  }

  toggleProduct(productId: number, checked: boolean): void
  {
    const selected = new Set(this.selectedProductIds());

    if (checked) { selected.add(productId); }
    else { selected.delete(productId); }

    this.selectedProductIds.set([...selected]);
  }

  togglePixel(pixelId: number, checked: boolean): void
  {
    const selected = new Set(this.selectedPixelIds());

    if (checked) { selected.add(pixelId); }
    else { selected.delete(pixelId); }

    this.selectedPixelIds.set([...selected]);
  }

  private syncSignalsFromForm(): void
  {
    this.isPublicApi.set(this.form.controls.isPublicApi.value === true);
    this.isBatchUrl.set(this.form.controls.isBatchUrl.value === true);
    this.isPixelScrape.set(this.form.controls.isPixelScrape.value === true);
  }

  private loadLookupData(): void
  {
    this.gameService.getAll().subscribe(games => this.games.set(games));
    this.productService.getAll().subscribe(products => this.products.set(products));
    this.pixelService.getAll().subscribe(pixels => this.pixels.set(pixels));
  }

  private loadGameUrl(id: number): void
  {
    this.gameUrlService.getById(id)
      .pipe(
        switchMap(gameUrl =>
        {
          this.form.patchValue({
            gameId: Number(gameUrl.gameId),
            name: gameUrl.name ?? '',
            partialUrl: gameUrl.partialUrl ?? '',
            isBatchUrl: gameUrl.isBatchUrl,
            startPage: gameUrl.startPage,
            endPage: gameUrl.endPage,
            isPixelScrape: gameUrl.isPixelScrape,

            pixelX: gameUrl.pixelX,
            pixelY: gameUrl.pixelY,
            pixelImageWidth: gameUrl.pixelImageWidth,
            pixelImageHeight: gameUrl.pixelImageHeight,
            isPublicApi: gameUrl.isPublicApi,
          });

          if (gameUrl.isPublicApi)
          {
            this.form.controls.isBatchUrl.disable({ emitEvent: false });
            this.form.controls.isPixelScrape.disable({ emitEvent: false });
          }

          this.syncSignalsFromForm();
          this.form.controls.gameId.disable();

          return forkJoin({
            products: this.gameUrlProductService.existsByGameUrl(id),
            pixels: this.gameUrlPixelService.existsByGameUrl(id),
          });
        })
      )
      .subscribe(({ products, pixels }) =>
      {
        this.initialProductIds = products.map(x => x.productId);
        this.initialPixelIds = pixels.map(x => x.pixelId);

        this.selectedProductIds.set([...this.initialProductIds]);
        this.selectedPixelIds.set([...this.initialPixelIds]);
      });
  }

  onSubmit(): void
  {
    if (this.form.invalid)
    {
      return;
    }

    if (this.isEditMode && this.gameUrlId)
    {
      const update: UpdateGameUrl = this.form.getRawValue();

      this.gameUrlService.update(this.gameUrlId, update)
        .pipe(switchMap(() => this.syncRelations(this.gameUrlId!)))
        .subscribe(() =>
        {
          this.router.navigate(['/game-urls']);
        });
    }
    else
    {
      const create: CreateGameUrl = this.form.getRawValue();

      this.gameUrlService.create(create)
        .pipe(switchMap(created => this.syncRelations(created.id)))
        .subscribe(() =>
        {
          this.router.navigate(['/game-urls']);
        });
    }
  }

  private syncRelations(gameUrlId: number)
  {
    const selectedProducts = this.selectedProductIds();
    const selectedPixels = this.selectedPixelIds();

    const productAdds = selectedProducts
      .filter(x => !this.initialProductIds.includes(x))
      .map(productId => this.gameUrlProductService.create({ productId, gameUrlId }));

    const productDeletes = this.initialProductIds
      .filter(x => !selectedProducts.includes(x))
      .map(productId => this.gameUrlProductService.delete(productId, gameUrlId));

    const pixelAdds = selectedPixels
      .filter(x => !this.initialPixelIds.includes(x))
      .map(pixelId => this.gameUrlPixelService.create({ pixelId, gameUrlId }));

    const pixelDeletes = this.initialPixelIds
      .filter(x => !selectedPixels.includes(x))
      .map(pixelId => this.gameUrlPixelService.delete(pixelId, gameUrlId));

    const requests = [...productAdds, ...productDeletes, ...pixelAdds, ...pixelDeletes];

    if (requests.length === 0)
    {
      return of(void 0);
    }

    return forkJoin(requests).pipe(switchMap(() => of(void 0)));
  }

  cancel(): void
  {
    this.router.navigate(['/game-urls']);
  }
}
