import { Component, OnInit, ViewChild,  } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { Router } from '@angular/router';
import { Game } from '../../../models/game.model';
import { TextFilterComponent } from "../../../components";
import { FormControl,  } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GameService } from '../../../services/game/game.service';

@Component({
  selector: 'steam-games-view',
  imports: [
    TextFilterComponent,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    CommonModule,
  ],
  templateUrl: './games-view.html',
  styleUrl: './games-view.scss',
})
export class GamesView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    'name',
    //'baseUrl',
    'pageUrl',
    'actions',
  ];

  games: Game[] = [];
  gamesFiltered: Game[] = [];
  dataSource = new MatTableDataSource<Game>([]);

  searchByName = new FormControl<string | null>('');

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private gameService: GameService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.fetchGames();
  }

  onNameFilterChanged(filter: string): void {
    this.searchByName.setValue(filter, { emitEvent: false });
    this.loadFilteredGames();
  }

  loadFilteredGames(): void {
    const nameFilter = this.searchByName.value?.toLowerCase() ?? '';

    this.gamesFiltered = this.games.filter((game) => {
      const matchesName =
        !nameFilter || game.name.toLowerCase().includes(nameFilter);

      return matchesName;
    });

    this.dataSource.data = this.gamesFiltered;
  }

  fetchGames(): void {
    this.gameService.getAll().subscribe({
      next: (games) => {
        this.dataSource.data = games;
        this.games = games;
        this.gamesFiltered = games;

        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
    });
  }

  clearFilters(): void {
    this.searchByName.setValue('');
    this.fetchGames();
  }

  createButtonClicked(): void {
    this.router.navigate(['/games/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/games/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    // if (!confirm('Delete this game?')) {
    //   return;
    // }

    this.gameService.delete(id).subscribe({
      next: () => this.fetchGames(),
    });
  }
}
