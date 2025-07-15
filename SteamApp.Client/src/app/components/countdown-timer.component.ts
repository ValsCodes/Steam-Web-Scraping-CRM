// src/app/components/countdown-timer/countdown-timer.component.ts
import {
  Component,
  Input,
  OnDestroy,
  OnChanges,
  SimpleChanges,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-countdown-timer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="countdown">
      <div class="display" [class.danger]="isTimerLow()" title="{{comment}}">
        {{ formattedTime() }}
      </div>
      <div *ngIf="showControls" class="controls">
        <button (click)="startPause()">
          {{ running ? 'Pause' : 'Start' }}
        </button>
        <button (click)="reset()">Reset</button>
      </div>
    </div>
  `,
  styles: [
    `
      .countdown {
        text-align: center;
        font-family: monospace;
      }
      .display {
        font-size: 1.25rem;
        margin-bottom: 0.5rem;
      }
      .display.danger {
        color: #dc3545;
      }
      .controls button {
        margin: 0 0.25rem;
        padding: 0.5rem 1rem;
        font-size: 0.75rem;
      }
    `,
  ],
})
export class CountdownTimerComponent implements OnDestroy, OnChanges {
  /** initial time in milliseconds (default = 1 hour) */
  @Input() initialMs = 5 * 60 * 1000;

  /** whether to show start/pause/reset buttons */
  @Input() showControls = true;
  @Input() startOnLoad = false;
  @Input() alertThreshold = 5 * 60 * 1000;
  @Input() comment = "timer";

  /** remaining time signal */
  private remainingMs = signal(this.initialMs);

  /** is the timer running? */
  running = false;

  private timerId: ReturnType<typeof setInterval> | null = null;
  private endTs = 0;

  /** formatted HH:mm:ss */
  formattedTime = computed(() => {
    let ms = this.remainingMs();
    if (ms < 0) ms = 0;
    const totalSec = Math.floor(ms / 1000);
    const hrs = Math.floor(totalSec / 3600);
    const mins = Math.floor((totalSec % 3600) / 60);
    const secs = totalSec % 60;
    const pad2 = (n: number) => n.toString().padStart(2, '0');
    return `${pad2(hrs)}:${pad2(mins)}:${pad2(secs)}`;
  });

  isTimerLow = computed(() => this.remainingMs() < this.alertThreshold);

  ngOnChanges(changes: SimpleChanges) {
    // If parent changes the initialMs, reset remainingMs accordingly
    if (changes['initialMs']) {
      this.resetInternal();
    }

    if (this.startOnLoad) {
      this.startPause();
    }
  }

  startPause() {
    if (this.running) {
      // pause
      this.stopInterval();
      this.running = false;
    } else {
      // start (or resume)
      this.endTs = Date.now() + this.remainingMs();
      this.running = true;
      this.timerId = setInterval(() => {
        const rem = this.endTs - Date.now();
        if (rem <= 0) {
          this.remainingMs.set(0);
          this.stopInterval();
          this.running = false;
        } else {
          this.remainingMs.set(rem);
        }
      }, 250);
    }
  }

  reset() {
    this.stopInterval();
    this.running = false;
    this.resetInternal();
  }

  /** internal helper to sync remainingMs with initialMs */
  private resetInternal() {
    this.remainingMs.set(this.initialMs);
  }

  private stopInterval() {
    if (this.timerId !== null) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }

  ngOnDestroy() {
    this.stopInterval();
  }
}
