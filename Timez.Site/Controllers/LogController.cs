using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Alias;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;
using Timez.Helpers;
using Timez.Utilities;

namespace Timez.Controllers
{
    [Authorize]
    public class LogController : BaseController
    {
        readonly KanbanFilterUtility _FilterUtility;

        public LogController()
        {
            _FilterUtility = new KanbanFilterUtility(this);
        }

        /// <summary>
        /// Фильтрация
        /// </summary>
        [HttpPost]
        [Permission(UserRole.Executor, UserRole.Customer)]
        public PartialViewResult Index(int id, FormCollection collection)
        {
			return Items(id, collection);
        }

        /// <summary>
        /// Представление доски
        /// Разные части подгружаются сами через рендер экшн
        /// </summary>
        [Permission(UserRole.Executor, UserRole.Customer)]
        public ViewResult Index(int id)
        {
            ViewData.Model = Utility.Boards.Get(id);
            return View();
        }

        [Permission(UserRole.Executor, UserRole.Customer)]
		public PartialViewResult Items(int id, FormCollection collection)
        {
            #region Получаем данные фильтра либо из запроса либо из кук, в зависимости от того действия

            List<int> userIds;
            List<int> projectIds;

            List<int> dummy;
            TasksSortType sortType;
            _FilterUtility.GetCurrentFilter(id, out userIds, out projectIds, out dummy, out sortType, out dummy, collection);
            EventType eventTypes = GetEventTypes(collection);

            if (collection != null && collection.Count > 0 && collection["Page"] == null)
            {
                // Сюда попадаем при фильтрации

                // затираем значение, что бы оно не менялось в куках
                collection["Statuses"] = string.Empty;
                collection["Colors"] = string.Empty; 
                
                // Запоминаем фильтры пользователя
                _FilterUtility.SaveFilterToCookies(id, collection);

                #region Сохранение типа
                int valueForCoocke = (int)EventType.All;
                if (collection["EventTypes"] != null && !string.IsNullOrEmpty(collection["EventTypes"]))
                {
                    // собираем все выбранные флаги в одно число и сохраняем исключенные
                    var checkedIds = GetIds(collection["EventTypes"]);
                    valueForCoocke = ~checkedIds.Aggregate((current, i) => current | i);
                }
                Cookies.AddToCookie("EventTypes", valueForCoocke.ToString());
                #endregion
            }

            #endregion

            #region
            int page;
            if (collection == null || collection["Page"] == null)
            {
                page = Cookies.GetFromCookies("LogTablePage").TryToInt(1);
            }
            else
            {
                page = collection["Page"].TryToInt(1);
                Cookies.AddToCookie("LogTablePage", page.ToString());
            }

            EventDataFilter filter = new EventDataFilter
                {
                    BoardId = id,
                    UserIds = userIds,
                    ProjectIds = projectIds,
                    EventTypes = eventTypes,
                    Page = page,
                    ItemsOnPage = Pager.DefaultItemsOnPage
                };

            int total;
            var data = Utility.Events.Get(filter, out total);
            #endregion

            ViewData.Model = data;
            ViewData.Add("Page", page);
            ViewData.Add("TotalItems", total);

			return PartialView("Items");
        }

        /// <summary>
        /// Считывание типа события
        /// </summary>
        private EventType GetEventTypes(FormCollection collection)
        {
            EventType eventTypes;
            if (collection != null && collection["X-Requested-With"] != null)
            {
                eventTypes = collection["EventTypes"] == null
                                 ? 0
                                 : (EventType)GetIds(collection["EventTypes"]).Sum();
            }
            else
            {
                // в EventTypes хранятся исключенные события
                eventTypes = (EventType)~Cookies.GetFromCookies("EventTypes").TryToInt(0);
            }
            return eventTypes;
        }

        [Permission(UserRole.Executor, UserRole.Customer)]
		[ChildActionOnly]
        public PartialViewResult Filter(int id)
        {
            #region Считываем данные из кук если они там есть
            List<int> userIds;
            List<int> projectIds;
            List<int> colorIds;
            List<int> statusIds;
            TasksSortType sortType;
            _FilterUtility.GetCurrentFilter(id, out userIds, out projectIds, out colorIds, out sortType, out statusIds, null);
            EventType eventTypes = GetEventTypes(null);
            #endregion

            #region Подготавливаем данные для представления
            var userData = Utility.Boards.GetAllExecutorsOnBoard(id);
            var usersView = new DropdownCheckList
                {
                    SelectList = userData.Select(x => new SelectListItem
                    {
                        Selected = userIds == null || userIds.Contains(x.Id),
                        Text = x.Nick,
                        Value = x.Id.ToString()
                    }).ToList(),
                    Label = "Все инициаторы действий",
                    Title = "Инициаторы действий",
                    Name = "Users"
                };

            var projectsData = Utility.Projects.GetByBoard(id);
            var projectsView = new DropdownCheckList
                {
                    SelectList = projectsData.Select(x => new SelectListItem
                    {
                        Selected = projectIds == null || projectIds.Contains(x.Id),
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }).ToList(),
                    Label = "Все проекты",
                    Title = "Проекты",
                    Name = "Projects"
                };


            var eventTypeView = new List<SelectListItem>();
            foreach (EventType item in Enum.GetValues(typeof(EventType)))
            {

                if ((item & EventType.All) == EventType.All)
                    continue;

                eventTypeView.Add(new SelectListItem
                {
                    Text = item.GetAlias(),
                    Value = ((int)item).ToString(),
                    Selected = (item & eventTypes) == item
                });
            }

            var eventTypeViewCheckList = new DropdownCheckList
                {
                    SelectList = eventTypeView,
                    Label = "Все события",
                    Title = "События",
                    Name = "EventTypes"
                };

            #endregion

            ViewData.Add("Users", usersView);
            ViewData.Add("Projects", projectsView);

            ViewData.Add("EventTypes", eventTypeViewCheckList);

            return PartialView("Filter");
        }

        /// <summary>
        /// Количество задач у пользователей в статусов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Permission(UserRole.Executor, UserRole.Customer)]
		[ChildActionOnly]
		[OutputCache(Duration = CacheDuration)]
        public PartialViewResult Statistics(int id)
        {
            var statuses = Utility.Statuses.GetByBoard(id);
            var users = Utility.Boards.GetAllExecutorsOnBoard(id);

            // <пользователь, <статус, количество>>
            Dictionary<string, Dictionary<string, int>> data = new Dictionary<string, Dictionary<string, int>>();

            var filter = Utility.Tasks.CreateFilter(id);
            foreach (var user in users)
            {
                var counts = new Dictionary<string, int>();
                data.Add(user.Nick, counts);

                foreach (var status in statuses)
                {
                    filter.Statuses = new[] { status.Id };
                    filter.ExecutorIds = new[] { user.Id };

                    int count = Utility.Tasks.Get(filter).Count;
                    counts.Add(status.Name, count);
                }
            }

            ViewData.Add("Statuses", statuses);
            ViewData.Model = data;

			return PartialView("Statistics");
        }

        [Permission(UserRole.Owner)]
        public RedirectToRouteResult Clear(int id)
        {
            Utility.Events.Clear(id);
            return RedirectToAction("Index", new { id });
        }
    }
}
