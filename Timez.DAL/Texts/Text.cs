using Common.Extentions;
using System;
using Timez.Entities;

namespace Timez.DAL.DataContext
{
	internal partial class Text : IText
	{
		public TextType Type
		{
			get { return (TextType)TypeId; }
		}		
	}
}
