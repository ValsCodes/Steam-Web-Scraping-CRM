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
  });

  games: Game[] = [];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private gameUrlService: GameUrlService,
    private gameService: GameService,
    private cdr: ChangeDetectorRef,
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
    this.gameUrlService.getById(id).subscribe((gameUrl) => {
      this.form.patchValue({
        gameId: Number(gameUrl.gameId),
        name: gameUrl.name ?? '',
        partialUrl: gameUrl.partialUrl ?? '',
        isBatchUrl: gameUrl.isBatchUrl,
        startPage: gameUrl.startPage,
        endPage: gameUrl.endPage,
        isPixelScrape: gameUrl.isPixelScrape,
      });

      // GameId should not change on edit
      this.form.controls.gameId.disable();
    });
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe({
      next: (games) => {
        this.games = games;
        this.cdr.detectChanges();
      },
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
