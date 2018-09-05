// Initialize and add the map
function initMap() {

 

    // The location of Uluru
    var uluru = { lat: -25.344, lng: 131.036 };


    // The map, centered at Uluru
    var map = new google.maps.Map(
        document.getElementById('map'), { zoom: 4, center: uluru });
    // The marker, positioned at Uluru
    var marker = new google.maps.Marker({ position: uluru, map: map });

    newMarker(-20, 120, map);

    getLastKnownLocation(map);
    

}


function newMarker(lattitude, logitude, userMap) {

    var marker = { lat: lattitude, lng: logitude }
    new google.maps.Marker({ position: marker, map: userMap });
}

function getLastKnownLocation(userMap) {
    var photoID = 100002;

    console.log("GETTING LOCATION");
    $.ajax({
        type: "GET",
        url: "https://localhost:5000/api/player/getPhotoLocation/" + photoID,
        // dataType="application/json",
        success: function (result) {
            console.log(result);

            var playerData = result.data;
            for (var i = 0; i < playerData.length; i++) {

                
                var _position = { lat: playerData[i].lat, lng: playerData[i].long };

                new google.maps.Marker({ position: _position, map: userMap });
            }

        }


    });
}

