IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ApplicationUsers] (
    [Id] int NOT NULL IDENTITY,
    [NtAccount] nvarchar(100) NOT NULL,
    [Role] nvarchar(50) NOT NULL,
    [DisplayName] nvarchar(100) NULL,
    CONSTRAINT [PK_ApplicationUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Benches] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [EquipmentNo] nvarchar(50) NULL,
    [AssetNo] nvarchar(50) NULL,
    [Location] nvarchar(100) NULL,
    [TestType] nvarchar(50) NULL,
    [TestObject] nvarchar(50) NULL,
    [Quantity] int NOT NULL,
    [WorkingHoursNorm] nvarchar(50) NULL,
    [BasicPerformanceAndConfiguration] nvarchar(500) NULL,
    [PictureUrl] nvarchar(255) NULL,
    [CurrentUser] nvarchar(50) NULL,
    [Project] nvarchar(100) NULL,
    [NextAvailableTime] datetime2 NULL,
    CONSTRAINT [PK_Benches] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Assignments] (
    [Id] int NOT NULL IDENTITY,
    [ApplicantNTAccount] nvarchar(50) NULL,
    [ApplicantName] nvarchar(50) NOT NULL,
    [ProjectName] nvarchar(100) NOT NULL,
    [Department] nvarchar(100) NULL,
    [BenchId] int NULL,
    [TestPlanId] int NULL,
    [RequestTime] datetime2 NOT NULL,
    [EstimatedSampleTime] datetime2 NOT NULL,
    [DesiredCompletionTime] datetime2 NOT NULL,
    [SampleQuantity] int NOT NULL,
    [SampleSpecification] nvarchar(200) NULL,
    [SampleBatchNo] nvarchar(100) NULL,
    [SampleRequirements] nvarchar(500) NULL,
    [TestContent] nvarchar(2000) NOT NULL,
    [TestStandard] nvarchar(500) NULL,
    [TestParameters] nvarchar(1000) NULL,
    [StageDescription] nvarchar(100) NULL,
    [SpecialRequirements] nvarchar(1000) NULL,
    [IsUrgent] bit NOT NULL,
    [UrgentReason] nvarchar(500) NULL,
    [Status] int NOT NULL,
    [Notes] nvarchar(1000) NULL,
    CONSTRAINT [PK_Assignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Assignments_Benches_BenchId] FOREIGN KEY ([BenchId]) REFERENCES [Benches] ([Id])
);
GO

CREATE TABLE [TestPlans] (
    [Id] int NOT NULL IDENTITY,
    [BenchId] int NOT NULL,
    [AssignmentId] int NULL,
    [ProjectName] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Status] int NOT NULL,
    [ScheduledDates] nvarchar(max) NOT NULL,
    [AssignedTo] nvarchar(100) NULL,
    [RequestedBy] nvarchar(max) NULL,
    [SampleNumber] nvarchar(max) NULL,
    [SampleQuantity] int NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_TestPlans] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TestPlans_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [Assignments] ([Id]),
    CONSTRAINT [FK_TestPlans_Benches_BenchId] FOREIGN KEY ([BenchId]) REFERENCES [Benches] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Assignments_BenchId] ON [Assignments] ([BenchId]);
GO

CREATE INDEX [IX_Assignments_TestPlanId] ON [Assignments] ([TestPlanId]);
GO

CREATE INDEX [IX_TestPlans_AssignmentId] ON [TestPlans] ([AssignmentId]);
GO

CREATE INDEX [IX_TestPlans_BenchId] ON [TestPlans] ([BenchId]);
GO

ALTER TABLE [Assignments] ADD CONSTRAINT [FK_Assignments_TestPlans_TestPlanId] FOREIGN KEY ([TestPlanId]) REFERENCES [TestPlans] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218033827_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ApplicationUsers] ADD [Department] nvarchar(100) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218035252_AddDepartmentToApplicationUser', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ReportApprovals] (
    [Id] int NOT NULL IDENTITY,
    [ReportTitle] nvarchar(200) NOT NULL,
    [ReportNumber] nvarchar(100) NOT NULL,
    [AssignmentId] int NULL,
    [ReportFilePath] nvarchar(500) NULL,
    [SubmitterNTAccount] nvarchar(100) NOT NULL,
    [SubmitterName] nvarchar(100) NULL,
    [SubmitTime] datetime2 NOT NULL,
    [ReviewerNTAccount] nvarchar(100) NOT NULL,
    [ReviewerName] nvarchar(100) NULL,
    [ReviewTime] datetime2 NULL,
    [ReviewComments] nvarchar(500) NULL,
    [ApproverNTAccount] nvarchar(100) NOT NULL,
    [ApproverName] nvarchar(100) NULL,
    [ApprovalTime] datetime2 NULL,
    [ApprovalComments] nvarchar(500) NULL,
    [Status] int NOT NULL,
    [Summary] nvarchar(1000) NULL,
    [Notes] nvarchar(1000) NULL,
    CONSTRAINT [PK_ReportApprovals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReportApprovals_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [Assignments] ([Id]) ON DELETE SET NULL
);
GO

CREATE INDEX [IX_ReportApprovals_AssignmentId] ON [ReportApprovals] ([AssignmentId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218060848_AddReportApproval', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218061821_AddReportApprovalTable', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [TestPlanHistories] (
    [Id] int NOT NULL IDENTITY,
    [TestPlanId] int NOT NULL,
    [ModifiedAt] datetime2 NOT NULL,
    [ModifiedBy] nvarchar(max) NOT NULL,
    [ChangeDescription] nvarchar(max) NOT NULL,
    [PreviousSnapshot] nvarchar(max) NULL,
    [NewSnapshot] nvarchar(max) NULL,
    [ChangedFields] nvarchar(max) NULL,
    [Reason] nvarchar(max) NULL,
    CONSTRAINT [PK_TestPlanHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TestPlanHistories_TestPlans_TestPlanId] FOREIGN KEY ([TestPlanId]) REFERENCES [TestPlans] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_TestPlanHistories_TestPlanId] ON [TestPlanHistories] ([TestPlanId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218072405_AddTestPlanHistory', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ReportApprovals] ADD [ReportFolderUrl] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251222032204_AddReportFolderUrlToReportApproval', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [BenchDocuments] (
    [Id] int NOT NULL IDENTITY,
    [BenchId] int NOT NULL,
    [FileName] nvarchar(200) NOT NULL,
    [FilePath] nvarchar(255) NOT NULL,
    [FileType] nvarchar(100) NULL,
    [FileSize] bigint NOT NULL,
    [Description] nvarchar(200) NULL,
    [UploadedAt] datetime2 NOT NULL,
    [UploadedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_BenchDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BenchDocuments_Benches_BenchId] FOREIGN KEY ([BenchId]) REFERENCES [Benches] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_BenchDocuments_BenchId] ON [BenchDocuments] ([BenchId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251222081711_AddBenchDocuments', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ApplicationUsers] ADD [Email] nvarchar(100) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251223065510_AddEmailToUser', N'8.0.10');
GO

COMMIT;
GO

