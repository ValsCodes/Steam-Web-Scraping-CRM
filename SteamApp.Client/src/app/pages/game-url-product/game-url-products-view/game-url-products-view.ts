import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { GameUrlProduct } from '../../../models';
import { GameUrlProductService } from '../../../services';
import { TextFilterComponent } from '../../../components';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-game-url-products',
  templateUrl: './game-url-products-view.html',
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    TextFilterComponent,
  ],
})
export class GameUrlProductsView implements AfterViewInit {
  displayedColumns: string[] = [
    'productName',
    'gameUrlName',
    'fullUrl',
    'isBatchUrl',
    'actions',
  ];

  dataSource = new MatTableDataSource<GameUrlProduct>([]);

  gameUrlProducts: GameUrlProduct[] = [];
  filteredGameUrlProduct: GameUrlProduct[] = [];

  searchByGameUrlNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  searchByProductNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private gameUrlProductService: GameUrlProductService,
    private router: Router,
  ) {}

  ngAfterViewInit(): void {
    throw new Error('Method not implemented.');
  }

  ngOnInit(): void {
    this.fetchGameUrlProducts();
  }

  fetchGameUrlProducts(): void {
    this.gameUrlProductService.getAll().subscribe((items) => {
      this.gameUrlProducts = items;
      this.filteredGameUrlProduct = items;

      this.dataSource.data = items;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/game-url-products/create']);
  }

  deleteButtonClicked(productId: number, gameUrlId: number): void {
    if (!confirm('Remove product from Game URL?')) {
      return;
    }

    this.gameUrlProductService.delete(productId, gameUrlId).subscribe(() => {
      this.fetchGameUrlProducts();
    });
  }

  clearFiltersButtonClicked(): void {
    this.searchByGameUrlNameFilterControl.setValue('', { emitEvent: false });
    this.searchByProductNameFilterControl.setValue('', { emitEvent: false });

    this.loadFilteredGameUrlProducts();
  }

  onGameUrlNameFilterChanged(filter: string): void {
    this.searchByGameUrlNameFilterControl.setValue(filter, {
      emitEvent: false,
    });
    this.loadFilteredGameUrlProducts();
  }

  onProductNameFilterChanged(filter: string): void {
    this.searchByProductNameFilterControl.setValue(filter, {
      emitEvent: false,
    });
    this.loadFilteredGameUrlProducts();
  }

  private loadFilteredGameUrlProducts(): void {
    const gameUrlNameFilter =
      this.searchByGameUrlNameFilterControl.value?.toLowerCase() ?? '';
    const productNameFilter =
      this.searchByProductNameFilterControl.value?.toLowerCase() ?? '';

    this.filteredGameUrlProduct = this.gameUrlProducts.filter((gameUrl) => {
      const matchesName =
        (!gameUrlNameFilter ||
          gameUrl.gameUrlName?.toLowerCase().includes(gameUrlNameFilter)) &&
        (!productNameFilter ||
          gameUrl.productName?.toLowerCase().includes(productNameFilter));

      return matchesName;
    });

    this.dataSource.data = this.filteredGameUrlProduct;
  }
}
