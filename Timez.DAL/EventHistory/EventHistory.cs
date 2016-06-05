using Timez.Entities;

namespace Timez.DAL.DataContext
{
    internal partial class EventHistory : IEventHistory
    {
        EventType IEventHistory.EventType
        {
            get
            {
                return (EventType)this.EventType;
            }
            //set
            //{
            //    this.EventType = (int)value;
            //}
        }
    }
}
