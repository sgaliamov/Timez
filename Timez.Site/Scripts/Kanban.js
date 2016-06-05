/// <reference path="~/Scripts/Main.js"/>
/// <reference path="~/Scripts/Kanban.TaskBehaviours.js"/>
/// <reference path="~/jQuery/jquery.contextMenu/jquery.contextMenu.js"/>
/// <reference path="~/jQuery/jquery.cookie.js"/>

var Kanban = (function ($k) {

	$k.Filter_OnSuccess = function () {
		Init();
		Kanban.AjustSize();
	};

	/* преобразует число в строку типа
	** часов:минут
	-----------------------------------------------------------------------------------------*/
	function MinutesToString(minutes) {
		var h = parseInt(minutes / 60);
		if (h < 10) {
			h = "0" + h.toString();
		}
		var m = (minutes % 60);
		if (m < 10) {
			m = "0" + m.toString();
		}
		return h + ":" + m;
	};

	/* Инициализирует канбан доску
	-----------------------------------------------------------------------------------------*/
	function Init() {
		var allTasks = $(".task");
		InitBehaviours(allTasks);

		// Таскаемость задач
		InitSortable($(".tasks ul"));

		// Настраиваем пейджеры
		$(".status").each(function () { InitPagers($(this)); });

		// Выравниваем столбцы       
		$(window).resize($k.AjustSize);

		$k.StartAutoUpdate();

		window.AjaxLoader.Dispose();
	};

	/* Автоматичесоке обновление доски
	-----------------------------------------------------------------------------------------*/
	$k.StartAutoUpdate = function () {
		if ($k.UpdatePeriod) {
			$k.TimerId = setTimeout(function () {
				// TODO: разобраться почему без этой проверки не работает,
				// переменная теоретически должна быть валидной и clearTimeout должен останавливать таймер
				if ($k.TimerId) {
					$k.StopAutoUpdate();
					window.location.href = window.location.href;
				}
			}, $k.UpdatePeriod);
		}
	};
	$k.UpdatePeriod = undefined;
	$k.TimerId = undefined;
	$k.StopAutoUpdate = function () {
		clearTimeout($k.TimerId);
		$k.TimerId = undefined;
	};

	/* Ид текущей доски
	-----------------------------------------------------------------------------------------*/
	$k.BoardId = undefined;

	/* Отправка на сервер постов
	*  url - куда отправляем
	*  data - что
	*  onSuccess - что делать после успешной отправки
	*  repost - нужно ли пробовать отправить повторно после обработки ошибки
	*  onError - что делать если отправка неудачна и пользователь не разрешил ошибку
	-----------------------------------------------------------------------------------------*/
	$k.Post = function (url, data, onSuccess, repost, onError) {
		$.post(url, data, function (response) {
			if (response != null && response.Error != undefined) {
				// функция для повторного запроса, когда пользователь разрешил пробелму
				var redo = function (data2Add) {
					if (data2Add != undefined) {
						data = $.extend(data, data2Add);
					}
					$k.Post(url, data, onSuccess, repost, onError);
				};

				$k.OnError(response, onError, repost == true ? redo : undefined, data);
			}
			else {
				// После успешно пройденных операций очищаем флаги принудительности, 
				// что бы они не повлияли на следующие запросы
				if ($k.Popup) { $k.Popup.ResetFlags(); }

				onSuccess(response);
			}
		});
	};

	/* Поправляет инфу о статусе при перемещении задачи
	** task - div
	-----------------------------------------------------------------------------------------*/
	$k.AjustStatusInfo = function (prevStatus, newStatus, prevTaskTime, newTaskTime) {

		if (prevStatus != undefined) {
			var prev = prevStatus.find(".count");
			prev.html(parseInt(prev.html()) - 1);
			if (prevTaskTime != 0) {
				var prevTime = prevStatus.find(".planning-time");
				var prevTimeVal = parseInt(prevTime.attr("time")) - prevTaskTime;
				prevTime.attr("time", prevTimeVal);
				prevTime.html(MinutesToString(prevTimeVal));
			}
		}

		if (newStatus != undefined) {
			var newCount = newStatus.find(".count");
			newCount.html(parseInt(newCount.html()) + 1);
			if (newTaskTime != 0) {
				var newTime = newStatus.find(".planning-time");
				var newTimeVal = parseInt(newTime.attr("time")) + newTaskTime;
				newTime.attr("time", newTimeVal);
				newTime.html(MinutesToString(newTimeVal));
			}
		}
	};

	/* Показывает прогресс при обновлении задачи
	-----------------------------------------------------------------------------------------*/
	$k.ShowTaskLoader = function (task) { Main.ShowLoader(task); window.AjaxLoader.Show(); };
	$k.HideTaskLoader = function (task) { Main.HideLoader(task); window.AjaxLoader.Dispose(); };

	/* Выравнивает размеры статусов 
	-----------------------------------------------------------------------------------------*/
	$k.AjustSize = function () {
		var kanban = $("#kanban");
		var statuses = $(".status");

		// Ширина, если алгоритм поменяется, поменять в $k.ascx
		var sw = kanban.innerWidth();
		var count = statuses.length; // количество развернутых статусов
		var hidens = $(".hidden");
		var hidenCount = hidens.length;
		sw -= (hidenCount * 31);
		var w = (sw / count) - 1;
		hidens.parent().width(30);
		statuses.width(w);
		statuses.parent().width(w);

		// Высота
		var uls = $("ul", statuses);
		var maxh = 100;
		uls.css("min-height", maxh);
		uls.each(function (item) {
			maxh = Math.max($(this).height(), maxh);
		});
		uls.css("min-height", maxh);
	};

	/* Сворачиваемость статуса
	-----------------------------------------------------------------------------------------*/
	$k.Collapse = function (sender) {
		var status = $(sender).parent().parent().parent();
		status.removeClass("status").addClass("hidden");
		status.next().show(0, function () {
			var statusId = status[0].id.replace("status_", "");
			var oldCookie = $.cookie("CollapsedStatuses");
			// формат записи кук - ||12||234||123||
			// что бы работала замена в случае - ,1,11,111 - если потребуется удалить 1, то удалиться во всех случаях
			// а так будем удалять из ||1||11||111|| строку |1|
			Main.SetCookie("CollapsedStatuses", (oldCookie != null ? oldCookie : "||") + statusId + "||");
			$k.AjustSize();
		});
	};

	/* Переинициализирует одну колонку статуса после обновления
	-----------------------------------------------------------------------------------------*/
	function ReinitStatus(status) {
		InitBehaviours($(".task", status));
		InitPagers(status);
		InitSortable($(".tasks ul", status));
	};

	function InitPagers(status) {
		var statusId = status[0].id.replace("status_", "");
		Pager(status, function (page, items, item) {
			status.children(".tasks").load("/Kanban/StatusTasks/", { boardId: $k.BoardId, statusId: statusId, page: page }, function () {
				Main.SetCookie("status-page-" + statusId, page);
				ReinitStatus(status);
				$k.AjustSize();

				// Меняем текущую страницу
				items.removeClass("selected");
				item.addClass("selected");
			});
		});
	};

	function InitSortable(ul) { if ($k.InitSortable) { $k.InitSortable(ul); } }

	function InitBehaviours(tasks) { if ($k.TaskBehaviours) { $k.TaskBehaviours(tasks); } }

	/* sender - кнопка сворачивания/разворачивания статуса
	-----------------------------------------------------------------------------------------*/
	$k.ShowStatus = function (sender) {
		var hidden = $(sender).parent();
		var status = hidden.prev();
		var statusId = status[0].id.replace("status_", "");
		var statusIdString = "|" + statusId + "|";

		// случается всегда 
		function OnEnd() {
			hidden.hide(0, function () {
				status.removeClass("hidden").addClass("status");
				$k.AjustSize();

				var oldCookie = $.cookie("CollapsedStatuses");
				Main.SetCookie("CollapsedStatuses", oldCookie.replace(statusIdString, ""));
			});
		}

		if (status.hasClass("collapsed")) {
			// статус был свернут, нужно догрузить задач
			var page = $.cookie("status-page-" + statusId);
			status.children(".tasks").load("/Kanban/StatusTasks/", { boardId: $k.BoardId, statusId: statusId, page: page }, function () {
				OnEnd();
				ReinitStatus(status);
				status.removeClass("collapsed");
			});
		}
		else {
			OnEnd();
		}
	};

	/* показывает всплывающее окно с подробной инфой по статусу
	-----------------------------------------------------------------------------------------*/
	$k.ShowInfo = function (boardId, statusId) { Main.LoadDialog("Информация о статусе", "/Kanban/StatusInfo/", { boardId: boardId, statusId: statusId }); };

	/* Старт -------------------------------------------------------------------------------*/
	$(function () {
		Init();

		// принудительно запускаем фильтр, чтоб обновить доску
		// TODO: найти более нормальное решение проблемы нажатия кнопки назад в браузере
		$("#filter").submit();
	});
	return $k;

} (Kanban || {}));