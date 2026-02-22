import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, startWith, tap } from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { Game, Product, Tag, ProductTag } from '../../../models';
import {
  GameService,
  ProductService,
  TagService,
  ProductTagService,
} from '../../../services';
import { ComboBoxComponent } from '../../../components/filter-components/combo-box-filter.component';

@Component({
  selector: 'app-product-tags',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    ComboBoxComponent,
  ],
  templateUrl: './product-tags-view.html',
})
export class ProductTagsView implements AfterViewInit {
  displayedColumns: string[] = [
    'productName',
    'tagName',
    'actions',
  ];

  dataSource = new MatTableDataSource<ProductTag>([]);

  productTags: ProductTag[] = [];
  filteredProductTags: ProductTag[] = [];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  readonly productsFiltered$ = new BehaviorSubject<readonly Product[]>([]);
  readonly tagsFiltered$ = new BehaviorSubject<readonly Tag[]>([]);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly productIdControl = new FormControl<number | null>(null);
  readonly tagIdControl = new FormControl<number | null>(null);

  readonly bindGameToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap(gameId => this.applyGameFilter(gameId))
  );

  readonly bindProductToExternal$ = this.productIdControl.valueChanges.pipe(
    startWith(this.productIdControl.value),
    tap(() => this.loadFilteredProductTags())
  );

  readonly bindTagToExternal$ = this.tagIdControl.valueChanges.pipe(
    startWith(this.tagIdControl.value),
    tap(() => this.loadFilteredProductTags())
  );

  private games: Game[] = [];
  private productsAll: Product[] = [];
  private tagsAll: Tag[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly productTagService: ProductTagService,
    private readonly gameService: GameService,
    private readonly productService: ProductService,
    private readonly tagService: TagService,
    private readonly router: Router,
  ) {}

  ngAfterViewInit(): void {
    this.loadGames();
    this.loadProducts();
    this.loadTags();
    this.fetchProductTags();
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe(games => {
      this.games = games;
      this.games$.next(games);
    });
  }

  private loadProducts(): void {
    this.productService.getAll().subscribe(products => {
      this.productsAll = products;
      this.applyGameFilter(this.gameIdControl.value);
    });
  }

  private loadTags(): void {
    this.tagService.getAll().subscribe(tags => {
      this.tagsAll = tags;
      this.applyGameFilter(this.gameIdControl.value);
    });
  }

  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.productsFiltered$.next([]);
      this.tagsFiltered$.next([]);
      this.productIdControl.reset();
      this.tagIdControl.reset();
      this.loadFilteredProductTags();
      return;
    }

    this.productsFiltered$.next(
      this.productsAll.filter(p => p.gameId === gameId)
    );

    this.tagsFiltered$.next(
      this.tagsAll.filter(t => t.gameId === gameId)
    );

    this.productIdControl.reset();
    this.tagIdControl.reset();
    this.loadFilteredProductTags();
  }

  fetchProductTags(): void {
    this.productTagService.getAll().subscribe(items => {
      this.productTags = items;
      this.filteredProductTags = items;

      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  private loadFilteredProductTags(): void {
    const gameId = this.gameIdControl.value;
    const productId = this.productIdControl.value;
    const tagId = this.tagIdControl.value;

    this.filteredProductTags = this.productTags.filter(x => {
      const matchesGame =
        gameId === null ||
        this.productsAll.some(
          p => p.id === x.productId && p.gameId === gameId
        );

      const matchesProduct =
        productId === null || x.productId === productId;

      const matchesTag =
        tagId === null || x.tagId === tagId;

      return matchesGame && matchesProduct && matchesTag;
    });

    this.dataSource.data = this.filteredProductTags;
  }

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.productIdControl.setValue(null);
    this.tagIdControl.setValue(null);
    this.loadFilteredProductTags();
  }

  createButtonClicked(): void {
    this.router.navigate(['/product-tags/create']);
  }

  deleteButtonClicked(productId: number, tagId: number): void {
    if (!confirm('Remove tag from product?')) {
      return;
    }

    this.productTagService.delete(productId, tagId).subscribe(() => {
      this.fetchProductTags();
    });
  }
}
