import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { CreateWatchList, UpdateWatchList } from '../../../models/watch-list.model';
import { WatchListService } from '../../../services/watch-list/watch-list.service';

@Component({
  selector: 'steam-watch-list-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './watch-list-form.html',
  styleUrl: './watch-list-form.scss'
})
export class WatchListForm implements OnInit {
  isEditMode = false;
  watchListId?: number;

  form = this.fb.nonNullable.group({
    gameId: [null as number | null],
    gameUrlId: [null as number | null],
    rating: [null as number | null],
    batchUrl: [''],
    name: [''],
    releaseDate: ['', Validators.required],
    description: ['']
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private watchListService: WatchListService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.watchListId = Number(idParam);
      this.loadWatchList(this.watchListId);
    }
  }

  private loadWatchList(id: number): void {
    this.watchListService.getById(id).subscribe(item => {
      this.form.patchValue({
        gameId: item.gameId ?? null,
        gameUrlId: item.gameUrlId ?? null,
        rating: item.rating ?? null,
        batchUrl: item.batchUrl ?? '',
        name: item.name ?? '',
        releaseDate: item.releaseDate,
        description: item.description ?? ''
      });
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.watchListId) {
      const update: UpdateWatchList = this.form.getRawValue();

      this.watchListService.update(this.watchListId, update).subscribe(() => {
        this.router.navigate(['/watch-list']);
      });
    } else {
      const create: CreateWatchList = this.form.getRawValue();

      this.watchListService.create(create).subscribe(() => {
        this.router.navigate(['/watch-list']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/watch-list']);
  }
}
