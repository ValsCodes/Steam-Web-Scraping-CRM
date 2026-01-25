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
  FormControl,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  GameService,
  GameUrlService,
  ProductService,
  WatchListService,
} from '../../../services';
import {
  CreateWatchList,
  Game,
  GameUrl,
  Product,
  UpdateWatchList,
} from '../../../models';
import { Subject, takeUntil } from 'rxjs';

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
  isBatchSelected = false;
  watchListId?: number;

  // holds edit data until dependent collections are ready
  private pendingEditItem: UpdateWatchList | null = null;

  form = this.fb.nonNullable.group({
    gameId: [0],
    gameUrlId: [null as number | null, Validators.required],
    productId: [null as number | null, Validators.required],
    rating: [null as number | null],
    batchNumber: [null as number | null],
    name: [''],
    releaseDate: ['', Validators.required],
    description: [''],
    isActive: [true],
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private watchListService: WatchListService,
    private gameService: GameService,
    private gameUrlService: GameUrlService,
    private productService: ProductService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  // ---------- State ----------
  games: Game[] = [];
  gameIdControl = new FormControl<number | null>(null);

  gameUrlsAll: GameUrl[] = [];
  gameUrlsFiltered: GameUrl[] = [];

  productsAll: Product[] = [];
  productsFiltered: Product[] = [];

  // ---------- Lifecycle ----------
  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    this.loadGames();
    this.loadGameUrls();
    this.loadProducts();
    this.bindGameSelection();
    this.bindGameUrlSelection();

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

  // ---------- Data loading ----------
  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games = games;
        this.cdr.markForCheck();
      });
  }

  private loadGameUrls(): void {
    this.gameUrlService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((urls) => {
        this.gameUrlsAll = urls;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private loadProducts(): void {
    this.productService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((products) => {
        this.productsAll = products;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private loadWatchList(id: number): void {
    this.watchListService
      .getById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((item) => {
        this.pendingEditItem = item;

        // step 1: replay game selection
        this.gameIdControl.setValue(item.gameId);

        // keep form gameId aligned
        this.form.controls.gameId.setValue(item.gameId);

        this.cdr.markForCheck();
      });
  }

  // ---------- Selection bindings ----------
  private bindGameSelection(): void {
    this.gameIdControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((gameId) => {
        this.form.controls.gameId.setValue(gameId ?? 0);
        this.applyGameFilter(gameId);
      });
  }

  private bindGameUrlSelection(): void {
    this.form.controls.gameUrlId.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((gameUrlId) => {
        this.isBatchSelected = this.resolveIsBatch(gameUrlId);
        this.updateBatchValidators();
        this.cdr.markForCheck();
      });
  }

  // ---------- Filtering ----------
  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.gameUrlsFiltered = [];
      this.productsFiltered = [];

      this.form.controls.gameUrlId.reset();
      this.form.controls.productId.reset();

      this.cdr.markForCheck();
      return;
    }

    this.gameUrlsFiltered = this.gameUrlsAll.filter(
      (url) => url.gameId === gameId,
    );

    this.productsFiltered = this.productsAll.filter(
      (product) => product.gameId === gameId,
    );

    // replay dependent selections in edit mode
    if (this.isEditMode && this.pendingEditItem) {
      this.form.patchValue({
        gameUrlId: this.pendingEditItem.gameUrlId ?? null,
        productId: this.pendingEditItem.productId ?? null,
        rating: this.pendingEditItem.rating ?? null,
        batchNumber: this.pendingEditItem.batchNumber ?? null,
        name: this.pendingEditItem.name ?? '',
        releaseDate: this.pendingEditItem.releaseDate ?? '',
        description: this.pendingEditItem.description ?? '',
        isActive: this.pendingEditItem.isActive,
      });

      this.pendingEditItem = null;

      this.form.controls.gameId.disable();
      this.form.controls.gameUrlId.disable();
      this.form.controls.productId.disable();
    } else {
      this.form.controls.gameUrlId.reset();
      this.form.controls.productId.reset();
    }

    this.cdr.markForCheck();
  }

  // ---------- Batch handling ----------
  private resolveIsBatch(gameUrlId: number | null): boolean {
    if (gameUrlId === null) {
      return false;
    }

    const gameUrl = this.gameUrlsFiltered.find(
      (url) => url.id === gameUrlId,
    );

    return gameUrl?.isBatchUrl === true;
  }

  private updateBatchValidators(): void {
    const batchControl = this.form.controls.batchNumber;

    if (this.isBatchSelected) {
      batchControl.addValidators(Validators.required);
    } else {
      batchControl.clearValidators();
      batchControl.reset();
    }

    batchControl.updateValueAndValidity({ emitEvent: false });
  }

  // ---------- Submit ----------
  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.watchListId !== undefined) {
      const update: UpdateWatchList = this.form.getRawValue();
      this.watchListService.update(this.watchListId, update).subscribe(() => {
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
