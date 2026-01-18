import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { CreateWishList, UpdateWishList } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';

@Component({
  selector: 'steam-wish-list-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './wish-list-form.html',
  styleUrl: './wish-list-form.scss'
})
export class WishListForm implements OnInit {
  isEditMode = false;
  wishListId?: number;

  form = this.fb.nonNullable.group({
    gameId: [0, Validators.required],
    price: [null as number | null],
    isActive: [true]
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private wishListService: WishListService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.wishListId = Number(idParam);
      this.loadWishList(this.wishListId);
    }
  }

  private loadWishList(id: number): void {
    this.wishListService.getById(id).subscribe(item => {
      this.form.patchValue({
        gameId: item.gameId,
        price: item.price,
        isActive: item.isActive
      });

      // GameId is immutable after creation
      this.form.controls.gameId.disable();
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.wishListId) {
      const update: UpdateWishList = {
        price: this.form.controls.price.value,
        isActive: this.form.controls.isActive.value
      };

      this.wishListService.update(this.wishListId, update).subscribe(() => {
        this.router.navigate(['/wish-list']);
      });
    } else {
      const create: CreateWishList = this.form.getRawValue();

      this.wishListService.create(create).subscribe(() => {
        this.router.navigate(['/wish-list']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/wish-list']);
  }
}
