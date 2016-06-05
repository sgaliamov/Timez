using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
    /// <summary>
    /// Инвайты
    /// </summary>
    public class InviteController : BaseController
    {
        /// <summary>
        /// Текущий пользователь принимает приглашение
        /// </summary>
        /// <returns>Страница досок</returns>
		/// TODO: OrganizationPermission - без проверки принадлежности к организации, так как списки сотрудников кешируются
		public RedirectToRouteResult AcceptInvite(int id)
        {
        	int organizationId = id;
            // Текущий пользователь принимает приглашение
            Utility.Invites.AcceptInvite(organizationId, Utility.Authentication.UserId);

            // Высылаем уведомление всем админам огранизации, чтобы они повключали пользователей на доски
            List<IUser> users = Utility.Organizations.GetEmployees(organizationId)
                .Where(x => x.Settings.GetUserRole().HasTheFlag(EmployeeRole.Administrator))
                .Select(x => x.User)
                .ToList();

            IOrganization organization = Utility.Organizations.Get(organizationId);
            foreach (IUser user in users)
            {
                string subj = "Пользователь " + Utility.Users.CurrentUser.Nick + " включен в огранизацию '" + organization.Name + "'";
                string rawMessage =
                    subj + "<br/>"
                    + "Теперь вы можете добавить его как учасника на ваши доски.";
                MailsManager.SendMail(user, subj, rawMessage);
            }

            return RedirectToAction("Index", "Boards");
        }

        /// <summary>
        /// Текущий пользователь откланяет приглашение организации
        /// </summary>
        /// <param name="id">Организация</param>
        /// <returns>Страница досок</returns>
        public ActionResult DeclineInvite(int id)
        {
            // TODO: без проверки принадлежности к организации, так как списки сотрудников кешируются

            Utility.Organizations.Leave(id);

            return RedirectToAction("Index", "Boards");
        }

        /// <summary>
        /// Партиал создания приглашения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OrganizationPermission(ResultType.Empty, EmployeeRole.Administrator)]
		[ChildActionOnly]        
		public PartialViewResult NewInvite(int id)
        {
            ViewData.Add("Message", "Введите email. Можно указать несколько через запятую или пробел.");

            IOrganization organization = Utility.Organizations.Get(id);
            ViewData.Model = organization;

            int? availableUsersCount = Utility.Tariffs.GetAvailableUsersCount(organization);
            ViewData.Add("AvailableUsersCount", availableUsersCount);

            return PartialView();
        }

        /// <summary>
        /// Создаем приглашение через имейл
        /// </summary>
        /// <param name="id">организация</param>
        /// <param name="collection"></param>
        /// <returns>PartialView("NewInvite")</returns>
        [HttpPost]
        [OrganizationPermission(ResultType.JsonError, EmployeeRole.Administrator)]
        public PartialViewResult NewInvite(int id, FormCollection collection)
        {
            string[] emails = collection["EMail"]
                .Trim()
                .ToLower()
                .Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            foreach (string email in emails)
            {
                string text = InviteParticipant(id, email);
                if (!sb.ToString().Contains(text))
                    sb.AppendLine(text);
            }

            string message = sb.ToString()
                .Trim()
                .Replace(Environment.NewLine, "<br/>");
            ViewData.Add("Message", message);

            ViewData.Model = Utility.Organizations.Get(id);

            return PartialView("NewInvite");
        }

        private string InviteParticipant(int organizationId, string email)
        {
            if (!email.IsValidEmail())
                ViewData.ModelState.AddModelError("EMail", "Формат email не верный: " + email);

            if (ViewData.ModelState.IsValid)
            {
                // TODO: потестить перед презинтацией
                IUser user = Utility.Users.GetByEmail(email);
                if (user != null)
                {
                    IOrganization organization = Utility.Organizations.Get(organizationId);

                    // Если пользощватель существует, то добавляем его
                    if (Utility.Organizations.AddUser(organization, user))
                    {
                        // И пердлагаем ему подтвердить присутсвие на доске на странице досок
                        string message = @"{0} приглашает вас присоединиться к <a href='{1}'>{2}</a>.<br/>Вы можете подтвердить или отклонить придложение на <a href='{3}'>этой</a> ({3}) станице."
                            .Params(Utility.Users.CurrentUser.Nick, // 0
                                     Url.Action("Index", "Organization", new { id = organizationId }, "http"), // 1
                                     organization.Name, // 2
                                     Url.Action("Index", "Boards", null, "http") // 3
                        );
                        MailsManager.SendMail(user, "Приглашение TimeZ.org", message);
                    }
                }
                else
                {
                    string siteUrl = Url.Action("Index", "Home", null, "http");
                    string inviteCode = Utility.Invites.CreateNewInvite(organizationId, email, Utility.Authentication.UserId);
                    string regUrl = Url.Action("Register", "User", new { id = inviteCode }, "http");
                    string message = string.Format(@"
{2} приглашает Вас на сайт <a href='{0}'>TimeZ.org</a>.<br/>
Пройдите по ссылке <a href='{1}'>{1}</a>, что бы зарегестрироваться на сайте.",
                        siteUrl,
                        regUrl,
                        Utility.Users.CurrentUser.Nick
                    );
                    MailsManager.SendMail(email, "Приглашение TimeZ.org", message);
                }

                return user == null
                    ? "Приглашение отослано на " + email + ", пользователь должен зарегестрироваться на сайте."
                    : "Приглашение отослано на " + email + ", пользователь должен подтвердить приглашение.";
            }
            return null;
        }

        /// <summary>
        /// Создание новой ссылки
        /// </summary>
        /// <param name="id">организация</param>
        /// <returns>строка с ссылкой</returns>
        [OrganizationPermission(ResultType.JsonError, EmployeeRole.Administrator)]
        public string GenerateQuickLing(int id)
        {
            return Utility.Invites.RefreshInviteCode(id);
        }

        /// <summary>
        /// Список приглашений в организации текущего пользователя
        /// </summary>
        /// <returns></returns>
		[ChildActionOnly]        
		public PartialViewResult List()
        {
            IEnumerable<IOrganization> organizations = Utility.Organizations.GetToAppove(Utility.Authentication.UserId);
            ViewData.Model = organizations;

            return PartialView("List");
        }
    }
}
