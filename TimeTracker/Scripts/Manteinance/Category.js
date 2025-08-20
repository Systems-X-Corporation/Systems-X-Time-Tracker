


function OnSuccess() {

    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'Category Created',
        content: 'Category succesfully created',
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
        title: 'Category Modified',
        content: 'Category succesfully modified ',
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


$("#category").on("click", ".btnEditCategory", function () {

    var idCategory = $(this).attr("CategoryId");


    $.ajax({
        type: 'POST',
        url: '../../Category/GetCategory',
        dataType: 'json',
        data:
        {
            "id": idCategory
        },
        success: function (Category) {

            $("#EdCategoryId").val(Category.CategoryId);
            $("#EdCategoryName").val(Category.CategoryName);


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


$("#category").on("click", ".btnDeleteCategory", function () {

    var idCategory = $(this).attr("CategoryId");
    var categoryName = $(this).attr("CategoryName");

    $.confirm({
        title: 'Delete Category',
        content: 'Do you want to delete: ' + categoryName + '?',
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
                    url: '../../Category/DeleteCategory',
                    dataType: 'json',
                    data:
                    {
                        "id": idCategory
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