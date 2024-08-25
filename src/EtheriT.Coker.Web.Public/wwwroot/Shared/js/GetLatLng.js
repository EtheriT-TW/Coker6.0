function GetLatLng() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            (position) => {
                const latitude = position.coords.latitude;
                const longitude = position.coords.longitude;
                console.log(`Latitude: ${latitude}, Longitude: ${longitude}`)
            },
            (error) => {
                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        console.log("GetLatLngError: User denied the request for Geolocation")
                        break;
                    case error.POSITION_UNAVAILABLE:
                        console.log("GetLatLngError: Location information is unavailable")
                        break;
                    case error.TIMEOUT:
                        console.log("GetLatLngError: The request to get user location timed out")
                        break;
                    case error.UNKNOWN_ERROR:
                        console.log("GetLatLngError: An unknown error occurred")
                        break;
                }
            }
        );
    } else {
        console.log("GetLatLngError: Geolocation is not supported by this browser")
    }
}