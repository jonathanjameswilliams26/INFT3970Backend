/*
 *  Copyright (c) 2015 The WebRTC project authors. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license
 *  that can be found in the LICENSE file in the root of the source
 *  tree.
 */
'use strict';

//Global vars
let width = 1000,
    height = 0,
    filter = 'none',
    streaming = false;


//DOM Elements
const video = document.getElementById('video');
const canvas = document.getElementById('canvas');
const photoButton = document.getElementById('photoButton');

// Put variables in global scope to make them available to the browser console.
const constraints = window.constraints = {
    audio: false,
    video: true
};

function handleSuccess(stream) {
    //const video = document.querySelector('video');
    const videoTracks = stream.getVideoTracks();
    console.log('Got stream with constraints:', constraints);
    console.log(`Using video device: ${videoTracks[0].label}`);
    window.stream = stream; // make variable available to browser console
    video.srcObject = stream;
}

function handleError(error) {
    if (error.name === 'ConstraintNotSatisfiedError') {
        let v = constraints.video;
        errorMsg(`The resolution ${v.width.exact}x${v.height.exact} px is not supported by your device.`);
    } else if (error.name === 'PermissionDeniedError') {
        errorMsg('Permissions have not been granted to use your camera and ' +
            'microphone, you need to allow the page access to your devices in ' +
            'order for the demo to work.');
    }
    errorMsg(`getUserMedia error: ${error.name}`, error);
}

function errorMsg(msg, error) {
    const errorElement = document.querySelector('#errorMsg');
    errorElement.innerHTML += `<p>${msg}</p>`;
    if (typeof error !== 'undefined') {
        console.error(error);
    }
}

navigator.mediaDevices
    .getUserMedia(constraints)
    .then(handleSuccess)
    .catch(handleError);



//Play when ready
video.addEventListener('canplay', function (e) {
    if (!streaming) {
        //Set video canvas height
        height = video.videoHeight / (video.videoWidth / width);

        video.setAttribute('width', width);
        video.setAttribute('height', height);
        canvas.setAttribute('width', width);
        canvas.setAttribute('height', height);
        streaming = true;
    }
}, false);


photoButton.addEventListener('click', function (e) {
    takePicture();
    e.preventDefault;
}, false);


function takePicture() {
    const context = canvas.getContext('2d');
    if (width && height) {
        //set the canvas props
        canvas.width = width;
        canvas.height = height;

        //Draw an image of the video on the canvas
        context.drawImage(video, 0, 0, width, height);

        //Create image from the canvas
        const imgUrl = canvas.toDataURL('image/png');

        const url = 'https://10.0.0.1:5001/api/values';

        console.log(url);
        console.log(imgUrl);
    }
}