import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DiagramModule, NodeModel, ConnectorModel, ShapeModel, BasicShape, PointModel } from '@syncfusion/ej2-angular-diagrams';
import { ProcedureTemplate, ProcedureStageTemplate } from '../../../models/procedure-template.model';

@Component({
  selector: 'app-procedure-diagram',
  standalone: true,
  imports: [CommonModule, DiagramModule],
  templateUrl: './procedure-diagram.component.html',
  styleUrls: ['./procedure-diagram.component.css']
})
export class ProcedureDiagramComponent {
  public nodes: NodeModel[] = [];
  public connectors: ConnectorModel[] = [];

  private _procedureTemplate: ProcedureTemplate | undefined;
  @Input()
  set procedureTemplate(value: ProcedureTemplate | undefined) {
    this._procedureTemplate = value;
    if (value) {
      this.transformTemplateToDiagram(value);
    } else {
      this.nodes = [];
      this.connectors = [];
    }
  }
  get procedureTemplate(): ProcedureTemplate | undefined {
    return this._procedureTemplate;
  }

  constructor() { }

  private transformTemplateToDiagram(template: ProcedureTemplate): void {
    this.nodes = [];
    this.connectors = [];
    const stageNodeMap = new Map<string, NodeModel>(); // To easily find nodes by stage ID

    // Create nodes
    template.procedureStages.forEach((stage, index) => {
      const nodeId = stage.id || `gen-node-${index}`; // Generate ID if missing
      const node: NodeModel = {
        id: nodeId,
        offsetX: (stage.order || index) * 180 + 100, // Use index as fallback for order
        offsetY: 150, // Fixed Y for simplicity, can be adjusted
        annotations: [{ content: stage.stageType || stage.defaultServiceName || 'Unnamed Stage' }],
        shape: { type: 'Basic', shape: 'Rectangle' } as ShapeModel, // Basic rectangle shape
        // Add other styling as needed, e.g., width, height, style
        width: 120,
        height: 60,
      };
      this.nodes.push(node);
      if (stage.id) { // Only map if original ID exists
        stageNodeMap.set(stage.id, node);
      }
    });

    // Create connectors
    template.procedureStages.forEach((stage, index) => {
      const sourceNodeId = stage.id || `gen-node-${index}`; // Use generated ID if original was missing

      if (stage.nextStageId) {
        // Check if the target node exists (it should, if nextStageId is valid)
        // If stage.id was undefined, nextStageId might refer to an original ID.
        // For simplicity, this code assumes nextStageId refers to an ID that will be found in stageNodeMap
        // or corresponds to a generated ID if the target stage also had no ID.
        // A more robust solution might involve pre-mapping all original IDs to generated IDs if necessary.
        
        // Attempt to find target node by its original ID first, then by generated ID pattern if not found
        let targetNode = stageNodeMap.get(stage.nextStageId);
        if (!targetNode) {
            // This part is tricky if IDs are mixed (original vs generated)
            // For now, we assume nextStageId corresponds to an ID that was used for a node.
            // A full solution would need to map all original stage IDs to their node IDs.
            // We will assume for now that if nextStageId is present, a node with that ID exists.
        }

        const targetNodeId = stage.nextStageId; // Assume this ID exists for a node

        // Ensure both source and target nodes are in the processed nodes list
        const sourceNodeExists = this.nodes.find(n => n.id === sourceNodeId);
        const targetNodeExists = this.nodes.find(n => n.id === targetNodeId);

        if (sourceNodeExists && targetNodeExists) {
          const connector: ConnectorModel = {
            id: `conn-${sourceNodeId}-${targetNodeId}`,
            sourceID: sourceNodeId,
            targetID: targetNodeId,
            // Optional: Add connector styling, e.g., type: 'Orthogonal' or 'Straight'
            type: 'Orthogonal'
          };
          this.connectors.push(connector);
        }
      }
    });
  }
}
