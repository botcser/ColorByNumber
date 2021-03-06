using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Assets.GifAssets.PowerGif
{
	/// <summary>
	/// Implements converting data from SimpleGif library (Texture2D and Color32).
	/// </summary>
	public static class Converter
	{
		/// <summary>
		/// Convert GIF frames from SimpleGif to PowerGif.
		/// </summary>
		public static List<GifFrame> ConvertFrames(List<SimpleGif.Data.GifFrame> frames)
		{
			var list = frames.Select(i => new GifFrame(ConvertTexture(i.Texture), i.Texture.width, i.Texture.height, i.Delay)).ToList();

			return list;
        }

		/// <summary>
		/// Convert GIF frames from PowerGif to SimpleGif.
		/// </summary>
		public static List<SimpleGif.Data.GifFrame> ConvertFrames(List<GifFrame> frames)
		{
			return frames.Select(i => new SimpleGif.Data.GifFrame { Texture = ConvertTexture(i.Encoded), Delay = i.Delay }).ToList();
		}

		public static byte[] ConvertTexture(SimpleGif.Data.Texture2D source)
		{
			var array = new byte[source.width * source.height * 4];
            var i = 0;

            foreach (var pixel in source.GetPixels32())
            {
				array[i++] = pixel.r;
                array[i++] = pixel.g;
                array[i++] = pixel.b;
                array[i++] = pixel.a;
			}
			
            var bytes = ImageConversion.EncodeArrayToPNG(array, GraphicsFormat.R8G8B8A8_UNorm, (uint) source.width, (uint) source.height);
			
			return bytes;
		}

		public static SimpleGif.Data.Texture2D ConvertTexture(byte[] source)
		{
			var temp = new Texture2D(2, 2) { filterMode = FilterMode.Point };

			temp.LoadImage(source);

			var texture = new SimpleGif.Data.Texture2D(temp.width, temp.height);
			var pixels = temp.GetPixels32().Select(i => new SimpleGif.Data.Color32(i.r, i.g, i.b, i.a)).ToArray();

			Object.Destroy(temp);

			texture.SetPixels32(pixels);
			texture.Apply();

			return texture;
		}

		/// <summary>
		/// Convert PowerGif.Gif to SimpleGif.Gif.
		/// </summary>
		public static SimpleGif.Gif Convert(Gif gif)
		{
			return new SimpleGif.Gif(ConvertFrames(gif.Frames));
		}
	}
}