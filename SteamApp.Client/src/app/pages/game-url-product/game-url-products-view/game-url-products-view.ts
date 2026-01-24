import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { GameUrlProduct } from '../../../models';
import { GameUrlProductService } from '../../../services';

@Component({
  selector: 'app-game-url-products',
  templateUrl: './game-url-products-view.html',
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule],
})
export class GameUrlProductsView implements AfterViewInit {
  displayedColumns: string[] = [
    'productName',
    'gameUrlName',
    'fullUrl',
    'actions',
  ];

  dataSource = new MatTableDataSource<GameUrlProduct>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private gameUrlProductService: GameUrlProductService,private router: Router) {}
  ngAfterViewInit(): void {
    throw new Error('Method not implemented.');
  }

  ngOnInit(): void {
    this.fetchGameUrlProducts();
  }

  fetchGameUrlProducts(): void {
    this.gameUrlProductService.getAll().subscribe((items) => {
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
}
