using System;
using JetBrains.Annotations;

namespace Print4e.Data
{
	/// <summary>
	///    Generic item data model.
	/// </summary>
	public class Card : SampleDataCommon
	{
		public Card([NotNull] String uniqueId,
			String title,
			String subtitle,
			String imagePath,
			String description,
			String content,
			Character group) : base(uniqueId, title, subtitle, imagePath, description)
		{
			_content = content;
			_group = group;
		}

		private string _content = string.Empty;
		public string Content { get { return _content; } set { SetProperty(ref _content, value); } }

		private Character _group;
		public Character Group { get { return _group; } set { SetProperty(ref _group, value); } }
	}
}