export interface StageResult {
  id?: string;
  applicationInstanceId: string;
  stageTemplateId: string;
  resultStatus: string;
  resultData?: string;
  completedAt: Date;
  integrationEventId: string;
}

export interface ApplicationInstance {
  id?: string;
  contestInstanceId: string;
  currentStageId?: string;
  status: string;
  createdAt: Date;
  updatedAt: Date;
  externalApplicationId?: string;
  stageResults?: StageResult[];
}

export interface CreateApplication {
  contestInstanceId: string;
  externalApplicationId?: string;
  applicationData: string;
}
