// src/app/components/stopwatch/stopwatch.component.ts
import { Component, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stopwatch',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stopwatch">
      <div class="display">{{ formattedTime() }}</div>
      <div class="controls">
        <button (click)="startStop()">
          {{ running ? 'Stop' : 'Start' }}
        </button>
        <button (click)="reset()">Reset</button>
      </div>
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
      .controls button {
        margin: 0 0.25rem;
        padding: 0.5rem 1rem;
        font-size: 0.9rem;
      }
    `,
  ],
})
export class StopwatchComponent implements OnDestroy {
  // elapsed milliseconds
  private elapsedMs = signal(0);

  // whether the timer is running
  running = false;

  private timerId: ReturnType<typeof setInterval> | null = null;

  private offset = 0;

  // formatted HH:mm:ss.cc
  formattedTime = computed(() => {
    const ms = this.elapsedMs();
    const cent = Math.floor(ms / 10) % 100;
    const sec = Math.floor(ms / 1000) % 60;
    const min = Math.floor(ms / 60000) % 60;
    const hr = Math.floor(ms / 3600000);
    const pad2 = (n: number) => n.toString().padStart(2, '0');
    return `${pad2(hr)}:${pad2(min)}:${pad2(sec)}.${pad2(cent)}`;
  });

  startStop() {
    if (this.running) {
      // STOP
      if (this.timerId !== null) {
        clearInterval(this.timerId);
        this.timerId = null;
      }
      this.offset = this.elapsedMs();
      this.running = false;
    } else {
      const startTs = Date.now() - this.offset;
      this.timerId = setInterval(() => {
        this.elapsedMs.set(Date.now() - startTs);
      }, 10);
      this.running = true;
    }
  }

  reset() {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
    this.running = false;
    this.offset = 0;
    this.elapsedMs.set(0);
  }

  ngOnDestroy() {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
    }
  }
}
