USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Creates a view of all Active, Non Deleted and Verfied Players
-- =============================================
CREATE VIEW vw_ActiveAndVerifiedPlayers
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
	VerificationCode,
	ConnectionID,
	IsConnected,
	HasLeftGame,
	IsDeleted,
	PlayerIsActive,
	PlayerIsDeleted,
	GameID

FROM tbl_Player
WHERE
	PlayerIsActive = 1 AND
	PlayerIsDeleted = 0 AND
	IsVerified = 1
GO