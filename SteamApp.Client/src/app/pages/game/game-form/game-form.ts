import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

import { Game } from '../../../models/game.model';
import { GameService } from '../../../services/game/game.service';


@Component({
  selector: 'steam-game-form',
  standalone: true,
  imports: [ReactiveFormsModule,CommonModule],
  templateUrl: './game-form.html',
  styleUrl: './game-form.scss',
})
export class GameForm implements OnInit {
  gameForm!: FormGroup;
  isEditMode = false;
  gameId?: number;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    this.gameForm = this.fb.group({
      name: ['', Validators.required],
      //baseUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\//)]],
      pageUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\//)]]
    });

    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.gameId = Number(idParam);
      this.loadGame(this.gameId);
    }
  }

  private loadGame(id: number): void {
    this.gameService.getById(id).subscribe(game => {
      this.gameForm.patchValue({
        name: game.name,
        //baseUrl: game.baseUrl,  
        pageUrl: game.pageUrl
      });
    });
  }

  onSubmit(): void {
    if (this.gameForm.invalid) {
      return;
    }

    const payload = this.gameForm.value as Omit<Game, 'id'>;

    if (this.isEditMode && this.gameId) {
      this.gameService.update(this.gameId, payload).subscribe(() => {
        this.router.navigate(['/games']);
      });
    } else {
      this.gameService.create(payload).subscribe(() => {
        this.router.navigate(['/games']);
      });
    }
  }

  backButtonClicked(): void {
    this.router.navigate(['/games']);
  }
}
