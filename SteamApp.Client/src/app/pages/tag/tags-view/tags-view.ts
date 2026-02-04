import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { GameService, TagService } from '../../../services';
import { Game, Tag } from '../../../models';
import { ComboBoxComponent } from '../../../components/filters/combo-box.component';
import { TextFilterComponent } from '../../../components';
import { BehaviorSubject, startWith, Subject, takeUntil, tap } from 'rxjs';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'steam-tags-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    ComboBoxComponent,
    TextFilterComponent,
  ],
  templateUrl: './tags-view.html',
  styleUrl: './tags-view.scss',
})
export class TagsView implements OnInit {
  displayedColumns: string[] = ['gameName', 'name', 'actions'];

  private games: Game[] = [];
  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  gameNameById = new Map<number, string>();

  private readonly destroy$ = new Subject<void>();

  readonly gameIdControl = new FormControl<number | null>(null);

  searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  dataSource = new MatTableDataSource<Tag>([]);
  tags: Tag[] = [];
  tagsFiltered: Tag[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private tagService: TagService,
    private router: Router,
    private gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();

    this.fetchTags();
  }

  fetchTags(): void {
    this.tagService.getAll().subscribe((tags) => {
      this.dataSource.data = tags;
      this.tags = tags;
      this.tagsFiltered = tags;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
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

  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredTags();
  }

  private loadFilteredTags(): void {
    const nameFilter =
      this.searchByNameFilterControl.value?.toLowerCase() ?? '';

    this.tagsFiltered = this.tags.filter((tag) => {
      const matchesName =
        !nameFilter || tag.name?.toLowerCase().includes(nameFilter);

      return matchesName;
    });

    this.dataSource.data = this.tagsFiltered;
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

  readonly bindToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap((gameId) => {
      this.applyGameFilter(gameId);
    }),
  );

  private applyGameFilter(gameId: number | null): void {
    this.tagsFiltered = this.tags.filter((x) => x.gameId === gameId);

    this.dataSource.data = this.tagsFiltered;

    this.cdr.markForCheck();
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });

    this.gameIdControl.setValue(null);

    this.searchByNameFilterControl.setValue('', { emitEvent: false });

    const gameId = this.gameIdControl.value;

    this.loadFilteredTags();
  }
}
