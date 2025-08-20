


function OnSuccess() {

    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'User Created',
        content: 'User succesfully created',
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
        title: 'User Modified',
        content: 'User succesfully modified',
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


$("#users-list").on("click", ".btnEditUsers", function () {

    var idUser = $(this).attr("UsersId");
  
    $.ajax({
        type: 'GET',
        url: '../../Users/GetUsers',
        dataType: 'json',
        data:
        {
            "id": idUser
        },
        success: function (data) {
            console.log(data.role);
            $("#EdUsersId").val(data.UserId);
            $("#eEmail").val(data.Email);
            $("#cboCompanyId").val(data.CompanyId);
            $("#eUsername").val(data.UserName);
            $("#eFirstName").val(data.FirstName);
            $("#eLastName").val(data.LastName);
            if (data.Active) {
                $("#customCheck").attr('checked');
            } else {
                $("#customCheck").prop('checked', false);
            }
            $("#cboERole").val(data.role).trigger('change');


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


$("#users-list").on("click", ".btnDeleteUsers", function () {

    var idUser = $(this).attr("UsersId");
    var UserName = $(this).attr("UsersName");

    $.confirm({
        title: 'Delete User',
        content: 'Do you want to delete: ' + UserName + '?',
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
                    url: '../../Users/DeleteUsers',
                    dataType: 'json',
                    data:
                    {
                        "id": idUser
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



$("#users-list").on("click", ".btnUnlockUsers", function () {

    var idUser = $(this).attr("UsersId");
    var UserName = $(this).attr("UsersName");

    $.confirm({
        title: 'Unlock User',
        content: 'Do you want to unlock: ' + UserName + '?',
        type: 'green',
        theme: 'material',
        closeIcon: true,
        icon: 'fa fa-warning',
        animateFromElement: false,
        animation: 'left',
        closeAnimation: 'right',
        buttons: {
            Unlock: function () {
                $.ajax({
                    type: 'POST',
                    url: '../../Users/UnlockUser',
                    dataType: 'json',
                    data:
                    {
                        "id": idUser
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


$('#btnNewCompany').click(function () {
    $('#container').hide();
    $('#containerNew').show();
});

$('#btnCancel').click(function () {
    $('#container').show();
    $('#containerNew').hide();
});

$(document).ready(function () {
    $('#containerNew').hide();
});