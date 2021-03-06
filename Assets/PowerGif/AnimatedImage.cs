using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GifAssets.PowerGif
{
	/// <summary>
	/// This script simply switches GIF-frames (textures) to get "animation" effect.
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class AnimatedImage : MonoBehaviour
	{
		public Image Image;
        public bool PlayOnEnable;
        public bool UnloadOnDisable;

        public Gif Gif;

        private Coroutine _coroutine;

        /// <summary>
        /// Will play GIF (if it's assigned) on app start if script is enabled.
        /// </summary>
        public void Start()
		{
            if (!Image)
            {
                Image = GetComponent<Image>();
            }
		}

        public void OnEnable()
        {
            if (PlayOnEnable && Gif != null) Play();
        }

        public void OnDisable()
        {
            if (UnloadOnDisable)
            {
                Unload();
            }
        }

        /// <summary>
        /// Play GIF.
        /// </summary>
        public void Play(Gif gif)
		{
			Gif = gif;

            if (gameObject.activeInHierarchy)
            {
                Play();
            }

            PlayOnEnable = !gameObject.activeInHierarchy;
        }

        public void SetStaticSprite(Sprite sprite)
        {
            Reset();
            Gif = null;
            Image.sprite = sprite;
        }

        public void Unload()
        {
            if (Gif == null)
            {
                if (Image.sprite != null && Image.sprite.texture != null)
                {
                    Destroy(Image.sprite.texture);
                }
            }
            else
            {
                Gif.Frames.ForEach(i => i.Unload());
            }

            Image.sprite = null;
        }

        private void Play()
        {
            Reset();
            _coroutine = StartCoroutine(Animate(0));
        }

		private IEnumerator Animate(int index)
		{
			var texture = Gif.Frames[index].ToTexture2D();

            Image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            Image.preserveAspect = true;

			if (Gif.Frames.Count == 1) yield break;

			var delay = Gif.Frames[index].Delay;

			if (delay < 0.02f) // Chrome browser behaviour
			{
				delay = 0.1f;
			}

			yield return new WaitForSeconds(delay);

			if (++index == Gif.Frames.Count)
			{
				index = 0;
			}

            yield return Animate(index);
		}

        private void Reset()
        {
            if (_coroutine == null) return;

            StopCoroutine(_coroutine);
            _coroutine = null;
        }
	}
}