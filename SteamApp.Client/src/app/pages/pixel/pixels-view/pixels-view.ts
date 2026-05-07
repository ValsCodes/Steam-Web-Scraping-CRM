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
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltip } from '@angular/material/tooltip';

import { Game, PixelListItem } from '../../../models';
import { CopyLinkComponent } from '../../../components';
import { GameService, PixelService } from '../../../services';
import {
  BehaviorSubject,
  combineLatest,
  finalize,
  startWith,
  Subject,
  takeUntil,
} from 'rxjs';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';
import { UpdatePixelStatus } from '../../../models';

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
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltip,
    CopyLinkComponent,
  ],
  templateUrl: './pixels-view.html',
  styleUrl: './pixels-view.scss',
})
export class PixelsView implements OnInit, OnDestroy {
  displayedColumns: string[] = ['gameName', 'name', 'rgb', 'isActive', 'actions'];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  dataSource = new MatTableDataSource<PixelListItem>([]);
  isGridLoading = false;
  pageSize = 25;
  readonly pageSizeOptions = [10, 25, 50, 100];
  private pixels: PixelListItem[] = [];
  private readonly deletingIds = new Set<number>();
  private readonly statusUpdatingIds = new Set<number>();

  private readonly destroy$ = new Subject<void>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly pixelService: PixelService,
    private readonly router: Router,
    private readonly gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog,
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
    this.isGridLoading = true;
    this.cdr.markForCheck();

    this.pixelService
      .getAll()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isGridLoading = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe((pixels) => {
        this.pixels = pixels;

        this.dataSource.data = pixels;
        this.dataSource.paginator = this.paginator;
        this.paginator.pageSize = this.pageSize;
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

  refreshButtonClicked(): void {
    this.fetchPixels();
  }

  pageSizeChanged(value: string | number): void {
    const pageSize = Number(value);
    if (Number.isNaN(pageSize) || pageSize <= 0 || this.pageSize === pageSize) {
      return;
    }

    this.pageSize = pageSize;

    if (this.paginator) {
      this.paginator.pageSize = pageSize;
      this.paginator.firstPage();
      this.dataSource.data = [...this.dataSource.data];
      this.cdr.markForCheck();
    }
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/pixels/edit', id]);
  }

  activeButtonClicked(pixel: PixelListItem): void {
    if (this.isStatusUpdating(pixel.id)) {
      return;
    }

    const nextIsActive = !pixel.isActive;

    const input: UpdatePixelStatus = {
      id: pixel.id,
      isActive: nextIsActive,
    };

    this.statusUpdatingIds.add(pixel.id);

    this.pixelService
      .updateStatus(input)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.statusUpdatingIds.delete(pixel.id);
          this.cdr.markForCheck();
        }),
      )
      .subscribe(() => {
        this.pixels = this.pixels.map((x) =>
          x.id === pixel.id ? { ...x, isActive: nextIsActive } : x,
        );

        this.applyFilters(this.gameIdControl.value, this.searchByNameFilterControl.value);
      });
  }

  deleteButtonClicked(id: number): void {
    if (this.isDeleting(id)) {
      return;
    }

    this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: {
          title: 'Delete Pixel',
          subtitle: 'This action cannot be undone.',
          message: 'Are you sure you want to delete this Pixel?',
          confirmText: 'Delete',
          cancelText: 'Cancel',
        },
      })
      .afterClosed()
      .subscribe((confirmed: boolean) => {
        if (!confirmed) {
          return;
        }

        this.deletingIds.add(id);
        this.cdr.markForCheck();

        this.pixelService.delete(id)
          .pipe(
            finalize(() => {
              this.deletingIds.delete(id);
              this.cdr.markForCheck();
            }),
          )
          .subscribe({
            next: () => {
              this.fetchPixels();
            },
          });
      });
  }

  isDeleting(id: number): boolean {
    return this.deletingIds.has(id);
  }

  isStatusUpdating(id: number): boolean {
    return this.statusUpdatingIds.has(id);
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
      IsActive: x.isActive,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Pixels');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Pixels.xlsx`);
  }

  formatRgbValue(pixel: PixelListItem): string {
    return `rgb(${pixel.redValue}, ${pixel.greenValue}, ${pixel.blueValue})`;
  }
}
