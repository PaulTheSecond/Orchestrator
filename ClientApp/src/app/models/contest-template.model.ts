export interface StageTemplate {
  id?: string;
  stageType: string;
  order: number;
  previousStageId?: string;
  nextStageId?: string;
  defaultServiceName: string;
}

export interface ContestTemplate {
  id?: string;
  procedureTemplateId: string;
  name: string;
  version: number;
  isPublished: boolean;
  statusModel: string[];
  stages: StageTemplate[];
}
