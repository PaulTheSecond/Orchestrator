namespace OrchestratorApp.Domain.Enums
{
    // This is a helper enum that combines ProcedureStageType and ContestStageType
    // for cases where we need to handle both types generically
    public enum StageType
    {
        // Procedure stage types
        Setup = 0,
        Contests = 1,
        Contracts = 2,
        Reporting = 3,
        
        // Contest stage types
        ApplicationSubmission = 100,
        Validation = 101,
        Expertise = 102,
        FundAllocation = 103,
        WinnerAnnouncement = 104
    }
}
