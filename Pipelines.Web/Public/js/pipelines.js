//Set Edit Mode To Inline
$.fn.editable.defaults.mode = 'popup';

//Connect To SignalR Hub
var connection = jQuery.connection.boardHub;
jQuery.connection.hub.start().done(function () {
    connection.server.join(document.title);
});

//SignalR Client Methods
connection.client.renderPipeline = function (pipelineId, title, subscriptionStatus, stageIds) {

    //Hack
    if (stageIds.constructor.name.toLowerCase() === 'string') {
        stageIds = JSON.parse(stageIds);
    }
    var pipeline = '<tr><td class="' + (subscriptionStatus === "True" ? 'pipeline-subscribed' : 'pipeline-unsubscribed') + '" data-pipeline="' + pipelineId + '">' + title + '</td>';

    for (i = 0; i < stageIds.length; i++) {
        pipeline += '<td class="dropArea" data-pipeline="' + pipelineId + '" data-stage="' + stageIds[i] + '"></td>';
    }

    $('#board').append(pipeline + '</tr>');

    $('td.dropArea[data-pipeline=' + pipelineId + ']').droppable({
        accept: 'div.card',
        hoverClass: 'cell-hover',
        drop: function (event, ui) {
            $(ui.draggable).detach().css({ top: 0, left: 0 }).appendTo($(this))
        }
    });
};
connection.client.renderCard = function (cardId, pipelineId, stageId, title, description, subscriptionStatus) {

    subscriptionStatus = subscriptionStatus === 'False' || subscriptionStatus === 'false' ? false : subscriptionStatus; //handle False string as false
    var card = $('div.card[data-card=' + cardId + ']');

    if (card.length) {
        subscriptionStatus = subscriptionStatus ? subscriptionStatus : $(card).find('.card-subscribed').length; //handle no subscription passed and card was previously rendered
        card.remove();
    }

    card = '<div class="card" data-card="' + cardId + '" >'
         + '  <div class="details">'
         + '      <div class="id" title="' + title + '">ID: ' + cardId
         + '          - <span data-type="text" data-pk="' + cardId + '" data-url="/Card/UpdateCardTitle" data-title="Enter title" class="x-editable" >' + title + '</span>'
         + '      </div>' + (subscriptionStatus ? '<div class="card-subscription card-subscribed"></div>' : '<div class="card-subscription card-unsubscribed"></div>')
         + '  </div>'
         + '  <div class="content"  data-open="false">'
         + '      <div class="data">'
         + '            <span data-type="text" data-pk="' + cardId + '" data-url="/Card/UpdateCardDescription" data-title="Enter Description" class="x-editable">' + description + '</span>'
         + '      </div>'
         + '  </div>'
         + '</div>';

    $('td[data-pipeline=' + pipelineId + '][data-stage=' + stageId + ']').append(card);

    card = $('div.card[data-card=' + cardId + ']');

    $(card).draggable({
        opacity: 0.5,
        scroll: true,
        revert: 'invalid',
        start: function (event, ui) {
            connection.server.lockCard($(this).data("card"), document.title);
        },
        revert: function (valid) {
            var self = $(this);

            if (!valid) {
                connection.server.unlockCard(self.data("card"), document.title);
                console.log('removing')
                return true;
            }
            else {
                if (valid.attr('id') === 'recycle-bin') {
                    jQuery.post('/Card/Remove', { id: self.data('card') }, function (data) {
                        if (data.status == "success") {
                            connection.server.removeCard(self.data('card'), document.title);
                        }
                        else {
                            return true; //Server Error
                        }
                    })
                    .fail(function () {
                        return true; //Unexpected Error
                    });
                }
                else {
                    connection.server.move($(this).data('card'), $(this).parent().data('stage'), $(this).parent().data('pipeline'), document.title, username);
                    connection.server.unlockCard($(this).data("card"), document.title);
                }
            }

        },
    });

    $(card).find('.x-editable').editable({
        success: function (response, newValue) {
            if (response.status == 'error') return response.message; //msg will be shown in editable form
        }
    });
    $(card).find('.x-editable').on('shown', function (e, editable) {
        connection.server.lockCard(cardId, document.title);
    }).on('hidden', function (e, editable) {
        connection.server.unlockCard(cardId, document.title);
    }).on('click', function (e) {
        e.stopPropagation();
    });
};
connection.client.renderStage = function (stageId, title) {
    $('#headers').append('<th data-stage="' + stageId + '" draggable="false" class="stage-unsubscribed"' + '>' + title + '</th>')

    $('tr:not(#headers)').each(function () {
        $(this).append('<td class="dropArea" data-pipeline="' + $(this).children(':last-child').data('pipeline') + '" data-stage="' + stageId + '"></td>');
    });

    $('td.dropArea[data-stage=' + stageId + ']').droppable({
        accept: 'div.card',
        hoverClass: 'cell-hover',
        drop: function (event, ui) {
            $(ui.draggable).detach().css({ top: 0, left: 0 }).appendTo($(this))
        }
    });
}
connection.client.removeCard = function (cardId) {
    $("*[data-card=" + cardId + "]").remove();
}
connection.client.lockCard = function (taskId) {
    $("*[data-card='" + taskId + "']").css({ "opacity": "0.5", "pointer-events": "none" });
};
connection.client.unlockCard = function (taskId) {
    $("*[data-card='" + taskId + "']").css({ "opacity": "", "pointer-events": "" });
};
connection.client.moveCard = function (taskId, stageId, pipelineId) {
    $("[data-stage='" + stageId + "'][data-pipeline='" + pipelineId + "']").append($("*[data-card='" + taskId + "']"));
};
connection.client.updateStatus = function (message) {
    $("#status").text(message);
}


