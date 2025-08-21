$(document).ready(function () {
    $("#data_form").submit(function (e) {
        e.preventDefault();
    });

    var calendar;
    var selectedEvent;

    $("#cboProject").val('').change();
    //$("#cboCustomer").val('').change();

    $("#time").on('keydown', function (e) {
        var keyCode = e.keyCode || e.which;

        if (keyCode == 9) {
            e.preventDefault();

            $('#cboProject').select2('open');
        }
    });

    $("#description").on('keydown', function (e) {
        var keyCode = e.keyCode || e.which;

        if (keyCode == 9) {
            e.preventDefault();

            $('#cboCategory').select2('open');
        }
    });

    //function select2settings() {
    //    $('#cboCustomer').on("select2:select", function () {
    //        $.ajax({
    //            type: 'GET',
    //            url: '../../System/GetCboProject',
    //            data:
    //            {
    //                "CustomerId": $(this).val()
    //            },
    //            success: function (response) {
    //                console.log(response);
    //                $("#CboProjectContent").html(response);

    //            },
    //            timeout: 30 * 60 * 1000
    //        }).fail(function (xhr, status) {
    //            if (status === "timeout") {
    //                console.log('Tiempo de respuesta agotado');
    //            }
    //            if (status === "error") {
    //                console.log('La cantidad de datos excede el limite por conexión');
    //            }
    //        });

    //    });

    //}

    //select2settings();

    $('#btnSave').on('click', function () {
        $.ajax({
            type: 'GET',
            url: '../../Time/SaveHours',
            data:
            {
                "id": $('#TimeHoursId').val(),
                "date": $('#date').val(),
                "start": $('#starttime').val(),
                "end": $('#endtime').val(),
                "projectId": $('#cboProject').val(),
                //"activityId": $('#cboActivity').val(),
                "categoryId": $('#cboCategory').val(),
                "description": $('#description').val(),
                "billable": $('#Billable').is(":checked"),
                "Thours": $('#time').val(),
                "InternalNotes": $('#InternalNotes').val()
            },
            success: function (response) {
                var event = calendar.getEventById(response.id);

                if (event != null) {
                    event.remove();
                }

                calendar.addEvent({
                    id: response.id,
                    title: response.project + ' ~ ' + $('#description').val(),
                    description: response.project + ' ~ ' + $('#description').val(),
                    start: $('#date').val() + ' ' + $('#starttime').val(),
                    end: $('#date').val() + ' ' + $('#endtime').val(),
                    allDay: false,
                    color: response.color
                })

                jQuery("#modal-view-event-add").modal('hide');

                $('#cboProject').val('').change();
                $('#cboCustomer').val('').change();
                //$('#cboActivity').val('').change();
                $('#cboCategory').val('').change();
                $('#description').val('');
                $('#InternalNotes').val('');
                $("#Billable").prop('checked', true);

                var view = calendar.currentData.currentViewType;
                var date = calendar.currentData.dateProfile.currentDate.toISOString().substring(0, 10);
                DailyViews(date, view);
            },
            timeout: 30 * 60 * 1000
        }).fail(function (xhr, status) {
            console.log(xhr);
            console.log(status);

            if (status === "timeout") {
                console.log('Tiempo de respuesta agotado');
            }
            if (status === "error") {
                console.log('La cantidad de datos excede el limite por conexión');
            }
        });
    });

    $('#btnDelete').on('click', function () {
        $.ajax({
            type: 'GET',
            url: '../../Time/DeleteHours',
            data:
            {
                "id": selectedEvent,
                "date": $('#date').val(),
                "start": $('#starttime').val(),
                "end": $('#endtime').val(),
                "projectId": $('#cboProject').val(),
                //"activityId": $('#cboActivity').val(),
                "description": $('#description').val(),
                "billable": $('#Billable').is(":checked")
            },
            success: function (response) {
                var event = calendar.getEventById(selectedEvent);
                event.remove();

                jQuery("#modal-view-event-add").modal('hide');

                $('#cboProject').val('').change();
                $('#cboCustomer').val('').change();
                //$('#cboActivity').val('').change();

                $('#description').val('');

                var view = calendar.currentData.currentViewType;
                var date = calendar.currentData.dateProfile.currentDate.toISOString().substring(0, 10);
                DailyViews(date, view);
            },
            timeout: 30 * 60 * 1000
        }).fail(function (xhr, status) {
            console.log(xhr);
            console.log(status);

            if (status === "timeout") {
                console.log('Tiempo de respuesta agotado');
            }
            if (status === "error") {
                console.log('La cantidad de datos excede el limite por conexión');
            }
        });
    });

    $('#starttime').on('change', function () {
        var msIn1Hour = 3600 * 1000;
        var start = new Date('2022-01-01T' + $('#starttime').val());
        var end = new Date('2022-01-01T' + $('#endtime').val());

        $('#time').val((end - start) / msIn1Hour);
    });

    $('#endtime').on('change', function () {
        var msIn1Hour = 3600 * 1000;
        var start = new Date('2022-01-01T' + $('#starttime').val());
        var end = new Date('2022-01-01T' + $('#endtime').val());

        $('#time').val((end - start) / msIn1Hour);
    });

    $('#time').on('change', function () {
        var msIn1Hour = 3600 * 1000;

        var start = new Date('2022-01-01T' + $('#starttime').val());
        //var end = new Date('2022-01-01T' + $('#endtime').val());
        var end = new Date(start.getTime() + $('#time').val() * 60 * 60 * 1000);
        console.log(end.toTimeString().substring(0, 8));
        $('#endtime').val(end.toTimeString().substring(0, 8));
    });

    function loadData() {
        $.ajax({
            type: 'GET',
            url: '../../Time/GetHours',

            success: function (response) {
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

                var currentdate;
                var calendarEl = document.getElementById('calendar');

                calendar = new FullCalendar.Calendar(calendarEl, {
                    headerToolbar: {
                        left: 'prev,next today',
                        center: 'title',
                        right: 'dayGridMonth,timeGridWeek,timeGridDay'
                    },
                    /*initialDate: '2023-01-12',*/
                    slotMinTime: "06:00:00",
                    slotMaxTime: "20:00:00",
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

                    allDaySlot: false,
                    initialView: 'timeGridWeek',
                    nowIndicator: true,

                    datesSet: function (dateInfo) {
                        currentdate = dateInfo.startStr;
                        var view = calendar.view.type;

                        DailyViews(currentdate, view)
                    },

                    navLinks: true, // can click day/week names to navigate views
                    selectable: true,
                    selectMirror: true,
                    //selectAllow: function (arg) {
                    //},
                    select: function (arg) {
                        var msIn1Hour = 3600 * 1000;

                        var date = arg.start;
                        var currentDate = date.toISOString().substring(0, 10);
                        var currentTime = date.toTimeString().substring(0, 8);
                        var enddate = arg.end;
                        var currentEndTime = enddate.toTimeString().substring(0, 8);

                        $.ajax({
                            type: 'GET',
                            url: '../../Time/IsDaySent',
                            data:
                            {
                                "date": currentDate
                            },
                            success: function (data) {
                                if (!data) {
                                    jQuery("#modal-view-event-add").modal();
                                    $("#Billable").prop('checked', true);
                                    document.getElementById('starttime').value = currentTime;
                                    document.getElementById('endtime').value = currentEndTime;
                                    var Thours = ((enddate - date) / msIn1Hour);
                                    document.getElementById('time').value = Thours;
                                    jQuery("#date").val(currentDate);
                                    $('#TimeHoursId').val(arg.id);
                                    $("#btnDelete").show();
                                    $('#btnSave').show();
                                    $("#date").prop("readonly", false);
                                    $('#cboCategory').select2({
                                        disabled: false
                                    });
                                    $('#cboProject').select2({
                                        disabled: false
                                    });
                                    $('#cboCustomer').select2({
                                        disabled: false
                                    });
                                    //$('#cboActivity').select2({
                                    //    disabled: false
                                    //});
                                    $("#cboCategory").val('').change();
                                    $("#cboProject").val('').change();
                                    $("#cboCustomer").val('').change();
                                    //$("#cboActivity").val('').change();
                                    $('#starttime').prop("readonly", false);
                                    $('#endtime').prop("readonly", false);
                                    $('#description').prop("readonly", false);
                                    $('#description').val("");
                                    $("#InternalNotes").val("");
                                    $('#TimeHoursId').prop("readonly", false);
                                    $('#time').prop("readonly", false);
                                    $("#Billable").prop("readonly", false);
                                    $("#InternalNotes").prop("readonly", false);
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
                        calendar.unselect()
                    },
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

                                $("#cboProject").val(response.project);
                                $('#cboProject').trigger('change.select2');

                                var project = response.project;
                                var customer = response.CustomerId;

                                //$.ajax({
                                //    type: 'GET',
                                //    url: '../../System/GetCboProject',
                                //    data:
                                //    {
                                //        "CustomerId": customer
                                //    },
                                //    success: function (response) {
                                //        console.log(response.project);
                                //        $("#CboProjectContent").html(response);
                                //        $("#cboProject").select2();
                                //        $("#cboProject").val(project);
                                //        $('#cboProject').trigger('change.select2');

                                //    },
                                //    timeout: 30 * 60 * 1000
                                //}).fail(function (xhr, status) {
                                //    if (status === "timeout") {
                                //        console.log('Tiempo de respuesta agotado');
                                //    }
                                //    if (status === "error") {
                                //        console.log('La cantidad de datos excede el limite por conexión');
                                //    }
                                //});

                                //$.ajax({
                                //    type: 'GET',
                                //    url: '../../System/GetCboActivity',
                                //    data:
                                //    {
                                //        "ProjectId": $("#cboProject").val()
                                //    },
                                //    success: function (response2) {
                                //        $("#CboActivityContent").html(response2);
                                //        $("#cboActivity").select2();
                                //        $("#cboActivity").val(response.activity);
                                //        $('#cboActivity').trigger('change.select2');
                                //    },
                                //    timeout: 30 * 60 * 1000
                                //}).fail(function (xhr, status) {
                                //    if (status === "timeout") {
                                //        console.log('Tiempo de respuesta agotado');
                                //    }
                                //    if (status === "error") {
                                //        console.log('La cantidad de datos excede el limite por conexión');
                                //    }
                                //});
                                //.then($("#cboActivity").val(response.activity).change())

                                if (response.Billable) {
                                    $("#Billable").prop('checked', true);
                                } else {
                                    $("#Billable").prop('checked', false);
                                }
                                document.getElementById('starttime').value = currentTime;
                                document.getElementById('endtime').value = currentEndTime;
                                document.getElementById('description').value = response.ActDescription;
                                document.getElementById('InternalNotes').value = response.InternalNote;
                                document.getElementById('time').value = response.THours;
                                document.getElementById('TimeHoursId').value = selectedEvent;

                                jQuery("#date").val(currentDate);

                                calendar.unselect()

                                if (arg.event.extendedProps.daystatus == "Sent" || arg.event.extendedProps.daystatus == "Approved") {
                                    //$("#cboActivity").prop("disabled", true);
                                    //console.log($("#cboActivity"));
                                    $("#btnDelete").hide();
                                    $('#btnSave').hide();
                                    $("#date").prop("readonly", true);

                                    //$('#cboActivity').select2({
                                    //    disabled: true
                                    //});
                                    $('#cboCategory').select2({
                                        disabled: true
                                    });
                                    $('#cboProject').select2({
                                        disabled: true
                                    });
                                    $('#cboCustomer').select2({
                                        disabled: true
                                    });
                                    //$('#cboActivity').prop("readonly", true);
                                    $('#time').prop("readonly", true);
                                    $('#starttime').prop("readonly", true);
                                    $('#endtime').prop("readonly", true);
                                    $('#description').prop("readonly", true);
                                    $('#InternalNotes').prop("readonly", true);
                                    $('#TimeHoursId').prop("readonly", true);
                                    $("#Billable").prop("readonly", true);

                                    //console.log($("#cboActivity"));
                                } else {
                                    $("#btnDelete").show();
                                    $('#btnSave').show();
                                    $("#date").prop("readonly", false);

                                    $('#cboCategory').select2({
                                        disabled: false
                                    });
                                    $('#cboProject').select2({
                                        disabled: false
                                    });
                                    $('#cboCustomer').select2({
                                        disabled: false
                                    });
                                    //$('#cboActivity').select2({
                                    //    disabled: false

                                    //});
                                    $('#time').prop("readonly", false);
                                    $('#starttime').prop("readonly", false);
                                    $('#endtime').prop("readonly", false);
                                    $('#description').prop("readonly", false);
                                    $('#TimeHoursId').prop("readonly", false);
                                    $("#Billable").prop("readonly", false);
                                    $('#InternalNotes').prop("readonly", false);
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
                    },
                    editable: true,
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
    }

    loadData();
    //select2settings();
    function DailyViews(currentdate, view) {
        $.ajax({
            type: 'GET',
            url: '../../Time/GetDaysStatus',
            data:
            {
                "date": currentdate,
                "CalView": view
            },
            success: function (response) {
                $('#dailystatus').html(response);

                $('#tblDaysStatus').on('click', '.btnSendApproval', function () {
                    var DayDate = $(this).attr("DayDate");

                    $.confirm({
                        title: 'Send for Approve',
                        content: 'Do you want to send for approve: ' + DayDate + '?',
                        type: 'orange',
                        theme: 'material',
                        closeIcon: true,
                        icon: 'fa fa-warning',
                        animateFromElement: false,
                        animation: 'left',
                        closeAnimation: 'right',
                        buttons: {
                            Send: function () {
                                $.ajax({
                                    type: 'GET',
                                    url: '../../Time/SendDay',
                                    dataType: 'json',
                                    data:
                                    {
                                        "date": DayDate
                                    },
                                    success: function (response) {
                                        if (response.msg == "Ok") {
                                            $.confirm({
                                                title: 'Day send',
                                                content: 'Day send succesfully for approval',
                                                type: 'blue',
                                                theme: 'material',
                                                closeIcon: true,
                                                animateFromElement: false,
                                                animation: 'left',
                                                closeAnimation: 'right',
                                                buttons: {
                                                    Ok: function () {
                                                        var view = calendar.view.type;

                                                        $.ajax({
                                                            type: 'GET',
                                                            url: '../../Time/GetDaysStatus',
                                                            data:
                                                            {
                                                                "date": currentdate,
                                                                "CalView": view
                                                            },
                                                            success: function (response) {
                                                                $('#dailystatus').html(response);
                                                            },
                                                            timeout: 30 * 60 * 1000
                                                        }).fail(function (xhr, status) {
                                                            console.log(xhr);
                                                            console.log(status);

                                                            if (status === "timeout") {
                                                                console.log('Tiempo de respuesta agotado');
                                                            }
                                                            if (status === "error") {
                                                                console.log('La cantidad de datos excede el limite por conexión');
                                                            }
                                                        });

                                                        loadData();
                                                    }
                                                }
                                            });
                                        } else {
                                            $.confirm({
                                                title: 'Day Send',
                                                content: response.msg,
                                                type: 'orange',
                                                theme: 'material',
                                                closeIcon: true,
                                                animateFromElement: false,
                                                animation: 'left',
                                                closeAnimation: 'right',
                                                buttons: {
                                                    Ok: function () {
                                                        var view = calendar.view.type;

                                                        $.ajax({
                                                            type: 'GET',
                                                            url: '../../Time/GetDaysStatus',
                                                            data:
                                                            {
                                                                "date": currentdate,
                                                                "CalView": view
                                                            },
                                                            success: function (response) {
                                                                $('#dailystatus').html(response);
                                                            },
                                                            timeout: 30 * 60 * 1000
                                                        }).fail(function (xhr, status) {
                                                            console.log(xhr);
                                                            console.log(status);

                                                            if (status === "timeout") {
                                                                console.log('Tiempo de respuesta agotado');
                                                            }
                                                            if (status === "error") {
                                                                console.log('La cantidad de datos excede el limite por conexión');
                                                            }
                                                        });

                                                        loadData();
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
                            },
                            Cancel: function () {
                                loadData();
                            }
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
    }
});