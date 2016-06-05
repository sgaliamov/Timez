namespace Timez.Helpers
{
    /// <summary>
    /// Данные для пейджера
    /// </summary>
    public class Pager
    {
        /// <summary>
        /// Номер текущей страницы
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Всего элементов
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// Количестов элементов на одной странице
        /// </summary>
        public int ItemsOnPage
        {
            get 
            {
                if (!_ItemsOnPage.HasValue)
                {
                    _ItemsOnPage = DefaultItemsOnPage;
                }
                return _ItemsOnPage.Value; 
            }
            set { _ItemsOnPage = value; } 
        }
        int? _ItemsOnPage;

        public bool NeedArrows = true;

        public string Id = string.Empty;

        /// <summary>
        /// Дефолтное количество элементов на странице с пейджером
        /// </summary>
        public const int DefaultItemsOnPage = 20;
    }
}