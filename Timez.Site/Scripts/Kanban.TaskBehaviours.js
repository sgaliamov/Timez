/// <reference path="~/Scripts/Main.js"/>
/// <reference path="~/Scripts/Kanban.js"/>
/// <reference path="~/jQuery/jquery.contextMenu/jquery.contextMenu.js"/>
/// <reference path="~/jQuery/jquery.cookie.js"/>

var Kanban = (function ($k) {

	/* Инициализация:
	*  - Контекстное меню у задач
	*  - кнопка подробнее
	-----------------------------------------------------------------------------------------*/
	$k.TaskBehaviours = function (tasks) {
		// подробнее
		tasks.find(".more").toggle(
			function () { $(this).next().slideDown("fast"); },
			function () { $(this).next().slideUp("fast"); }
		);

		// Двойной клик открывает редактирование
		tasks.dblclick(function () { TaskActinon("edit", $(this), undefined); });

		// меню
		tasks.contextMenu({ menu: 'taskMenu', inSpeed: 0, outSpeed: 0 },
			function (action, el, pos, link) {
				var task = el;
				var value = link.attr("val");
				TaskActinon(action, task, value);
			}
		);
	};

	// перенос задачи в архив
	function ToArchive(fromStatus, task, id) {
		$k.ShowTaskLoader(task);
		$k.Post("/Kanban/ToArchive/", { boardId: $k.BoardId, taskId: id },
                function () {
                	var taskWrapper = task.parent();
                	$k.TaskToStatus(fromStatus, undefined, undefined, taskWrapper, false);
                	$k.HideTaskLoader(task);
                }, false, function () { $k.HideTaskLoader(task); }
            );
	};

	// на описание задачи
	$k.ToDetails = function (taskId) { window.location.href = "/Tasks/Details/" + $k.BoardId + "/" + taskId; };

	// Операции над задачей
	function TaskActinon(action, task, value) {
		var taskWrapper = task.parent(); // li
		var taskId = task[0].id.replace("task_", "");
		var prevStatus = taskWrapper.parent().parent().parent();

		switch (action) {
			case "toRight":
				// При отправке вправо только вычисляем ид следующего статуса 
				// и делаем тоже самое что и при case "toStatus"
				var nextTD = prevStatus.parent().next();
				if (nextTD.length == 0) {
					if (window.confirm('Отправить в архив?')) {
						ToArchive(prevStatus, task, taskId);
					}
					break;
				}
				else {
					value = nextTD.data("status-id");
				}
			case "toStatus":
				$k.ShowTaskLoader(task);
				$k.Post("/Kanban/UpdateStatus/",
                    { boardId: $k.BoardId, taskId: taskId, statusId: value },
                    function (data) {
                    	var divStatus = $("#status_" + value);
                    	$k.TaskToStatus(prevStatus, divStatus, taskWrapper, $(data));
                    }, true,
                    function () {
                    	$k.HideTaskLoader(task);
                    });
				break;

			case "toArchive":
				ToArchive(prevStatus, task, taskId);
				break;

			case "setColor":
				$k.ShowTaskLoader(task);
				$k.Post("/Kanban/SetColor/", { taskId: taskId, colorId: value, boardId: $k.BoardId }, function (data) {
					if (data != "") {
						task.find(".color").css("background-color", data);
					}
					$k.HideTaskLoader(task);
				}, false, function () { $k.HideTaskLoader(task); });
				break;

			case "setUser":
				$k.ShowTaskLoader(task);
				$k.Post("/Kanban/SetExecutor/", { taskId: taskId, userId: value, boardId: $k.BoardId }, function (data) {
					if (data != "") {
						task.find(".user").html(data);
					}
					$k.HideTaskLoader(task);
				}, false, function () { $k.HideTaskLoader(task); });
				break;

			case "details":
				$k.ToDetails(taskId);
				break;

			case "edit":
				$k.Popup.Edit($k.BoardId, taskId);
				break;

			case "delete":
				if (window.confirm("Вы уверены что хотите удалить задачу?")) {
					$k.ShowTaskLoader(task);
					$k.Post("/Kanban/DeleteTask/",
                        { taskId: taskId, boardId: $k.BoardId },
                        function () {
                        	taskWrapper.remove();
                        	var prevTaskTime = parseInt(task.attr("time"));
                        	$k.AjustStatusInfo(prevStatus, undefined, prevTaskTime, 0);
                        	$k.AjustSize();
                        	$k.HideTaskLoader(task);
                        },
                        false, function () { $k.HideTaskLoader(task); });
				}
				break;

			case "setProject":
				$k.ShowTaskLoader(task);
				$k.Post("/Kanban/SetProject/", { taskId: taskId, projectId: value, boardId: $k.BoardId }, function (data) {
					if (data != "") {
						task.find(".project").html(data);
					}
					$k.HideTaskLoader(task);
				}, false, function () { $k.HideTaskLoader(task); });
				break;
		} // switch
	}

	/* Добавляет задачу в newStatus и удаляет из prevStatus
	*  статусы это div с классом status
	*  враперы это li с классо task-wrapper
	-----------------------------------------------------------------------------------------*/
	$k.TaskToStatus = function (prevStatus, newStatus, prevTaskWrapper, taskWrapper, isDrag) {
		var task = taskWrapper.children();
		//var taskId = task[0].id.replace("task_", "");

		// Если статус видим, то не скрывать, а переносить туда
		if (newStatus == undefined || newStatus.hasClass("collapsed")) {
			taskWrapper.remove();
		}
		else {
			if (isDrag || (prevStatus != undefined && newStatus[0] == prevStatus[0])) {
				// Помещаем туда куда положили, иначе карточка скачет
				prevTaskWrapper.after(taskWrapper);
			}
			else {
				// Если задача новая, или назначается статус через контекстное меню, помещаем ее в начало
				taskWrapper.prependTo(newStatus.children(".tasks").children("ul"));
			}

			// Повторно инициализируем контекстное меню для задачи
			$k.TaskBehaviours(task);
			$k.HideTaskLoader(task);
		}

		if (prevTaskWrapper != undefined) {
			prevTaskWrapper.remove();
		}

		var prevTaskTime = prevTaskWrapper
            ? parseInt(prevTaskWrapper.children().attr("time"))
            : 0;
		var newTaskTime = parseInt(task.attr("time"));
		$k.AjustStatusInfo(prevStatus, newStatus, prevTaskTime, newTaskTime);
		$k.AjustSize();
	};

	/* Инициализирует таскаемость задач
	-----------------------------------------------------------------------------------------*/
	$k.InitSortable = function (ul) {
		ul.sortable({
			helper: 'clone',
			placeholder: "task-wrapper-highlight",
			connectWith: ".tasks ul",
			stop: function (event, ui) {
				// На завершении таскания отправляем на сервер новую позицию
				var task = ui.item.children(".task");
				var taskId = task[0].id.replace("task_", "");
				var newStatus = ui.item.parent().parent().parent();
				var newStatusId = newStatus[0].id.replace("status_", "");
				var prevStatus = $(this).parent().parent();
				var prevStatusId = prevStatus[0].id.replace("status_", "");
				var self = $(this);

				// на тот же статус кидать нельзя
				if (newStatusId != prevStatusId) {
					$k.ShowTaskLoader(task);
					$k.Post("/Kanban/UpdateStatus/", { boardId: $k.BoardId, taskId: taskId, statusId: newStatusId },
					function (data) {
						$k.TaskToStatus(prevStatus, newStatus, ui.item, $(data), true);
					}, true, function () {
						self.sortable('cancel');
						$k.HideTaskLoader(task);
					}
				);
				}
				else {
					// Отменяем таскание если статус не меняется
					self.sortable('cancel');
				}
			}
		});
	};

	return $k;
} (Kanban || {}));