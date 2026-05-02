import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { finalize, defer, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingStateService {
  private readonly document = inject(DOCUMENT);
  private readonly pendingOperations = signal(0);

  readonly isLoading = computed(() => this.pendingOperations() > 0);

  constructor() {
    effect(() => {
      const loading = this.isLoading();
      const body = this.document.body;

      this.document.documentElement.classList.toggle('app-loading', loading);
      body?.classList.toggle('app-loading', loading);
    });
  }

  begin(): void {
    this.pendingOperations.update((count) => count + 1);
  }

  end(): void {
    this.pendingOperations.update((count) => Math.max(0, count - 1));
  }

  track<T>(source$: Observable<T>): Observable<T> {
    return defer(() => {
      this.begin();
      return source$.pipe(finalize(() => this.end()));
    });
  }
}
