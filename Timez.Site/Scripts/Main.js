/// <reference path="~/jQuery/jquery-1.5.1.js"/>
/// <reference path="~/Scripts/AjaxLoader.js"/>
/// <reference path="~/jQuery/jquery-ui-1.8.16.custom.js"/>
/// <reference path="~/jQuery/jquery-validate/jquery.validate.unobtrusive.js"/>


var Main = (function (main) {

	// Добавление в список удаленных
	main.Delete = function (sender, id, storage) {
		$(sender).parent().parent().hide();
		document.getElementById(storage).value += "," + id;
	};

	// Отмена дефолтного действия у браузера
	main.StopEvent = function (e) {
		if (!e || e == undefined) e = window.event;

		if (e != undefined) {
			//e.cancelBubble is supported by IE - this will kill the bubbling process.
			e.cancelBubble = true;
			e.returnValue = false;

			//e.stopPropagation works only in Firefox.
			if (e.stopPropagation) {
				e.stopPropagation();
				e.preventDefault();
			}
		}

		return false;
	};

	/* Показывает попап из паршал вью
	** - url адрес попапа
	** - data данные для поста
	** - validate требуется ли unobtrusive валидация, поумолчанию false
	-----------------------------------------------------------------------------------------*/
	main.LoadDialog = function (title, url, data, validate, options) {
		var div = $("<div id='dlg' style='position:relative'></div>");
		var settings = {
			autoOpen: true,
			modal: true,
			resizable: false,
			title: title,
			width: 450,
			minHeight: 95,
			open: function () {
				// закрытие при клике в молоко
				$('.ui-widget-overlay').unbind("click").click(function () { dlg.dialog("close"); });

				Main.ShowLoader(div);
				$.get(url, data, function (html) {
					if (html.Error) {
						html = html.Message;
					}
					div.html(html);
					if (validate != undefined && validate) { $.validator.unobtrusive.parse(div); }

					div.find(".close-button").click(function () {
						$("form", div).validate().resetForm();
						dlg.dialog('close');
					});
					div.find(".save-button").click(function (e) {
						if ($("form", div).valid()) {
							// уничтожаем диалог через некоторое время, что бы дать успеть отработать скриптам на нем при 
							// закрытии. в частности, не успевает сделаться post.
							// если решение работать не будет, то
							// либо не делать $.get при повторном открытии диалога
							// либо дожидаться окончания поста у формы

							// чтобы нельзя было несколько раз нажать на кнопку, отключаем её							
							var button = $(this).click(function (be) {
								//var button = $(this).attr("disabled", "disabled"); так нельзя, так как нужно чтобы кнопки были активны
								main.StopEvent(be);
								return false;
							});
							setInterval(function () {
								button.removeAttr("disabled");
								dlg.dialog('close');
							}, 500);
						}
						else {
							main.StopEvent(e);
						}
					});

					div.find(".focused").focus();

					dlg.dialog("option", "position", 'center');
					Main.HideLoader(div);
				});
			},
			close: function () {
				// уничтожаем диалог при закрытии, так как иногода нужно, что бы при открытии срабатывали
				// какие-то скрипты. если старый диалог в domе, то он влияет на свежий диалог
				div.dialog("destroy").remove();
			}
		};
		settings = $.extend(settings, options);
		var dlg = div.dialog(settings);
	};

	/* Обычное сообщение
	-----------------------------------------------------------------------------------------*/
	main.Message = function (title, message, options) {
		if (message) {
			while (message.indexOf("\n") > -1)
				message = message.replace("\n", "<br />");
		}

		var div = $("<div style='position:relative'>" + message + "</div>");
		var settings =
		{
			autoOpen: true,
			modal: true,
			resizable: false,
			title: title,
			width: 350,
			minHeight: 14,
			open: function () {
				// закрытие при клике в молоко
				$('.ui-widget-overlay').unbind("click").click(function () { div.dialog("close"); });
				div.dialog("option", "position", 'center');
			},
			close: function () { div.dialog("destroy").remove(); },
			buttons: {
				OK: function () {
					div.dialog("close");
				}
			}
		};
		settings = $.extend(settings, options);
		div.dialog(settings);
	};

	/* Добавляет крутилку
	-----------------------------------------------------------------------------------------*/
	main.ShowLoader = function (container) {
		container.append($("<span class='loader'></span>"));
	};

	/* Убирает крутилку
	-----------------------------------------------------------------------------------------*/
	main.HideLoader = function (container) {
		container.find(".loader").remove();
	};

	/* Обычный гет
	-----------------------------------------------------------------------------------------*/
	//    function Get(url, data, onSuccess, onError) {
	//        $.get(url, data, function (response) {
	//            if (response.Error) {
	//                $k.OnError(response, onError);
	//            }
	//            else {
	//                onSuccess(response);
	//            }
	//        });
	//    };

	/* Установка кук
	-----------------------------------------------------------------------------------------*/
	main.SetCookie = function (key, value, options) {
		$.cookie(
			key,
			value,
			$.extend({ expires: 1024, path: '/' }, options) // secure: true использовать нельзя, так как не видятся на сервере
		);
	};


	/* Обработка аякса
	-----------------------------------------------------------------------------------------*/
	// Когда происходит ошибка MS Ajax
	main.OnFailure = function (data) {
		var html = data.responseText;
		if (html == undefined) {
			html = data.get_response().get_responseData();
		}
		document.write(html);
	};

	main.OnBegin = function (data) {
		AjaxLoader.Show();
	};

	main.OnSuccess = function (response) {
		if (response && response.Error) {
			alert(response.Message);
		}
		AjaxLoader.Dispose();
	};

	main.OnComplete = function (data) {
		AjaxLoader.Dispose();
	};



	$(function () {

		$.datepicker.setDefaults($.datepicker.regional['ru']);

		// Автоувеличивающиеся текстареи
		$("textarea.autogrow").autogrow();

		// Что бы на сервере можно было посмотреть возможные ширины
		// TODO: убрать, когда будет ясно как на сервере это прощитать
		Main.SetCookie("document.width", $(document).width());
		Main.SetCookie("clientTimezoneOffset", new Date().getTimezoneOffset());

		//window.history.forward();
		//		window.addEventListener('popstate', function (e) {
		//			debugger;
		//		}, false);
	});

	return main;
} (Main || {}));


/// Класс для пейджеров
// container - где лежит пейджированный контент
// action - что происзойдет при выборе страницы
function Pager(container, action) {
	// При клике на элемент подгружаем в контейнер вьюшку
	var pager = $(".pager", container);
	var items = $("b", pager);
	items.click(function () {
		var item = $(this);
		var page = item.data("page");

		if ($.isFunction(action)) {
			action(page, items, item);
		} else {
			AjaxLoader.Show();
			container.load(action, { page: page }, function () {
				AjaxLoader.Dispose();
			});
		}
	});
	return pager;
};