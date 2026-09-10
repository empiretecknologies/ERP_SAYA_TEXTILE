var empr_InspectionProcessImages = {
    cropper: null,
    activeImageBox: null,

    InitEvents: function () {
        var self = this;
        $(document).ready(function () {
            console.log('empr_InspectionProcessImages connected');

            // 1. Updated Preview Trigger
            $('body').on('change', '#fileInput', function (e) {
                // Check karein konsa tab active hai taake preview sahi jagah dikhe
                var source = $('#specs-tab').hasClass('active') ? 'specs' : 'inspection';
                self.previewImages(e, source);
                $(this).val('');
            });

            // Finding Tab Individual Delete
            $('#image-upload-container').on('click', '#DEL_IMG', function (e) {
                e.preventDefault();
                var $parent = $(this).closest('.image-box');
                var tranId = $parent.data('tranid');
                var dtCode = $parent.data('dtcode');

                if (tranId && dtCode) {
                    self.deleteImage(tranId, dtCode, false, $parent, false);
                } else {
                    $parent.remove();
                }
            });

            $('#image-upload-container-Specs').on('click', '#DEL_IMG', function (e) {
                e.preventDefault();
                var $parent = $(this).closest('.image-box');
                var tranId = $parent.data('tranid');
                var dtCode = $parent.data('dtcode');

                if (tranId && dtCode) {
                    self.deleteImage(tranId, dtCode, false, $parent, true);
                } else {
                    $parent.remove();
                }
            });

            //$('body').on('click', '.image-box img', function () {
            //    self.activeImageBox = $(this);
            //    var src = $(this).attr('src');

            //    var image = document.getElementById('cropperImage');

            //    if (self.cropper) {
            //        self.cropper.destroy();
            //        self.cropper = null;
            //    }

            //    image.src = src;

            //    $('#cropperModal').modal('show');
            //});
            $(document).off('click', '.image-box img').on('click', '.image-box img', function () {
                // 'self' ki jagah seedha object name use karen taake scope ka masla hi khatam ho jaye
                empr_InspectionProcessImages.activeImageBox = $(this);

                var src = $(this).attr('src');
                var image = document.getElementById('cropperImage');

                // Reset image source
                image.src = src;

                // Show Modal
                $('#cropperModal').modal('show');
            });

            //$('#cropperModal').on('shown.bs.modal', function () {
            //    var image = document.getElementById('cropperImage');

            //    self.cropper = new Cropper(image, {
            //        aspectRatio: 1,
            //        viewMode: 1,
            //        autoCropArea: 1,
            //        checkOrientation: false,
            //        ready: function () {
            //            console.log("Cropper Ready!");
            //        }
            //    });
            //});
            $('#cropperModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                var self = empr_InspectionProcessImages;
                var image = document.getElementById('cropperImage');

                // Purana instance destroy karen agar exist karta hai
                if (self.cropper) {
                    self.cropper.destroy();
                    self.cropper = null;
                }

                self.cropper = new Cropper(image, {
                    aspectRatio: 1,
                    viewMode: 1,
                    autoCropArea: 1,
                    checkOrientation: false,
                    responsive: true,
                    ready: function () {
                        console.log("Cropper Ready on Tab!");
                    }
                });
            });

            $('#cropperModal').on('hidden.bs.modal', function () {
                if (self.cropper) {
                    self.cropper.destroy();
                    self.cropper = null;
                }
                document.getElementById('cropperImage').src = '';
            });

            //$('#cropAndSave').on('click', function () {
            //    if (self.cropper) {
            //        var canvas = self.cropper.getCroppedCanvas({
            //            width: 500,
            //            height: 500
            //        });

            //        var croppedImageDataURL = canvas.toDataURL('image/jpeg');

            //        self.activeImageBox.attr('src', croppedImageDataURL);
            //        $('#cropperModal').modal('hide');
            //    }
            //});
            $('#cropAndSave').off('click').on('click', function () {
                var self = empr_InspectionProcessImages;
                if (self.cropper) {
                    var canvas = self.cropper.getCroppedCanvas({
                        width: 500,
                        height: 500
                    });

                    var croppedImageDataURL = canvas.toDataURL('image/jpeg');

                    // Update the active image box
                    if (self.activeImageBox) {
                        self.activeImageBox.attr('src', croppedImageDataURL);
                    }

                    $('#cropperModal').modal('hide');
                }
            });

            $('body').on('click', '#dltAllImages', function (e) {
                self.handleDeleteAll(e, '#image-upload-container', false);
            });

            $('body').on('click', '#dltAllImagesSpecs', function (e) {
                self.handleDeleteAll(e, '#image-upload-container-Specs', true);
            });

            $('body').on('click', '#btnSubmitInspection', function () {
                self.submitData('inspection');
            });

            $('body').on('click', '#btnSubmitSpecs', function () {
                self.submitData('specs');
            });
        });
    },

    handleDeleteAll: function (e, containerSelector, isSpecs) {
        e.preventDefault();
        var self = this;
        var $savedImages = $(containerSelector + ' .image-box').filter(function () {
            return $(this).data('tranid') !== "" && $(this).data('tranid') !== undefined && $(this).data('tranid') !== 0;
        });

        if ($savedImages.length === 0) {
            $(containerSelector + ' .image-box').remove();
            return;
        }
        self.deleteImage(0, 0, true, $savedImages, isSpecs);
    },

    deleteImage: function (tranId, dtCode, isAllDelete, $element, isSpecs) {
        var jobNo = $('#key_hidden').val();
        var pTranId = $('#Code').val();

        var deleteRequestModel = {
            TranId: tranId || 0,
            DtCode: dtCode || 0,
            JobNo: jobNo || 0,
            PTranId: pTranId || 0,
            IsAllDelete: isAllDelete,
            IsSpecs: isSpecs || false
        };

        var swalText = isAllDelete ? 'Are you sure you want to delete all images?' : 'Are you sure you want to delete this image?';

        swal({
            title: swalText,
            type: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'No, cancel!',
            confirmButtonClass: 'btn btn-success mr-5',
            cancelButtonClass: 'btn btn-danger',
            buttonsStyling: false
        }).then(function () {
            ajaxHelper.ajaxPostJsonData(deleteRequestModel, "/InspectionProcess/DeleteImage", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    $element.remove();
                }
            }, false, true);
        });
    },

    previewImages: function (event, source) {
        const containerId = (source === 'specs') ? 'image-upload-container-Specs' : 'image-upload-container';
        const placeholderId = (source === 'specs') ? 'placeholder-box-Specs' : 'placeholder-box';

        const container = document.getElementById(containerId);
        const placeholder = document.getElementById(placeholderId);

        if (!event.target.files || event.target.files.length === 0) return;

        const files = event.target.files;
        for (let i = 0; i < files.length; i++) {
            const reader = new FileReader();
            reader.onload = function (e) {
                const div = document.createElement('div');
                div.className = 'image-box';
                div.setAttribute('data-tranid', '');
                div.setAttribute('data-dtcode', '');

                div.innerHTML = `
                    <img src="${e.target.result}" alt="preview" style="cursor:pointer">
                    <button type="button" class="remove-btn" id="DEL_IMG">&times;</button>
                `;
                container.insertBefore(div, placeholder);
            }
            reader.readAsDataURL(files[i]);
        }
    },

    submitData: function (source) {
        var self = this;
        var formData = new FormData();
        var imagesList = [];
        var containerId = (source === 'specs') ? '#image-upload-container-Specs' : '#image-upload-container';

        $(containerId + ' .image-box').each(function (index, box) {
            var $box = $(box);
            var imageData = $box.find('img').attr('src');
            var tId = $box.attr('data-tranid') || 0;
            var dCode = $box.attr('data-dtcode') || 0;

            if (imageData.startsWith('data:image')) {
                var blob = self.dataURLtoBlob(imageData);
                var fileName = 'img_' + source + '_' + index + '.jpg'; // Added source to filename to avoid conflict
                formData.append('Files', blob, fileName);
                imagesList.push({ TRAN_ID: 0, DT_CODE: 0, FileName: fileName, IsNew: true });
            } else {
                imagesList.push({ TRAN_ID: tId, DT_CODE: dCode, FileName: imageData, IsNew: false });
            }
        });

        var jobNo = $('#key_hidden').val();
        var ptranId = $('#Code').val();

        var modelData = {
            PTRAN_ID: ptranId || 0,
            JOB_NO: jobNo || 0,
            JobName: $('#jobValue_hidden').val(),
            Images: imagesList,
            IsSpecs: (source === 'specs')
        };

        formData.append('modelJson', JSON.stringify(modelData));

        $.ajax({
            url: '/InspectionProcess/SaveInspectionImages',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                empr_helper.notify(res.msg, res.msgType);
                if (res.msgType == 1) {
                    self.loadImages(ptranId, jobNo, source);
                }
            }
        });
    },

    loadImages: function (PTRAN_ID, JOB_NO, source) {
        debugger;
        var isSpecs = (source === 'specs');
        var url = '/InspectionProcess/GetInspectionImagesByCode?ptranId=' + PTRAN_ID + '&jobNo=' + JOB_NO + '&isSpecs=' + isSpecs;

        ajaxHelper.ajaxGetJson(url, function (res) {
            if (res.msgType === 1) {
                var containerId = isSpecs ? '#image-upload-container-Specs' : '#image-upload-container';
                var placeholderId = isSpecs ? '#placeholder-box-Specs' : '#placeholder-box';

                $(containerId + ' .image-box').remove();

                res.data.forEach(function (img) {
                    var imageHtml = `
                        <div class="image-box" data-tranid="${img.traN_ID}" data-dtcode="${img.dT_CODE}">
                            <img src="${img.imG_URL}" alt="Image" style="cursor:pointer">
                            <button type="button" class="remove-btn" id="DEL_IMG">&times;</button>
                        </div>`;
                    $(placeholderId).before(imageHtml);
                });
            }
        }, false, true);
    },

    dataURLtoBlob: function (dataurl) {
        var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
        while (n--) { u8arr[n] = bstr.charCodeAt(n); }
        return new Blob([u8arr], { type: mime });
    }
}