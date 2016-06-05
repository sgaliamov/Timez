using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Entities;

namespace Timez.Models
{
	public class News //: IText
	{
		public News(IText text)
		{
			Id = text.Id;
			Title = text.Title;
			Content = text.Content;
			//TypeId = text.TypeId;
			CreationDateTime = text.CreationDateTime;
			IsVisible = text.IsVisible;
		}

		public News() { }

		[HiddenInput(DisplayValue = false)]
		public int? Id { get; set; }

		[Display(Name = "Заголовок")]
		[Required]
		public string Title { get; set; }

		/// <summary>
		/// Краткая вырезка первых N символов
		/// или текст до ката 
		/// </summary>
		[Display(Name = "Превью")]
		[UIHint("MultilineText")]
		[ScaffoldColumn(false)]
		public string Brief
		{
			get
			{
				int begin = Content.IndexOf(CutTagBegin, 0, StringComparison.InvariantCultureIgnoreCase);
				if (begin >= 0)
				{
					int end = Content.IndexOf(CutTagEnd, begin + CutTagBegin.Length, StringComparison.InvariantCultureIgnoreCase);
					if (end > begin)
					{
						return Content.Substring(begin + CutTagBegin.Length, end - begin - CutTagBegin.Length);
					}
				}

				// TODO: окуратно закрывать теги
				return Content.CutText(CutLenght);
			}
		}

		[Display(Name = "Текст")]
		[UIHint("MultilineText")]
		[Required]
		public string Content { get; set; }

		[DataType(DataType.DateTime)]
		[Display(Name = "Дата создания")]
		[HiddenInput(DisplayValue = true)]
		public DateTimeOffset? CreationDateTime { get; set; }

		[Display(Name = "Показывать")]
		[DefaultValue(true)]
		public bool IsVisible { get; set; }

		const string CutTagBegin = "<cut>";
		const string CutTagEnd = "</cut>";
		const int CutLenght = 500;		

		//public int TypeId
		//{
		//    get
		//    {
		//        throw new NotImplementedException();
		//    }
		//    set
		//    {
		//        throw new NotImplementedException();
		//    }
		//}

		//public TextType Type
		//{
		//    get { return TextType.News; }
		//}
	}
}