import {
  Component,
  DestroyRef,
  HostListener,
  WritableSignal,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { A11yModule } from '@angular/cdk/a11y';

import { ConfirmDialogComponent } from '../confirm-dialog.component';
import { ExternalLinkDirective, openableExternalUrl } from '../../common';
import { WatchList } from '../../models';
import { AuthService, WatchListService } from '../../services';
import { WatchListForm } from '../../pages/watch-list/watch-list-form/watch-list-form';

type PanelView = 'list' | 'create' | 'edit';

@Component({
  selector: 'steam-watch-list-panel',
  standalone: true,
  imports: [A11yModule, CommonModule, ExternalLinkDirective, WatchListForm],
  templateUrl: './watch-list-panel.component.html',
  styleUrl: './watch-list-panel.component.scss',
})
export class WatchListPanelComponent {
  readonly openableExternalUrl = openableExternalUrl;

  private readonly auth = inject(AuthService);
  private readonly watchListService = inject(WatchListService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  readonly isLoggedIn = signal(false);
  readonly isOpen = signal(false);
  readonly isLoading = signal(false);
  readonly items = signal<readonly WatchList[]>([]);
  readonly view = signal<PanelView>('list');
  readonly selectedItem = signal<WatchList | null>(null);
  readonly activeCount = computed(() => this.items().filter(item => item.isActive).length);

  private readonly updatingIds = signal<ReadonlySet<number>>(new Set<number>());
  private readonly deletingIds = signal<ReadonlySet<number>>(new Set<number>());
  private hasLoaded = false;

  constructor() {
    this.auth.loggedIn$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(isLoggedIn => {
        this.isLoggedIn.set(isLoggedIn);

        if (!isLoggedIn) {
          this.isOpen.set(false);
          this.items.set([]);
          this.hasLoaded = false;
          this.showList();
        }
      });
  }

  open(): void {
    this.isOpen.set(true);

    if (!this.hasLoaded) {
      this.refresh();
    }
  }

  close(): void {
    this.isOpen.set(false);
    this.showList();
  }

  @HostListener('document:keydown.escape')
  closeOnEscape(): void {
    if (this.isOpen()) {
      this.close();
    }
  }

  refresh(): void {
    if (this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.watchListService.getAll()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false)),
      )
      .subscribe({
        next: items => {
          this.items.set(items);
          this.hasLoaded = true;
        },
        error: () => undefined,
      });
  }

  showCreateForm(): void {
    this.selectedItem.set(null);
    this.view.set('create');
  }

  showEditForm(item: WatchList): void {
    this.selectedItem.set(item);
    this.view.set('edit');
  }

  showList(): void {
    this.selectedItem.set(null);
    this.view.set('list');
  }

  handleSaved(): void {
    this.showList();
    this.refresh();
  }

  toggleActive(item: WatchList): void {
    if (this.isUpdating(item.id) || this.isDeleting(item.id)) {
      return;
    }

    const nextIsActive = !item.isActive;
    this.addId(this.updatingIds, item.id);

    this.watchListService.updateStatus({ id: item.id, isActive: nextIsActive })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.removeId(this.updatingIds, item.id)),
      )
      .subscribe({
        next: () => this.items.update(items => items.map(current =>
          current.id === item.id ? { ...current, isActive: nextIsActive } : current,
        )),
        error: () => undefined,
      });
  }

  delete(item: WatchList): void {
    if (this.isDeleting(item.id) || this.isUpdating(item.id)) {
      return;
    }

    this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete Watch List Item',
        subtitle: 'This action cannot be undone.',
        message: `Are you sure you want to delete “${item.name ?? 'this watch list item'}”?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    }).afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed: boolean) => {
        if (!confirmed) {
          return;
        }

        this.addId(this.deletingIds, item.id);
        this.watchListService.delete(item.id)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.removeId(this.deletingIds, item.id)),
          )
          .subscribe({
            next: () => this.items.update(items => items.filter(current => current.id !== item.id)),
            error: () => undefined,
          });
      });
  }

  isUpdating(id: number): boolean {
    return this.updatingIds().has(id);
  }

  isDeleting(id: number): boolean {
    return this.deletingIds().has(id);
  }

  private addId(target: WritableSignal<ReadonlySet<number>>, id: number): void {
    target.set(new Set([...target(), id]));
  }

  private removeId(target: WritableSignal<ReadonlySet<number>>, id: number): void {
    const nextIds = new Set(target());
    nextIds.delete(id);
    target.set(nextIds);
  }
}
