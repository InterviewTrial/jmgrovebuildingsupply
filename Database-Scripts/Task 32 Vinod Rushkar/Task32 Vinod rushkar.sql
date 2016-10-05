
CREATE TYPE [dbo].[TypeInstallUser] AS TABLE(
	FristName [varchar](50) NULL,
	LastName [varchar](50) NULL,
	Email [varchar](100) NULL,
	Designation [varchar](50) NULL,
	Source [varchar](max) NULL,
	Phone [varchar](20) NULL,
	[Status] [varchar](20) NULL
)
GO
-- =============================================  
-- Author:  Vinod Rushkar
-- Create date: 06 Sep 2016 
-- Description: insert bulk Users  
-- =============================================  
Create PROCEDURE [dbo].InsertBulkUsers  
(
@tblUsers TypeInstallUser READONLY ,
@needtoEnterDuplicate bit
)
 AS  
BEGIN  
 SET NOCOUNT ON;  
 
 
 Select * from tblInstallUsers where 
 ( Phone in (select phone from @tblUsers) and Phone <> '')
  OR
 (Email in (select Email from @tblUsers) and Email <> '')
 

IF(@needtoEnterDuplicate = 0)
BEGIN
MERGE tblInstallUsers
USING @tblUsers as Temp
ON   ( IsNull(tblInstallUsers.Email,'') = IsNull(Temp.Email,'')
	   AND IsNull(tblInstallUsers.Phone,'') = IsNull(Temp.Phone,'') )

---- Insert Data
WHEN NOT MATCHED THEN
   
   INSERT  (
           FristName
           ,LastName
           ,Email
           ,Phone
           ,Designation
           ,[Status]
           ,Source)
     VALUES
           (Temp.FristName
           ,Temp.LastName
           ,Temp.Email
           ,Temp.Phone
           ,Temp.Designation
           ,Temp.Status
           ,Temp.Source);
 END
 ELSE
 BEGIN
 UPDATE tiu 
 SET tiu.FristName =tblu.FristName,tiu.LastName =tblu.LastName,tiu.Email =tblu.Email,tiu.Phone =tblu.Phone,tiu.Designation =tblu.Designation,tiu.Status =tblu.Status,tiu.Source =tblu.Source
 FROM tblInstallUsers tiu
 INNER JOIN @tblUsers tblu ON tiu.Email =tblu.Email
 UPDATE tiu 
 SET tiu.FristName =tblu.FristName,tiu.LastName =tblu.LastName,tiu.Email =tblu.Email,tiu.Phone =tblu.Phone,tiu.Designation =tblu.Designation,tiu.Status =tblu.Status,tiu.Source =tblu.Source
 FROM tblInstallUsers tiu
 INNER JOIN @tblUsers tblu ON tiu.Phone =tblu.Phone
 END
 
END  