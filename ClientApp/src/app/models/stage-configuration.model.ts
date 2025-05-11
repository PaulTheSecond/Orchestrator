export interface StageConfiguration {
  id?: string;
  contestInstanceId?: string;
  stageTemplateId: string;
  startDate?: Date;
  endDate?: Date;
  serviceName: string;
}
