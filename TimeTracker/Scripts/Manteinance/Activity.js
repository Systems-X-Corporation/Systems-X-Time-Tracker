    


function OnSuccess() {

    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'Work Created',
        content: 'Work succesfully created',
        type: 'blue',
        theme: 'material',
        closeIcon: true,
        animateFromElement: false,
        animation: 'left',
        closeAnimation: 'right',
        buttons: {
            Ok: function () {
                location.reload();
            }
        }
    });
    LoadingPanel.Hide();
}

function OnSuccessEdit() {
    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'Work Modified',
        content: 'Work succesfully modified ',
        type: 'blue',
        theme: 'material',
        closeIcon: true,
        animateFromElement: false,
        animation: 'left',
        closeAnimation: 'right',
        buttons: {
            Ok: function () {
                location.reload();
            }
        }
    });

}

function one(e, callback) {
    if (typeof (callback) == 'function') {
        callback(e);
    }
    $('#cboECustomer').val(e).trigger('change');

}

function two(e) {
    $('#cboEProject').val(e).trigger('change');
}

$("#projects").on("click", ".btnEditActivity", function () {

    var idActivity = $(this).attr("ActivityId");


    $.ajax({
        type: 'POST',
        url: '../../Activity/GetActivity',
        dataType: 'json',
        data:
        {
            "id": idActivity
        },
        success: function (Activity) {

           

            $("#EdActivityId").val(Activity.ActivityId);
            $("#EdActivityName").val(Activity.ActivityName);
            var value = Activity.ProjectId;
            one(Activity.CustomerId, function () {
                two(value);
            });

            

        },
        timeout: 30 * 60 * 1000
    }).fail(function (xhr, status) {

        if (status === "timeout") {
            console.log('Tiempo de respuesta agotado');
        }
        if (status === "error") {
            console.log('La cantidad de datos excede el limite por conexión');
        }
    });

});


$("#projects").on("click", ".btnDeleteActivity", function () {

    var idActivity = $(this).attr("ActivityId");
    var projectName = $(this).attr("ActivityName");

    $.confirm({
        title: 'Work Activity',
        content: 'Do you want to delete: ' + projectName + '?',
        type: 'orange',
        theme: 'material',
        closeIcon: true,
        icon: 'fa fa-warning',
        animateFromElement: false,
        animation: 'left',
        closeAnimation: 'right',
        buttons: {
            Delete: function () {
                $.ajax({
                    type: 'POST',
                    url: '../../Activity/DeleteActivity',
                    dataType: 'json',
                    data:
                    {
                        "id": idActivity
                    },
                    success: function (response) {

                    },
                    timeout: 30 * 60 * 1000
                }).fail(function (xhr, status) {

                    if (status === "timeout") {
                        console.log('Tiempo de respuesta agotado');
                    }
                    if (status === "error") {
                        console.log('La cantidad de datos excede el limite por conexión');
                    }
                });
                location.reload();
            },
            Cancel: function () {

            }
        }
    });

    location.reload();
});



$(document).ready(function () {


    $("#cboCustomer").val('').change();
    

    $('#cboCustomer').on("select2:select", function () {

        $.ajax({
            type: 'GET',
            url: '../../System/GetCboProject',
            data:
            {
                "CustomerId": $(this).val()
            },
            success: function (response) {
                
                $("#CboProjectContent").html(response);
                $("#cboProject").select2();
            },
            timeout: 30 * 60 * 1000
        }).fail(function (xhr, status) {

            if (status === "timeout") {
                console.log('Tiempo de respuesta agotado');
            }
            if (status === "error") {
                console.log('La cantidad de datos excede el limite por conexión');
            }
        });

      
    });





});

