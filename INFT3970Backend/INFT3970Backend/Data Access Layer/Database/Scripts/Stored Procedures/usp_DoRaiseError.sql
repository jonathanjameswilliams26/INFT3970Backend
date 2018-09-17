USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_DoRaiseError] 
	@result INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	IF(@result <> 1)
	BEGIN
		RAISERROR('',16,1);
	END	
END
GO