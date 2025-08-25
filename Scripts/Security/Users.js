


function OnSuccess() {

    $('.modal-backdrop').remove();

    dataTable();

    $.confirm({
        title: 'User Created',
        content: 'User succesfully created',
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
        title: 'User Modified',
        content: 'User succesfully modified',
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


$("#users-list").on("click", ".btnEditUsers", function () {
    
    var idUser = $(this).attr("UsersId");
  
    $.ajax({
        type: 'GET',
        url: '../../Users/GetUsers',
        dataType: 'json',
        data:
        {
            "id": idUser
        },
        success: function (data) {
            console.log(data.role);
            $("#EdUsersId").val(data.UserId);
            $("#eEmail").val(data.Email);
            $("#cboCompanyId").val(data.CompanyId);
            $("#eUserName").val(data.UserName);
            $("#eFirstName").val(data.FirstName);
            $("#eLastName").val(data.LastName);
            $("#eOfficeId").val(data.OfficeId).trigger('change');
            if (data.Active) {
                $("#EActive").prop('checked', true);
            } else {
                $("#EActive").prop('checked', false);
            }
            $("#cboERole").val(data.role).trigger('change');
            
            // Load projects for the user's office
            if (data.OfficeId && data.OfficeId > 0) {
                setTimeout(function() {
                    loadProjectsForOffice(data.OfficeId, 'edit');
                }, 500);
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


$("#users-list").on("click", ".btnDeleteUsers", function () {

    var idUser = $(this).attr("UsersId");
    var UserName = $(this).attr("UsersName");
    LoadingPanel.Show();
    $.confirm({
        title: 'Delete User',
        content: 'Do you want to delete: ' + UserName + '?',
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
                    url: '../../Users/DeleteUsers',
                    dataType: 'json',
                    data:
                    {
                        "id": idUser
                    },
                    success: function (response) {
                        if (response.success) {
                            // Refresh the users list
                            $.ajax({
                                type: 'GET',
                                url: '../../Users/ListUsers',
                                success: function (data) {
                                    $('#container').html(data);
                                    dataTable();
                                    
                                    $.confirm({
                                        title: 'User Deleted',
                                        content: 'User successfully deleted',
                                        type: 'green',
                                        theme: 'material',
                                        closeIcon: true,
                                        animateFromElement: false,
                                        animation: 'left',
                                        closeAnimation: 'right',
                                        buttons: {
                                            Ok: function () {}
                                        }
                                    });
                                }
                            });
                        } else {
                            $.confirm({
                                title: 'Error',
                                content: 'Error deleting user: ' + response.message,
                                type: 'red',
                                theme: 'material',
                                closeIcon: true,
                                buttons: {
                                    Ok: function () {}
                                }
                            });
                        }
                    },
                    timeout: 30 * 60 * 1000
                }).fail(function (xhr, status) {
                    $.confirm({
                        title: 'Error',
                        content: 'Error deleting user: Connection failed',
                        type: 'red',
                        theme: 'material',
                        closeIcon: true,
                        buttons: {
                            Ok: function () {}
                        }
                    });

                    if (status === "timeout") {
                        console.log('Tiempo de respuesta agotado');
                    }
                    if (status === "error") {
                        console.log('La cantidad de datos excede el limite por conexión');
                    }
                });
            },
            Cancel: function () {

            }
        }
    });
    LoadingPanel.Hide();
});



$("#users-list").on("click", ".btnUnlockUsers", function () {

    var idUser = $(this).attr("UsersId");
    var UserName = $(this).attr("UsersName");

    $.confirm({
        title: 'Unlock User',
        content: 'Do you want to unlock: ' + UserName + '?',
        type: 'green',
        theme: 'material',
        closeIcon: true,
        icon: 'fa fa-warning',
        animateFromElement: false,
        animation: 'left',
        closeAnimation: 'right',
        buttons: {
            Unlock: function () {
                $.ajax({
                    type: 'POST',
                    url: '../../Users/UnlockUser',
                    dataType: 'json',
                    data:
                    {
                        "id": idUser
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


$('#btnNewCompany').click(function () {
    $('#container').hide();
    $('#containerNew').show();
});

$('#btnCancel').click(function () {
    $('#container').show();
    $('#containerNew').hide();
});

$(document).ready(function () {
    $('#containerNew').hide();
    
    // Handle office selection change for Create modal
    $(document).on('change', '#OfficeId', function () {
        var officeId = $(this).val();
        if (officeId && officeId > 0) {
            loadProjectsForOffice(officeId, 'create');
        } else {
            $('#projectsPreviewSection').hide();
            $('#projectsPreview').html('<div class="text-muted">Select an office to see available projects</div>');
        }
    });
    
    // Handle office selection change for Edit modal
    $(document).on('change', '#eOfficeId', function () {
        var officeId = $(this).val();
        if (officeId && officeId > 0) {
            loadProjectsForOffice(officeId, 'edit');
        } else {
            $('#editProjectsSection').hide();
            $('#editProjectsList').html('<div class="text-muted">Select an office to see available projects</div>');
        }
    });
    
    // Clear project selections when modals are closed
    $('#modalAddUser').on('hidden.bs.modal', function () {
        $('#projectsPreviewSection').hide();
        $('#projectsPreview').html('<div class="text-muted">Select an office to see available projects</div>');
        $('#SelectedProjects').val('');
        $('#OfficeId').val('').trigger('change');
    });
    
    $('#modalEditUser').on('hidden.bs.modal', function () {
        $('#editProjectsSection').hide();
        $('#editProjectsList').html('<div class="text-muted">Loading projects...</div>');
        $('#SelectedEditProjects').val('');
    });
});

// Function to load projects for a specific office
function loadProjectsForOffice(officeId, mode) {
    $.ajax({
        type: 'GET',
        url: '../../Users/GetProjectsByOffice',
        dataType: 'json',
        data: { officeId: officeId },
        success: function (projects) {
            if (mode === 'create') {
                displayProjectsForCreate(projects);
            } else if (mode === 'edit') {
                displayProjectsForEdit(projects);
            }
        },
        error: function () {
            if (mode === 'create') {
                $('#projectsPreview').html('<div class="text-danger">Error loading projects</div>');
            } else {
                $('#editProjectsList').html('<div class="text-danger">Error loading projects</div>');
            }
        }
    });
}

// Function to display projects in create modal
function displayProjectsForCreate(projects) {
    var html = '';
    var selectedProjects = [];
    
    if (projects.length === 0) {
        html = '<div class="text-muted">No projects found for this office</div>';
    } else {
        projects.forEach(function (project) {
            selectedProjects.push(project.ProjectId);
            html += '<div class="form-check mb-2">' +
                    '<input class="form-check-input project-checkbox" type="checkbox" value="' + project.ProjectId + '" id="project_' + project.ProjectId + '" checked>' +
                    '<label class="form-check-label" for="project_' + project.ProjectId + '">' +
                '<strong>' + project.ProjectName + '</strong> <small class="text-muted">(' + project.CustomerName + ' (' + project.CustomerOffice + '))</small>' +
                    '</label>' +
                    '</div>';
        });
    }
    
    $('#projectsPreview').html(html);
    $('#SelectedProjects').val(selectedProjects.join(','));
    $('#projectsPreviewSection').show();
    
    // Handle checkbox changes
    $(document).on('change', '.project-checkbox', function () {
        updateSelectedProjects('create');
    });
}

// Function to display projects in edit modal
function displayProjectsForEdit(projects) {
    var html = '';
    
    if (projects.length === 0) {
        html = '<div class="text-muted">No projects found for this office</div>';
    } else {
        projects.forEach(function (project) {
            html += '<div class="form-check mb-2">' +
                    '<input class="form-check-input edit-project-checkbox" type="checkbox" value="' + project.ProjectId + '" id="edit_project_' + project.ProjectId + '">' +
                    '<label class="form-check-label" for="edit_project_' + project.ProjectId + '">' +
                '<strong>' + project.ProjectName + '</strong> <small class="text-muted">(' + project.CustomerName + ' (' + project.CustomerOffice + '))</small>' +
                    '</label>' +
                    '</div>';
        });
    }
    
    $('#editProjectsList').html(html);
    $('#editProjectsSection').show();
    
    // Load current user projects and check them
    loadCurrentUserProjects();
    
    // Handle checkbox changes
    $(document).on('change', '.edit-project-checkbox', function () {
        updateSelectedProjects('edit');
    });
}

// Function to load current user projects for edit modal
function loadCurrentUserProjects() {
    var userId = $('#EdUsersId').val();
    if (userId) {
        $.ajax({
            type: 'GET',
            url: '../../Users/GetUserProjects',
            dataType: 'json',
            data: { userId: userId },
            success: function (userProjects) {
                var selectedProjects = [];
                userProjects.forEach(function (project) {
                    $('#edit_project_' + project.ProjectId).prop('checked', true);
                    selectedProjects.push(project.ProjectId);
                });
                $('#SelectedEditProjects').val(selectedProjects.join(','));
            }
        });
    }
}

// Function to update selected projects
function updateSelectedProjects(mode) {
    var selectedProjects = [];
    var checkboxClass = mode === 'create' ? '.project-checkbox' : '.edit-project-checkbox';
    var inputField = mode === 'create' ? '#SelectedProjects' : '#SelectedEditProjects';
    
    $(checkboxClass + ':checked').each(function () {
        selectedProjects.push($(this).val());
    });
    
    $(inputField).val(selectedProjects.join(','));
}