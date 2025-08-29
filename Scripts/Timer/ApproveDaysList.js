

$(document).ready(function () {

    // Initialize with all pending approvals
    showAllPendingApprovals();

    // Initialize dropdowns
    $("#cboCustomer").val('').change();
    $("#cboUsers").val('').change();

    // Quick Actions Event Handlers
    $("#btnShowAllPending").on("click", function () {
        showAllPendingApprovals();
    });

    $("#btnShowToday").on("click", function () {
        showTodaysPending();
    });

    $("#btnShowThisWeek").on("click", function () {
        showThisWeeksPending();
    });

    $("#btnGetReport").on("click", function () {
        generatePendingApprovalsReport();
    });

    $("#btnClearFilters").on("click", function () {
        clearAllFilters();
    });

    // Quick Filter change handler
    $("#cboQuickFilter").on("change", function () {
        var filterType = $(this).val();
        if (filterType === "custom") {
            $("#customDateRange").show();
        } else {
            $("#customDateRange").hide();
            if (filterType === "day") {
                showTodaysPending();
            } else if (filterType === "week") {
                showThisWeeksPending();
            } else if (filterType === "lastweek") {
                showLastWeeksPending();
            }
        }
    });

    $('#cboCustomer').on("select2:select", function () {
        $.ajax({
            type: 'GET',
            url: '../../System/GetCboProject',
            data: {
                "CustomerId": $(this).val()
            },
            success: function (response) {
                $("#CboProjectContent").html(response);
                $("#cboProject").select2();
                $("#cboProject").val('').change();
                
                // Auto-update table when customer changes
                buscar();
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

    // Auto-update when customer is cleared
    $('#cboCustomer').on("select2:clear", function () {
        $("#CboProjectContent").html('<select class="custom-select2 form-control" id="cboProject" name="cboProject"><option value="">Select Project...</option></select>');
        $("#cboProject").select2();
        buscar();
    });

    $("#btnSearch").on("click", function () {
        buscar();
    });

    // Auto-update when project changes
    $(document).on('change', '#cboProject', function () {
        buscar();
    });

    // Auto-update when user changes
    $('#cboUsers').on("select2:select", function () {
        buscar();
    });

    $('#cboUsers').on("select2:clear", function () {
        buscar();
    });

    $('#cboUsers').on("change", function () {
        buscar();
    });

    // Auto-update when status changes
    $('#cboRecords').on("select2:select select2:clear", function () {
        buscar();
    });

    // Auto-update when date inputs change
    $('#from, #to').on('change', function () {
        if ($('#cboQuickFilter').val() === 'custom') {
            var fromVal = $('#from').val();
            var toVal = $('#to').val();
            console.log('Date filter changed - From:', fromVal, 'To:', toVal, 'FilterType: custom');
            buscar();
        }
    });

    function buscar() {
        var filterType = $("#cboQuickFilter").val();
        var customerVal = $('#cboCustomer').val();
        var projectVal = $('#cboProject').val();
        var userVal = $('#cboUsers').val();
        // Handle multiple select - convert array to comma-separated string
        if (Array.isArray(userVal) && userVal.length > 0) {
            userVal = userVal.join(','); // Join multiple users with comma
        } else if (Array.isArray(userVal) && userVal.length === 0) {
            userVal = ''; // No selection
        }
        var recordsVal = $('#cboRecords').val();
        var fromVal = $('#from').val();
        var toVal = $('#to').val();
        
        // Debug: Log all values being sent to controller
        console.log('=== BUSCAR DEBUG ===');
        console.log('User filter value:', userVal);
        console.log('Date values - From:', fromVal, 'To:', toVal);
        console.log('All filter values:', {
            customer: customerVal,
            project: projectVal,
            user: userVal,
            records: recordsVal,
            filterType: filterType,
            from: fromVal,
            to: toVal
        });
        console.log('==================');
        
        // Show loading indicator
        $("#ApproveList").html('<div class="text-center p-4"><i class="fa fa-spinner fa-spin"></i> Loading...</div>');
        
        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/DaysList',
            data: {
                "customer": customerVal,
                "project": projectVal,
                "user": userVal,
                "from": fromVal,
                "to": toVal,
                "records": recordsVal,
                "filterType": filterType
            },
            success: function (response) {
                $("#ApproveList").html(response);
                
                // Show summary of applied filters
                var filtersApplied = [];
                if (customerVal) filtersApplied.push("Client");
                if (projectVal) filtersApplied.push("Project");
                if (userVal) {
                    var userCount = userVal.split(',').length;
                    filtersApplied.push("User" + (userCount > 1 ? "s (" + userCount + ")" : ""));
                }
                if (recordsVal) filtersApplied.push("Status: " + recordsVal);
                if (filterType) filtersApplied.push("Quick Filter: " + filterType);
                if (fromVal || toVal) filtersApplied.push("Date Range");
                
                if (filtersApplied.length > 0) {
                    $("#summaryContent").html('<strong>Active Filters:</strong> ' + filtersApplied.join(', '));
                    $("#summarySection").show();
                } else {
                    $("#summarySection").hide();
                }
            },
            timeout: 30 * 60 * 1000
        }).fail(function (xhr, status) {
            $("#ApproveList").html('<div class="alert alert-danger">Error loading data. Please try again.</div>');
            
            if (status === "timeout") {
                console.log('Tiempo de respuesta agotado');
            }
            if (status === "error") {
                console.log('La cantidad de datos excede el limite por conexión');
            }
        });
    }

    function showAllPendingApprovals() {
        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/GetAllPendingApprovals',
            success: function (response) {
                $("#ApproveList").html(response);
                $("#summaryContent").html('<strong>Showing all pending approval records</strong> - No filters applied');
                $("#summarySection").show();
                
                // Clear filters
                $("#cboCustomer").val('').trigger('change');
                $("#cboProject").val('').trigger('change');
                $("#cboUsers").val('').trigger('change');
                $("#cboRecords").val('').trigger('change');
                $("#cboQuickFilter").val('').trigger('change');
            },
            error: function () {
                alert('Error loading pending approvals');
            }
        });
    }

    function showTodaysPending() {
        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/GetTodaysPendingApprovals',
            dataType: 'json',
            success: function (data) {
                if (data.error) {
                    alert('Error: ' + data.error);
                    return;
                }
                
                $("#summaryContent").html(
                    '<strong>Today\'s Pending Approvals (' + data.Date + ')</strong><br>' +
                    'Total Records: <span class="badge badge-warning">' + data.Count + '</span>'
                );
                $("#summarySection").show();
                
                // Load the actual data
                loadDataWithFilter('day', data.Date);
            },
            error: function () {
                alert('Error loading today\'s pending approvals');
            }
        });
    }

    function showThisWeeksPending() {
        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/GetWeeksPendingApprovals',
            dataType: 'json',
            success: function (data) {
                if (data.error) {
                    alert('Error: ' + data.error);
                    return;
                }
                
                var summaryHtml = '<strong>This Week\'s Pending Approvals (' + data.WeekStart + ' to ' + data.WeekEnd + ')</strong><br>' +
                                'Total Records: <span class="badge badge-warning">' + data.Count + '</span><br>' +
                                'Daily Breakdown: ';
                
                data.DailySummary.forEach(function(day) {
                    summaryHtml += '<span class="badge badge-info mr-1">' + day.Date + ': ' + day.Count + '</span>';
                });
                
                $("#summaryContent").html(summaryHtml);
                $("#summarySection").show();
                
                // Load the actual data
                loadDataWithFilter('week', data.WeekStart);
            },
            error: function () {
                alert('Error loading this week\'s pending approvals');
            }
        });
    }

    function showLastWeeksPending() {
        // Calculate last week's start date (7 days before this week)
        var today = new Date();
        var thisWeekStart = new Date(today);
        thisWeekStart.setDate(today.getDate() - today.getDay()); // This week's Sunday
        var lastWeekStart = new Date(thisWeekStart);
        lastWeekStart.setDate(thisWeekStart.getDate() - 7); // Last week's Sunday
        
        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/GetWeeksPendingApprovals',
            dataType: 'json',
            data: {
                "startDate": lastWeekStart.toISOString()
            },
            success: function (data) {
                if (data.error) {
                    alert('Error: ' + data.error);
                    return;
                }
                
                var summaryHtml = '<strong>Last Week\'s Pending Approvals (' + data.WeekStart + ' to ' + data.WeekEnd + ')</strong><br>' +
                                'Total Records: <span class="badge badge-warning">' + data.Count + '</span><br>' +
                                'Daily Breakdown: ';
                
                data.DailySummary.forEach(function(day) {
                    summaryHtml += '<span class="badge badge-info mr-1">' + day.Date + ': ' + day.Count + '</span>';
                });
                
                $("#summaryContent").html(summaryHtml);
                $("#summarySection").show();
                
                // Load the actual data
                loadDataWithFilter('lastweek', data.WeekStart);
            },
            error: function () {
                alert('Error loading last week\'s pending approvals');
            }
        });
    }

    function generatePendingApprovalsReport() {
        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/GetPendingApprovalsReport',
            dataType: 'json',
            success: function (data) {
                if (data.error) {
                    alert('Error: ' + data.error);
                    return;
                }
                
                var reportHtml = '<div class="row">' +
                    '<div class="col-md-4">' +
                    '<h6 class="mb-2">Overall Summary</h6>' +
                    '<ul class="list-unstyled">' +
                    '<li><strong>Total Pending:</strong> <span class="badge badge-danger">' + data.TotalPending + '</span></li>' +
                    '<li><strong>Sent:</strong> <span class="badge badge-warning">' + data.SentCount + '</span></li>' +
                    '<li><strong>Under Review:</strong> <span class="badge badge-info">' + data.UnderReviewCount + '</span></li>' +
                    '<li><strong>Oldest Record:</strong> ' + (data.OldestPending ? formatDate(data.OldestPending) : 'N/A') + '</li>' +
                    '</ul>' +
                    '</div>' +
                    '<div class="col-md-4">' +
                    '<h6>By User</h6>' +
                    '<div style="max-height: 200px; overflow-y: auto;">';
                
                data.UsersSummary.forEach(function(user) {
                    reportHtml += '<div class="mb-1"><strong>' + user.UserName + ':</strong> ' +
                        '<span class="badge badge-secondary">' + user.PendingCount + '</span> ' +
                        '<small>(Oldest: ' + formatDate(user.OldestDate) + ')</small></div>';
                });
                
                reportHtml += '</div></div>' +
                    '<div class="col-md-4">' +
                    '<h6>By Project</h6>' +
                    '<div style="max-height: 200px; overflow-y: auto;">';
                
                data.ProjectsSummary.forEach(function(project) {
                    reportHtml += '<div class="mb-1"><strong>' + project.ProjectName + '</strong> (' + project.CustomerName + '): ' +
                        '<span class="badge badge-secondary">' + project.PendingCount + '</span></div>';
                });
                
                reportHtml += '</div></div></div>';
                
                $("#summaryContent").html(reportHtml);
                $("#summarySection").show();
            },
            error: function () {
                alert('Error generating report');
            }
        });
    }

    function loadDataWithFilter(filterType, dateValue) {
        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/DaysList',
            data: {
                "customer": 0,
                "project": 0,
                "user": "",
                "from": dateValue,
                "to": '',
                "records": "",
                "filterType": filterType
            },
            success: function (response) {
                $("#ApproveList").html(response);
            },
            error: function () {
                alert('Error loading filtered data');
            }
        });
    }

    function clearAllFilters() {
        // Clear all select2 dropdowns
        $("#cboCustomer").val('').trigger('change');
        $("#cboProject").val('').trigger('change');
        $("#cboUsers").val('').trigger('change');
        $("#cboRecords").val('').trigger('change');
        $("#cboQuickFilter").val('').trigger('change');
        
        // Clear date inputs
        $("#from").val('');
        $("#to").val('');
        
        // Hide custom date range
        $("#customDateRange").hide();
        
        // Show all pending approvals (default view)
        showAllPendingApprovals();
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

    // Helper function to format dates properly
    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        
        try {
            // Now dates come as ISO strings (yyyy-MM-dd) from the server
            if (typeof dateString === 'string') {
                // If it's already in a readable format (yyyy-MM-dd), just convert to MM/dd/yyyy
                if (dateString.match(/^\d{4}-\d{2}-\d{2}$/)) {
                    var parts = dateString.split('-');
                    return parts[1] + '/' + parts[2] + '/' + parts[0];
                }
                
                // Try parsing as date
                var date = new Date(dateString);
                if (!isNaN(date.getTime())) {
                    return date.toLocaleDateString();
                }
            }
            
            return 'Invalid Date';
        } catch (e) {
            console.log('Error formatting date:', dateString, e);
            return 'Invalid Date';
        }
    }

});


