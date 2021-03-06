BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
DROP TABLE dbo.TestItem

CREATE TABLE dbo.TestItem
	(
	Id uniqueidentifier NOT NULL ROWGUIDCOL,
	Value nvarchar(50) NULL,
	IncreasingNumber int NOT NULL default(0),
	NumberModulo int NOT NULL default(0),
	DecreasingString nvarchar(50) NULL,
	ParentId uniqueidentifier NULL,
	Etag nvarchar(50) NULL,
	RecordVersion rowversion NOT NULL,
	RecordCreatedAt datetimeoffset(7) NOT NULL,
	RecordUpdatedAt datetimeoffset(7) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TestItem ADD CONSTRAINT
	DF_TestItem_Id DEFAULT (newid()) FOR Id
GO
ALTER TABLE dbo.TestItem ADD CONSTRAINT
	DF_TestItem_RecordCreatedAt DEFAULT (getutcdate()) FOR RecordCreatedAt
GO
ALTER TABLE dbo.TestItem ADD CONSTRAINT
	DF_TestItem_RecordUpdatedAt DEFAULT (getutcdate()) FOR RecordUpdatedAt
GO
ALTER TABLE dbo.TestItem ADD CONSTRAINT
	PK_TestItem PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX IX_TestItem_RecordCreatedAt ON dbo.TestItem
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_TestItem_ParentId ON dbo.TestItem
	(
	ParentId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.TestItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.TestItem', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TestItem', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TestItem', 'Object', 'CONTROL') as Contr_Per 