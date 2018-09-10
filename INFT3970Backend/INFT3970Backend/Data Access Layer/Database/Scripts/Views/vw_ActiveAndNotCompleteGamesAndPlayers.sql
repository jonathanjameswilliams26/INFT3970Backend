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
	GameIsActive,
	PlayerID,
	Nickname,
	Phone,
	Email,
	SelfieDataURL,
	NumKills,
	NumDeaths,
	NumPhotosTaken,
	IsHost,
	IsVerified,
	VerificationCode,
	ConnectionID,
	IsConnected,
	HasLeftGame,
	PlayerIsActive
FROM tbl_Game g
		INNER JOIN tbl_Player p ON (g.GameID = p.GameID)
WHERE
	GameState NOT LIKE 'COMPLETE'
	AND GameIsActive = 1
	AND PlayerIsActive = 1
GO