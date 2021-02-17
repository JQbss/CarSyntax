using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace CarSyntax.utils
{
	public static class BaseToImageConverter
	{		public static BitmapImage Base64StringToBitmap(string base64String)
		{
			byte[] byteBuffer = Convert.FromBase64String(base64String);
			BitmapImage bitmapImage = new BitmapImage();
			MemoryStream ms = new MemoryStream(byteBuffer);
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = ms;
			bitmapImage.EndInit();

			return bitmapImage;
		}
	}
}
