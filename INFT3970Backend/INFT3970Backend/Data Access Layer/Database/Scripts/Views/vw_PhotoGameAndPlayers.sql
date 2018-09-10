-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	A view which joins the Photo. game and player records
-- =============================================
USE udb_CamTag
GO
CREATE VIEW vw_PhotoGameAndPlayers
AS
SELECT 
	PhotoID,
	Lat,
	Long,
	PhotoDataURL,
	TimeTaken,
	VotingFinishTime,
	NumYesVotes,
	NumNoVotes,
	IsVotingComplete,
	PhotoIsActive,
	p.GameID,
	GameCode,
	NumOfPlayers,
	GameMode,
	StartTime,
	EndTime,
	GameState,
	IsJoinableAtAnytime,
	GameIsActive,
	TakenByPlayerID,
	takenBy.Nickname AS TakenByPlayerNickname,
	takenBy.Phone AS TakenByPlayerPhone,
	takenBy.Email AS TakenByPlayerEmail,
	takenBy.SelfieDataURL AS TakenByPlayerSelfieDataURL,
	takenBy.NumKills AS TakenByPlayerNumKills,
	takenBy.NumDeaths AS TakenByPlayerNumDeaths,
	takenBy.NumPhotosTaken AS TakenByPlayerNumPhotosTaken,
	takenBy.IsHost AS TakenByPlayerIsHost,
	takenBy.IsVerified AS TakenByPlayerIsVerified,
	takenBy.ConnectionID AS TakenByPlayerConnectionID,
	takenBy.IsConnected AS TakenByPlayerIsConnected,
	takenBy.PlayerIsActive AS TakenByPlayerIsActive,
	PhotoOfPlayerID,
	photoOf.Nickname AS PhotoOfPlayerNickname,
	photoOf.Phone AS PhotoOfPlayerPhone,
	photoOf.Email AS PhotoOfPlayerEmail,
	photoOf.SelfieDataURL AS PhotoOfPlayerSelfieDataURL,
	photoOf.NumKills AS PhotoOfPlayerNumKills,
	photoOf.NumDeaths AS PhotoOfPlayerNumDeaths,
	photoOf.NumPhotosTaken AS PhotoOfPlayerNumPhotosTaken,
	photoOf.IsHost AS PhotoOfPlayerIsHost,
	photoOf.IsVerified AS PhotoOfPlayerIsVerified,
	photoOf.ConnectionID AS PhotoOfPlayerConnectionID,
	photoOf.IsConnected AS PhotoOfPlayerIsConnected,
	photoOf.PlayerIsActive AS PhotoOfPlayerIsActive  
FROM tbl_Photo p
	INNER JOIN tbl_Game g ON (p.GameID = g.GameID)
	INNER JOIN tbl_Player takenBy ON (p.TakenByPlayerID = takenBy.PlayerID)
	INNER JOIN tbl_Player photoOf ON (p.PhotoOfPlayerID = photoOf.PlayerID)
GO
