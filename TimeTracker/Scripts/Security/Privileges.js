$(document).ready(function () {

    $('#cboRoleId').val(-1).trigger('change');


    $('#cboRoleId').on("select2:select", function () {
        
        $.ajax({
            type: "GET",
            url: "../../Privileges/GetPrivileges",
            data: {
                id: $(this).val()
            },
            success: function (response) {
                $("#Treecontainer").html(response);
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



function onSelectionChanged(s, e) {

    s.GetSelectedNodeValues("PrivilegeId", GetSelectedNodeValuesCallback);  
}

function GetSelectedNodeValuesCallback(values) {

    console.log(values);
    var privilege = values;
    $.ajax({
        type: "POST",
        url: "Privileges/AsignPrivilegeRole",
        data: {
            role: $("#cboRoleId").val(),
            privilege: privilege
        }
    });
}

function OnInitTreelistUsuarios(s, e) {
    AdjustSizeTreelistUsuarios();
}

function AdjustSizeTreelistUsuarios() {
    var height = document.getElementById("Treecontainer").clientHeight;
    
    tlPrivilegios.SetHeight(height);
    if (height != tlPrivilegios.GetHeight()) {
        tlPrivilegios.AdjustControl();
    }
}


