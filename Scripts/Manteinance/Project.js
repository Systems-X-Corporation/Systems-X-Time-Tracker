

$(document).ready(function () {

    $('#projects thead tr')
        .clone(true)
        .addClass('filters')
        .appendTo('#projects thead');

    var table = $('#projects').DataTable({
        order: [[1, 'asc']],
        orderCellsTop: true,
        fixedHeader: true,
        initComplete: function () {
            var api = this.api();

            // For each column
            api
                .columns()
                .eq(0)
                .each(function (colIdx) {
                    // Set the header cell to contain the input element
                    var cell = $('.filters th').eq(
                        $(api.column(colIdx).header()).index()
                    );
                    var title = $(cell).text();
                    $(cell).html('<input type="text" placeholder="' + title + '" />');

                    // On every keypress in this input
                    $(
                        'input',
                        $('.filters th').eq($(api.column(colIdx).header()).index())
                    )
                        .off('keyup change')
                        .on('change', function (e) {
                            // Get the search value
                            $(this).attr('title', $(this).val());
                            var regexr = '({search})'; //$(this).parents('th').find('select').val();

                            var cursorPosition = this.selectionStart;
                            // Search the column for that value
                            api
                                .column(colIdx)
                                .search(
                                    this.value != ''
                                        ? regexr.replace('{search}', '(((' + this.value + ')))')
                                        : '',
                                    this.value != '',
                                    this.value == ''
                                )
                                .draw();
                        })
                        .on('keyup', function (e) {
                            e.stopPropagation();

                            $(this).trigger('change');
                            $(this)
                                .focus()[0]
                                .setSelectionRange(cursorPosition, cursorPosition);
                        });
                });
        },
    });



});



function OnSuccess() {

    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'Project Created',
        content: 'Project succesfully created',
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
        title: 'Project Modified',
        content: 'Project succesfully modified',
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


$("#projects").on("click", ".btnEditProject", function () {

    var idProject = $(this).attr("ProjectId");


    $.ajax({
        type: 'POST',
        url: '../../Projects/GetProject',
        dataType: 'json',
        data:
        {
            "id": idProject
        },
        success: function (Project) {
            console.log(Project.pms);
            $("#EdProjectId").val(Project.ProjectId);
            $("#EdProjectName").val(Project.ProjectName);
            $("#cboECustomer").val(Project.CustomerId);
            $("#ecolor").asColorPicker('val', Project.color);
            $("#cboEPM").val(Project.pms).trigger('change');
            $("#cboEUsers").val(Project.users).trigger('change');
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


$("#projects").on("click", ".btnDeleteProject", function () {

    var idProject = $(this).attr("ProjectId");
    var projectName = $(this).attr("ProjectName");


    $.ajax({
        type: 'POST',
        url: '../../Projects/CanDeleteProject',
        dataType: 'json',
        data:
        {
            "id": idProject
        },
        success: function (response) {
            console.log(response);
            if (response) {
                $.confirm({
                    title: 'Delete Project',
                    content: 'Cannot delete project, because it contains assigned work. You must first delete the work',
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
                    title: 'Delete Project',
                    content: 'Do you want to delete the project: ' + projectName + '?',
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
                                url: '../../Projects/DeleteProject',
                                dataType: 'json',
                                data:
                                {
                                    "id": idProject
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
                           
                        },
                        Cancel: function () {
                            location.reload();
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