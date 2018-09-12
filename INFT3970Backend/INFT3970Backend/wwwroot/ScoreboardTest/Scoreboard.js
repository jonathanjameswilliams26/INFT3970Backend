

var playerID = 100001;

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


      
            var li1 = document.createElement("p");
            var li2= document.createElement("p");
            var li3 = document.createElement("p");

       
            
            li1.textContent = playerData[i].nickname;
            document.getElementById("nickname").appendChild(li1);

            li2.textContent = playerData[i].numKills;
            document.getElementById("numKills").appendChild(li2);

            li3.textContent = playerData[i].numDeaths;
            document.getElementById("numDeaths").appendChild(li3);
            
        }
    }
});

var gameID = 100000;

$.ajax({
    type: "GET",
    url: "https://localhost:5000/api/game/getGame/" + gameID,
    //dataType: "application/json",
    success: function (result) {
        console.log(result);

        //Get the player data from the result
        var gameData = result.data;


        var div = document.createElement("p");
      
        var endMinute = String(new Date(gameData.endTime).getMinutes());
        var endHour = String(new Date(gameData.endTime).getHours());


        var endTime = endHour + ":" + endMinute;


        div.textContent = endTime;
       
        document.getElementById("endTime").appendChild(div);
        console.log(timeToGo());
        
    }
});

function timeToGo(){

    var currentTime = new Date().toLocalString();

    return currentTime;
    
}




