import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProcedureDiagramComponent } from './procedure-diagram.component';

describe('ProcedureDiagramComponent', () => {
  let component: ProcedureDiagramComponent;
  let fixture: ComponentFixture<ProcedureDiagramComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProcedureDiagramComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProcedureDiagramComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
