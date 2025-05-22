import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { DiagramModule } from '@syncfusion/ej2-angular-diagrams';
import { ProcedureDiagramComponent } from './procedure-diagram.component';
import { ProcedureTemplate, ProcedureStageTemplate } from '../../../models/procedure-template.model';

describe('ProcedureDiagramComponent', () => {
  let component: ProcedureDiagramComponent;
  let fixture: ComponentFixture<ProcedureDiagramComponent>;

  const mockStage1: ProcedureStageTemplate = {
    id: 'stage1',
    stageType: 'Setup',
    order: 1,
    defaultServiceName: 'Initial Setup',
    nextStageId: 'stage2'
  };

  const mockStage2: ProcedureStageTemplate = {
    id: 'stage2',
    stageType: 'Execution',
    order: 2,
    defaultServiceName: 'Main Execution',
    nextStageId: null // Or some other ID if there's a next stage
  };

  const mockTemplate: ProcedureTemplate = {
    id: 'template1',
    name: 'Test Procedure Template',
    version: 1,
    isPublished: false,
    procedureStages: [mockStage1, mockStage2]
  };

  const mockTemplateSingleStage: ProcedureTemplate = {
    id: 'template2',
    name: 'Single Stage Template',
    version: 1,
    isPublished: false,
    procedureStages: [{ ...mockStage1, nextStageId: null }] // Ensure no connector for single stage
  };


  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ProcedureDiagramComponent, // Import standalone component
        DiagramModule,             // Import Syncfusion DiagramModule
        NoopAnimationsModule       // To handle animations gracefully in tests
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProcedureDiagramComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges(); // Trigger initial data binding and lifecycle hooks
    expect(component).toBeTruthy();
  });

  it('should initialize nodes and connectors when procedureTemplate is provided', () => {
    component.procedureTemplate = mockTemplate;
    fixture.detectChanges();

    expect(component.nodes.length).toBe(mockTemplate.procedureStages.length);
    // We expect 1 connector because mockStage1 has nextStageId = 'stage2'
    expect(component.connectors.length).toBe(1); 
  });
  
  it('should have no connectors if no nextStageId is defined', () => {
    component.procedureTemplate = mockTemplateSingleStage;
    fixture.detectChanges();

    expect(component.nodes.length).toBe(mockTemplateSingleStage.procedureStages.length);
    expect(component.connectors.length).toBe(0);
  });

  it('should clear nodes and connectors if procedureTemplate becomes undefined', () => {
    // Set initial template
    component.procedureTemplate = mockTemplate;
    fixture.detectChanges();
    
    expect(component.nodes.length).toBeGreaterThan(0); // Verify initial state

    // Set template to undefined
    component.procedureTemplate = undefined;
    fixture.detectChanges();

    expect(component.nodes.length).toBe(0);
    expect(component.connectors.length).toBe(0);
  });

  it('should render <ejs-diagram> element when procedureTemplate is provided', () => {
    component.procedureTemplate = mockTemplate;
    fixture.detectChanges();

    const diagramElement = fixture.debugElement.query(By.css('ejs-diagram'));
    expect(diagramElement).not.toBeNull();
  });

  it('should not render <ejs-diagram> element when procedureTemplate is undefined', () => {
    component.procedureTemplate = undefined;
    fixture.detectChanges();

    const diagramElement = fixture.debugElement.query(By.css('ejs-diagram'));
    expect(diagramElement).toBeNull(); // Check based on the *ngIf in the template
  });
});
