import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UpdateProduct } from '../../../models/product.model';
import { ProductService } from '../../../services/product/product.service';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router,ActivatedRoute } from '@angular/router';

@Component({
  selector: 'steam-edit-product-form',
  standalone: true,
  imports: [CommonModule,
    ReactiveFormsModule,  ],
  templateUrl: './edit-product-form.component.html',
  styleUrl: './edit-product-form.component.scss'
})
export class EditProductFormComponent implements OnInit {
  productForm!: FormGroup;
  productId!: number;
  qualities = [
    { id: 1, label: 'Poor' },
    { id: 2, label: 'Fair' },
    { id: 3, label: 'Good' },
    { id: 4, label: 'Excellent' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    // get id from route
    this.productId = Number(this.route.snapshot.paramMap.get('id'));

    // init form
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      qualityId: [null],
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
      isSold: [false]
    });

    // load existing product
    this.productService.getProductById(this.productId).subscribe(product => {
      this.productForm.patchValue({
        name: product.name,
        qualityId: product.qualityId,
        description: product.description,
        dateBought: product.dateBought ? this.formatDate(product.dateBought) : null,
        dateSold: product.dateSold ? this.formatDate(product.dateSold) : null,
        costPrice: product.costPrice,
        targetSellPrice1: product.targetSellPrice1,
        targetSellPrice2: product.targetSellPrice2,
        targetSellPrice3: product.targetSellPrice3,
        targetSellPrice4: product.targetSellPrice4,
        soldPrice: product.soldPrice,
        isHat: product.isHat,
        isWeapon: product.isWeapon,
        isSold: product.isSold
      });
    });
  }

  private formatDate(date: Date): string {
    // format yyyy-MM-dd for input[type=date]
    const d = new Date(date);
    const month = ('0' + (d.getMonth() + 1)).slice(-2);
    const day = ('0' + d.getDate()).slice(-2);
    return `${d.getFullYear()}-${month}-${day}`;
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const updated: UpdateProduct = {
      id: this.productId,
      ...this.productForm.value
    };

    this.productService.updateProduct(updated).subscribe({
      next: product => {
        console.log('Updated product', updated);
      this.router.navigate(['products-catalog']);
      }, 
      error: err => console.error('Error updating product:', err)})
  }
}
