// SampleDataCommon.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System;
using JetBrains.Annotations;
using Print4e.Common;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Print4e.Data
{
	/// <summary>
	///    Base class for <see cref="Card" /> and <see cref="Character" /> that
	///    defines properties common to both.
	/// </summary>
	public abstract class SampleDataCommon : BindableBase
	{
		private static readonly Uri BaseUri = new Uri("ms-appx:///");

		protected SampleDataCommon([NotNull] String uniqueId,
			String title,
			String subtitle,
			String imagePath,
			String description)
		{
			_uniqueId = uniqueId;
			_title = title;
			_subtitle = subtitle;
			_description = description;
			_imagePath = imagePath;
		}

		[NotNull] private string _uniqueId = string.Empty;

		[NotNull]
		public string UniqueId { get { return _uniqueId; } set { SetProperty(ref _uniqueId, value); } }

		private string _title = string.Empty;
		public string Title { get { return _title; } set { SetProperty(ref _title, value); } }

		private string _subtitle = string.Empty;
		public string Subtitle { get { return _subtitle; } set { SetProperty(ref _subtitle, value); } }

		private string _description = string.Empty;
		public string Description { get { return _description; } set { SetProperty(ref _description, value); } }

		private ImageSource _image;
		private String _imagePath;

		public ImageSource Image
		{
			get
			{
				if (_image == null && _imagePath != null) _image = new BitmapImage(new Uri(BaseUri, _imagePath));
				return _image;
			}

			set
			{
				_imagePath = null;
				SetProperty(ref _image, value);
			}
		}

		public void SetImage(String path)
		{
			_image = null;
			_imagePath = path;
			OnPropertyChanged("Image");
		}

		public override string ToString()
		{
			return Title;
		}
	}
}
