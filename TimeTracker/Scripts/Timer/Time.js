$("document").ready(function () {

    $('#cboActivity').val(-1).trigger('change');


    deleteRow();

    $("#week").on('change', function () {
        $.ajax({
            type: 'GET',
            url: '../../Time/WeekTimeList',
            data:
            {
                "week": $(this).val()
            },
            success: function (response) {

                $("#ListContainer").html(response);
           
                deleteRow();
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

    
})



function deleteRow() {
    $("#week-list").on("click", ".btnDelete", function () {

        var idUser = $(this).attr("UsersId");
        var UserName = $(this).attr("UsersName");

        $.confirm({
            title: 'Delete Row',
            content: 'Do you want to delete the row',
            type: 'orange',
            theme: 'material',
            closeIcon: true,
            icon: 'fa fa-warning',
            animateFromElement: false,
            animation: 'left',
            closeAnimation: 'right',
            buttons: {
                Delete: function () {
                    var row = $('.btnDelete').closest("TR");
                    var name = $("TD", row).eq(0).html();
                    var table = $("#week-list")[0];
                    table.deleteRow(row[0].rowIndex);
                },
                Cancel: function () {

                }
            }
        });
    });
};


    $("#addrow").on('click', function () {
        var cboActivity;
        var cboHours;

        $.ajax({
            type: 'GET',
            url: '../../System/GetCboAllActivity',
            success: function (response) {
                cboActivity = response;
                $.ajax({
                    type: 'GET',
                    url: '../../System/GetCboHours',
                    success: function (response) {
                        cboHours = response;
                        $('#week-list tbody tr').last().after('<tr><td>' + cboActivity + '</td><td><input type="text" name="name" value="" /></td><td>' + cboHours + '</td><td>' + cboHours + '</td><td>' + cboHours + '</td><td>' + cboHours + '</td><td>' + cboHours + '</td><td> <a class="dropdown-item btnDelete" href="#"><i class="dw dw-delete-3"></i></a></td></tr>');
                        $("#cboActivity").select2();
                        
                    }
                });
            }
        });

    });


$("#save").on('click', function () {


    var hours = new Array();

    $("#week-list TBODY TR").each(function () {
        var row = $(this);
        var hour = {};
        hour.activity = row.find("TD").eq(0).find('select').val();
        hour.description = row.find("TD").eq(1).find('input').val();
        hour.mon = row.find("TD").eq(2).find('select').val();
        hour.tue = row.find("TD").eq(3).find('select').val();
        hour.wed = row.find("TD").eq(4).find('select').val();
        hour.thu = row.find("TD").eq(5).find('select').val();
        hour.fri = row.find("TD").eq(6).find('select').val();
        hour.week = $("#week").val();
        hours.push(hour);
    });
    console.log(hours);
    
    $.ajax({
        type: "POST",
        url: "../../Time/InsertHours",
        contentType: 'application/json; charset=utf-8',
        datatype: 'json',
        data:JSON.stringify(hours),
        success: function (r) {
            alert(" record(s) inserted.");
            $("#ListContainer").html(response);
            addRow();
            deleteRow();
        }
    });


});

$("#approval").on('click', function () {
    console.log("appr");
});