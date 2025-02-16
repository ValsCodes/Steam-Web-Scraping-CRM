import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FrontendProjectsComponent } from './frontend-projects.component';

describe('FrontendProjectsComponent', () => {
  let component: FrontendProjectsComponent;
  let fixture: ComponentFixture<FrontendProjectsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FrontendProjectsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FrontendProjectsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
