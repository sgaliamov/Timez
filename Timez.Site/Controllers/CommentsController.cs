using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
    [Authorize]
    public class CommentsController : BaseController
    {
        /// <summary>
        /// Комменты задачи
        /// </summary>
        /// <param name="boardId">доска</param>
        /// <param name="id">задача</param>
        /// <param name="isArchive"></param>
        [HttpGet]
        [Permission("boardId", "id", "isArchive", ResultType.View, UserRole.Executor, UserRole.Customer)]
		public PartialViewResult List(int boardId, int id, bool? isArchive)
        {
            List<ITasksComment> comments = Utility.Comments.Get(boardId, id);
            ViewData.Model = comments;

            return PartialView("List");
        }

        /// <summary>
        /// Добавление коммента
        /// </summary>        
        /// <returns>PartialView("List")</returns>
        [HttpPost, ValidateInput(false)]
        [Permission("boardId", "id", "isArchive", ResultType.View, UserRole.Executor, UserRole.Customer)]
		public PartialViewResult List(int boardId, int id, bool? isArchive, FormCollection collection)
        {
            ITask task = Utility.Tasks.Get(boardId, id, isArchive);
            string comment = collection["new-comment"];
            int? parentId = collection["parent-id"].TryToInt();
            string parrentComment = collection["parent-comment"];
			Utility.Comments.Add(task, Utility.Users.CurrentUser, comment, parentId, parrentComment);

            return List(boardId, id, isArchive);
        }

        /// <summary>
        /// Удаление коммента
        /// </summary>
        /// <param name="boardId">доска</param>
        /// <param name="id">Ид коммента</param>
        /// <param name="taskId">ид задачи</param>
        /// <param name="isArchive"></param>
        /// <returns>PartialView("List")</returns>
        [Permission("boardId", "taskId", "isArchive", ResultType.View, UserRole.Executor, UserRole.Customer)]
		public PartialViewResult Delete(int boardId, int id, int taskId, bool? isArchive)
        {
			Utility.Comments.Delete(id);

            return List(boardId, taskId, isArchive);
        }
    }
}