var Overview = (function () {

    var createVoting = function () {
        $("#newVotingModal").modal();
    }

    var saveVoting = function () {
        var title = $("#fwNewVotingName").val();
        var desc = $("#fwNewVotingDescription").val();
        var imageData = '';
        var validationResult = fwValidate('#fwStartDate', '#fwStartDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/);
        validationResult = fwValidate('#fwEndDate', '#fwEndDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/) && validationResult;
        validationResult = fwValidate('#fwStartTime', '#fwStartTimeValidation', /^[0-2]?\d:[0-5]\d$/) && validationResult;
        validationResult = fwValidate('#fwEndTime', '#fwEndTimeValidation', /^[0-2]?\d:[0-5]\d$/) && validationResult;
        validationResult = fwValidate('#fwNewVotingName', '#fwNewVotingNameValidation', /^(?!\s*$).+/) && validationResult;
        if (!validationResult)
            return;

        if ($('#fw-voting-img-real').is(':visible')) {
            var canvas = document.getElementById('fw-voting-img-real');
            imageData = canvas.toDataURL();
        }

        const startDate = parseDateTime($("#fwStartDate").val(), $("#fwStartTime").val());
        const endDate = parseDateTime($("#fwEndDate").val(), $("#fwEndTime").val());

        $.post({
            url: 'UpdateVoting',
            data: {
                "title": title,
                "desc": desc,
                "id": null,
                "imageData": imageData,
                "startDate": startDate.toISOString(),
                "endDate": endDate.toISOString(),
            },
            success: function (data) {
                $('#newVotingModal').modal('hide');
                window.location.replace(`Edit?id=${data}&isNew=true`);
            },
            error: function (err) {
                showErrorMessage('Beim Speichern der Abstimmung ist ein Fehler aufgetreten!', err);
            }
        });
    }


    var renderOpenVoting = function (voting) {
        let item = '<div class="col-xl-4 col-md-6 col-12 fw-open-voting mb-3"><div class="card fw-overview-card">' +
            `<a href="Edit?id=${voting.id}" style="height:12rem;">`;
        if (voting.imageData) {
            item += `<img class="card-img-top fw-open-vote" src="${voting.imageData}">`;
        } else {
            item +=
                '<i class="material-icons fw-voting-img" style="text-align:center;width:100%">how_to_vote</i>';
        }

        item +=
            `</a><div class="card-body d-flex flex-column justify-content-between" style="height:12rem"><h5 class="card-title">${
            voting.title}</h5>` +
            `<p class="card-text">${truncate(voting.description, 50)}</p>\r\n` +
        `<div><a class="fw-card-link-icon float-left p-2 border fw-delete-voting button-shadow" href="javascript:void(0);" data-votingid="${voting.id}"><i class="material-icons">delete</i></a>\r\n` +
            `<a class="fw-card-link-icon bg-primary float-right p-2 button-shadow" href="Edit?id=${voting.id
            }"><i class="material-icons text-white">edit</i></a></div></div></div></div>`;

        return item;
    }

    var renderClosedVoting = function(voting) {
        let imagePart;
        if (voting.imageData) {
            imagePart = `<img src="${voting.imageData}" class="fw-finished-vote" alt="Card image cap">`;
        }
        else {
            imagePart = '<i class="material-icons fw-closed-voting-img" style="text-align:center;width:100%">how_to_vote</i>';
        }
        return `<a href="Results?id=${voting.id}"><div class="col-xl-4 col-md-6 col-sm-12 my-2">
                <div class="fw-finished-vote-card">
                    ${imagePart}
                    <div class="fw-finished-vote-card-content">
                        <div class="fw-finished-vote-card-text">
                            <h5>${voting.title}</h5>
                            <p>${truncate(voting.description, 50)}</p>
                        </div>
                    </div>
                </div>
            </div></a>`;
    }

    var showOverviewData = function (data) {
        var openVotingItems = [];
        var closedVotingItems = [];
        var hasOpenVotings = false;
        for (var i = 0; i < data.length; i++) {
            const voting = data[i];
            if (voting.status === 1 || voting.status === 0) {
                hasOpenVotings = true;
                openVotingItems.push(renderOpenVoting(voting));
            }
            if (voting.status === 2) {
                closedVotingItems.push(renderClosedVoting(voting));
            }
        }
        $("#fw-active-voting-count").text(openVotingItems.length);
        if (openVotingItems.length > 0) {
            $("#fw-num-open-votings-info").removeClass("d-none");
        } else {
            $("#fw-num-open-votings-info").addClass("d-none");
        }
        $("#fw-closed-voting-count").text(closedVotingItems.length);

        if (closedVotingItems.length > 0) {
            $("#fw-num-closed-votings-info").removeClass("d-none");
            $("#fw-closed-voting-header").removeClass("d-none");
            $("#fw-closed-voting-list").removeClass("d-none");
        } else {
            $("#fw-num-closed-votings-info").addClass("d-none");
            $("#fw-closed-voting-header").addClass("d-none");
            $("#fw-closed-voting-list").addClass("d-none");
        }

        $(openVotingItems.join("\n")).insertAfter("#fw-new-voting-onboarding");
        $('#fw-closed-voting-list div.row').html(closedVotingItems.join("\n"));

        $("#fw-load-votings-card").hide();
        if (!hasOpenVotings) {
            $("#fw-new-voting-onboarding").removeClass("d-none").addClass("d-flex");
            $("#fw-new-voting-button").addClass("d-none");
        } else {
            $("#fw-new-voting-onboarding").removeClass("d-flex").addClass("d-none");
            $("#fw-new-voting-button").removeClass("d-none");
        }

        $(".fw-overview-card").mouseover(function () {
            $(this).removeClass('fw-overview-card-inactive');
            $(this).addClass('fw-overview-card-active');
        });


        $(".fw-overview-card").mouseout(function () {
            $(this).removeClass('fw-overview-card-active');
            $(this).addClass('fw-overview-card-inactive');
        });
    }

    var showOverview = function () {
        $(".fw-open-voting").remove();
        $("#fw-load-votings-card").show();

        $.ajax({
            url: 'GetVotingsForUser',
            type: 'GET',
            datatype: 'json',
            success: showOverviewData,
            error: function (data) {
                $("#fw-load-votings-card").hide();
                showErrorMessage('Beim Laden Ihres Benutzerprofils ist ein Fehler aufgetreten!', data);
            }
        });
    }

    var deleteVoting = function (id) {
        if (confirm("Sind Sie sicher, dass Sie diese Abstimmung unwiderruflich löschen wollen?")) {

            $.ajax({
                url: 'DeleteVoting',
                data: { "id": id },
                type: 'POST',
                success: function (data) {
                    showOverview(firebase.auth().currentUser);
                },
                error: function (data) {
                    showErrorMessage('Beim Löschen der Abstimmung ist ein Fehler aufgetreten!', data);
                }
            });
        }
    }

    var init = function () {
        initApp();

        $('document').ready(function () {
            $('#fw-user-img-input').change(function () { previewAndUploadFile('user'); });
            $('#fw-voting-img-input').change(function () { previewAndUploadFile('voting'); });
            $('#fwStartDate').change(function () { fwValidate('#fwStartDate', '#fwStartDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/); });
            $('#fwEndDate').change(function () { fwValidate('#fwEndDate', '#fwEndDateValidation', /^[0-3]?\d\.[0,1]?\d\.20\d{2}$/); });
            $('#fwStartTime').change(function () { fwValidate('#fwStartTime', '#fwStartTimeValidation', /^[0-2]?\d:[0-5]\d$/); });
            $('#fwEndTime').change(function () { fwValidate('#fwEndTime', '#fwEndTimeValidation', /^[0-2]?\d:[0-5]\d$/); });
            $('#fwNewVotingName').change(function () { fwValidate('#fwNewVotingName', '#fwNewVotingNameValidation', /^(?!\s*$).+/); });

            $('#fwBtnCreateVoting').click(saveVoting);
            $('#fw-new-voting-onboarding').click(createVoting);
            $('#fw-new-voting-button').click(createVoting);

            $('#fw-open-voting-list').on("click", ".fw-delete-voting", function () {
                deleteVoting($(this).attr("data-votingid"));
            });

            showOverview();
            $('#fw-user-img-content').hover(
                function () { highlightImgSelector('user', 'rgba(255, 255, 255, 0.5)'); },
                function () { resetImgSelector('user', 'rgba(255, 255, 255, 0.5)'); }
            );

            $('#fw-voting-img-content').hover(
                function () { highlightImgSelector('voting', 'rgba(101, 127, 140, 0.5)'); },
                function () { resetImgSelector('voting', 'rgba(101, 127, 140, 0.5)'); }
            );

            $('.form-group.date').datepicker({
                format: "dd.mm.yyyy",
                autoclose: true,
                language: "de",
                orientation: "top left"
            });
        });
    }

    return {
        init: init,
        showOverview: showOverview
    }

})();
