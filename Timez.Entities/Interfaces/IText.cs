using System;

namespace Timez.Entities
{
	public interface IText : IId
	{
		string Title { get; set; }
		string Content { get; set; }
		int TypeId { get; set; }
		DateTimeOffset CreationDateTime { get; set; }
		bool IsVisible { get; set; }

		#region Дополнительные методы

		TextType Type { get; }

		#endregion
	}
}