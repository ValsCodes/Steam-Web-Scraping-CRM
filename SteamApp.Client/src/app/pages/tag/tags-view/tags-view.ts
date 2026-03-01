import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { GameService, TagService } from '../../../services';
import { Game, Tag } from '../../../models';
import { BehaviorSubject, combineLatest, startWith, Subject, takeUntil } from 'rxjs';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import * as XLSX from 'xlsx';

@Component({
  selector: 'steam-tags-grid',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
  ],
  templateUrl: './tags-view.html',
  styleUrl: './tags-view.scss',
})
export class TagsView implements OnInit, OnDestroy {
  displayedColumns: string[] = ['gameName', 'name', 'actions'];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  private readonly destroy$ = new Subject<void>();

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', { nonNullable: true });

  dataSource = new MatTableDataSource<Tag>([]);
  private tags: Tag[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly tagService: TagService,
    private readonly router: Router,
    private readonly gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.fetchTags();

    combineLatest([
      this.gameIdControl.valueChanges.pipe(startWith(this.gameIdControl.value)),
      this.searchByNameFilterControl.valueChanges.pipe(startWith(this.searchByNameFilterControl.value)),
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

    const filtered = this.tags.filter(t => {
      if (gameId !== null && t.gameId !== gameId) { return false; }
      if (nameFilter && !(t.name ?? '').toLowerCase().includes(nameFilter)) { return false; }
      return true;
    });

    this.dataSource.data = filtered;
    this.cdr.markForCheck();
  }

  fetchTags(): void {
    this.tagService.getAll().subscribe(tags => {
      this.tags = tags;

      this.dataSource.data = tags;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;

      this.applyFilters(this.gameIdControl.value, this.searchByNameFilterControl.value);
    });
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(games => {
        this.games$.next(games);
        this.cdr.markForCheck();
      });
  }

  createButtonClicked(): void {
    this.router.navigate(['/tags/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/tags/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this tag?')) {
      return;
    }

    this.tagService.delete(id).subscribe(() => {
      this.fetchTags();
    });
  }

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.searchByNameFilterControl.setValue('');

  }

  exportButtonClicked(): void
{
  const dataToExport = this.dataSource.data.map(x => ({
    Game: x.gameName,
    Name: x.name ?? '',
  }));

  const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

  const workbook: XLSX.WorkBook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Tags');

  const today = new Date();
  XLSX.writeFile(workbook, `Export_${today.toDateString()}_Tags.xlsx`);
}
}