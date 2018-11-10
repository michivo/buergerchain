function isEmpty(str) {
    return (!str || 0 === str.length || str === '0');
}

function formatDateTimeSeconds(secondsSinceEpoch) {
    var date = new Date(0);
    date.setUTCSeconds(secondsSinceEpoch);
    return `${date.getDate()}.${(date.getMonth() + 1).toString().padStart(2, '0')}.${date.getFullYear()} ${date.getHours()}:${date.getMinutes().toString().padStart(2, '0')}`;
}

function formatDateTimeSecondsForMail(secondsSinceEpoch) {
    var date = new Date(0);
    date.setUTCSeconds(secondsSinceEpoch);
    return `${date.getDate()}.${date.getMonth() + 1}.${date.getFullYear()} um ${date.getHours()}:${date.getMinutes().toString().padStart(2, '0')}`;
}

function truncate(val, maxLength) {
    if (val.length < maxLength)
        return val;

    return val.substr(0, maxLength) + '...';
}


function parseDateTime(date, time) {
    const dateParts = date.split('.');
    const timeParts = time.split(':');
    const day = parseInt(dateParts[0]);
    const month = parseInt(dateParts[1]);
    const year = parseInt(dateParts[2]);
    const hour = parseInt(timeParts[0]);
    const minute = parseInt(timeParts[1]);
    return new Date(year, month - 1, day, hour, minute);
}

function highlightImgSelector(type, color) {
    $("#fw-" + type + "-img-selector").css("border", "1px solid " + color);
    $("#fw-" + type + "-img-selector").css({
        backgroundColor: 'rgba(0,0,0,.5)'
    });
    $("#fw-" + type + "-img-selector").css("box-shadow", "1px 1px 1px rgba(0, 0, 0, 0.5)");
    $("#fw-" + type + "-img-icon").animate(
        { fontSize: "1.5rem", color: "white" });
    $("#fw-" + type + "-img-text").animate({
        opacity: 1
    });
}

function resetImgSelector(type, color) {
    $("#fw-" + type + "-img-selector").css("border", "0px solid " + color);
    $("#fw-" + type + "-img-selector").css({
        backgroundColor: 'rgba(0,0,0,0)',
    });
    $("#fw-" + type + "-img-icon").animate(
        { fontSize: "2rem" });
    $("#fw-" + type + "-img-text").animate({
        opacity: 0
    });
    $("#fw-" + type + "-img-selector").css("box-shadow", "0px 0px 0px");
    $('#fw-' + type + '-img-upload').css('display', 'none');
}

function showFileSelector(elementId) {
    $(elementId).css("display", "inline");
}

function previewAndUploadFile(type) {
    var file = $('#fw-' + type + '-img-input')[0].files[0];
    if (!file.type.match(/image.*/)) {
        return; // TODO: inform user to upload actual image
    };
    var img = new Image();
    var reader = new FileReader();
    reader.onload = function (e) {
        img.src = e.target.result;
    }

    img.onload = function () {
        var canvas = document.getElementById('fw-' + type + '-img-real');
        var context = canvas.getContext('2d');
        var MAX_WIDTH = 300;
        var MAX_HEIGHT = 300;
        var width = img.width;
        var height = img.height;

        if (width > height) {
            if (width > MAX_WIDTH) {
                height *= MAX_WIDTH / width;
                width = MAX_WIDTH;
            }
        } else {
            if (height > MAX_HEIGHT) {
                width *= MAX_HEIGHT / height;
                height = MAX_HEIGHT;
            }
        }
        canvas.width = width;
        canvas.height = height;
        context.drawImage(img, 0, 0, width, height);
        $('#fw-' + type + '-img-dummy').hide();
        $('#fw-' + type + '-img').hide();
        $('#fw-' + type + '-img-real').show();
        if (type === 'user')
            uploadFile();
    }
    reader.readAsDataURL(file);
}

function uploadFile() {
    var canvas = document.getElementById('fw-user-img-real');
    var dataURL = canvas.toDataURL();
    $.ajax({
        url: 'UpdateUserImage',
        type: 'POST',
        data: {
            imageData: dataURL
        },
        success: updateImage,
        error: function (data) {
            // TODO
        }
    });
}

function updateImage(data) {
    $('#fw-' + type + '-img').show();
    $('#fw-' + type + '-img-real').hide();
}

var fwValidate = function (elementId, validationId, regex) {
    const startDate = $(elementId).val();
    if (!startDate.match(regex)) {
        $(elementId).addClass('is-invalid');
        $(validationId).addClass('d-block');
        return false;
    }

    $(elementId).removeClass('is-invalid');
    $(validationId).removeClass('d-block');
    return true;
}