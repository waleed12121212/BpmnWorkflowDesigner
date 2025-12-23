-- Camunda Integration Migration Script
-- Add Camunda-related fields to Workflows table and create ProcessInstances table

-- ========================================
-- 1. Add Camunda fields to Workflows table
-- ========================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Workflows]') AND name = 'CamundaDeploymentId')
BEGIN
    ALTER TABLE [dbo].[Workflows]
    ADD [CamundaDeploymentId] NVARCHAR(64) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Workflows]') AND name = 'CamundaProcessDefinitionId')
BEGIN
    ALTER TABLE [dbo].[Workflows]
    ADD [CamundaProcessDefinitionId] NVARCHAR(64) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Workflows]') AND name = 'IsDeployed')
BEGIN
    ALTER TABLE [dbo].[Workflows]
    ADD [IsDeployed] BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Workflows]') AND name = 'LastDeployedAt')
BEGIN
    ALTER TABLE [dbo].[Workflows]
    ADD [LastDeployedAt] DATETIME2 NULL;
END

-- ========================================
-- 2. Create ProcessInstances table
-- ========================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProcessInstances]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProcessInstances] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [WorkflowId] UNIQUEIDENTIFIER NOT NULL,
        [CamundaProcessInstanceId] NVARCHAR(64) NOT NULL,
        [CamundaProcessDefinitionId] NVARCHAR(64) NULL,
        [BusinessKey] NVARCHAR(255) NULL,
        [StartedBy] UNIQUEIDENTIFIER NOT NULL,
        [StartedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [EndedAt] DATETIME2 NULL,
        [State] INT NOT NULL DEFAULT 0, -- 0=Running, 1=Completed, 2=Cancelled, 3=Failed
        [IsSuspended] BIT NOT NULL DEFAULT 0,
        [Variables] NVARCHAR(MAX) NULL, -- JSON
        [EndReason] NVARCHAR(500) NULL,
        
        CONSTRAINT [FK_ProcessInstances_Workflows] FOREIGN KEY ([WorkflowId]) 
            REFERENCES [dbo].[Workflows]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProcessInstances_Users] FOREIGN KEY ([StartedBy]) 
            REFERENCES [dbo].[Users]([Id])
    );
    
    -- Create indexes for better performance
    CREATE INDEX [IX_ProcessInstances_WorkflowId] ON [dbo].[ProcessInstances]([WorkflowId]);
    CREATE INDEX [IX_ProcessInstances_StartedBy] ON [dbo].[ProcessInstances]([StartedBy]);
    CREATE INDEX [IX_ProcessInstances_CamundaProcessInstanceId] ON [dbo].[ProcessInstances]([CamundaProcessInstanceId]);
    CREATE INDEX [IX_ProcessInstances_State] ON [dbo].[ProcessInstances]([State]);
    CREATE INDEX [IX_ProcessInstances_StartedAt] ON [dbo].[ProcessInstances]([StartedAt] DESC);
END

-- ========================================
-- 3. Create indexes on Workflows for Camunda fields
-- ========================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Workflows_CamundaDeploymentId' AND object_id = OBJECT_ID('dbo.Workflows'))
BEGIN
    CREATE INDEX [IX_Workflows_CamundaDeploymentId] ON [dbo].[Workflows]([CamundaDeploymentId]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Workflows_IsDeployed' AND object_id = OBJECT_ID('dbo.Workflows'))
BEGIN
    CREATE INDEX [IX_Workflows_IsDeployed] ON [dbo].[Workflows]([IsDeployed]);
END

PRINT 'Camunda integration migration completed successfully!';
