USE [jgrove_JGP]
GO
/****** Object:  StoredProcedure [jgrov_User].[ExportAllSalesUsersData]    Script Date: 6/10/2016 9:07:21 PM ******/
DROP PROCEDURE [jgrov_User].[ExportAllSalesUsersData]
GO
/****** Object:  StoredProcedure [dbo].[UDP_GetallInstallusersdataNew]    Script Date: 6/10/2016 9:07:21 PM ******/
DROP PROCEDURE [dbo].[UDP_GetallInstallusersdataNew]
GO
/****** Object:  StoredProcedure [dbo].[GetAllEditSalesUser]    Script Date: 6/10/2016 9:07:21 PM ******/
DROP PROCEDURE [dbo].[GetAllEditSalesUser]
GO
/****** Object:  StoredProcedure [dbo].[GetAllAnnualEvent]    Script Date: 6/10/2016 9:07:21 PM ******/
DROP PROCEDURE [dbo].[GetAllAnnualEvent]
GO
/****** Object:  StoredProcedure [dbo].[ExportAllInstallUsersData]    Script Date: 6/10/2016 9:07:21 PM ******/
DROP PROCEDURE [dbo].[ExportAllInstallUsersData]
GO
/****** Object:  StoredProcedure [dbo].[ExportAllInstallUsersData]    Script Date: 6/10/2016 9:07:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ExportAllInstallUsersData]
	-- Add the parameters for the stored procedure here
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
SELECT t.Id,t.FristName,t.LastName,t.Email,t.Phone,t.Address,t.Zip,t.State,t.City,t.Designation,t.Status,t.Bussinessname,t.SSN,t.SSN1,t.SSN2,t.Signature,t.DOB,t.Citizenship,t.TaxpayerIdentificationno,t.EIN1,t.EIN2,t.A,t.B,t.C,t.D,t.E,t.F,t.G,t.H,t.maritalstatus,t.Source,t.Notes,t.StatusReason,t.HireDate,t.TerminitionDate,t.WorkersCompCode,t.NextReviewDate,t.EmpType,t.LastReviewDate,t.PayRates,t.ExtraEarning,t.ExtraEarningAmt,t.PayMethod,t.Deduction,t.DeductionType,t.AbaAccountNo,t.AccountNo,t.AccountType,t.InstallId,
	t.PTradeOthers,t.STradeOthers,t.DeductionReason,t.[StartDate]SuiteAptRoom,t.FullTimePosition,t.ContractorsBuilderOwner,
	t.MajorTools,t.DrugTest,t.ValidLicense,t.TruckTools,t.PrevApply,t.LicenseStatus,t.CrimeStatus,
	t.StartDate,t.SalaryReq,t.Avialability,t.skillassessmentstatus,t.WarrentyPolicy,t.CirtificationTraining,
	t.businessYrs,t.underPresentComp,t.websiteaddress,t.PersonName,t.PersonType,t.CompanyPrinciple,
	t.UserType,t.Email2,t.Phone2,t.CompanyName,t.SourceUser,t.DateSourced,t.InstallerType,t.BusinessType,
	t.CEO,t.InterviewTime,t.ActivationDate,t.UserActivated,t.LIBC,t.CruntEmployement,
	t.CurrentEmoPlace,t.LeavingReason,t.CompLit,t.FELONY,t.shortterm,t.LongTerm,t.BestCandidate,
	t.TalentVenue,t.Boardsites,t.NonTraditional,t.ConSalTraning,t.BestTradeOne,t.BestTradeTwo,
	t.BestTradeThree,t.aOne,t.aOneTwo,t.bOne,t.cOne,t.aTwo,t.aTwoTwo,t.bTwo,t.cTwo,t.aThree,
	t.aThreeTwo,t.bThree,t.cThree,t.RejectionDate,t.RejectionTime,t.RejectedUserId,
	d.TradeName AS 'PTradeName', Isnull(t.Source,'') AS Source, t.CreatedDateTime,
	SourceUser, ISNULL(U.Username,'')  AS AddedBy,
	InterviewDetail = case when (t.Status='InterviewDate' or t.Status='Interview Date') then coalesce(RejectionDate,'') + ' ' + coalesce(InterviewTime,'') else '' end,
	RejectDetail = case when (t.Status='Rejected' ) then coalesce(RejectionDate,'') + ' ' + coalesce(RejectionTime,'') + ' ' + '-' + coalesce(ru.LastName,'') else '' end

	  FROM tblInstallUsers t INNER JOIN Trades d
	  ON d.Id = t.PrimeryTradeId
	  	LEFT OUTER JOIN tblUsers U ON U.Id = t.SourceUser
	  LEFT OUTER JOIN tblUsers ru on t.RejectedUserId=u.Id
	  WHERE t.usertype = 'installer' OR t.usertype = 'Prospect' OR (t.usertype IS NULL AND t.Designation = 'SubContractor') OR (t.usertype IS NULL AND t.Designation = 'Installer')
	  order by Id desc
END


GO
/****** Object:  StoredProcedure [dbo].[GetAllAnnualEvent]    Script Date: 6/10/2016 9:07:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetAllAnnualEvent]
	-- Add the parameters for the stored procedure here
	@Year varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--select a.ID,a.EventName,a.EventDate,a.EventAddedBy,a.ApplicantId,u.Username,i.FristName,i.LastName,i.Phone
	--from tbl_AnnualEvents a INNER JOIN  tblUsers u ON u.Id = a.EventAddedBy LEFT JOIN tblInstallUsers i ON i.Id = a.ApplicantId
	--where DATEPART(yyyy,EventDate)=@Year

	select a.ID,a.EventName,a.EventDate,a.EventAddedBy,a.ApplicantId,u.Username,i.FristName,i.LastName,i.Phone,i.Id,
	i.Status
	from tbl_AnnualEvents a INNER JOIN  tblUsers u ON u.Id = a.EventAddedBy LEFT JOIN tblInstallUsers i ON i.Id = a.ApplicantId 
	WHERE a.EventName IS NOT NULL AND a.EventName !='InterViewDetails' AND DATEPART(yyyy,EventDate)=@Year
	Union 

	select a.ID,( a.EventName + '  '+u.Username + ' '+i.Designation+  '  ' + i.Phone +'   '+i.FristName) as EventName,
	EventDate = CONVERT(datetime,EventDate) + CONVERT(datetime,InterviewTime),a.EventAddedBy,a.ApplicantId,u.Username,i.FristName,i.LastName,i.Phone,i.Id,
	i.Status
	from tbl_AnnualEvents a INNER JOIN  tblUsers u ON u.Id = a.EventAddedBy 
	LEFT JOIN tblInstallUsers i ON i.Id = a.ApplicantId
	WHERE a.EventName='InterViewDetails' AND DATEPART(yyyy,EventDate)=@Year





END

GO
/****** Object:  StoredProcedure [dbo].[GetAllEditSalesUser]    Script Date: 6/10/2016 9:07:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetAllEditSalesUser]
	-- Add the parameters for the stored procedure here
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	SELECT t.Id,t.FristName,t.LastName,t.Phone,t.Zip,t.Designation,t.Status,t.HireDate,t.InstallId,t.picture, t.CreatedDateTime, Isnull(Source,'') AS Source,
	SourceUser, ISNULL(U.Username,'')  AS AddedBy,
	InterviewDetail = case when (t.Status='InterviewDate' or t.Status='Interview Date') then coalesce(RejectionDate,'') + ' ' + coalesce(InterviewTime,'') else '' end,
	RejectDetail = case when (t.Status='Rejected' ) then coalesce(RejectionDate,'') + ' ' + coalesce(RejectionTime,'') + ' ' + '-' + coalesce(ru.LastName,'') else '' end

	FROM tblInstallUsers t 
	LEFT OUTER JOIN tblUsers U ON U.Id = t.SourceUser
	LEFT OUTER JOIN tblUsers ru on t.RejectedUserId=ru.Id	  
	WHERE (t.UserType = 'SalesUser' OR t.UserType = 'sales') AND t.Status <> 'Deactive' 
	ORDER BY Id DESC
	
  --select t.Id,r.InstallerId,t.InstallId,t.FristName,t.LastName,t.HireDate,t.Phone,t.Zip,t.Designation,t.Status,t.Picture 
  --FROM tblInstallUsers t 
	 -- WHERE t.UserType = 'SalesUser' OR t.UserType = 'sales'
	 -- order by Id desc 
 
END



GO
/****** Object:  StoredProcedure [dbo].[UDP_GetallInstallusersdataNew]    Script Date: 6/10/2016 9:07:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE ProcEDURE [dbo].[UDP_GetallInstallusersdataNew] 
AS
BEGIN

	SELECT t.Id,t.FristName,t.LastName,t.Phone,t.Zip,t.Designation,t.Status,t.PrimeryTradeId,d.TradeName AS 'PTradeName',t.HireDate,t.InstallId,t.picture,t.CreatedDateTime, Isnull(Source,'') AS Source,
	SourceUser, ISNULL(U.Username,'')  AS AddedBy,
	InterviewDetail = case when (t.Status='InterviewDate' or t.Status='Interview Date') then coalesce(RejectionDate,'') + ' ' + coalesce(InterviewTime,'') else '' end,
	RejectDetail = case when (t.Status='Rejected' ) then coalesce(RejectionDate,'') + ' ' + coalesce(RejectionTime,'') + ' ' + '-' + coalesce(ru.LastName,'') else '' end

	FROM tblInstallUsers t 
	LEFT OUTER JOIN Trades d ON d.Id = t.PrimeryTradeId
	LEFT OUTER JOIN tblUsers U ON U.Id = t.SourceUser
	LEFT OUTER JOIN tblUsers ru on t.RejectedUserId=ru.Id
	WHERE  t.Status <> 'Deactive'
	and (t.usertype = 'installer' OR t.usertype = 'Prospect' OR (t.usertype IS NULL AND t.Designation = 'SubContractor') OR (t.usertype IS NULL AND t.Designation = 'Installer'))
	ORDER BY Id DESC
	  
END



GO
/****** Object:  StoredProcedure [jgrov_User].[ExportAllSalesUsersData]    Script Date: 6/10/2016 9:07:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [jgrov_User].[ExportAllSalesUsersData] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT t.Id,t.FristName,t.LastName,t.Email,t.Phone,t.Address,t.Zip,t.State,t.City,t.Designation,t.Status,t.Bussinessname,t.SSN,t.SSN1,t.SSN2,t.Signature,t.DOB,t.Citizenship,t.TaxpayerIdentificationno,t.EIN1,t.EIN2,t.A,t.B,t.C,t.D,t.E,t.F,t.G,t.H,t.maritalstatus,t.Source,t.Notes,t.StatusReason,t.HireDate,t.TerminitionDate,t.WorkersCompCode,t.NextReviewDate,t.EmpType,t.LastReviewDate,t.PayRates,t.ExtraEarning,t.ExtraEarningAmt,t.PayMethod,t.Deduction,t.DeductionType,t.AbaAccountNo,t.AccountNo,t.AccountType,t.InstallId,
	t.PTradeOthers,t.STradeOthers,t.DeductionReason,t.[StartDate]SuiteAptRoom,t.FullTimePosition,t.ContractorsBuilderOwner,
	t.MajorTools,t.DrugTest,t.ValidLicense,t.TruckTools,t.PrevApply,t.LicenseStatus,t.CrimeStatus,
	t.StartDate,t.SalaryReq,t.Avialability,t.skillassessmentstatus,t.WarrentyPolicy,t.CirtificationTraining,
	t.businessYrs,t.underPresentComp,t.websiteaddress,t.PersonName,t.PersonType,t.CompanyPrinciple,
	t.UserType,t.Email2,t.Phone2,t.CompanyName,t.SourceUser,t.DateSourced,t.InstallerType,t.BusinessType,
	t.CEO,t.InterviewTime,t.ActivationDate,t.UserActivated,t.LIBC,t.CruntEmployement,
	t.CurrentEmoPlace,t.LeavingReason,t.CompLit,t.FELONY,t.shortterm,t.LongTerm,t.BestCandidate,
	t.TalentVenue,t.Boardsites,t.NonTraditional,t.ConSalTraning,t.BestTradeOne,t.BestTradeTwo,
	t.BestTradeThree,t.aOne,t.aOneTwo,t.bOne,t.cOne,t.aTwo,t.aTwoTwo,t.bTwo,t.cTwo,t.aThree,
	t.aThreeTwo,t.bThree,t.cThree,t.RejectionDate,t.RejectionTime,t.RejectedUserId,
	InterviewDetail = case when (t.Status='InterviewDate' or t.Status='Interview Date') then coalesce(RejectionDate,'') + ' ' + coalesce(InterviewTime,'') else '' end,
	RejectDetail = case when (t.Status='Rejected' ) then coalesce(RejectionDate,'') + ' ' + coalesce(RejectionTime,'') + ' ' + '-' + coalesce(ru.LastName,'') else '' end

	  FROM tblInstallUsers t 
		LEFT OUTER JOIN tblUsers ru on t.RejectedUserId=ru.Id	  
	  WHERE t.UserType = 'sales' OR t.UserType = 'SSE'
	  order by Id desc
END

GO
