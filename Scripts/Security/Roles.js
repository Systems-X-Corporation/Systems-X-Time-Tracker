


function OnSuccess() {

    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'Role Created',
        content: 'Role succesfully created',
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
        title: 'Role Modified',
        content: 'Role succesfully modified',
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

$(document).ready(function () {

$("#roles-list").on("click", ".btnEditRoles", function () {
    console.log("ed");
    var idRole = $(this).attr("roleid");
    console.log(idRole);
    $.ajax({
        type: 'GET',
        url: '../../Roles/GetRoles',
        dataType: 'json',
        data:
        {
            "id": idRole
        },
        success: function (data) {
            console.log(data);
            $("#RoleId").val(data.RoleId);
            $("#eDescription").val(data.RoleDescription);
            $("#eComment").val(data.RoleComment);

            if (data.Active) {
                $("#eActive").attr('checked');
            } else {
                $("#eActive").prop('checked', false);
            }



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


$("#roles-list").on("click", ".btnDeleteRoles", function () {
    console.log("del");
    var idRole = $(this).attr("RoleId");
    var RoleName = $(this).attr("RolesName");

    $.confirm({
        title: 'Delete Role',
        content: 'Do you want to delete: ' + RoleName + '?',
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
                    url: '../../Roles/DeleteRoles',
                    dataType: 'json',
                    data:
                    {
                        "id": idRole
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
});


}
);
