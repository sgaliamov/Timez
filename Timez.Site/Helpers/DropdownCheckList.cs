using System.Collections.Generic;
using System.Web.Mvc;

namespace Timez.Helpers
{
    /// <summary>
    /// Данные для выпадающего списка с чекбоксами
    /// </summary>
    public sealed class DropdownCheckList
    {
        public IEnumerable<SelectListItem> SelectList { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Label { get; set; }
    }
}