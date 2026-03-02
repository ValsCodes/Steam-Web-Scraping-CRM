import {
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { Game, PixelListItem } from '../../../models';
import { GameService, PixelService } from '../../../services';
import {
  BehaviorSubject,
  combineLatest,
  startWith,
  Subject,
  takeUntil,
} from 'rxjs';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import * as XLSX from 'xlsx';

@Component({
  selector: 'steam-pixels-grid',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
  ],
  templateUrl: './pixels-view.html',
  styleUrl: './pixels-view.scss',
})
export class PixelsView implements OnInit, OnDestroy {
  displayedColumns: string[] = ['gameName', 'name', 'rgb', 'actions'];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  dataSource = new MatTableDataSource<PixelListItem>([]);
  private pixels: PixelListItem[] = [];

  private readonly destroy$ = new Subject<void>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly pixelService: PixelService,
    private readonly router: Router,
    private readonly gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.fetchPixels();

    combineLatest([
      this.gameIdControl.valueChanges.pipe(startWith(this.gameIdControl.value)),
      this.searchByNameFilterControl.valueChanges.pipe(
        startWith(this.searchByNameFilterControl.value),
      ),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([gameId, name]) => {
        this.applyFilters(gameId, name);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private applyFilters(gameId: number | null, name: string): void {
    const nameFilter = (name ?? '').trim().toLowerCase();

    const filtered = this.pixels.filter((p) => {
      if (gameId !== null && p.gameId !== gameId) {
        return false;
      }
      if (nameFilter && !(p.name ?? '').toLowerCase().includes(nameFilter)) {
        return false;
      }
      return true;
    });

    this.dataSource.data = filtered;
    this.cdr.markForCheck();
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games$.next(games);
        this.cdr.markForCheck();
      });
  }

  fetchPixels(): void {
    this.pixelService.getAll().subscribe((pixels) => {
      this.pixels = pixels;

      this.dataSource.data = pixels;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;

      this.applyFilters(
        this.gameIdControl.value,
        this.searchByNameFilterControl.value,
      );
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

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.searchByNameFilterControl.setValue('');
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data.map((x) => ({
      Game: x.gameName ?? '',
      Name: x.name ?? '',
      RGB: `rgb(${x.redValue}, ${x.greenValue}, ${x.blueValue})`,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Pixels');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Pixels.xlsx`);
  }
}
