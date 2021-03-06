using System.Collections.Generic;
using SimpleGif.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GifAssets.PowerGif.Examples.Scripts
{
	/// <summary>
	/// Example how to convert True Color image to 8 bit image.
	/// </summary>
	public class ConvertTo8BitsExample : ExampleBase
	{
		public Image Source;
		public Image Converted666;
		public Image Converted676;
		public Image Converted685;
		public Image Converted884;
		public Image ConvertedGrayscale;

		public void Start()
		{
			var frame = new GifFrame(((Texture2D) Source.mainTexture).EncodeToPNG(), Source.mainTexture.width, Source.mainTexture.height, 1);
			var frames = new List<GifFrame> { frame };
			var gif = new Gif(frames);

			Convert(gif, Converted666, MasterPalette.Levels666);
			Convert(gif, Converted676, MasterPalette.Levels676);
			Convert(gif, Converted685, MasterPalette.Levels685);
			Convert(gif, Converted884, MasterPalette.Levels884);
			Convert(gif, ConvertedGrayscale, MasterPalette.Grayscale);
		}

		private static void Convert(Gif gif, Image image, MasterPalette palette)
		{
			var bytes = gif.Encode(palette: palette);
			var texture = Gif.Decode(bytes).Frames[0].ToTexture2D();

			image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
		}
	}
}