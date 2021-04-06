using System;
using System.Collections;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    public static class Extensions
    {
        public static T ToEnum<T>(this string value)
        {
            return (T) Enum.Parse(typeof(T), value);
        }

        public static bool IsEmpty(this string target)
        {
            return string.IsNullOrEmpty(target);
        }

        public static void Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

        public static void ClearImmediately(this Transform transform)
        {
            while (transform.childCount > 0)
            {
                Object.DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public static void SetActive(this Component target, bool active)
        {
            target.gameObject.SetActive(active);
        }

        public static void SetParentActive(this Component target, bool active)
        {
            target.transform.parent.gameObject.SetActive(active);
        }

        /// <summary>
        /// Consider using CopyTo and CopyFrom to avoid memory allocations.
        /// </summary>
        public static Texture2D Copy(this Texture2D texture)
        {
            var copy = new Texture2D(texture.width, texture.height) { filterMode = texture.filterMode };

            copy.SetPixels(texture.GetPixels());
            copy.Apply();

            return copy;
        }

        public static Color[] GetPixels(this Texture2D texture, ImageRect rect)
        {
            var min = rect.Min;
            var max = rect.Max;

            if (min.X < 0) min.X = 0;
            if (min.Y < 0) min.Y = 0;
            if (min.X >= texture.width) min.X = texture.width - 1;
            if (min.Y >= texture.height) min.Y = texture.height - 1;

            if (max.X < 0) max.X = 0;
            if (max.Y < 0) max.Y = 0;
            if (max.X >= texture.width) max.X = texture.width - 1;
            if (max.Y >= texture.height) max.Y = texture.height - 1;

            return texture.GetPixels(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
        }

        public static void SetPixels(this Texture2D texture, ImageRect rect, Color[] pixels)
        {
            texture.SetPixels(rect.X, rect.Y, rect.Width, rect.Height, pixels);
        }

        public static void CopyTo(this Texture2D source, Texture2D target)
        {
            if (target == null)
            {
                target = TextureHelper.CreateTexture(source.width, source.height);
            }
            else if (target.width != source.width || target.height != source.height)
            {
                target.Resize(source.width, source.height, source.format, false);
            }

            target.SetPixels(source.GetPixels());
            target.Apply();
        }

        public static void CopyFrom(this Texture2D target, Texture2D source)
        {
            if (target == null)
            {
                target = TextureHelper.CreateTexture(source.width, source.height);
            }
            else if (target.width != source.width || target.height != source.height)
            {
                target.Resize(source.width, source.height, source.format, false);
            }

            target.SetPixels(source.GetPixels());
            target.Apply();
        }

        public static void SetAlpha(this Image target, float alpha)
        {
            var color = target.color;

            color.a = alpha;
            target.color = color;
        }

        public static bool SimilarTo(this Color32 a, Color32 b, int tolerance)
        {
            return Math.Abs(a.r - b.r) <= tolerance &&
                   Math.Abs(a.g - b.g) <= tolerance &&
                   Math.Abs(a.b - b.b) <= tolerance &&
                   Math.Abs(a.a - b.a) <= tolerance;
        }

        public static Coroutine ExecuteInNextUpdate(this MonoBehaviour monoBehaviour, Action action)
        {
            IEnumerator ExecuteInNextUpdate()
            {
                yield return null;
                action();
            }

            return monoBehaviour.StartCoroutine(ExecuteInNextUpdate());
        }

        public static Coroutine ExecuteIn(this MonoBehaviour monoBehaviour, Action action, float seconds)
        {
            IEnumerator ExecuteInNextUpdate()
            {
                yield return new WaitForSeconds(seconds);
                action();
            }

            return monoBehaviour.StartCoroutine(ExecuteInNextUpdate());
        }
    }
}