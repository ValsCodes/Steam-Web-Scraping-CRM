import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CreateProduct, Product } from '../../../models/product.model';
import { ProductService } from '../../../services/product/product.service';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { qualitiesCollection, Quality, qualitiesMap } from '../../../models/enums/quality.enum';
import { paintsCollection, paintsMap, Paint } from '../../../models/enums/paint.enum';
import { Sheen, sheensCollection, sheensMap } from '../../../models/enums/sheen.enum';

@Component({
    selector: 'steam-create-product-form',
    imports: [CommonModule,
        ReactiveFormsModule,],
    templateUrl: './create-product-form.component.html',
    styleUrl: './create-product-form.component.scss'
})
export class CreateProductFormComponent implements OnInit {
  productForm!: FormGroup;

 qualities = qualitiesCollection;
        sheens = sheensCollection;
    paints = paintsCollection;

  constructor(
    private router:Router,
    private fb: FormBuilder,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      qualityId: [null],
      paintId: [null],
      sheenId: [null],
      description: [''],
      dateBought: [null],
      dateSold: [null],
      costPrice: [null, [Validators.min(0)]],
      targetSellPrice1: [null, [Validators.min(0)]],
      targetSellPrice2: [null, [Validators.min(0)]],
      targetSellPrice3: [null, [Validators.min(0)]],
      targetSellPrice4: [null, [Validators.min(0)]],
      soldPrice: [null, [Validators.min(0)]],
      isHat: [false],
      isWeapon: [false],
      isSold: [false],
      isStrange:[false]
    });
  }

    backButtonClicked() {
    this.router.navigate(['products-catalog']);
  }

   getQualityLabel(id: Quality) {
    return qualitiesMap[id];
  }

    getPaintLabel(id: Paint) {
      return paintsMap[id];
    }

    getSheenLabel(id: Sheen) {
      return sheensMap[id];
    }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const newProduct: CreateProduct = {
      id: 0,
      ...this.productForm.value
    };

    this.productService.createProduct(newProduct).subscribe({
      next: product => {
        console.log('Created product', product);
        this.router.navigate(['products-catalog']);
      },
      error: err => console.error('Error creating product:', err)
    });
  }
}
