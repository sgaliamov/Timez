/// <reference path="~/Scripts/Kanban.js"/>

var Kanban = (function ($k) {
	var ErrorType = {
		PlanningTimeRequered: "PlanningTimeRequered",
		TaskCountLimitIsReached: "TaskCountLimitIsReached",
		PlanningTimeIsExceeded: "PlanningTimeIsExceeded",
		TaskNotFoundException: "TaskNotFoundException"
	};

	var $p = $k.Popup;

	/* Обработка ошибок
	** response - данные об ошибке
	** redo - дополнительные действия для обработки ошибки, обычно это повтороный пост. это - что делать если пользователь разрешил проблему.
	** onError - что сделать после ошибки, если пользователь не разрешил её
	** prevPostData - данные с предыдущего постинга, что бы можно было их соединить с новым постом, в них хранится инфа о принудительности поста (forsed)
	-----------------------------------------------------------------------------------------*/
	$k.OnError = function (response, onError, redo, prevPostData) {
		if (response.Error.Type) {

			// Стандартная обработка ошибки
			function End() {
				if (onError) {
					onError(response);
					// Очищаем флаги принудительности, что бы они не использовались для других запросов
					if ($p) {
						$p.ResetFlags();
					}
				}
			}

			switch (response.Error.Type) {
				// Когда требуется указать время                                                        
				case ErrorType.PlanningTimeRequered:
					OnErrorPlanningTimeRequered(response, onError, redo, prevPostData);
					break;

				// Привешения лимитов         
				case ErrorType.PlanningTimeIsExceeded:
				case ErrorType.TaskCountLimitIsReached:
					window.Main.Message("Предупреждение", response.Message, {
						buttons: {
							"OK": function (e) {
								// простое условие, так как всего 2 кейса
								if (response.Error.Type == ErrorType.PlanningTimeIsExceeded) {
									$("#task-forsed-time").val(true);
									redo({ "task-forsed-time": true });
								} else {
									$("#task-forsed-count").val(true);
									redo({ "task-forsed-count": true });
								}
								var dlg = $(this);
								dlg.dialog("close");
							},
							"Отмена": function (e) {
								$(this).dialog("close");
								End();
							}
						}
					});
					break;

				case ErrorType.TaskNotFoundException:
					window.Main.Message("Ошибка", response.Message);
					var task = $("#task_" + response.Data.TaskId);
					var status = task.parents(".status");
					var prevTaskTime = parseInt(task.attr("time"));
					task.parent().remove(); // удаляем контейнер задачи
					$k.AjustStatusInfo(status, undefined, prevTaskTime, 0);
					break;

				default:
					window.Main.Message("Ошибка", response.Message);
					End();
					break;
			} // switch end
		}
	};

	/* Когда требуется указать время    
	-----------------------------------------------------------------------------------------*/
	function OnErrorPlanningTimeRequered(response, onError, redo, prevPostData) {
		var html = "<div class='time-requered-dialog'>"
                + "<p>" + response.Message + "</p>"
                + "<label>Задать:</label>&nbsp;"
                + "<input type='text' class='hours' maxlength='3' />&nbsp;час.&nbsp;&nbsp;"
                + "<input type='text' class='minutes' maxlength='3'/>&nbsp;мин."
                + "</div>";

		var buttons = {
			"Ok": function () {
				var h = parseInt(hours.val()) * 60;
				if (!h) { h = 0; }
				var m = parseInt(minutes.val());
				if (!m) { m = 0; }
				var timeVal = h + m;

				if (timeVal > 0) {
					var postData = $.extend({
						boardId: $k.BoardId,
						taskId: response.Data.TaskId,
						time: timeVal,
						statusId: response.Data.NewStatusId
					}, prevPostData); // передаем и старые данные

					$k.Post("/Kanban/SetPlanningTime/",
                            postData,
                            function () {
                            	onError = undefined; // Отменяем вызов при закрытии окна, так как ошбика обработана
                            	if (redo) {
                            		redo();
                            	}
                            	dlg.dialog("close");
                            },
                            true // В случае ошибки делаем репост
                        );
				}
				else { alert("Введите положительное время!"); }
			},
			"Отмена": function () { dlg.dialog("close"); }
		};

		var dlg = $(html).dialog({
			autoOpen: true,
			modal: true,
			resizable: false,
			title: "Предупреждение",
			buttons: buttons,
			minHeight: 58,
			close: function () {
				if (onError) {
					onError();
					if ($p) { $p.ResetFlags(); }
				}
				dlg.dialog("destroy").remove();
			}
		});
		var hours = dlg.find("input.hours").focus();
		var minutes = dlg.find("input.minutes");
	}

	return $k;
} (Kanban || {}));