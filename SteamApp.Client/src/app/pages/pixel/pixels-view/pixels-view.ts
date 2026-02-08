import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { Game, Pixel, PixelListItem, Tag } from '../../../models';
import { GameService, PixelService } from '../../../services';
import { ComboBoxComponent } from "../../../components/filter-components/combo-box-filter.component";
import { TextFilterComponent } from "../../../components";
import { BehaviorSubject, startWith, Subject, takeUntil, tap } from 'rxjs';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'steam-pixels-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    ComboBoxComponent,
    TextFilterComponent,
  ],
  templateUrl: './pixels-view.html',
  styleUrl: './pixels-view.scss',
})
export class PixelsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    'gameName',
    'name',
    'rgb',
    'actions',
  ];

  private readonly destroy$ = new Subject<void>();

  readonly gameIdControl = new FormControl<number | null>(null);
  private games: Game[] = [];
  readonly games$ = new BehaviorSubject<readonly Game[]>([]);

  searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredTags();
  }

  dataSource = new MatTableDataSource<PixelListItem>([]);
  pixels: PixelListItem[] = [];
  pixelsFiltered: PixelListItem[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private pixelService: PixelService,
    private router: Router,
    private gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();

    this.fetchPixels();
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games = games;
        this.games$.next(games);
        this.cdr.markForCheck();
      });
  }

  fetchPixels(): void {
    this.pixelService.getAll().subscribe((pixels) => {
      this.dataSource.data = pixels;
      this.pixels = pixels;
      this.pixelsFiltered = pixels;

      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/pixels/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/pixels/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this pixel?')) {
      return;
    }

    this.pixelService.delete(id).subscribe(() => {
      this.fetchPixels();
    });
  }

  rgb(pixel: PixelListItem): string {
    return `rgb(${pixel.redValue}, ${pixel.greenValue}, ${pixel.blueValue})`;
  }

  readonly bindToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap((gameId) => {
      this.applyGameFilter(gameId);
    }),
  );

  private applyGameFilter(gameId: number | null): void {
    this.pixelsFiltered = this.pixels.filter((x) => x.gameId === gameId);
    this.dataSource.data = this.pixelsFiltered;

    this.cdr.markForCheck();
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });

    this.gameIdControl.setValue(null);

    this.searchByNameFilterControl.setValue('', { emitEvent: false });

    const gameId = this.gameIdControl.value;

    this.loadFilteredTags();
  }

  private loadFilteredTags(): void {
    const nameFilter =
      this.searchByNameFilterControl.value?.toLowerCase() ?? '';

    this.pixelsFiltered = this.pixels.filter((tag) => {
      const matchesName =
        !nameFilter || tag.name?.toLowerCase().includes(nameFilter);

      return matchesName;
    });

    this.dataSource.data = this.pixelsFiltered;
  }
}
