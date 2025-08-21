$(document).ready(function () {


    $('#btnSync').on('click', function () {

        $.ajax({
            type: 'GET',
            url: '../../OAuth/SyncEvents',
            dataType: 'json',
            beforeSend: function () {
                LoadingPanel.Show();
            },
            success: function (data) {
                console.log(data);
                $.confirm({
                    title: 'Sync Events',
                    content: data.data,
                    type: 'blue',
                    theme: 'material',
                    closeIcon: true,
                    animateFromElement: false,
                    animation: 'left',
                    closeAnimation: 'right',
                    buttons: {
                        Ok: function () {
                            LoadingPanel.Hide();
                        }
                    }
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

});

$("#myform").submit(function (e) {
    LoadingPanel.Show();
});

function OnSuccess() {

    /*$('.modal-backdrop').remove();*/

    $.confirm({
        title: 'User Information',
        content: 'User information succesfully updated',
        type: 'blue',
        theme: 'material',
        closeIcon: true,
        animateFromElement: false,
        animation: 'left',
        closeAnimation: 'right',
        buttons: {
            Ok: function () {
                $.ajax({
                    type: 'GET',
                    url: '../../System/Navbar',
                    success: function (response) {
                        $("#navbardiv").html(response);
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


            }
        }
    });

    LoadingPanel.Hide();

}