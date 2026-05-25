import { Clipboard, ClipboardModule } from '@angular/cdk/clipboard';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
} from '@angular/core';

@Component({
  selector: 'app-copy-link',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ClipboardModule],
  template: `
    <div class="space-y-2">
      <button
        type="button"
        title="Click to Copy"
        (click)="copyText()"
        class="text-indigo-600 hover:text-indigo-800 transition-colors duration-200"
      >
        {{ displayText }}
      </button>
    </div>
  `,
})
export class CopyLinkComponent implements OnChanges, OnDestroy {
  @Input() textToCopy = '';
  @Input() textToShow = '';

  copied = false;
  private resetTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private readonly clipboard: Clipboard,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  get displayText(): string {
    return this.copied ? 'Copied!' : this.textToShow;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      (changes['textToCopy'] || changes['textToShow']) &&
      !this.allChangesAreFirstChanges(changes)
    ) {
      this.clearCopiedState();
    }
  }

  ngOnDestroy(): void {
    this.clearResetTimer();
  }

  copyText(): void {
    if (!this.textToCopy) {
      return;
    }

    this.clipboard.copy(this.textToCopy);
    this.copied = true;
    this.cdr.markForCheck();

    this.clearResetTimer();
    this.resetTimer = setTimeout(() => {
      this.copied = false;
      this.resetTimer = null;
      this.cdr.markForCheck();
    }, 1500);
  }

  private clearCopiedState(): void {
    this.clearResetTimer();

    if (this.copied) {
      this.copied = false;
      this.cdr.markForCheck();
    }
  }

  private clearResetTimer(): void {
    if (this.resetTimer === null) {
      return;
    }

    clearTimeout(this.resetTimer);
    this.resetTimer = null;
  }

  private allChangesAreFirstChanges(changes: SimpleChanges): boolean {
    return Object.values(changes).every((change) => change.firstChange);
  }
}
