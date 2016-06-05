using System.Web.Mvc.Ajax;

namespace Timez.Helpers
{
    /// <summary>
    /// Опции которые нужны на всех аякс формах и экшенах
    /// </summary>
    public class CommonAjaxOptions : AjaxOptions
    {
        public CommonAjaxOptions(string updateTargetId)
            : this()
        {
            UpdateTargetId = updateTargetId;
        }

        public CommonAjaxOptions()
        {
            OnFailure = "Main.OnFailure";
            OnBegin = "Main.OnBegin";
            OnSuccess = "Main.OnSuccess";
            OnComplete = "Main.OnComplete";

            // Крутилкой нужно управлять вручную, иначе сбивается счетчик в крутилке
            //base.LoadingElementId = "ajaxloader";
        }

    }
}