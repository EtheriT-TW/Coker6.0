Coker.extend({
    File: {
        Upload: function (formData) {
            return $.ajax({
                url: '/api/FileUpload/uploadFiles',
                type: 'POST',
                data: formData,
                headers: _c.Data.Header,
                contentType: false,
                crossDomain: true,
                dataType: 'json',
                mimeType: "multipart/form-data",
                processData: false,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Upload360: function (formData) {
            return $.ajax({
                url: '/api/FileUpload/upload360Files',
                type: 'POST',
                data: formData,
                headers: _c.Data.Header,
                contentType: false,
                crossDomain: true,
                dataType: 'json',
                mimeType: "multipart/form-data",
                processData: false,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        UploadYTLink: function (data) {
            return $.ajax({
                url: "/api/FileUpload/uploadYTLink",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getFileList: function (obj) {
            return $.ajax({
                url: "/api/FileUpload/getFileList",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ type: obj.type, id: obj.id }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getImgFile: function (data) {
            return $.ajax({
                url: "/api/FileUpload/getImgFiles",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getAdFile: function (Aid) {
            return $.ajax({
                url: "/api/FileUpload/getAdvertiseFiles/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Aid: Aid },
            });
        },
        fileSortChange: function (data) {
            return $.ajax({
                url: "/api/FileUpload/fileSortChange",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Delete: function (guid) {
            return $.ajax({
                url: "/api/FileUpload/DeleteFile",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ key: guid }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        DeleteFileById: function (data) {
            return $.ajax({
                url: "/api/FileUpload/DeleteFileById",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        UploadImageInit: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "image/*",
                multiple: true,
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇圖片...",
                    label: "圖片上傳",
                },
            });
            return upload;
        },
        UploadFileInit: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: ".pdf,.dwg,.csv,.doc,.docx,.xls,.xlsx",
                multiple: true,
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇圖片...",
                    label: "圖片上傳",
                },
            });
            return upload;
        },
        Upload360Init: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "image/*",
                multiple: true,
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇多張圖片...",
                    label: "360圖片上傳(檔名請按編號排序 ex: image-1.jpg、image-2.jpg...)",
                    selectedCount: "張圖片已選擇",
                },
            });
            return upload;
        },
        UploadVideoInit: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "video/*",
                multiple: true,
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇檔案...",
                    label: "影片上傳",
                },
            });
            return upload;
        },
        UploadVideoPreviewInit: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "video/*",
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇檔案...",
                    label: "影片上傳",
                },
            });
            return upload;
        }
    }
});