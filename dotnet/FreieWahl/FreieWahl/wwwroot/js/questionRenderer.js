function renderDecisionQuestion(question) {
    var result = '<div class="card my-4">\n<div class="card-header" style="background-color:#657f8C;color:white;font-size:2.5rem;font-weight: 100">\n' +
        'Entscheidungsfrage\n' +
        '<a class="float-right" href="#"><i class="material-icons" style="font-size:2.5rem;color:white">more_vert</i></a><!-- TODO: active link state -->\n' +
        '</div>\n<div class="card-body">\n<h1 class="card-title" style="border-bottom: 1px solid #657F8C">' +
        question.text +
        '</h1>\n<p class="card-text" style="border-bottom: 1px solid #C0C0C0">\n' +
        question.description +
        '</p>\n<div class="container">\n<div class="row">\n';
    for (var i = 0; i < question.answerOptions.length; i++) {
        result += renderDecisionOption(question.answerOptions[i]);
    }
    result += '</div>\n</div>\n<a href="#" class="btn btn-success float-right mx-3">Für Wahl freischalten</a>\n<a href="#" class="btn btn-light float-right mx-3">Frage löschen</a>\n</div>\n</div>\n';
    return result;
}

function renderDecisionOption(option) {
    return '<div class="col-lg-6 col-12">\n' +
             '<label style="display: block; position: relative; padding-left: 35px; margin-bottom: 12px;">\n' +
               '<span>' + option.text + '</span><br/>\n' +
               '<span class="text-primary small border-bottom">' + option.description + '</span>\n' +
               '<input type="radio" style="position:absolute;opacity: 0;"><span style="position: absolute; top: 0; left: 0; height: 23px; width: 23px; background-color: #eee;border:3px solid #657f8C;border-radius:50%"></span>\n' +
             '</label>\n' +
           '</div>\n';

}