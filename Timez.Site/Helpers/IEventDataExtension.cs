using Timez.Entities;

namespace Timez.Helpers
{
    /// <summary>
    /// Расширения модели лога событий
    /// </summary>
    public static class IEventDataExtension
    {
        public static string HtmlClass(this IEventHistory data)
        {
            if ((data.EventType & EventType.Error) == EventType.Error)
                return "event-error-row";

            if ((data.EventType & EventType.Warning) == EventType.Warning)
                return "event-warning-row";

            return "event-data-row";
        }
    }
}