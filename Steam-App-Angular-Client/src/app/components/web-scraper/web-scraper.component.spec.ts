import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WebScraperComponent } from './web-scraper.component';

describe('WebScraperComponent', () => {
  let component: WebScraperComponent;
  let fixture: ComponentFixture<WebScraperComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WebScraperComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WebScraperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
