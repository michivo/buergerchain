function setupOrderingQuestion(questionIdx) {
    setupSelectedOptionsList(questionIdx);
    setupSelectableOptionsList(questionIdx);
}

function setupSelectableOptionsList(questionIdx) {
    var optionsGroup = document.getElementById(`options-${questionIdx}`);
    Sortable.create(optionsGroup, {
        group: `options-group-${questionIdx}`,
        onStart: function (evt) {
            $(`#info-options-${questionIdx}`).hide();
        },
        onAdd: function (evt) {
            $(`#options-${questionIdx} div.option-numbering`).detach();
            $(`#options-${questionIdx} .option-spacer`).detach();
        }
    });
}

function setupSelectedOptionsList(questionIdx) {
    var selectedOptions = document.getElementById(`selected-options-${questionIdx}`);
    Sortable.create(selectedOptions, {
        group: `options-group-${questionIdx}`,
        onRemove: function (evt) {
            if ($(`#selected-options-${questionIdx} div.fw-answer-title`).length == 0) {
                $(`#info-options-${questionIdx}`).show();
            }
            if ($(`#selected-options-${questionIdx} div.fw-answer-title`).length <= 3) {
                $(`#selected-options-${questionIdx}`).parent().css("border", "2px solid #C0C0C0");
                $(`#alert-question-${questionIdx}`).addClass("d-none");
                $(`#submit-question-${questionIdx}`).removeClass("d-none");
            }
        },
        onAdd: function (evt) {
            if ($(`#selected-options-${questionIdx} div.fw-answer-title`).length > 3) {
                $(`#selected-options-${questionIdx}`).parent().css("border", "3px solid red");
                $(`#alert-question-${questionIdx}`).removeClass("d-none");
                $(`#submit-question-${questionIdx}`).addClass("d-none");
            }
        },
        onSort: function (evt) {
            updateSorting(`#selected-options-${questionIdx}`);
        }
    });
}

function updateSorting(selector) {
    $(selector + ' .option-numbering').detach();
    $(selector + ' .option-spacer').detach();
    $(selector + ' div.fw-answer-title').each(function (index, val) {
        $(val).prepend(`<div class="bg-primary text-white text-center my-2 mr-2 option-numbering rounded-circle float-left" style="height:2rem;width:2rem;font-size:1.25rem">${index + 1}</div>`);
        $(val).append('<div class="option-spacer" style="clear: both;"></div>');
    });
}
