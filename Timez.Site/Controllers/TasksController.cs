using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;
using Timez.Helpers;
using Timez.Utilities;

namespace Timez.Controllers
{
    [Authorize]
    public sealed class TasksController : BaseController
    {
        readonly KanbanFilterUtility _FilterUtility;

        public TasksController()
        {
            _FilterUtility = new KanbanFilterUtility(this);
        }

        #region Все задачи

        /// <summary>
        /// Все задачи доски
        /// </summary>
        [Permission(UserRole.Executor, UserRole.Customer, UserRole.Observer)]
        public ViewResult Board(int id)
        {
            return View();
        }

        /// <summary>
        /// Фильтрация
        /// </summary>
        [HttpPost]
        [Permission(UserRole.Executor, UserRole.Customer, UserRole.Observer)]
		public PartialViewResult Board(int id, FormCollection collection)
        {
            return TasksTable(id, collection);
        }

        /// <summary>
        /// таблица всех задач
        /// HttpPost - для пейджинга, 
        /// HttpGet - для первой загрузки
        /// </summary>
        [Permission(UserRole.Executor, UserRole.Customer, UserRole.Observer)]
		public PartialViewResult TasksTable(int id, FormCollection collection)
        {
            // Заполняем модель задачами
            FillTasks(collection, id, "TasksTablePage", false);

            ViewData.Add("Statuses", Utility.Statuses.GetByBoard(id).ToList());

            return PartialView("TasksTable");
        }

        #endregion

        #region Общее
        ///// <summary>
        ///// Сохраняет файлы в задачу
        ///// И удаляет удаленные файлы
        ///// </summary>
        //[Permition(UserRole.Executor,Owner,Customer)]
        //private void SaveFiles(ITask task, FormCollection collection)
        //{
        //    if (collection["DeletedFiles"] != null)
        //    {
        //        // Удаляем удаленные файлв
        //        List<string> toDelete = collection["DeletedFiles"].Split(
        //            new[] { ',' },
        //            StringSplitOptions.RemoveEmptyEntries).ToList();

        //        foreach (string filePath in toDelete)
        //            FilesManager.Delete(filePath);

        //        // добавляем новые
        //        HttpFileCollectionBase files = Request.Files;
        //        for (int i = 0; i < files.Count; i++)
        //            FilesManager.Save(task, files[i]);
        //    }
        //}

        /// <summary>
        /// Подробное описание задачи
        /// </summary>
        [Permission("boardId", "id", "isArchive", ResultType.View, UserRole.Executor, UserRole.Customer)]
        public ViewResult Details(int boardId, int id, bool? isArchive)
        {
            ITask task = Utility.Tasks.Get(boardId, id, isArchive);
            ViewData.Model = task;
            ViewData.Add("isArchive", isArchive == true);

            IUser user = Utility.Users.Get(task.CreatorUserId);
            if (user != null)
                ViewData.Add("CreatorUser", user.Nick);

            return View();
        }

        /// <summary>
        /// Заполняет модель данными
        /// </summary>
        private void FillTasks(FormCollection collection, int id, string cookiePage, bool isArchive)
        {
            // Получаем данные либо из запроса либо из кук, в зависимости от типа действия
            TaskFilter filter = _FilterUtility.GetCurrentFilter(id, !isArchive, collection);
            if (isArchive && filter.SortType == TasksSortType.ByStatus)
                filter.SortType = TasksSortType.ByName;

            #region Подготавливаем данные

            int page;
            if (collection["Page"] == null)
            {
                page = Cookies.GetFromCookies(cookiePage).TryToInt(1);

                // Сюда попадаем при фильтрации, при пейджинге не попадаем
                // Запоминаем фильтры пользователя
                _FilterUtility.SaveFilterToCookies(id, collection);
            }
            else
            {
                page = collection["Page"].TryToInt(1);
                Cookies.AddToCookie(cookiePage, page.ToString());
            }

            // Данные для отображения
            List<ITask> tasks = isArchive
				? Utility.Tasks.GetFromArchive(filter)
                : Utility.Tasks.Get(filter);
            var pagedTasks = new PagedTasks(page, tasks);
            ViewData.Model = pagedTasks.Tasks;

            ViewData.Add("Page", page);
            ViewData.Add("TotalItems", pagedTasks.TotalCount);
            ViewData.Add("isArchive", isArchive);

            #endregion
        }

        [Permission("boardId", "id", "isArchive", ResultType.View, UserRole.Executor, UserRole.Customer)]
        public PartialViewResult Delete(int boardId, int id, bool? isArchive, FormCollection collection)
        {
            if (isArchive.HasValue && isArchive.Value)
            {
				Utility.Tasks.DeleteFromArchive(id);
                return ArchiveTable(boardId, collection);
            }

            ITask task = Utility.Tasks.Get(boardId, id);
            Utility.Tasks.Delete(task);
            return TasksTable(boardId, collection);
        }

        #endregion

        #region Архив

        /// <summary>
        /// Представление таблицы
        /// </summary>
        [Permission(UserRole.Executor, UserRole.Customer)]
        public PartialViewResult ArchiveTable(int id, FormCollection collection)
        {
            // Заполняем модель задачами
            FillTasks(collection, id, "ArchiveTablePage", true);

            return PartialView("TasksTable");
        }

        /// <summary>
        /// Страница архива
        /// </summary>
        [Permission(UserRole.Executor, UserRole.Customer)]
        public ViewResult Archive(int id)
        {
            ViewData.Model = id;
            return View();
        }

        /// <summary>
        /// Фильтрация архива
        /// </summary>
        [HttpPost]
        [Permission(UserRole.Executor, UserRole.Customer)]
        public PartialViewResult Archive(int id, FormCollection collection)
        {
            return ArchiveTable(id, collection);
        }

        /// <summary>
        /// Восстановление задачи из архива
        /// </summary>
        [Permission("boardId", "id", "isArchive", ResultType.View, UserRole.Owner)]
        public RedirectToRouteResult Restore(int boardId, int id, bool? isArchive)
        {
            Utility.Tasks.Restore(id);
            return RedirectToAction("Details", new { boardId, id });
        }

        [Permission(UserRole.Owner)]
        public RedirectToRouteResult ClearArchive(int id)
        {
            Utility.Tasks.ClearArchive(id);
            return RedirectToAction("Archive", new { id });
        }

        #endregion
    }
}
