import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateGameUrlProduct, GameUrl, Product } from '../../../models';
import { GameUrlProductService, GameUrlService, ProductService } from '../../../services';
@Component({
  selector: 'steam-game-url-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './game-url-product-form.html',
  styleUrl: './game-url-product-form.scss'
})
export class GameUrlProductForm implements OnInit {
  isEditMode = false;

  originalProductId?: number;
  originalGameUrlId?: number;

  gameUrls: GameUrl[] = [];
  products: Product[] = [];

  form = this.fb.nonNullable.group({
    productId: [0, Validators.required],
    gameUrlId: [0, Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private service: GameUrlProductService,
    private gameUrlService: GameUrlService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.loadGameUrls();
    this.loadProducts();

    const productId = this.route.snapshot.paramMap.get('productId');
    const gameUrlId = this.route.snapshot.paramMap.get('gameUrlId');

    if (productId && gameUrlId) {
      this.isEditMode = true;

      this.originalProductId = Number(productId);
      this.originalGameUrlId = Number(gameUrlId);

      this.form.patchValue({
        productId: this.originalProductId,
        gameUrlId: this.originalGameUrlId
      });

      this.form.controls.productId.disable();
      this.form.controls.gameUrlId.disable();
    }
  }

  private loadGameUrls(): void {
    this.gameUrlService.getAll().subscribe(urls => {
      this.gameUrls = urls;
    });
  }

  private loadProducts(): void {
    this.productService.getAll().subscribe(products => {
      this.products = products;
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    const payload: CreateGameUrlProduct = this.form.getRawValue();

    if (this.isEditMode &&
        this.originalProductId !== undefined &&
        this.originalGameUrlId !== undefined) {

      this.service
        .delete(this.originalProductId, this.originalGameUrlId)
        .subscribe(() => {
          this.service.create(payload).subscribe(() => {
            this.router.navigate(['/game-url-products']);
          });
        });

      return;
    }

    this.service.create(payload).subscribe(() => {
      this.router.navigate(['/game-url-products']);
    });
  }

  cancel(): void {
    this.router.navigate(['/game-url-products']);
  }
}
