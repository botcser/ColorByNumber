using SimpleGif.Enums;
using UnityEngine;

namespace Assets.GifAssets.PowerGif
{
	/// <summary>
	/// Texture + delay + disposal method
	/// </summary>
	public class GifFrame
	{
		public byte[] Encoded;
		public int Width;
		public int Height;
		public float Delay;
		public DisposalMethod DisposalMethod;

		public GifFrame(byte[] encoded, int width, int height, float delay, DisposalMethod disposalMethod = DisposalMethod.RestoreToBackgroundColor)
		{
			Encoded = encoded;
			Width = width;
			Height = height;
			Delay = Mathf.Max(0.02f, delay);
			DisposalMethod = disposalMethod;
		}

		private Texture2D _texture;
		private int _hash;

		public Texture2D ToTexture2D()
		{
			if (_texture == null || Encoded.Length != _hash)
			{
				_texture = new Texture2D(2, 2) { filterMode = FilterMode.Point };
				_texture.LoadImage(Encoded);
				_hash = Encoded.Length;
			}

			return _texture;
		}

        public void Unload()
        {
            if (_texture != null)
            {
				Object.Destroy(_texture);
                _texture = null;
            }
		}
	}
}