


function OnSuccess() {

    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'Client Created',
        content: 'Client succesfully created',
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
        title: 'Client Modified',
        content: 'Client succesfully modified',
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


$("#customer").on("click", ".btnEditCustomer", function () {

    var idCustomer = $(this).attr("CustomerId");


    $.ajax({
        type: 'POST',
        url: '../../Customer/GetCustomer',
        dataType: 'json',
        data:
        {
            "id": idCustomer
        },
        success: function (Customer) {

            $("#EdCustomerId").val(Customer.CustomerId);
            $("#EdCustomerName").val(Customer.CustomerName);
            $('#eOfficeId').val(Customer.OfficeId).trigger('change'); 

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


$("#customer").on("click", ".btnDeleteCustomer", function () {
    
    var idCustomer = $(this).attr("CustomerId");
    var customerName = $(this).attr("CustomerName");

    $.ajax({
        type: 'POST',
        url: '../../Customer/CanDeleteCustomer',
        dataType: 'json',
        data:
        {
            "id": idCustomer
        },
        success: function (response) {
            console.log(response);
            if (response) {
                $.confirm({
                    title: 'Delete Client',
                    content: 'Cannot delete client, because it contains assigned projects. You must first delete the projects',
                    type: 'orange',
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
            else {
                $.confirm({
                    title: 'Delete Client',
                    content: 'Do you want to delete: ' + customerName + '?',
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
                                url: '../../Customer/DeleteCustomer',
                                dataType: 'json',
                                data:
                                {
                                    "id": idCustomer
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