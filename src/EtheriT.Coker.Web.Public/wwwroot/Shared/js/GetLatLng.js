function GetLatLng(root) {
    const $root = root ? (root.jquery ? root : $(root)) : $(document);
    const $targets = $root.is(".getlatlng")
        ? $root
        : $root.find(".getlatlng");

    if (!$targets.length) return;

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            (position) => {
                const latitude = position.coords.latitude;
                const longitude = position.coords.longitude;
                $targets.data("longitude", longitude);
                $targets.data("latitude", latitude);
                $targets.removeData("page");
                $targets.removeData("prevdirid");
                $targets.each(function () {
                    initElemntAndLoadDir($(this));
                });
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

function cokerI18n(key, ...args) {
    let str = local[key];
    if (!str) return ""; // 沒找到 key 就回空字串

    // 安全替換 {0} {1} ...
    return str.replace(/\{(\d+)\}/g, (match, index) => {
        return typeof args[index] !== "undefined" ? args[index] : match;
    });
}
