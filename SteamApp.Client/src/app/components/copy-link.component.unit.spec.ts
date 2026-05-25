import { Clipboard } from '@angular/cdk/clipboard';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

import { CopyLinkComponent } from './copy-link.component';

@Component({
  standalone: true,
  imports: [CopyLinkComponent],
  template: `
    <app-copy-link [textToCopy]="textToCopy" [textToShow]="textToShow"></app-copy-link>
  `,
})
class CopyLinkHostComponent {
  textToCopy = 'rgb(255, 105, 180)';
  textToShow = 'Pink as Hell';
}

describe('CopyLinkComponent', () => {
  let fixture: ComponentFixture<CopyLinkHostComponent>;
  let host: CopyLinkHostComponent;
  let clipboard: jasmine.SpyObj<Clipboard>;

  beforeEach(async () => {
    clipboard = jasmine.createSpyObj<Clipboard>('Clipboard', ['copy']);

    await TestBed.configureTestingModule({
      imports: [CopyLinkHostComponent],
      providers: [{ provide: Clipboard, useValue: clipboard }],
    }).compileComponents();

    fixture = TestBed.createComponent(CopyLinkHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('copies text and returns to the input label after the feedback timeout', fakeAsync(() => {
    getButton().click();
    fixture.detectChanges();

    expect(clipboard.copy).toHaveBeenCalledWith('rgb(255, 105, 180)');
    expect(getButton().textContent?.trim()).toBe('Copied!');

    tick(1500);
    fixture.detectChanges();

    expect(getButton().textContent?.trim()).toBe('Pink as Hell');
  }));

  it('clears copied feedback when the displayed input changes', fakeAsync(() => {
    getButton().click();
    fixture.detectChanges();

    expect(getButton().textContent?.trim()).toBe('Copied!');

    host.textToShow = 'Australium Gold';

    expect(() => fixture.detectChanges()).not.toThrow();
    expect(getButton().textContent?.trim()).toBe('Australium Gold');

    tick(1500);
  }));

  function getButton(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('button') as HTMLButtonElement;
  }
});
