import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SharedListComponent } from './shared-list.component';

describe('SharedListComponent', () => {
  let component: SharedListComponent;
  let fixture: ComponentFixture<SharedListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SharedListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SharedListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
