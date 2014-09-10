$(document).ready(function () {
    var deleteLinkObj;
    $('.delete-link').click(function () {
        deleteLinkObj = $(this);
        $('#delete-dialog').dialog('open');
        return false;
    });

    $('#delete-dialog').dialog({
        autoOpen: false, width: 400, resizable: false, modal: true,
        buttons: {
            "Usuń": function () {
                $.post(deleteLinkObj[0].href, function (data) {
                    if (data == '@Boolean.TrueString') {
                        deleteLinkObj.closest("li").hide('fast');
                        deleteLinkObj.siblings("li").hide('fast');
                    }
                    else {
                        //(opcjonalnie) wyswietl blad
                    }
                });
                $(this).dialog("close"); //
            },
            "Anuluj": function () {
                $(this).dialog("close");
            }
        }
    });
});