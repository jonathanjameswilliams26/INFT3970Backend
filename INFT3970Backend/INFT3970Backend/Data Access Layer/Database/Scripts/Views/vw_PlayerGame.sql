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
	SelfieFilePath,
	NumKills,
	NumDeaths,
	NumPhotosTaken,
	IsHost,
	IsVerified,
	p.IsActive AS PlayerIsActive,
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
	g.IsActive AS GameIsActive

FROM tbl_Player p
	INNER JOIN tbl_Game g ON (p.GameID = g.GameID)
GO