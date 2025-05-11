import { ContestInstance } from './contest-instance.model';

export interface ProcedureInstance {
  id?: string;
  procedureTemplateId: string;
  templateVersion: number;
  name: string;
  currentStageId?: string;
  status: string;
  createdAt: Date;
  updatedAt: Date;
  contestInstances?: ContestInstance[];
}

export interface CreateProcedureInstance {
  procedureTemplateId: string;
  name: string;
}

export interface TransitionProcedureStage {
  procedureInstanceId: string;
}
