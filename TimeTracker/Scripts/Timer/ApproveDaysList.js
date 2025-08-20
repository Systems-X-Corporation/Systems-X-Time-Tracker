

$(document).ready(function () {


    $("#cboCustomer").val('').change();
    
    $("#cboUsers").val('').change();

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

    $("#btnSearch").on("click", function () {

        buscar();

    });

    function buscar() {

        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/DaysList',
            data:
            {
                "customer": $('#cboCustomer').val(),
                "project": $('#cboProject').val(),
                "user": $('#cboUsers').val(),
                "from": $('#from').val(),
                "to": $('#to').val(),
                "records": $('#cboRecords').val()
            },
            success: function (response) {

                $("#ApproveList").html(response);


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

    $('#month').on("change", function () {

        var month = $(this).val();

        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/DaysList',
            data:
            {
                "month": $(this).val(),
                "user": $('#cboUsers').val()
            },
            success: function (response) {
                console.log(response);
                $("#ApproveListCont").html(response);
                
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


