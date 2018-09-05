-- =============================================
-- Author:		Jonathan Williams
-- Create date: 06/09/18
-- Description:	Gets the game record matching the specified ID
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetGameByID] 
	-- Add the parameters for the stored procedure here
	@gameID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM tbl_Game
	WHERE GameID = @gameID
END
GO