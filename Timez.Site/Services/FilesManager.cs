using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Entities;

namespace Timez.Utilities
{
    /// <summary>
    /// Отвечает за файлы
    /// </summary>
    public static class FilesManager// : BaseModel
    {
        const string FileStorageFolder = "~/FileStorage/";
        const string TaskFileStorage = FileStorageFolder + "Task/";

        static string GetStoragePath(ITask entity)
        {
            return HttpContext.Current.Server.MapPath(GetServerUrl(entity));
        }

        static string GetServerUrl(ITask task)
        {
            return TaskFileStorage
                + task.BoardId.ToString() + "/"
                + task.Id.ToString() + "/";
        }

        /// <summary>
        /// Ссылки на файлы задачи
        /// </summary>
        /// <param name="task"></param>
        /// <param name="urlHelper"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetUrls(ITask task, UrlHelper urlHelper)
        {
            // задачи нет - нет файлов
            if (task == null)
                return null;

            string folder = GetStoragePath(task);
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder);
                string url = GetServerUrl(task);

                // ссылки
                return files.Select(x => urlHelper.Content(url + Path.GetFileName(x)));
            }

            // список файлов для задчи пуст
            return new List<string>();
        }

        /// <summary>
        /// Сохранение файла для задачи
        /// </summary>
        /// <param name="task"></param>
        /// <param name="file"></param>
        internal static void Save(ITask task, HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                string folder = GetStoragePath(task);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string path = folder + file.FileName;

                if (File.Exists(path))
                {
                    // Добавляем индекс к файлу
                    string name = Path.GetFileNameWithoutExtension(file.FileName);
                    int num = Directory.EnumerateFiles(folder)
                        .Select(x => Path.GetFileName(x))
                        .Where(x => x.Contains(name + "_"))
                        .Select(x => x.Replace(name, "0").Replace("_", "0").ToInt())
                        .OrderBy(x => x)
                        .LastOrDefault() + 1;

                    path = folder + name + "_" + num.ToString() + Path.GetExtension(file.FileName);
                }

                file.SaveAs(path);
            }
        }

        /// <summary>
        /// Удаление файла в задаче
        /// </summary>
        internal static void Delete(string fileName)
        {
            string path = HttpContext.Current.Server.MapPath(fileName);
            string folder = Path.GetDirectoryName(path);
        	Debug.Assert(folder != null, "folder != null");
        	if (Directory.Exists(folder))
            {
                if (File.Exists(path))
                    File.Delete(path);

                if (!Directory.GetFiles(folder).Any())
                    Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// Удаление всех файлов задачи
        /// </summary>
        /// <param name="task"></param>
        internal static void Delete(ITask task)
        {
            string folder = GetStoragePath(task);
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

        }
    }
}