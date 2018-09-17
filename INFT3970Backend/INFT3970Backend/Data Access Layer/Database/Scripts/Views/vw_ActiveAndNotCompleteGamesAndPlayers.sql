-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	A view which contains all the active 
--				games and non complete games as well as 
--				all the active players in the game.
-- =============================================

CREATE VIEW vw_ActiveAndNotCompleteGamesAndPlayers
AS
SELECT
	*
FROM vw_PlayerGame
WHERE
	GameState NOT LIKE 'COMPLETE'
	AND GameIsActive = 1
	AND PlayerIsActive = 1
	AND PlayerIsDeleted = 0
	AND GameIsDeleted = 0
GO