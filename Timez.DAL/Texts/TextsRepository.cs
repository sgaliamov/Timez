using System;
using Common.Extentions;
using System.Linq;
using Timez.DAL.DataContext;
using System.Data.Linq;
using Timez.Entities;

namespace Timez.DAL.Texts
{
	public interface ITextsRepository
	{
		IQueryable<IText> Get(TextType type, int page, int count, out int total, bool? visible);
		IText Get(int id);
		IText Save(int? id, string title, string content, bool? isVisible, TextType? type = null);
		IText Delete(int id);
	}

	class TextsRepository : BaseRepository<TextsRepository>, ITextsRepository
	{
		public IQueryable<IText> Get(TextType type, int page, int count, out int total, bool? visible)
		{
			var list = _Context.Texts
				.Where(x => x.TypeId == (int)type && (!visible.HasValue || x.IsVisible == visible))
				.OrderByDescending(x => x.CreationDateTime)
				.GetPaged(page, count, out total);

			return list;
		}

		public IText Get(int id)
		{
			return _Get(_Context, id);
		}
		static readonly Func<TimezDataContext, int, Text> _Get =
			CompiledQuery.Compile((TimezDataContext ctx, int id) => ctx.Texts.FirstOrDefault(x => x.Id == id));

		public IText Save(int? id, string title, string content, bool? isVisible, TextType? type = null)
		{
			Text text;
			if (id.HasValue)
			{
				text = _Get(_Context, id.Value);
				if (text != null)
				{
					text.Title = title;
					text.Content = content;

					if (isVisible.HasValue)
						text.IsVisible = isVisible.Value;

					if (type.HasValue)
						text.TypeId = (int)type.Value;
				}
			}
			else
			{
				text = new Text
				{
					Title = title,
					Content = content,
					CreationDateTime = DateTimeOffset.Now,
					IsVisible = isVisible ?? true,
					TypeId = (int)(type ?? TextType.News)
				};
				_Context.Texts.InsertOnSubmit(text);
			}
			_Context.SubmitChanges();

			return text;
		}

		public IText Delete(int id)
		{
			Text text = _Get(_Context, id);
			if (text != null)
			{
				_Context.Texts.DeleteOnSubmit(text);
				_Context.SubmitChanges();
			}

			return text;
		}
	}
}