$(document).ready(function () {
    //Board Creation
    $('#create-new-board').on('click', function () {
        $("#create-board-modal").modal('show');
    });
    $('#new-board-name').focus(function () {
        if ($(this).val() === 'Name cannot be empty.' || $(this).val().substring(0, 16) == 'There is already') {
            $(this).val('')
        }
        $(this).removeClass('valid');
        $(this).removeClass('invalid');
    });
    $('#new-board-name').blur(function () {
        if ($('#new-board-name').val().trim().length > 0 && $('#new-board-name').val().trim() != '') {
            jQuery.post('/Board/Validate', { name: $('#new-board-name').val().trim() }, function (response) {
                if (response.status == 'false') {
                    $('#new-board-name').addClass('valid');
                }
                else {
                    $('#new-board-name').addClass('invalid');
                    $('#new-board-name').val('There is already a board called "' + $('#new-board-name').val().trim() + '"');
                }
            });
        }
        else {
            $('#new-board-name').addClass('invalid');
            $('#new-board-name').val('Name cannot be empty.');
        }
    });
    $('#save-new-board-button').on('click', function () {
        var validated = true;

        if ($('#new-board-name').val().trim().length > 0 == false) {
            $('#new-board-name').addClass('invalid');
            $('#new-board-name').val('Name cannot be empty.');
            validated = false;
        }

        if ($('#new-board-name').val() == 'Name cannot be empty.' || $('#new-board-name').val().substring(0, 16) == 'There is already') {
            validated = false;
        }

        $('#create-board-form .bootstrap-tagsinput').each(function () {
            if (($(this).children('span').length > 0) == false) {
                $(this).addClass('invalid');
                $(this).find('input:first').css('min-width', '100%')
                if ($(this).parent().find('input:first').attr('id') == 'stages') {
                    $(this).find('input:first').val('You need a minimum of 1 stage to get started.');
                }
                else {
                    $(this).find('input:first').val('You need a minimum of 1 pipeline to get started.');
                }

                validated = false;
            }
        })

        if (validated) {
            jQuery.ajax({
                url: '/Board/Add',
                type: "POST",
                data: JSON.stringify({ name: $('#new-board-name').val(), stages: $('#stages').tagsinput('items'), pipelines: $('#pipelines').tagsinput('items') }),
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.status == 'success') {
                        $("#create-board-modal").modal('hide');
                        $("#board-list li:last-child").before('<li class="list-group-item"><a href="/board/' + data.message + '">' + data.message + '</a></li>');
                    }

                    $('#new-board-name').val('');
                    $('#new-board-name').removeClass('valid');
                    $('.bootstrap-tagsinput').each(function () {
                        $(this).removeClass('valid');
                        $(this).find('> span').remove();
                    });
                }
            });
        }
    });



    //Fix For Enter Key On Modal
    $('form').on('submit', function (e) {
        e.preventDefault();
        $(this).closest('.modal').find('button[data-role=form-submit]')[0].click();
    });


    //New Stage
    $(document).on('click', '#create-new-stage-button', function () {
        $("#create-stage-modal").modal('show');
    });
    $(document).on('click', '#save-new-stage-button', function () {
        jQuery.post('/Stage/Add', $('#create-stage-form').serialize(), function (data) {
            $("#create-stage-modal").modal('hide');
            connection.server.renderStage(data.stageId, data.name, document.title)
        })
        .fail(function (data) {
            alert("Something went wrong");
        })
        .always(function () {
            $('#create-stage-form')[0].reset();
        })
    })



    //Card Drop Down
    $(document).on('click', 'div.card > div.content', function () {
        if ($(this).data('open') === false) {
            $(this).find('div.data').slideDown();
            $(this).data('open', true);
        } else {
            $(this).find('div.data').slideUp();
            $(this).data('open', false);
        }
    });

    //Recycle Bin Binding
    $('#recycle-bin').droppable({
        accept: 'div.card',
        tolerance: 'touch',
        over: function (event, ui) {
            $(this).children('i').css('color', 'red')
        },
        drop: function (event, ui) {
            $(this).children('i').css('color', 'black')
        },
        out: function (event, ui) {
            $(this).children('i').css('color', 'black')
        }
    });

    $(document).on('click', '#create-new-card-button', function () {
        $("#create-card-modal").modal('show');
    });

    $(document).on('click', '#save-new-card-button', function () {
        jQuery.post('/Card/Add', $('#create-card-form').serialize(), function () {
            $("#create-card-modal").modal('hide');
            $('#create-card-form')[0].reset();
        })
        .fail(function () {
            $("#create-card-modal").effect('shake')
        })
    });

    $(document).on('click', '#create-new-pipeline-button', function () {
        $("#create-pipeline-modal").modal('show');
    });

    $(document).on('click', '#save-new-pipeline-button', function () {
        jQuery.post('/Pipeline/Add', $('#create-pipeline-form').serialize(), function () {
            $("#create-pipeline-modal").modal('hide');
            $('#create-pipeline-form')[0].reset();
        })
        .fail(function () {
            $("#create-pipeline-modal").effect('shake')
        })
    });

    $(document).on('click', 'div.card div.details .card-subscription.card-subscribed', function () {
        jQuery.get(('/Card/Unsubscribe/' + $(this).parent().parent().data("card")));
        $(this).removeClass('card-subscribed');
        $(this).addClass('card-unsubscribed');
    });
    $(document).on('click', 'div.card div.details .card-subscription.card-unsubscribed', function () {
        jQuery.get(('/Card/Subscribe/' + $(this).parent().parent().data("card")));
        $(this).removeClass('card-unsubscribed');
        $(this).addClass('card-subscribed');
    });
    $(document).on('click', 'th.stage-subscribed', function () {
        jQuery.get(('/Stage/Unsubscribe/' + $(this).data("stage")));
        $(this).removeClass('stage-subscribed');
        $(this).addClass('stage-unsubscribed');

    });
    $(document).on('click', 'th.stage-unsubscribed', function () {
        jQuery.get(('/Stage/Subscribe/' + $(this).data("stage")));
        $(this).removeClass('stage-unsubscribed');
        $(this).addClass('stage-subscribed');
    });
    $(document).on('click', 'td.pipeline-subscribed', function () {
        jQuery.get(('/Pipeline/Unsubscribe/' + $(this).data("pipeline")));
        $(this).removeClass('pipeline-subscribed');
        $(this).addClass('pipeline-unsubscribed');
    });
    $(document).on('click', 'td.pipeline-unsubscribed', function () {
        jQuery.get(('/Pipeline/Subscribe/' + $(this).data("pipeline")));
        $(this).removeClass('pipeline-unsubscribed');
        $(this).addClass('pipeline-subscribed');
    });
});



