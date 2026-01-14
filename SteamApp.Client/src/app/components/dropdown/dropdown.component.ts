import { CommonModule } from '@angular/common';
import { Component, Input, HostListener, ElementRef } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'steam-dropdown',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './dropdown.component.html',
  styleUrl: './dropdown.component.scss'
})

export class DropdownComponent {
  @Input() label!: string;
  @Input() links: { label: string; route: string }[] = [];
  open = false;

  constructor(private e: ElementRef) {}

  toggle() {
    this.open = !this.open;
  }
  close() {
    this.open = false;
  }

  @HostListener('document:click', ['$event'])
  onOutsideClick(e: Event) {
    if (!this.e.nativeElement.contains(e.target)) {
      this.close();
    }
  }
}
