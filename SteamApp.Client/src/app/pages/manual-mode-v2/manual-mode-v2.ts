import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { CONSTANTS } from '../../common/constants';
import { ChangeDetectorRef } from '@angular/core';

import {
  Game,
  GameUrl,
  GameUrlProduct
} from '../../models/index';
import {
  GameService,
  GameUrlProductService,
  GameUrlService,
} from '../../services';
import { CopyLinkComponent } from "../../components";

@Component({
  selector: 'steam-manual-mode-v2',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule, CopyLinkComponent],
  templateUrl: './manual-mode-v2.html',
  styleUrl: './manual-mode-v2.scss',
})
export class ManualModeV2 implements OnInit, OnDestroy {
currentIndex: any;
batchSize: any;
clearButtonClicked() {
throw new Error('Method not implemented.');
}
openAllButtonClicked() {
throw new Error('Method not implemented.');
}
resetButtonClicked() {
throw new Error('Method not implemented.');
}
startBatchButtonClicked() {
throw new Error('Method not implemented.');
}
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
    private readonly gameUrlProductService: GameUrlProductService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  games: Game[] = [];
  gameIdControl = new FormControl<number | null>(null);

  gameUrlsAll: GameUrl[] = [];
  gameUrlsFiltered: GameUrl[] = [];
  gameUrlIdControl = new FormControl<number | null>(null);

  products: GameUrlProduct[] = [];

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.bindGameSelection();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

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

  private bindGameSelection(): void {
    this.gameIdControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((gameId) => {
        this.applyGameFilter(gameId);
      });
  }

  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.gameUrlsFiltered = [];
      this.gameUrlIdControl.reset();
      this.cdr.markForCheck();
      return;
    }

    this.gameUrlsFiltered = this.gameUrlsAll.filter(
      (url) => url.gameId === gameId,
    );

    this.gameUrlIdControl.reset();
    this.cdr.markForCheck();
  }

  showAllButtonClicked(): void {
    const gameUrlId = this.gameUrlIdControl.value;
    if (gameUrlId === null) {
      return;
    }

    this.gameUrlProductService
      .existsByGameUrl(gameUrlId)
      .pipe(takeUntil(this.destroy$))
      .subscribe((products) => {
        this.products = products;
        this.cdr.markForCheck();
      });
  }

  trackByProductId(_: number, product: GameUrlProduct): number {
    return product.productId;
  }
}
