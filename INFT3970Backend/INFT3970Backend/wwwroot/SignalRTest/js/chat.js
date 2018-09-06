"use strict";
var playerID = 100000;
var connection = new signalR.HubConnectionBuilder().withUrl("/app?playerID=" + playerID).build();


//Connect to the hub
connection.start().catch(function (err) {
    return console.error(err.toString());
});


//Make a call to the API to get the list of all players when joining the lobby
console.log("GETTING PLAYERS LIST");
$.ajax({
    type: "GET",
    url: "https://localhost:5000/api/player/getAllPlayersInGame/" + playerID,
    //dataType: "application/json",
    success: function (result) {
        console.log(result);

        //Get the player data from the result
        var playerData = result.data;
        for (var i = 0; i < playerData.length; i++) {
            var li = document.createElement("li");
            li.textContent = playerData[i].nickname;
            document.getElementById("lobby").appendChild(li);
        }
    }
});



//Hub Client Function
//Updates the list of players in the lobby
connection.on("UpdateGameLobbyList", function () {
    //Clear the users list
    document.getElementById("lobby").innerHTML = "";

    //Make a call to the API to 
    $.ajax({
        type: "GET",
        url: "https://localhost:5000/api/player/getAllPlayersInGame/" + playerID,
        //dataType: "application/json",
        success: function (result) {
            console.log(result);

            //Get the player data from the result
            var playerData = result.data;
            for (var i = 0; i < playerData.length; i++) {
                var li = document.createElement("li");
                li.textContent = playerData[i].nickname;
                document.getElementById("lobby").appendChild(li);
            }
        }
    });
});



//Hub Client Function
//Updates the list of notifications a player has
connection.on("UpdateNotifications", function () {
    //Clear the users list
    document.getElementById("lobby").innerHTML = "";

    //Make a call to the API to 
    $.ajax({
        type: "GET",
        url: "https://localhost:5000/api/player/getNotifications/" + playerID,all
        //dataType: "application/json",
        success: function (result) {
            console.log(result);

            //Get the player data from the result
            var playerData = result.data;
            for (var i = 0; i < playerData.length; i++) {
                var li = document.createElement("li");
                li.textContent = notifData.MessageText
                document.getElementById("lobby").appendChild(li);
            }
        }
    });
});