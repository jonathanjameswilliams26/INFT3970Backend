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
	g.GameID,
	GameCode,
	NumOfPlayers,
	GameMode,
	StartTime,
	EndTime,
	GameState,
	IsJoinableAtAnytime,
	g.IsActive AS GameIsActive,
	PlayerID,
	Nickname,
	Phone,
	Email,
	SelfieFilePath,
	NumKills,
	NumDeaths,
	NumPhotosTaken,
	IsHost,
	IsVerified,
	VerificationCode,
	ConnectionID,
	p.IsActive AS PlayerIsActive
FROM tbl_Game g
		INNER JOIN tbl_Player p ON (g.GameID = p.GameID)
WHERE
	GameState NOT LIKE 'COMPLETE'
	AND g.IsActive = 1
	AND p.IsActive = 1
GO