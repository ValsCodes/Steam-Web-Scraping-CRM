import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateGameUrlPixel, Game, GameUrl, Pixel } from '../../../models';
import {
  GameService,
  GameUrlPixelService,
  GameUrlService,
  PixelService
} from '../../../services';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'steam-game-url-pixel-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './game-url-pixel-form.html',
  styleUrl: './game-url-pixel-form.scss'
})
export class GameUrlPixelForm implements OnInit, OnDestroy {

  isEditMode = false;

  originalPixelId?: number;
  originalGameUrlId?: number;

  form = this.fb.nonNullable.group({
    pixelId: [0, Validators.required],
    gameUrlId: [0, Validators.required]
  });

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private gameUrlPixelService: GameUrlPixelService,
    private gameUrlService: GameUrlService,
    private pixelService: PixelService,
    private gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  games: Game[] = [];
  gameIdControl = new FormControl<number | null>(null);

  gameUrlsAll: GameUrl[] = [];
  gameUrlsFiltered: GameUrl[] = [];

  pixelsAll: Pixel[] = [];
  pixelsFiltered: Pixel[] = [];

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.loadPixels();

    const pixelId = this.route.snapshot.paramMap.get('pixelId');
    const gameUrlId = this.route.snapshot.paramMap.get('gameUrlId');

    if (pixelId && gameUrlId) {
      this.isEditMode = true;

      this.originalPixelId = Number(pixelId);
      this.originalGameUrlId = Number(gameUrlId);

      this.form.patchValue({
        pixelId: this.originalPixelId,
        gameUrlId: this.originalGameUrlId
      });

      this.form.controls.pixelId.disable();
      this.form.controls.gameUrlId.disable();
    }

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
      .subscribe(games => {
        this.games = games;
        this.cdr.markForCheck();
      });
  }

  private loadGameUrls(): void {
    this.gameUrlService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(urls => {
        this.gameUrlsAll = urls;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private loadPixels(): void {
    this.pixelService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(pixels => {
        this.pixelsAll = pixels;
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
      this.gameUrlsFiltered = [];
      this.pixelsFiltered = [];
      this.cdr.markForCheck();
      return;
    }

    this.gameUrlsFiltered = this.gameUrlsAll.filter(
      url => url.gameId === gameId
    );

    this.pixelsFiltered = this.pixelsAll.filter(
      pixel => pixel.gameId === gameId
    );

    this.cdr.markForCheck();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    const payload: CreateGameUrlPixel = this.form.getRawValue();

    if (
      this.isEditMode &&
      this.originalPixelId !== undefined &&
      this.originalGameUrlId !== undefined
    ) {
      this.gameUrlPixelService
        .delete(this.originalPixelId, this.originalGameUrlId)
        .subscribe(() => {
          this.gameUrlPixelService.create(payload).subscribe(() => {
            this.router.navigate(['/game-url-pixels']);
          });
        });

      return;
    }

    this.gameUrlPixelService.create(payload).subscribe(() => {
      this.router.navigate(['/game-url-pixels']);
    });
  }

  cancel(): void {
    this.router.navigate(['/game-url-pixels']);
  }
}
