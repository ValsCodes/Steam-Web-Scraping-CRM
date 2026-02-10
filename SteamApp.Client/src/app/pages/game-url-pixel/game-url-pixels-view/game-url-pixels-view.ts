import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, startWith, tap } from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { Game, GameUrl, Pixel, GameUrlPixel } from '../../../models';
import {
  GameService,
  GameUrlService,
  PixelService,
  GameUrlPixelService,
} from '../../../services';
import { ComboBoxComponent } from '../../../components/filter-components/combo-box-filter.component';

@Component({
  selector: 'app-game-url-pixels',
  standalone: true,
  templateUrl: './game-url-pixels-view.html',
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    ComboBoxComponent,
  ],
})
export class GameUrlPixelsView implements AfterViewInit, OnInit {
  displayedColumns: string[] = [
    'pixelName',
    'gameUrlName',
    'pixelLocation',
    'pixelColor',
    'actions',
  ];

  dataSource = new MatTableDataSource<GameUrlPixel>([]);

  gameUrlPixels: GameUrlPixel[] = [];
  filteredGameUrlPixels: GameUrlPixel[] = [];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  readonly gameUrlsFiltered$ = new BehaviorSubject<readonly GameUrl[]>([]);
  readonly pixelsFiltered$ = new BehaviorSubject<readonly Pixel[]>([]);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);
  readonly pixelIdControl = new FormControl<number | null>(null);

  readonly bindGameToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap(gameId => this.applyGameFilter(gameId))
  );

  readonly bindGameUrlToExternal$ = this.gameUrlIdControl.valueChanges.pipe(
    startWith(this.gameUrlIdControl.value),
    tap(() => this.loadFilteredGameUrlPixels())
  );

  readonly bindPixelToExternal$ = this.pixelIdControl.valueChanges.pipe(
    startWith(this.pixelIdControl.value),
    tap(() => this.loadFilteredGameUrlPixels())
  );

  private games: Game[] = [];
  private gameUrlsAll: GameUrl[] = [];
  private pixelsAll: Pixel[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly gameUrlPixelService: GameUrlPixelService,
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
    private readonly pixelService: PixelService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.loadPixels();
    this.fetchGameUrlPixels();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe(games => {
      this.games = games;
      this.games$.next(games);
    });
  }

  private loadGameUrls(): void {
    this.gameUrlService.getAll().subscribe(urls => {
      this.gameUrlsAll = urls;
      this.applyGameFilter(this.gameIdControl.value);
    });
  }

  private loadPixels(): void {
    this.pixelService.getAll().subscribe(pixels => {
      this.pixelsAll = pixels;
      this.applyGameFilter(this.gameIdControl.value);
    });
  }

  fetchGameUrlPixels(): void {
    this.gameUrlPixelService.getAll().subscribe(items => {
      this.gameUrlPixels = items;
      this.filteredGameUrlPixels = items;
      this.dataSource.data = items;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/game-url-pixels/create']);
  }

  deleteButtonClicked(pixelId: number, gameUrlId: number): void {
    if (!confirm('Remove pixel from Game URL?')) {
      return;
    }

    this.gameUrlPixelService
      .delete(pixelId, gameUrlId)
      .subscribe(() => this.fetchGameUrlPixels());
  }

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);
    this.pixelIdControl.setValue(null);
    this.loadFilteredGameUrlPixels();
  }

  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.gameUrlsFiltered$.next([]);
      this.pixelsFiltered$.next([]);
      this.gameUrlIdControl.reset();
      this.pixelIdControl.reset();
      this.loadFilteredGameUrlPixels();
      return;
    }

    const urls = this.gameUrlsAll.filter(u => u.gameId === gameId);
    this.gameUrlsFiltered$.next(urls);

    const pixels = this.pixelsAll.filter(p => p.gameId === gameId);
    this.pixelsFiltered$.next(pixels);

    this.gameUrlIdControl.reset();
    this.pixelIdControl.reset();
    this.loadFilteredGameUrlPixels();
  }

  private loadFilteredGameUrlPixels(): void {
    const gameUrlId = this.gameUrlIdControl.value;
    const pixelId = this.pixelIdControl.value;

    this.filteredGameUrlPixels = this.gameUrlPixels.filter(x => {
      const matchesGame =
        this.gameIdControl.value === null ||
        this.gameUrlsAll.some(
          u => u.id === x.gameUrlId && u.gameId === this.gameIdControl.value
        );

      const matchesGameUrl =
        gameUrlId === null || x.gameUrlId === gameUrlId;

      const matchesPixel =
        pixelId === null || x.pixelId === pixelId;

      return matchesGame && matchesGameUrl && matchesPixel;
    });

    this.dataSource.data = this.filteredGameUrlPixels;
  }
}
