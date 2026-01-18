import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { CreateProduct, UpdateProduct } from '../../../models/product.model';
import { ProductService } from '../../../services/product/product.service';

@Component({
  selector: 'steam-product-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss'
})
export class ProductForm implements OnInit {
  isEditMode = false;
  productId?: number;

  form = this.fb.nonNullable.group({
    gameUrlId: [0, Validators.required],
    name: ['']
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.productId = Number(idParam);
      this.loadProduct(this.productId);
    }
  }

  private loadProduct(id: number): void {
    this.productService.getById(id).subscribe(product => {
      this.form.patchValue({
        gameUrlId: product.gameUrlId,
        name: product.name ?? ''
      });

      // GameUrlId is immutable after creation
      this.form.controls.gameUrlId.disable();
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.productId) {
      const update: UpdateProduct = {
        name: this.form.controls.name.value
      };

      this.productService.update(this.productId, update).subscribe(() => {
        this.router.navigate(['/products']);
      });
    } else {
      const create: CreateProduct = this.form.getRawValue();

      this.productService.create(create).subscribe(() => {
        this.router.navigate(['/products']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/products']);
  }
}
