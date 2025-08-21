$('#btnCode').click(function () {

    if ($('#cboMFA').val() == "Microsoft / Google") {

        window.location.href = '/UserAccount/MFAGoogle?id=' + $('#user').val()

    } else {

        $.ajax({
            type: 'GET',
            url: '../../UserAccount/GetMFACode',
            dataType: 'json',
            data:
            {
                "id": $('#user').val(),
                "mfa": $('#cboMFA').val()
            },
            beforeSend: function () {
                LoadingPanel.Show();
            },
            success: function (data) {

                if (data.msg) {
                    $("#request").val(data.data);
                    LoadingPanel.Hide();
                    jQuery.confirm({
                        title: 'Code request',
                        content: 'The code has been request succesfully',
                        type: 'blue',
                        theme: 'material',
                        closeIcon: true,
                        animateFromElement: false,
                        animation: 'left',
                        closeAnimation: 'right',
                        buttons: {
                            Ok: function () {

                            }
                        }
                    });
                } else {
                    jQuery.confirm({
                        title: 'Code request',
                        content: 'There is a problem with the code, please contact support.',
                        type: 'blue',
                        theme: 'material',
                        closeIcon: true,
                        animateFromElement: false,
                        animation: 'left',
                        closeAnimation: 'right',
                        buttons: {
                            Ok: function () {

                            }
                        }
                    });
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

    }
});


$('#cboMFA').on('change', function () {
    if ($('#cboMFA').val() == "Microsoft / Google") {

        window.location.href = '/UserAccount/MFAGoogle?id=' + $('#user').val()
    }
});