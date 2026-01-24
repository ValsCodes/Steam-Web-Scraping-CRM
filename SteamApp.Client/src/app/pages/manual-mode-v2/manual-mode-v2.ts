import {
  Component,
  OnInit,
  OnDestroy,
  QueryList,
  ViewChildren,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { CONSTANTS } from '../../common/constants';
import { ChangeDetectorRef } from '@angular/core';

import { Game, GameUrl, GameUrlProduct, Item, Product, UpdateItem } from '../../models/index';
import { GameService, GameUrlProductService, GameUrlService, ProductService } from '../../services';

@Component({
  selector: 'steam-manual-mode-v2',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './manual-mode-v2.html',
  styleUrl: './manual-mode-v2.scss',
})
export class ManualModeV2 implements OnInit, OnDestroy {

  private readonly constants = CONSTANTS;
  private readonly destroy$ = new Subject<void>();

    constructor(
    private readonly gameUrlProductService: GameUrlProductService,
    private readonly cdr: ChangeDetectorRef,
    private gameService: GameService,
    private gameUrlService: GameUrlService
  ) {}

  ngOnInit(): void {
    this.loadGameUrls();
    this.loadGames();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  productsCollection: GameUrlProduct[] = [];

  weaponsBatchSize = 3;
  currentIndex = 1;

  hatStatusLabel = '';
  weaponStatusLabel = '';


  showAllButtonClicked() {
    this.loadGameUrlProducts();
  }
  resetButtonClicked() {
    throw new Error('Method not implemented.');
  }
  startBatchButtonClicked() {

    throw new Error('Method not implemented.');

  }

  games: Game[] = [];
  gameIdControl = new FormControl<number | null>(null);
  gameNameById = new Map<number, string>();

  private loadGames(): void {
    this.gameService.getAll().subscribe({
      next: (games) => {
        this.games = games;

        this.gameNameById.clear();
        for (const game of games) {
          this.gameNameById.set(game.id, game.name);
        }
      },
    });
  }

  gameUrls: GameUrl[] = [];
  gameUrlNameById = new Map<number, string>();
gameUrlIdControl = new FormControl<number | null>(null);

  private loadGameUrls(): void {
    this.gameUrlService.getAll().subscribe({
      next: (gameUrls) => {
        this.gameUrls = gameUrls;

        this.gameUrlNameById.clear();
        for (const gameUrl of gameUrls) {
          this.gameUrlNameById.set(gameUrl.id, gameUrl.name ?? '-');
        }
      },
    });
  }

  private get selectedGameUrlId(): number | null {
  return this.gameUrlIdControl.value;
}

  loadGameUrlProducts(): void {
  const gameUrlId = this.selectedGameUrlId;

  if (gameUrlId === null) {
    return;
  }

    this.gameUrlProductService
      .existsByGameUrl(gameUrlId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.productsCollection = data;
        },
        error: (err) => {
          console.error('Error Loading Items:', err);
        },
      });
  }
}
