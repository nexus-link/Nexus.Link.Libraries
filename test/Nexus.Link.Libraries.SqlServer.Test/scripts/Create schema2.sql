/*
   den 28 mars 201815:34:51
   User: 
   Server: WIN-7B74C50VA4D
   Database: LibrariesSqlServerUnitTest
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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

/*


  TestTrans1


*/

CREATE TABLE dbo.TestTrans1
	(
	Id uniqueidentifier NOT NULL ROWGUIDCOL,
	Field1 nvarchar(50) NULL,
	Field2 nvarchar(50) NULL,
	ParentId uniqueidentifier NULL,
	Etag nvarchar(50) NULL,
	RecordCreatedAt datetimeoffset(7) NOT NULL,
	RecordUpdatedAt datetimeoffset(7) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TestTrans1 ADD CONSTRAINT
	DF_TestTrans1_Id DEFAULT (newid()) FOR Id
GO
ALTER TABLE dbo.TestTrans1 ADD CONSTRAINT
	DF_TestTrans1_RecordCreatedAt DEFAULT (getutcdate()) FOR RecordCreatedAt
GO
ALTER TABLE dbo.TestTrans1 ADD CONSTRAINT
	DF_TestTrans1_RecordUpdatedAt DEFAULT (getutcdate()) FOR RecordUpdatedAt
GO
ALTER TABLE dbo.TestTrans1 ADD CONSTRAINT
	PK_TestTrans1 PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX IX_TestTrans1_RecordCreatedAt ON dbo.TestTrans1
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_TestTrans1_ParentId ON dbo.TestTrans1
	(
	ParentId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.TestTrans1 SET (LOCK_ESCALATION = TABLE)
GO

/*


  TestTrans2


*/

CREATE TABLE dbo.TestTrans2
	(
	Id uniqueidentifier NOT NULL ROWGUIDCOL,
	Field1 nvarchar(50) NULL,
	Field2 nvarchar(50) NULL,
	ParentId uniqueidentifier NULL,
	Etag nvarchar(50) NULL,
	RecordCreatedAt datetimeoffset(7) NOT NULL,
	RecordUpdatedAt datetimeoffset(7) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TestTrans2 ADD CONSTRAINT
	DF_TestTrans2_Id DEFAULT (newid()) FOR Id
GO
ALTER TABLE dbo.TestTrans2 ADD CONSTRAINT
	DF_TestTrans2_RecordCreatedAt DEFAULT (getutcdate()) FOR RecordCreatedAt
GO
ALTER TABLE dbo.TestTrans2 ADD CONSTRAINT
	DF_TestTrans2_RecordUpdatedAt DEFAULT (getutcdate()) FOR RecordUpdatedAt
GO
ALTER TABLE dbo.TestTrans2 ADD CONSTRAINT
	PK_TestTrans2 PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX IX_TestTrans2_RecordCreatedAt ON dbo.TestTrans2
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_TestTrans2_ParentId ON dbo.TestTrans2
	(
	ParentId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.TestTrans2 SET (LOCK_ESCALATION = TABLE)
GO

COMMIT
select Has_Perms_By_Name(N'dbo.TestTrans1', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TestTrans1', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TestTrans1', 'Object', 'CONTROL') as Contr_Per 
select Has_Perms_By_Name(N'dbo.TestTrans2', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.TestTrans2', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.TestTrans2', 'Object', 'CONTROL') as Contr_Per 