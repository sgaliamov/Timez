/// <reference path="~/jQuery/jquery-1.5.1.js"/>
/// <reference path="~/Scripts/Kanban.js"/>
/// <reference path="~/Scripts/Main.js"/>

/* Модуль диалога редактирования/создания задачи
---------------------------------------------------------------------------------------------*/
Kanban.Popup = (function ($p) {

	var validator = undefined;
	var minutes = undefined;
	var hours = undefined;
	var $k = Kanban;

	/* Инициализация модуля попапа
	-----------------------------------------------------------------------------------------*/
	$p.Init = function () {
		$p.Dialog = $("#task-popup").dialog({
			autoOpen: false,
			modal: true,
			resizable: false,
			width: 640,
			close: function () {
				validator.resetForm();
				$(".value", $p.Dialog).val('');
				$k.StartAutoUpdate();
			},
			open: function () {
				$k.StopAutoUpdate();
				$("#task-name").focus();
				RefreshSelected();
				$("#task-description").autogrow();
			}
		});

		if ($p.Dialog.length == 1) {
			hours = $("#task-planning-hours");
			minutes = $("#task-planning-minutes");

			// ctrl+enter
			$p.Dialog.keypress(function (e) {
				if (e.ctrlKey && e.which == 13) { // Ctrl + Enter
					$("form", $p.Dialog).submit();
					Main.StopEvent(e);
				}
			});

			// Валидация
			var form = $("form", $p.Dialog);
			validator = form.validate({
				errorLabelContainer: ".validation-summary-valid ul",
				wrapper: "li"
			});

			// Табы
			var tabs = $("#tabs", $p.Dialog).tabs({ cookie: { expires: 1} });

			// При клике на настройку меняем значение в хиденфилде
			$(".selectable-list", $p.Dialog).each(function () {
				var list = $(this);
				var target = $(list.data("target"));
				$("li", list).click(function () {
					target.val($(this).data("val")).change();
					RefreshSelected();
				});
			});

			// При установки значения в хиден, нужно поменять выделенность настроек
			$("input[type='hidden']", tabs).change(function () {
				var list = $("#" + this.id + "-list");
				list.find(".selected").removeClass("selected");
				list.find('li[data-val="' + this.value + '"]').addClass("selected");
			});
		}
	};
	$($p.Init);

	/* Что бы было видно, что выбрано в попапе
	-----------------------------------------------------------------------------------------*/
	function RefreshSelected() {
		var text = '';
		$(".selected", $p.Dialog).each(function () { text += $(this).html() + "&nbsp;|&nbsp;"; });
		// контейнер для вывода выбранных значений
		$("#task-settings-selected").html(text.substr(0, text.length - 13));
	}

	/* Показать окошко для создания задачи
	-----------------------------------------------------------------------------------------*/
	$p.Create = function (boardId, statusId) {
		$("#task-statusid").val(statusId).change();

		var selectedUser = $.cookie("SelectedUser");
		if (selectedUser) {
			$('#task-userid-list li[data-val="' + selectedUser + '"]').click();
		}

		$p.Dialog.dialog('option', 'title', "Создание задачи").dialog('open');
	};

	/* при начале постинга в попапе
	-----------------------------------------------------------------------------------------*/
	$p.OnBeginPost = function (response) {
		// Проверяем выбран ли статус, для которого обязательно время и задано ли время
		var validTime = !$("#task-tab-status .selected").hasClass("required")
			|| !!parseInt(minutes.val())
			|| !!parseInt(hours.val());
		var valid = validator.form() && validTime;
		if (!validTime) { validator.showErrors({ "task-planning-minutes": "Требуется указать планируемое время" }); }
		if (valid) { window.AjaxLoader.Show(); }
		return valid;
	};

	/* при успешном завершении постинга в попапе
	-----------------------------------------------------------------------------------------*/
	$p.OnSuccess = function (response) {
		var json = response; //.get_response().get_object();
		if (json.Error == undefined) {
			var data = response; //.get_response().get_responseData();
			var task = $(data);
			var taskId = task.children().attr("Id").replace("task_", "");
			var statusId = $("#task-statusid").val();
			var newStatus = $("#status_" + statusId);
			var prevTask = $("#task_" + taskId);
			if (prevTask.length == 0) {
				// Создание
				$k.TaskToStatus(undefined, newStatus, undefined, task);
			} else {
				// Редактирование
				$k.TaskToStatus(prevTask.parents(".status"), newStatus, prevTask.parent(), task);
			}

			$p.Close();
		} else {
			$k.OnError(json, undefined, function () {
				$("form", $p.Dialog).submit();
			});
		}

		window.AjaxLoader.Dispose();
	};

	/* Открытие попапа на редактирование
	-----------------------------------------------------------------------------------------*/
	$p.Edit = function (boardId, taskId) {
		$k.Post("/Kanban/Task", { id: taskId, boardId: boardId }, function (task) {
			$("#task-id").val(task.Id);
			$("#task-name").val(task.Name);
			if (task.PlanningTime) {
				var h = parseInt(task.PlanningTime / 60);
				$("#task-planning-hours").val(h == 0 ? "" : h);
				var m = parseInt(task.PlanningTime % 60);
				$("#task-planning-minutes").val(m == 0 ? "" : m);
			}
			$("#task-description").val(task.Description);

			// инициализируем выбранные значения
			$("#task-colorid").val(task.ColorId).change();
			$("#task-userid").val(task.ExecutorUserId).change();
			$("#task-projectsid").val(task.ProjectId).change();
			$("#task-statusid").val(task.TaskStatusId).change();

			$p.Dialog.dialog('option', 'title', "Редактирование задачи")
				.dialog("option", "position", 'center')
				.dialog('open');
		});
	};

	/* Закрытие диалога
	-----------------------------------------------------------------------------------------*/
	$p.Close = function () { $p.Dialog.dialog('close'); };

	/* Очистка флагов принудительности
	-----------------------------------------------------------------------------------------*/
	$p.ResetFlags = function () { if ($p.Dialog) { $(".forsed", $p.Dialog).val(''); } };

	return $p;
} (Kanban.Popup || {}));