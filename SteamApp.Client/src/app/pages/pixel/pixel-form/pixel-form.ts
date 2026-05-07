import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { GameService, PixelService } from '../../../services';
import { CreatePixel, UpdatePixel, Game } from '../../../models';

@Component({
  selector: 'steam-pixel-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './pixel-form.html',
  styleUrl: './pixel-form.scss',
})
export class PixelForm implements OnInit {
  isEditMode = false;
  pixelId?: number;
  isSubmitting = false;

  games: Game[] = [];

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    gameId: [0, Validators.required],
    redValue: [0, [Validators.required, Validators.min(0), Validators.max(255)]],
    greenValue: [0, [Validators.required, Validators.min(0), Validators.max(255)]],
    blueValue: [0, [Validators.required, Validators.min(0), Validators.max(255)]],
    isActive: [true],
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private pixelService: PixelService,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    this.loadGames();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.pixelId = Number(idParam);
      this.loadPixel(this.pixelId);
    }
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe(games => {
      this.games = games.filter(game => game.isActive);
    });
  }

  private loadPixel(id: number): void {
    this.pixelService.getById(id).subscribe(pixel => {
      this.form.patchValue({
        name: pixel.name,
        gameId: pixel.gameId,
        redValue: pixel.redValue,
        greenValue: pixel.greenValue,
        blueValue: pixel.blueValue,
        isActive: pixel.isActive,
      });
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const request$: Observable<unknown> = this.isEditMode && this.pixelId
      ? this.pixelService.update(this.pixelId, this.form.getRawValue() as UpdatePixel)
      : this.pixelService.create(this.form.getRawValue() as CreatePixel);

    request$
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe(() => {
        this.router.navigate(['/pixels']);
      });
  }

  cancel(): void {
    this.router.navigate(['/pixels']);
  }
}
