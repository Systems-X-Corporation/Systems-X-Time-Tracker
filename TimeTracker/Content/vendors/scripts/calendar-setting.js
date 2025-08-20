jQuery(document).ready(function () {
	jQuery("#add-event").submit(function () {
		alert("Submitted");
		var values = {};
		$.each($("#add-event").serializeArray(), function (i, field) {
			values[field.name] = field.value;
		});
		console.log(values);
	});
});

(function () {
	"use strict";
	// ------------------------------------------------------- //
	// Calendar
	// ------------------------------------------------------ //
	jQuery(function () {
		// page is ready
		jQuery("#calendar").fullCalendar({
			
			googleCalendarApiKey: 'AIzaSyA0B3OLn5GyjeOTHc5WAgu2QLj73iJyaW8',
			themeSystem: "bootstrap4",
			// emphasizes business hours
			businessHours: true,
			defaultView: "agendaWeek",
			allDaySlot: false,
			// event dragging & resizing
			editable: true,
			selectable: true,
			unselectAuto: true,
			businessHours:
			{

				start: '07:00',
				end: '19:00',
				dow: [1, 2, 3, 4, 5]
			},
			// header
			header: {
				left: "title",
				center: "month,agendaWeek,agendaDay",
				right: "today prev,next",
			},
			events: [
				{
					googleCalendarId: 'gabriel.sanchez@systems-x.com',
					title: "Barber",
					description:
						"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras eu pellentesque nibh. In nisl nulla, convallis ac nulla eget, pellentesque pellentesque magna.",
					start: "2023-02-02T09:00:00",
					end: "2023-02-02T11:00:00",
					className: "fc-bg-default",
					icon: "circle",
				}
			],
			select: function (selectionInfo) {
				console.log(selectionInfo);
				console.log(selectionInfo.end);
            },
			dayClick: function (date, hsevent, view) {
				jQuery("#modal-view-event-add").modal();

				console.log(date.format() +" " + view.name);
				console.log(hsevent);
				console.log(view);
			},
			eventClick: function (event, jsEvent, view) {

				jQuery(".event-icon").html("<i class='fa fa-" + event.icon + "'></i>");
				jQuery(".event-title").html(event.title);
				jQuery(".event-body").html(event.description);
				jQuery(".eventUrl").attr("href", event.url);
				jQuery("#modal-view-event").modal();
			},
		});
	});
})(jQuery);
