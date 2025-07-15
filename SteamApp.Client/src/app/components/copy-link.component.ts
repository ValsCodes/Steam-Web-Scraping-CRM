import { Component, Input } from '@angular/core';
import { ClipboardModule, Clipboard } from '@angular/cdk/clipboard';

@Component({
  selector: 'app-copy-link',
  standalone: true,
  imports: [ClipboardModule],    // ← import the module, not the service
  template: `
    <div class="space-y-2">
      <a
        href="#"
        title="Click to Copy"
        (click)="copyText(); $event.preventDefault()"
        class="text-indigo-600 hover:text-indigo-800 transition-colors duration-200"
      >
        {{ copied ? 'Copied!' : textToShow }}
      </a>
    </div>
  `,
})
export class CopyLinkComponent {
  @Input() textToCopy = '';
  @Input() textToShow = '';
  public copied = false;

  constructor(private clipboard: Clipboard) {}

  copyText() {
    if (!this.textToCopy) return;
    this.clipboard.copy(this.textToCopy);
    this.copied = true;
    setTimeout(() => (this.copied = false), 1500);
  }
}
