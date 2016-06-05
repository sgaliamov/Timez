using System.Linq;
using System.Collections.Generic;
using Timez.Entities;

namespace Timez.BLL.Tasks
{
    public class CommentsUtility : BaseUtility<CommentsUtility>
    {
        public List<ITasksComment> Get(int boardId, int id)
        {
            return Repository.Comments.Get(boardId, id).ToList();
        }

        public void Add(ITask task, IUser author, string comment, int? parentId, string parrentComment)
        {
            Repository.Comments.Add(task, author, comment, parentId, parrentComment);
        }

        public void Delete(int id)
        {
            Repository.Comments.Delete(id);
        }
    }    
}