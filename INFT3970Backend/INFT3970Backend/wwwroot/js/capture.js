//Global vars
let width = 500,
    height = 0,
    filter = 'none',
    streaming = false;


//DOM Elements
const video = document.getElementById('video');
const canvas = document.getElementById('canvas');
const photoButton = document.getElementById('photo-button');


//Get media stream
navigator.mediaDevices.getUserMedia({ video: true, audio: false }
)
    .then(function (stream) {
        //Link to the video source
        video.srcObject = stream;
        //Play the video
        video.play();
    })
    .catch(function (err) {
        console.log('Error: ${err}');
    });


//Play when ready
video.addEventListener('canplay', function (e) {
    if (!streaming) {
        //Set video canvas height
        height = video.videoHeight / (video.videoWidth / width);

        video.setAttribute('width', width);
        video.setAttribute('height', height);
        canvas.setAttribute('width', width);
        canvas.setAttribute('height', height);
        video.setAttribute("playsinline", true);

        streaming = true;
    }
}, false);
