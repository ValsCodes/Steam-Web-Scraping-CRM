import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TagsView } from './tags-view';

describe('TagsView', () => {
  let component: TagsView;
  let fixture: ComponentFixture<TagsView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TagsView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TagsView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
