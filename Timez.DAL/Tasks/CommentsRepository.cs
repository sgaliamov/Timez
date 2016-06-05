using System;
using System.Collections.Generic;
using System.Linq;
using Timez.Entities;
using Timez.DAL.DataContext;
using System.Data.Linq;

namespace Timez.DAL.Tasks
{
    public interface ICommentsRepository
    {
        IOrderedQueryable<ITasksComment> Get(int boardId, int id);
        void Add(ITask task, IUser author, string comment, int? parentId, string parrentComment);
        void Delete(int id);
    }

    class CommentsRepository : BaseRepository<CommentsRepository>, ICommentsRepository
    {
        public IOrderedQueryable<ITasksComment> Get(int boardId, int id)
        {
            return _Get(_Context, boardId, id);
        }
        static readonly Func<TimezDataContext, int, int, IOrderedQueryable<TasksComment>> _Get =
            CompiledQuery.Compile((TimezDataContext ctx, int boardId, int id) => ctx.TasksComments
                .Where(x => x.BoardId == boardId && x.TaskId == id && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate));

        public void Add(ITask task, IUser author, string comment, int? parentId, string parrentComment)
        {
            TasksComment tasksComment = new TasksComment
            {
                AuthorUser = author.Nick,
                BoardId = task.BoardId,
                AuthorUserId = author.Id,
                Comment = comment.TrimEnd(),
                CreationDate = DateTimeOffset.Now,
                ParentComment = parrentComment ?? "",
                ParentId = parentId,
                IsDeleted = false,
                TaskId = task.Id
            };

            _Context.TasksComments.InsertOnSubmit(tasksComment);
            _Context.SubmitChanges();
        }

        public void Delete(int id)
        {
            TasksComment comment = _Context.TasksComments.FirstOrDefault(x => x.Id == id);
            if (comment != null)
            {
                // TODO: Переодически чистить удаленные записи и перестраивать индексы
                comment.IsDeleted = true;
                _Context.SubmitChanges();
            }
        }
    }
}