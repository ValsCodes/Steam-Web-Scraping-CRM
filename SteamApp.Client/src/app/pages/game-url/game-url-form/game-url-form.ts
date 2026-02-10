import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { GameUrlService } from '../../../services/game-url/game-url.service';
import { CreateGameUrl, UpdateGameUrl } from '../../../models/game-url.model';
import { GameService } from '../../../services/game/game.service';
import { Game } from '../../../models';

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

  games: Game[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly gameUrlService: GameUrlService,
    private readonly gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.loadGames();

    if (idParam) {
      this.isEditMode = true;
      this.gameUrlId = Number(idParam);
      this.loadGameUrl(this.gameUrlId);
    }
  }

  private loadGameUrl(id: number): void {
    this.gameUrlService.getById(id).subscribe(gameUrl => {
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

      this.form.controls.gameId.disable();
    });
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe(games => {
      this.games = games;
      this.cdr.detectChanges();
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.gameUrlId) {
      const update: UpdateGameUrl = this.form.getRawValue();
      this.gameUrlService.update(this.gameUrlId, update).subscribe(() => {
        this.router.navigate(['/game-urls']);
      });
    } else {
      const create: CreateGameUrl = this.form.getRawValue();
      this.gameUrlService.create(create).subscribe(() => {
        this.router.navigate(['/game-urls']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/game-urls']);
  }
}
