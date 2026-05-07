import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, Observable } from 'rxjs';

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
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    gameId: [0],
    name: [''],
    isActive: [true],
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
        isActive: tag.isActive,
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
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const request$: Observable<unknown> = this.isEditMode && this.tagId
      ? this.tagService.update(this.tagId, {
          name: this.form.controls.name.value,
          isActive: this.form.controls.isActive.value,
        } as UpdateTag)
      : this.tagService.create(this.form.getRawValue() as CreateTag);

    request$
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe(() => {
        this.router.navigate(['/tags']);
      });
  }

  cancel(): void {
    this.router.navigate(['/tags']);
  }
}
