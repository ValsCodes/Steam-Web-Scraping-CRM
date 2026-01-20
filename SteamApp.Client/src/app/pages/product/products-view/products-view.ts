import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { Product } from '../../../models/product.model';
import { ProductService } from '../../../services/product/product.service';
import { encode } from '../../../common/url.encoder';
import { GameUrlService } from '../../../services/game-url/game-url.service';
import { Game } from '../../../models';
import { GameService } from '../../../services/game/game.service';
import { GameUrl } from '../../../models/game-url.model';

@Component({
  selector: 'steam-products-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './products-view.html',
  styleUrl: './products-view.scss'
})
export class ProductsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    //'gameId',
    'gameUrlId',
    'fullUrl',
    'name',
    'actions'
  ];

  dataSource = new MatTableDataSource<Product>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productService: ProductService,
    private gameUrlService: GameUrlService,
    private gameService: GameService,
    private router: Router
  ) {}

  ngOnInit(): void {

    this.loadGames();
    this.loadGameUrls();
    this.fetchProducts();
  }

  fetchProducts(): void {
    this.productService.getAll().subscribe(products => {
      this.dataSource.data = products;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }
  
  games: Game[] = [];
  gameNameById = new Map<number, string>();

  private loadGames(): void
{
    this.gameService.getAll().subscribe({
        next: games =>
        {
            this.games = games;

            this.gameNameById.clear();
            for (const game of games) {
                this.gameNameById.set(game.id, game.name);
            }
        }
    });
}

  gameUrls: GameUrl[] = [];
  gameUrlNameById = new Map<number, string>();

  private loadGameUrls(): void
{
    this.gameUrlService.getAll().subscribe({
        next: gameUrls =>
        {
            this.gameUrls = gameUrls;

            this.gameUrlNameById.clear();
            for (const gameUrl of gameUrls) {
                this.gameUrlNameById.set(gameUrl.id, gameUrl.name ?? '-');
            }
        }
    });
}

  createButtonClicked(): void {
    this.router.navigate(['/products/create']);
  }

    encodeText(value:string): string {
    return encode(value);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/products/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this product?')) {
      return;
    }

    this.productService.delete(id).subscribe(() => {
      this.fetchProducts();
    });
  }
}
