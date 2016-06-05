using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;
using System.Runtime.Serialization;

namespace Timez.Entities
{
    /// <summary>
    /// Для передачи данных по сети
    /// </summary>
    [DataContract(IsReference = false)]
    public class TimezTaskCollection : IQueryable<TimezTask>
    {
        [DataMember]
        public List<TimezTask> Collection { get; set; }


        public IEnumerator<TimezTask> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(TimezTask); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return Collection.AsQueryable().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return Collection.AsQueryable().Provider; }
        }
    }

    /// <summary>
    /// Публичная обертка над задачей
    /// </summary>
    [DataServiceKeyAttribute("Id")]
    [DataContract]
    public class TimezTask : ITask // !!! ОБНОВИ КОНСТРУКТОР ПОСЛЕ ИЗМЕНЕНИЯ ИНТЕРФЕЙСА !!!
    {
        public TimezTask() { }

        public TimezTask(ITask task)
        {
            BoardId = task.BoardId;
            ColorHEX = task.ColorHEX;
            ColorId = task.ColorId;
            ColorName = task.ColorName;
            ColorPosition = task.ColorPosition;
            CreationDateTime = task.CreationDateTime;
            CreatorUserId = task.CreatorUserId;
            Description = task.Description;
            ExecutorEmail = task.ExecutorEmail;
            ExecutorNick = task.ExecutorNick;
            ExecutorUserId = task.ExecutorUserId;
            Id = task.Id;
            Name = task.Name;
            PlanningTime = task.PlanningTime;
            ProjectId = task.ProjectId;
            ProjectName = task.ProjectName;
            StatusChangeDateTime = task.StatusChangeDateTime;
            TaskStatusId = task.TaskStatusId;
            TaskStatusPosition = task.TaskStatusPosition;
            TaskStatusName = task.TaskStatusName;
        }

        [DataMemberAttribute]
        public string Name { get; set; }

        [DataMemberAttribute]
        public string Description { get; set; }

        [DataMemberAttribute]
        public int BoardId { get; set; }

        [DataMemberAttribute]
        public int ExecutorUserId
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int Id
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int ProjectId
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int TaskStatusId
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int ColorId
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public DateTimeOffset CreationDateTime
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public DateTimeOffset StatusChangeDateTime
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int? PlanningTime
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int CreatorUserId
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public string ColorHEX
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public string ColorName
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int ColorPosition
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public string ProjectName
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public string ExecutorNick
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public string ExecutorEmail
        {
            get;
            set;
        }

        [DataMemberAttribute]
        public int TaskStatusPosition { get; set; }

        [DataMemberAttribute]
        public string TaskStatusName { get; set; }

        [DataMemberAttribute]
        public bool IsDeleted { get; set; }
    }
}
