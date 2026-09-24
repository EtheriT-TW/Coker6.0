// Keep canvas image uploads limited to formats that the current upload pipeline
// can store or convert into a browser-displayable asset. In particular, do not
// use `image/*`: operating systems expose TIFF as image/tiff even though browsers
// generally cannot render the uploaded file in an <img> element.
export const imageAssetAccept = [
    'image/avif',
    'image/bmp',
    'image/gif',
    'image/heic',
    'image/heif',
    'image/jpeg',
    'image/png',
    'image/svg+xml',
    'image/webp',
    '.avif',
    '.bmp',
    '.gif',
    '.heic',
    '.heif',
    '.jpg',
    '.jpeg',
    '.png',
    '.svg',
    '.webp'
].join(',');
