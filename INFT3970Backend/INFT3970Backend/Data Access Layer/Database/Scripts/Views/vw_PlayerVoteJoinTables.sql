-- =============================================
-- Author:		Jonathan Williams
-- Create date: 11/09/18
-- Description:	Creates a view of The PlayerVotePhoto records with the joined Player and Photo tables
-- =============================================
USE udb_CamTag
GO
CREATE VIEW vw_PlayerVoteJoinTables
AS
SELECT 
	pvp.VoteID,
	pvp.IsPhotoSuccessful,
	pvp.PlayerVotePhotoIsActive,
	pgp.*,
	pg.PlayerID,
	pg.Nickname,
	pg.Phone,
	pg.Email,
	pg.SelfieDataURL,
	pg.NumKills,
	pg.NumDeaths,
	pg.NumPhotosTaken,
	pg.IsHost,
	pg.IsVerified,
	pg.ConnectionID,
	pg.IsConnected,
	pg.HasLeftGame,
	pg.PlayerIsActive
FROM tbl_PlayerVotePhoto pvp
	INNER JOIN vw_PhotoGameAndPlayers pgp ON (pvp.PhotoID = pgp.PhotoID)
	INNER JOIN vw_PlayerGame pg ON (pvp.PlayerID = pg.PlayerID)
GO