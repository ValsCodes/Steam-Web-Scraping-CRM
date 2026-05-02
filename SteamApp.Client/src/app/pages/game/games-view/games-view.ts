import {
  Component,
  OnInit,
  ViewChild,
  signal,
  computed,
  effect,
  ChangeDetectorRef,
  inject,
} from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Game } from '../../../models/game.model';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GameService } from '../../../services/game/game.service';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';
import * as XLSX from 'xlsx';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'steam-games-view',
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './games-view.html',
  styleUrl: './games-view.scss',
})
export class GamesView implements OnInit {
  displayedColumns: string[] = ['name', 'pageUrl', 'internalId', 'actions'];

  searchByName = new FormControl<string>('', { nonNullable: true });

  // signals
  readonly games = signal<readonly Game[]>([]);
  readonly nameFilter = signal<string>('');

  readonly gamesFiltered = computed<readonly Game[]>(() => {
    const filter = this.nameFilter().trim().toLowerCase();
    const list = this.games();

    if (!filter) {
      return list;
    }

    return list.filter((g) => g.name.toLowerCase().includes(filter));
  });

  readonly dataSource = new MatTableDataSource<Game>([]);
  private readonly dialog = inject(MatDialog);
  private readonly deletingIds = new Set<number>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly gameService: GameService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {
    effect(() => {
      this.dataSource.data = [...this.gamesFiltered()];
    });
  }

  ngOnInit(): void {
    this.fetchGames();

    this.nameFilter.set(this.searchByName.value ?? '');
  }

  onNameFilterChanged(filter: string): void {
    this.searchByName.setValue(filter, { emitEvent: false });
    this.nameFilter.set(filter);
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);
    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Games.xlsx`);
  }

  refreshButtonClicked(): void {
    this.fetchGames();
  }

  fetchGames(): void {
    this.gameService.getAll().subscribe({
      next: (games) => {
        this.games.set(games);

        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
    });
  }

  clearFilters(): void {
    this.searchByName.setValue('');
    this.nameFilter.set('');
  }

  createButtonClicked(): void {
    this.router.navigate(['/games/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/games/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (this.isDeleting(id)) {
      return;
    }

    this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: {
          title: 'Delete Game',
          subtitle: 'This action cannot be undone.',
          message: 'Are you sure you want to delete this Game?',
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

        this.gameService
          .delete(id)
          .pipe(
            finalize(() => {
              this.deletingIds.delete(id);
              this.cdr.markForCheck();
            }),
          )
          .subscribe({
            next: () => {
              this.fetchGames();
            },
          });
      });
  }

  isDeleting(id: number): boolean {
    return this.deletingIds.has(id);
  }
}
