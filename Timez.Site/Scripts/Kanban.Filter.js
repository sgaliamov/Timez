/* Фильтр
-----------------------------------------------------------------------------------------*/
var Kanban = (function ($k) {
	$k.Filter = (function ($f) {

		/* Инициализация
		-----------------------------------------------------------------------------------------*/
		var _Filter = undefined; // TODO: не понятно почему переменная существует после загрузки страницы
		function Init() {
			_Filter = $("#Users, #Projects, #Colors, #Statuses").dropdownchecklist({ firstItemChecksAll: true, width: 150, emptyText: "Ничего не выбрано" });
			$("#filter-reset").click(function (e) {
				$("#Search").val('');
				$("#SortType").val(1);
				_Filter.find("option").attr('selected', 'selected');
				_Filter.dropdownchecklist("refresh");
			});
		};

		$(Init);
		return $f;
	} ($k.Filter || {}));

	return $k;
} (Kanban || {}));