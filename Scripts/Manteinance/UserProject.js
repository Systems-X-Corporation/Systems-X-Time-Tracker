
function OnSuccess() {

    $('.modal-backdrop').remove();

    $.confirm({
        title: 'Assigned User',
        content: 'User assigned succesfully',
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






$(document).ready(function () {

    var demo = $('.duallistbox-multi-selection').bootstrapDualListbox({
        nonSelectedListLabel: 'All users',
        selectedListLabel: 'Users in the Project',
        preserveSelectionOnMove: 'moved',
        moveOnSelect: false
    });




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
                $("#cboProject").val('').change();

                $('#cboProject').on("select2:select", function () {

                    $('#idProject').val($(this).val());

                    
                    demo.empty(); 

                    $.ajax({
                        type: 'GET',
                        url: '../../UserProject/AllUsers',
                        data:
                        {
                            "projectId": $(this).val()
                        },
                        success: function (data) {
                            

                            var options = '';

                            for (let value of data) {
                                let $option = $('<option value="' + value.UserId + '" >' + value.UserName + '</option>');
                                demo.append($option);
                            }

                            demo.bootstrapDualListbox('refresh', true);


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


                    $.ajax({
                        type: 'GET',
                        url: '../../UserProject/ProjectUsers',
                        data:
                        {
                            "projectId": $(this).val()
                        },
                        success: function (data) {


                            var options = '';

                            for (let value of data) {
                                let $option = $('<option value="' + value.UserId + '" selected="selected" >' + value.UserName + '</option>');
                                demo.append($option);
                            }

                            demo.bootstrapDualListbox('refresh', true);


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
