USE [jgrove_JGP]
GO
/****** Object:  StoredProcedure [dbo].[UDP_UpdateInstallUserOfferMade]    Script Date: 6/20/2016 10:04:07 PM ******/
DROP PROCEDURE [dbo].[UDP_UpdateInstallUserOfferMade]
GO
/****** Object:  StoredProcedure [dbo].[UDP_UpdateInstallUserOfferMade]    Script Date: 6/20/2016 10:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[UDP_UpdateInstallUserOfferMade]
	-- Add the parameters for the stored procedure here
	@id int,  
	@Email varchar(100),  
	@password varchar(30)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	update tblInstallUsers set Email=@Email,[Password]=@password
		where Id=@id

END

GO
