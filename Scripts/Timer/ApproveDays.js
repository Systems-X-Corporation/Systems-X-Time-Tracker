
$(document).ready(function () {

    $('#btnApprove').hide();
    $('#btnDismiss').hide();
    var calendar;

    $("#cboUsers").val('').change();


    $('#cboUsers').on("select2:select", function () {
        var initDate;
        $('#btnApprove').hide();
        $('#btnDismiss').hide();

        if ($('#calendarList').children().length > 0) {
            calendar.destroy();
        }

        
        $.ajax({
            type: 'GET',
            url: '../../System/GetCboSendDays',
            data:
            {
                "id": $(this).val()
            },
            success: function (response) {

                $("#cboDatesContent").html(response);
                $("#cboDates").select2();
                $("#cboDates").val('').change();


                $('#cboDates').on("change", function (e) {

                    initDate = $(this).val();
                    $.ajax({
                        type: 'GET',
                        url: '../../ApproveDays/GetDayData',
                        data:
                        {
                            "date": $(this).val(),
                            "user": $('#cboUsers').val()
                        },
                        success: function (response) {

                            $('#btnApprove').show();
                            $('#btnDismiss').show();

                            var buildingEvents = $.map(response, function (item) {
                                return {
                                    id: item.id,
                                    title: item.title,
                                    start: item.start,
                                    end: item.end,
                                    allDay: false,
                                    color: item.color,
                                    daystatus: item.daystatus,
                                    approved: item.approved
                                };
                            });

                            var calendarEl = document.getElementById('calendarList');

                            var newDate = new Date(initDate);

                            calendar = new FullCalendar.Calendar(calendarEl, {
                                headerToolbar: {
                                    left: '',
                                    center: 'title',
                                    right: ''
                                },
                                initialDate: newDate,
                                slotDuration: '00:15:00',
                                slotLabelInterval: "01:00",
                                showNonCurrentDates: true,
                                businessHours: true,
                                businessHours:
                                {
                                    daysOfWeek: [1, 2, 3, 4, 5], // Monday - Thursday

                                    startTime: '07:00', // a start time (10am in this example)
                                    endTime: '19:00',
                                },

                                initialView: 'listDay',
                                nowIndicator: true,


                                selectable: true,
                                selectMirror: true,
                               
                                eventClick: function (arg) {
                                    
                                    var date = arg.event.start;
                                    var currentDate = date.toISOString().substring(0, 10);
                                    var currentTime = date.toTimeString().substring(0, 8);
                                    var enddate = arg.event.end;
                                    var currentEndTime = enddate.toTimeString().substring(0, 8);
                                    selectedEvent = arg.event.id;


                                    $.ajax({
                                        type: 'GET',
                                        url: '../../Time/GetHourData',
                                        data:
                                        {
                                            "id": arg.event.id,
                                            "title": arg.event.title,
                                            "start": currentTime,
                                            "end": currentEndTime,
                                            "date": currentDate
                                        },
                                        success: function (response) {

                                            jQuery("#modal-view-event-add").modal();

                                            $("#cboCategory").val(response.category);
                                            $('#cboCategory').trigger('change.select2');

                                            $('#cboCategory').select2({
                                                disabled: true
                                            });

                                            $("#cboProject").val(response.project);
                                            $('#cboProject').trigger('change.select2');
                                            $('#cboProject').select2({
                                                disabled: true
                                            });
                                            $.ajax({
                                                type: 'GET',
                                                url: '../../System/GetCboActivity',
                                                data:
                                                {
                                                    "ProjectId": $("#cboProject").val()
                                                },
                                                success: function (response2) {

                                                    $("#CboActivityContent").html(response2);
                                                    $("#cboActivity").select2();
                                                    $("#cboActivity").val(response.activity);
                                                    $('#cboActivity').trigger('change.select2');
                                                    $('#cboActivity').select2({
                                                        disabled: true
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


                                            if (response.Billable) {
                                                $("#Billable").prop('checked', true);
                                            } else {
                                                $("#Billable").prop('checked', false);
                                            }
                                            $("#Billable").prop('readonly', 'readonly');
                                            document.getElementById('starttime').value = currentTime;
                                            document.getElementById('endtime').value = currentEndTime;
                                            document.getElementById('description').value = response.ActDescription;

                                            document.getElementById('TimeHoursId').value = selectedEvent;

                                            jQuery("#date").val(currentDate);

                                            calendar.unselect()
                                            
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
                                editable: false,
                                eventSources: [{
                                    events: buildingEvents,
                                }
                                ]
                            });

                            calendar.render();

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

    $('#btnDismiss').on('click', function () {


        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/DismissDay',
            data:
            {
                "User": $('#cboUsers').val(),
                "Date": $('#cboDates').val()
            },
            success: function (response) {

                if (response.msg == "Ok") {
                    $.confirm({
                        title: 'Dismmis Day',
                        content: 'Day succesfully dismiss',
                        type: 'blue',
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
                } else {
                    $.confirm({
                        title: 'Dismiss Day',
                        content: response.msg,
                        type: 'blue',
                        theme: 'orange',
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

    $('#btnApprove').on('click', function () {

        $.ajax({
            type: 'GET',
            url: '../../ApproveDays/ApproveDay',
            data:
            {
                "User": $('#cboUsers').val(),
                "Date": $('#cboDates').val()
            },
            success: function (response) {

                if (response.msg == "Ok") {
                    $.confirm({
                        title: 'Approve Day',
                        content: 'Day succesfully approved',
                        type: 'blue',
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
                } else {
                    $.confirm({
                        title: 'Approve Day',
                        content: response.msg,
                        type: 'blue',
                        theme: 'orange',
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

