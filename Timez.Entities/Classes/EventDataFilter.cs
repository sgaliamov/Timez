using System.Collections.Generic;
using Common.Extentions;

namespace Timez.Entities
{
    // TODO: сделать так же как TaskFilter
    public class EventDataFilter
    {
        public int BoardId { get; set; }
        public IEnumerable<int> UserIds { get; set; }
        public IEnumerable<int> ProjectIds { get; set; }
        public EventType EventTypes = EventType.All;
        
        /// <summary>
        /// Начинается с 1
        /// </summary>
        public int? Page { get; set; }
        public int? ItemsOnPage { get; set; }
        
        public string Key
        {
            get
            {
                return
                   "Log_"
                   + BoardId.ToString()
                   + UserIds.ToString('_')
                   + ProjectIds.ToString('_')
                    // + sortType.ToString() не влияет наличие задач в кеше, по этому не нужно кешировать по этому значению
                   + (ItemsOnPage.HasValue ? ItemsOnPage.Value.ToString() : string.Empty)
                   + (Page.HasValue ? Page.Value.ToString() : string.Empty)
                   + ((int)EventTypes).ToString();
            }
        }

        /// <summary>
        /// Соответствует фильтр всем задачам
        /// </summary>
        public bool NeedAllTasks
        {
            get
            {
                return UserIds == null
                    && ProjectIds == null
                    && Page == null
                    && ItemsOnPage == null
                    && (EventTypes & EventType.All) == EventType.All;
            }
        }

    }
}
