import { Component, OnInit, signal } from '@angular/core';
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

  readonly games = signal<readonly Game[]>([]);
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

    const idParam = this.route.snapshot.paramMap.get('id');

    this.loadGames();

    if (idParam)
    {
      this.isEditMode = true;
      this.gameUrlId = Number(idParam);
      this.loadGameUrl(this.gameUrlId);
    }
  }

  private syncSignalsFromForm(): void
  {
    this.isPublicApi.set(this.form.controls.isPublicApi.value === true);
    this.isBatchUrl.set(this.form.controls.isBatchUrl.value === true);
    this.isPixelScrape.set(this.form.controls.isPixelScrape.value === true);
  }

  private loadGames(): void
  {
    this.gameService.getAll().subscribe(games =>
    {
      this.games.set(games);
    });
  }

  private loadGameUrl(id: number): void
  {
    this.gameUrlService.getById(id).subscribe(gameUrl =>
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

      this.gameUrlService.update(this.gameUrlId, update).subscribe(() =>
      {
        this.router.navigate(['/game-urls']);
      });
    }
    else
    {
      const create: CreateGameUrl = this.form.getRawValue();

      this.gameUrlService.create(create).subscribe(() =>
      {
        this.router.navigate(['/game-urls']);
      });
    }
  }

  cancel(): void
  {
    this.router.navigate(['/game-urls']);
  }
}