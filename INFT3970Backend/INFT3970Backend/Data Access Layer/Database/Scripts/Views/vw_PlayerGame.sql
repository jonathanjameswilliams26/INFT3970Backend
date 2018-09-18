USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Creates a view of Players and their game
-- =============================================
CREATE VIEW vw_PlayerGame
AS
SELECT
	PlayerID,
	Nickname,
	Phone,
	Email,
	SelfieDataURL,
	AmmoCount,
	NumKills,
	NumDeaths,
	NumPhotosTaken,
	IsHost,
	IsVerified,
	PlayerIsActive,
	PlayerIsDeleted,
	HasLeftGame,
	ConnectionID,
	IsConnected,
	g.GameID,
	GameCode,
	NumOfPlayers,
	GameMode,
	StartTime,
	EndTime,
	GameState,
	IsJoinableAtAnytime,
	GameIsActive,
	GameIsDeleted
FROM tbl_Player p
	INNER JOIN tbl_Game g ON (p.GameID = g.GameID)
GO