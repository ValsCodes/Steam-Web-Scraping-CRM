import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
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

  games: Game[] = [];

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    gameId: [0, Validators.required],
    redValue: [0, [Validators.required, Validators.min(0), Validators.max(255)]],
    greenValue: [0, [Validators.required, Validators.min(0), Validators.max(255)]],
    blueValue: [0, [Validators.required, Validators.min(0), Validators.max(255)]],
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
      this.games = games;
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
      });
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.pixelId) {
      const update: UpdatePixel = this.form.getRawValue();
      this.pixelService.update(this.pixelId, update).subscribe(() => {
        this.router.navigate(['/pixels']);
      });
    } else {
      const create: CreatePixel = this.form.getRawValue();
      this.pixelService.create(create).subscribe(() => {
        this.router.navigate(['/pixels']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/pixels']);
  }
}
