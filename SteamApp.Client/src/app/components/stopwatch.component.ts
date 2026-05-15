// src/app/components/stopwatch/stopwatch.component.ts
import { Component, OnDestroy, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stopwatch',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stopwatch">
      <div class="display">{{ formattedTime() }}</div>
    </div>
  `,
  styles: [
    `
      .stopwatch {
        display: inline-flex;
        flex-direction: column;
        align-items: center;
        font-family: monospace;
        gap: 0.5rem;
      }

      .display {
        font-size: 1.25rem;
        min-width: 10ch;
        text-align: center;
      }
    `,
  ],
})
export class StopwatchComponent implements OnDestroy {
  private readonly elapsedMs = signal(0);
  private timerId: ReturnType<typeof setInterval> | null = null;
  private startedAt = 0;
  private running = false;

  readonly formattedTime = computed(() => {
    const ms = this.elapsedMs();
    const cent = Math.floor(ms / 10) % 100;
    const sec = Math.floor(ms / 1000) % 60;
    const min = Math.floor(ms / 60000) % 60;
    const hr = Math.floor(ms / 3600000);
    const pad2 = (n: number) => n.toString().padStart(2, '0');

    return `${pad2(hr)}:${pad2(min)}:${pad2(sec)}.${pad2(cent)}`;
  });

  start(): void {
    if (this.running) {
      return;
    }

    this.reset();
    this.running = true;
    this.startedAt = Date.now();

    this.timerId = setInterval(() => {
      this.elapsedMs.set(Date.now() - this.startedAt);
    }, 10);
  }

  stop(): void {
    if (!this.running) {
      return;
    }

    if (this.timerId !== null) {
      clearInterval(this.timerId);
      this.timerId = null;
    }

    this.elapsedMs.set(Date.now() - this.startedAt);
    this.running = false;
  }

  reset(): void {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
      this.timerId = null;
    }

    this.running = false;
    this.startedAt = 0;
    this.elapsedMs.set(0);
  }

  ngOnDestroy(): void {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
    }
  }
}