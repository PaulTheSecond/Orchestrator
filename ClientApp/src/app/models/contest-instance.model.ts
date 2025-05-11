import { StageConfiguration } from './stage-configuration.model';
import { ApplicationInstance } from './application-instance.model';

export interface ContestInstance {
  id?: string;
  procedureInstanceId: string;
  contestTemplateId: string;
  contestTemplateVersion: number;
  name: string;
  currentStageId?: string;
  status: string;
  startDate?: Date;
  endDate?: Date;
  stageConfigurations?: StageConfiguration[];
  applicationInstances?: ApplicationInstance[];
}

export interface CreateContestInstance {
  procedureInstanceId: string;
  contestTemplateId: string;
  name: string;
  stageConfigurations: StageConfiguration[];
}

export interface InterruptContest {
  contestInstanceId: string;
  reason: string;
}
