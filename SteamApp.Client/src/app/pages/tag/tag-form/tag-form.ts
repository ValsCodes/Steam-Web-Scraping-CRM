import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { GameService, TagService } from '../../../services';
import { Game, CreateTag, UpdateTag } from '../../../models';

@Component({
  selector: 'steam-tag-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tag-form.html',
  styleUrl: './tag-form.scss',
})
export class TagForm implements OnInit {
  isEditMode = false;
  tagId?: number;

  form = this.fb.nonNullable.group({
    gameId: [0],
    name: [''],
  });

  games: Game[] = [];
  gameNameById = new Map<number, string>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private tagService: TagService,
    private gameService: GameService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    this.loadGames();

    if (idParam) {
      this.isEditMode = true;
      this.tagId = Number(idParam);
      this.loadTag(this.tagId);
    }
  }

  private loadTag(id: number): void {
    this.tagService.getById(id).subscribe(tag => {
      this.form.patchValue({
        gameId: tag.gameId,
        name: tag.name ?? '',
      });

      this.form.controls.gameId.disable();
    });
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe(games => {
      this.games = games;

      this.gameNameById.clear();
      for (const game of games) {
        this.gameNameById.set(game.id, game.name);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.tagId) {
      const update: UpdateTag = {
        name: this.form.controls.name.value,
      };

      this.tagService.update(this.tagId, update).subscribe(() => {
        this.router.navigate(['/tags']);
      });
    } else {
      const create: CreateTag = this.form.getRawValue();

      this.tagService.create(create).subscribe(() => {
        this.router.navigate(['/tags']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/tags']);
  }
}