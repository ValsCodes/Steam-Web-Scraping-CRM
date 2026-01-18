import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CreatePixel, UpdatePixel } from '../../../models/pixel.model';
import { PixelService } from '../../../services/pixel/pixel.service';

@Component({
  selector: 'steam-pixel-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './pixel-form.html',
  styleUrl: './pixel-form.scss'
})
export class PixelForm implements OnInit {
  isEditMode = false;
  pixelId?: number;

  form = this.fb.nonNullable.group({
    gameUrlId: [0, Validators.required],
    value: [0, Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private pixelService: PixelService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.pixelId = Number(idParam);
      this.loadPixel(this.pixelId);
    }
  }

  private loadPixel(id: number): void {
    this.pixelService.getById(id).subscribe(pixel => {
      this.form.patchValue({
        gameUrlId: pixel.gameUrlId,
        value: pixel.value
      });

      // GameUrlId is immutable after creation
      this.form.controls.gameUrlId.disable();
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.pixelId) {
      const update: UpdatePixel = {
        value: this.form.controls.value.value
      };

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
