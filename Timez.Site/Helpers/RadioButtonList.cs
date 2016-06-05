using System.Web.Mvc;

namespace Timez.Helpers
{
    /// <summary>
    /// Данные для списка радио-кнопок
    /// </summary>
    public sealed class TimezRadioButtonList
    {
        public string Name { get; set; }
        public SelectList SelectList { get; set; }
        public string Title { get; set; }
        public string Class { get; set; }
    }
}