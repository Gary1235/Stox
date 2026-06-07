import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CapitalFlowComponent } from './capital-flow.component';

describe('CapitalFlowComponent', () => {
  let component: CapitalFlowComponent;
  let fixture: ComponentFixture<CapitalFlowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CapitalFlowComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CapitalFlowComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
