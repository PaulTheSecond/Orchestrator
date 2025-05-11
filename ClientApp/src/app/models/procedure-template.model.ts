export interface ProcedureStageTemplate {
  id?: string;
  stageType: string;
  order: number;
  previousStageId?: string;
  nextStageId?: string;
  defaultServiceName?: string;
}

export interface ProcedureTemplate {
  id?: string;
  name: string;
  version: number;
  isPublished: boolean;
  procedureStages: ProcedureStageTemplate[];
  contestTemplates?: ContestTemplate[];
}

import { ContestTemplate } from './contest-template.model';
