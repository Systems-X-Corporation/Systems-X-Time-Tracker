

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

        $.ajax({
            type: 'GET',
            url: '../../Reports/DetailList',
            data:
            {
                "project": $('#cboProject').val(),
                "user": $('#cboUsers').val(),
                "from": $('#from').val(),
                "to": $('#to').val(),
                "records": $('#cboRecords').val()
            },
            success: function (response) {

                $("#AllDataList").html(response);


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


function AdjustSize(s,e) {
    var height = document.getElementById("AllDataList").clientHeight;
    gvAllDataList.SetHeight(height);
    if (height != gvAllDataList.GetHeight()) {
        gvAllDataList.AdjustControl();
    }
}