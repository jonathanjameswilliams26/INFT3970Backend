var map; 
function loadMapScenario()
{
    // icon image base64 test
    var base64Image = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABkAAAAcBAMAAABmCgnjAAAAFVBMVEVHcEz///8aGhpsuy2s4oDRuPMaADiElDMGAAAAAXRSTlMAQObYZgAAAFNJREFUeF7FkLENgDAQA52GGiIxwD8bPBsgBmD/aXCs6JMNcpWtk1wYx8SO6gO2N3kWtDuJBc39UtYTPEc69BIpTVqLgjJOpS7tQ1I5NyiGia2rHyT7OTg7xhBoAAAAAElFTkSuQmCC';

    // The navigation mode of the map
    var navigationBarMode = Microsoft.Maps.NavigationBarMode;
    // API key for Bing maps
    var key = 'AtRYc_SVF87Tlw1x0AlR7T2oE2qOFZNXtXuKjh8inv8FWnzBKo7QtRMzh3qP - D46';
    
    // options and settings for the map
    var mapOptions = {
        credentials: key,           // Key for the map
        zoom: 15,                    // Default zoom set for the map
       navigationBarMode: navigationBarMode.compact,       // navigation bar set to the smallest one
       supportedMapTypes: [Microsoft.Maps.MapTypeId.road]  // only setting to Road maps
    };

    // This function sets the user current location to the center of the map and puts a pin where this is
    navigator.geolocation.getCurrentPosition(function (position) {
        // Location of the center in Lattitude and Longitude
        var loc = new Microsoft.Maps.Location(
            position.coords.latitude,
            position.coords.longitude);

        //Add a pushpin at the user's location.
        var pin = new Microsoft.Maps.Pushpin(loc);
        map.entities.push(pin);

        //Center the map on the user's location.
        map.setView({ center: loc,});
    });
    // Initialsing the map
    map = new Microsoft.Maps.Map(document.getElementById('bingMap'), mapOptions);

    // Putting in all the pins for the last known locations 
    getLastKnownLocation();
    
}

var playerID = 100004;
// Gets all the latest photos for each user in the game. Each photo is the last one the user has taken
function getLastKnownLocation() {
   
    console.log("GETTING LOCATIONS");
    $.ajax({
        type: "GET",
        url: "https://localhost:5000/api/map/getLastPhotoLocations/" + playerID,
        success: function (result) {
            console.log(result);

            var photoData = result.data;
            for (var i = 0; i < photoData.length; i++)
            {
                // Creates the Pin in the map based on the lattitude and longitude of the photo, 
                // & puts in the players selfie into  the icon for the pin
                var pin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(photoData[i].lat, photoData[i].long), { icon: photoData[i].takenByPlayer.selfieDataURL });

                // The Date and time of the photo when it was taken  
                var timestamp = new Date(photoData[i].timeTaken);
                // Calling the function to format the time into a more user friendly format
                var correctTime = timestamp.toLocaleTimeString('en-US');
                // Calls the function to create the information box for the pin
                createInfoBox(photoData[i].lat, photoData[i].long, photoData[i].takenByPlayer.nickname, pin, correctTime);

                map.entities.push(pin);

               
            }
        }
    });
}

// Creates an information box for the last known location of the photo, Displays the Nickname and the Timestamp for each of the pins
function createInfoBox(lattitude, longitude, nickname, pin, timestamp)
{
    var infobox = new Microsoft.Maps.Infobox(new Microsoft.Maps.Location(lattitude, longitude),
        {
            title: nickname,
            description:timestamp ,
            visible: false
        });

    infobox.setMap(map);
    Microsoft.Maps.Events.addHandler(pin, 'click', function () {
        infobox.setOptions({ visible: true });
    });
}


