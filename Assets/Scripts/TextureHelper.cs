using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Assets.Scripts.Data;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts
{
	public static class TextureHelper
	{
		public static Sprite CreateSprite(Texture2D texture)
		{
			return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
		}

	    public static Sprite CreateSprite(byte[] bytes)
	    {
	        return CreateSprite(CreateTexture(bytes));
	    }

	    public static Sprite CreateSprite(string path) // TODO: https://fogbugz.unity3d.com/default.asp?1191022_6od906uj8kcsck5s
        {
		    if (!File.Exists(path)) return null;

			var bytes = File.ReadAllBytes(path);
			var texture = new Texture2D(2, 2) { filterMode = FilterMode.Point };

		    if (!texture.LoadImage(bytes)) return null;

		    return CreateSprite(texture);
		}

		public static Texture2D CreateTexture(int width, int height)
		{
			return CreateTexture(width, height, Color.clear);
		}

		public static Texture2D CreateTexture(int width, int height, Color backgroundColor)
		{
			var texture = new Texture2D(width, height) { filterMode = FilterMode.Point };
			var pixels = new Color[width * height];

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i] = backgroundColor;
			}

			texture.SetPixels(pixels);
			texture.Apply();

			return texture;
		}

		public static Texture2D CreateTexture(string base64)
		{
			var texture = new Texture2D(2, 2) { filterMode = FilterMode.Point };

			texture.LoadImage(Convert.FromBase64String(base64));
			texture.Apply();

			return texture;
		}

		public static Texture2D CreateTexture(byte[] bytes)
		{
			var texture = new Texture2D(2, 2) { filterMode = FilterMode.Point };

			texture.LoadImage(bytes);
			texture.Apply();

			return texture;
		}

		public static void ClearTexture(ref Texture2D texture, Color color, bool apply = true)
		{
			var pixels = new Color[texture.width * texture.height];

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i] = color;
			}

			texture.SetPixels(pixels);
			texture.Apply();
		}

		public static void FillTexture(ref Texture2D texture, Position position, Color32 color, float tolerance)
		{
			var pixels = FillTexture(texture.GetPixels32(), texture.width, texture.height, position, color, tolerance);

			texture.SetPixels32(pixels);
			texture.Apply();
		}

		public static Color32[] FillTexture(Color32[] pixels, int width, int height, Position position, Color32 color, float tolerance)
		{
			pixels = FloodFillScanLine(pixels, width, height, position, color, tolerance);

			return pixels;
		}

		public static List<Position> MagicWandSelect(ref Texture2D texture, Position position, float tolerance)
		{
			var positions = new List<Position>();
            
			FloodFillScanLine(texture.GetPixels32(), texture.width, texture.height, position, Color.white - texture.GetPixel(position.X, position.Y), tolerance, positions);

			return positions;
		}

	    public static Color32[] DrawGradient(ImageRect rect, Color32 startColor, Color32 endColor, Vector2 direction, int steps)
	    {
	        steps = Mathf.Max(2, steps);
	        steps = Mathf.Min(Constants.MaxGradientSteps, steps);

            var pixels = new Color32[rect.Width * rect.Height];
	        var gradient = new Color32[steps];

	        for (var i = 0; i < steps; i++)
	        {
	            var part = (float) i / (steps - 1);

	            var r = (byte) Mathf.RoundToInt(startColor.r + (endColor.r - startColor.r) * part);
	            var g = (byte) Mathf.RoundToInt(startColor.g + (endColor.g - startColor.g) * part);
	            var b = (byte) Mathf.RoundToInt(startColor.b + (endColor.b - startColor.b) * part);
	            var a = (byte) Mathf.RoundToInt(startColor.a + (endColor.a - startColor.a) * part);

	            gradient[i] = new Color32(r, g, b, a);
            }

            for (var y = 0; y < rect.Height; y++)
	        {
	            for (var x = 0; x < rect.Width; x++)
	            {
	                int index;

	                if (direction == Vector2.right)
	                {
	                    index = Mathf.FloorToInt((float) x / rect.Width * steps);
	                }
	                else if (direction == Vector2.down)
	                {
	                    index = Mathf.FloorToInt((float) (rect.Height - y - 1) / rect.Height * steps);
	                }
	                else if (direction == Vector2.left)
	                {
	                    index = Mathf.FloorToInt((float) (rect.Width - x - 1) / rect.Width * steps);
	                }
	                else if (direction == Vector2.up)
	                {
	                    index = Mathf.FloorToInt((float) y / rect.Height * steps);
	                }
	                else
	                {
	                    throw new NotSupportedException();
	                }
                    
                    pixels[x + y * rect.Width] = gradient[index];
                }
	        }

            return pixels;
	    }

	    public static bool IsErrorTexture(Texture2D texture)
	    {
            return texture.width == 8 && texture.height == 8 && Md5.ComputeHash(texture.EncodeToPNG()) == "268d45e0a005cf993b3dd90495a94957";
	    }

        private static readonly Stack<Position> Stack = new Stack<Position>();

	    private static Color32[] FloodFillStackBased(Color32[] pixels, int width, int height, Position position, Color32 color, float tolerance = 0, List<Position> positions = null)
	    {
	        var targetColor = pixels[position.X + position.Y * width];
	        var tolerance255 = Mathf.RoundToInt(255 * tolerance);

	        if (tolerance255 == 0 && targetColor.Equals(color)) return pixels;

            Stack.Clear();
	        Stack.Push(position);

	        Func<Color32, Color32, bool> equals;

            if (tolerance255 == 0)
            {
                equals = (a, b) => a.Equals(b);
            }
            else
            {
                equals = (a, b) => a.SimilarTo(b, tolerance255) && !a.Equals(color);
            }

            var sw = Stopwatch.StartNew();

            while (Stack.Count > 0)
	        {
	            var first = Stack.Pop();

                if (first.X < width && first.X >= 0 && first.Y < height && first.Y >= 0)
	            {
                    if (equals(pixels[first.X + first.Y * width], targetColor))
	                {
	                    pixels[first.X + first.Y * width] = color;
	                    positions?.Add(first);
                        Stack.Push(new Position(first.X - 1, first.Y));
	                    Stack.Push(new Position(first.X + 1, first.Y));
	                    Stack.Push(new Position(first.X, first.Y - 1));
	                    Stack.Push(new Position(first.X, first.Y + 1));
	                }
	            }

	            if (sw.Elapsed.TotalSeconds > 10)
	            {
	                Debug.LogError("Loop!");
	                break;
	            }
            }

	        Stack.Clear();

	        return pixels;
        }

        private static Color32[] FloodFillScanLine(Color32[] pixels, int width, int height, Position position, Color32 color, float tolerance = 0, List<Position> positions = null)
        {
            var targetColor = pixels[position.X + position.Y * width];
		    var tolerance255 = Mathf.RoundToInt(255 * tolerance);

            if (tolerance255 == 0 && targetColor.Equals(color)) return pixels;

			Stack.Clear();
			Stack.Push(position);

            Func<Color32, Color32, bool> equals;

            if (tolerance255 == 0)
            {
                equals = (a, b) => a.Equals(b);
            }
            else
            {
                equals = (a, b) => a.SimilarTo(b, tolerance255) && !a.Equals(color);
            }

            var sw = Stopwatch.StartNew();

		    while (Stack.Count > 0)
			{
			    var temp = Stack.Pop();
                var y1 = temp.Y;

                while (y1 >= 0 && equals(pixels[temp.X + y1 * width], targetColor))
				{
					y1--;
				}

				y1++;

				var spanLeft = false;
				var spanRight = false;

			    while (y1 < height && equals(pixels[temp.X + y1 * width], targetColor))
			    {
			        pixels[temp.X + y1 * width] = color;
                    positions?.Add(new Position(temp.X, y1));

			        if (!spanLeft && temp.X > 0 && equals(pixels[temp.X - 1 + y1 * width], targetColor))
			        {
                        Stack.Push(new Position(temp.X - 1, y1));
			            spanLeft = true;
			        }
			        else if (spanLeft && (temp.X - 1 == 0 || !equals(pixels[temp.X - 1 + y1 * width], targetColor)))
                    {
			            spanLeft = false;
			        }
			        if (!spanRight && temp.X < width - 1 && equals(pixels[temp.X + 1 + y1 * width], targetColor))
                    {
                        Stack.Push(new Position(temp.X + 1, y1));
			            spanRight = true;
			        }
			        else if (spanRight && (temp.X < width - 1 || !equals(pixels[temp.X + 1 + y1 * width], targetColor)))
                    {
			            spanRight = false;
			        }

			        y1++;
			    }

                if (sw.Elapsed.TotalSeconds > 10)
			    {
			        Debug.LogError("Loop!");
			        break;
			    }
            }

			Stack.Clear();

			return pixels;
		}

		public static void ReplaceColor(ref Texture2D texture, Color32 target, Color32 color, float tolerance)
		{
			var pixels = ReplaceColor(texture.GetPixels32(), target, color, tolerance);

			texture.SetPixels32(pixels);
			texture.Apply();
		}

		public static Color32[] ReplaceColor(Color32[] pixels, Color32 target, Color32 color, float tolerance)
		{
		    var tolerance255 = Mathf.RoundToInt(255 * tolerance);

            for (var i = 0; i < pixels.Length; i++)
			{
				if (pixels[i].SimilarTo(target, tolerance255))
				{
					pixels[i] = color;
				}
			}

			return pixels;
		}

		public static Texture2D ScaleTexture(Texture2D target, int scale)
		{
			if (scale <= 1) return target;

			var texture = new Texture2D(scale * target.width, scale * target.height);
			var pixels = target.GetPixels();
			var pixelsEx = new Color[scale * scale * pixels.Length];

			for (var y = 0; y < texture.height; y++)
			{
				for (var x = 0; x < texture.width; x++)
				{
					pixelsEx[x + y * texture.width] = pixels[x / scale + y / scale * target.width];
				}
			}

			texture.SetPixels(pixelsEx);
			texture.Apply();

			return texture;
		}

		public static Color32[] ResizeTexture(Color32[] pixels, int width, int height, float scale)
		{
			var widthEx = (int) (width * scale);
			var heightEx = (int) (height * scale);
			var pixelsEx = new Color32[widthEx * heightEx];
			var offsetX = (int) ((widthEx - width) / 2f);
			var offsetY = (int) ((heightEx - height) / 2f);

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var x_ = x + offsetX;
					var y_ = y + offsetY;

					if (x_ >= 0 && x_ < widthEx && y_ >= 0 && y_ < heightEx)
					{
						pixelsEx[x_ + y_ * widthEx] = pixels[x + y * width];
					}
				}
			}

			return pixelsEx;
		}

		public static Texture2D ResizeCanvas(Texture2D texture, int width, int height, Vector2 pivot)
		{
			var textureEx = CreateTexture(width, height);
			var pixelsEx = textureEx.GetPixels();
			var pixels = texture.GetPixels();

			var offsetX = (int) pivot.x == -1 ? 0 : (int) pivot.x == 1 ? textureEx.width - texture.width : (textureEx.width - texture.width) / 2;
			var offsetY = (int) pivot.y == -1 ? 0 : (int) pivot.y == 1 ? textureEx.height - texture.height : (textureEx.height - texture.height) / 2;

			for (var y = 0; y < texture.height; y++)
			{
				for (var x = 0; x < texture.width; x++)
				{
					var x_ = x + offsetX;
					var y_ = y +  offsetY;

					if (x_ >= 0 && x_ < textureEx.width && y_ >= 0 && y_ < textureEx.height)
					{
						pixelsEx[x_ + y_ * textureEx.width] = pixels[x + y * texture.width];
					}
				}
			}

			textureEx.SetPixels(pixelsEx);

			return textureEx;
		}

	    /*public static void ResizeTexture(ref Texture2D texture, int width, int height, bool useBilinear = false)
	    {
            TextureScalerCpu.Scale(texture, width, height, useBilinear);
        }
		*/
		public static void AddOutline(Texture2D texture, int outline = 1, bool apply = true)
		{
			if (outline == 0) return;

			var source = texture.GetPixels32();
			var target = source.ToArray();

			for (var y = 0; y < texture.height; y++)
			{
				for (var x = 0; x < texture.width; x++)
				{
					if (source[x + y * texture.width].a == 0) continue; 

					var near = new[]
					{
						new Position(x - 1, y - 1), new Position(x, y - 1), new Position(x + 1, y - 1),
						new Position(x - 1, y), new Position(x + 1, y),
						new Position(x - 1, y + 1), new Position(x, y + 1), new Position(x + 1, y + 1)
					};

					foreach (var p in near)
					{
						if (p.X >= 0 && p.X < texture.width && p.Y >= 0 && p.Y < texture.height)
						{
							if (source[p.X + p.Y * texture.width].a == 0)
							{
								target[p.X + p.Y * texture.width] = Color.black;
							}
							else
							{
								target[p.X + p.Y * texture.width] = source[p.X + p.Y * texture.width];
							}
						}
					}
				}
			}

			texture.SetPixels32(target);

			if (apply)
			{
				texture.Apply();
			}
		}

		public static Texture2D Crop(Texture2D texture, ImageRect rect)
		{
			var source = texture.GetPixels32();
			var pixels = new Color32[rect.Width * rect.Height];

			for (var x = 0; x < rect.Width; x++)
			{
				for (var y = 0; y < rect.Height; y++)
				{
					pixels[x + y * rect.Width] = source[x + rect.Min.X + (y + rect.Min.Y) * texture.width];
				}
			}

			var cropped = new Texture2D(rect.Width, rect.Height) { filterMode = texture.filterMode };

			cropped.SetPixels32(pixels);
			cropped.Apply();

			return cropped;
		}
		/*
		public static Position EvaluateSpriteSheetSize(Project project, int columns = 0)
        {
			columns = columns == 0 ? Mathf.CeilToInt(Mathf.Sqrt(project.ActiveClip.Frames.Count)) : columns;

			var rows = Mathf.CeilToInt((float) project.ActiveClip.Frames.Count / columns);

			return new Position(columns, rows);
		}

		public static Texture2D CreateSpriteSheet(Project project, int columns = 0, int spacing = 0, Texture2D t = null)
        {
            var size = EvaluateSpriteSheetSize(project, columns);
            var width = project.Width * size.X + spacing * (size.X - 1);
            var height = project.Height * size.Y + spacing * (size.Y - 1);

			if (width > Constants.MaxImageSizeEx || height > Constants.MaxImageSizeEx)
			{
				throw new Exception(string.Format("Unable to fit the project to {0}x{0} px atlas.", Constants.MaxImageSizeEx));
			}

			var texture = t ?? CreateTexture(width, height);

            if (t != null)
            {
                texture.Resize(width, height);
				ClearTexture(ref texture, Color.clear, apply: false);
            }

			var index = 0;

			for (var y = 0; y < size.Y; y++)
			{
				for (var x = 0; x < size.X; x++)
				{
					var pixels = project.ActiveClip.Frames[index++].FinalizeTexture(project).GetPixels32();

					texture.SetPixels32(x * (project.Width + spacing), (size.Y - y - 1) * (project.Height + spacing), project.Width, project.Height, pixels);

					if (index == project.ActiveClip.Frames.Count)
					{
						texture.Apply();

						return texture;
					}
				}
			}

			throw new Exception("Unknown exception.");
		}
		*/
		public static bool InBounds(this Texture2D texture, Position position)
		{
			return position.X >= 0 && position.X < texture.width && position.Y >=0 && position.Y < texture.height;
		}

	    public static void Rotate90(Texture2D texture, bool right = true)
	    {
	        var pixels = texture.GetPixels32();
            var rotated = new Color32[pixels.Length];

            for (var y = 0; y < texture.height; y++)
	        {
	            for (var x = 0; x < texture.width; x++)
	            {
	                var p = pixels[x + y * texture.width];

	                if (p.a <= 0) continue;

	                var rx = right ? y : texture.height - 1 - y;
	                var ry = !right ? x : texture.width - 1 - x;

	                rotated[rx + ry * texture.height] = p;
	            }
	        }

	        texture.Resize(texture.height, texture.width);
	        texture.SetPixels32(rotated);
	        texture.Apply();
        }

	    public static void SetPixels32(this Texture2D texture, ImageRect rect, Color32[] pixels)
	    {
	        texture.SetPixels32(rect.X, rect.Y, rect.Width, rect.Height, pixels);
	    }

        public static void SetOpaquePixels32(this Texture2D texture, ImageRect rect, Color32[] pixels)
	    {
	        var source = texture.GetPixels(rect.X, rect.Y, rect.Width, rect.Height);

	        for (var i = 0; i < source.Length; i++)
	        {
	            if (pixels[i].a > 0)
	            {
	                source[i] = pixels[i];
	            }
	        }

            texture.SetPixels(rect.X, rect.Y, rect.Width, rect.Height, source);
        }

	    public static Color MergeColors(Color background, Color foreground)
	    {
	        if (Mathf.Approximately(foreground.a, 0) || Mathf.Approximately(foreground.a, 1) || Mathf.Approximately(background.a, 0)) return foreground;

            foreground.r *= foreground.a;
	        foreground.g *= foreground.a;
	        foreground.b *= foreground.a;

	        return background * (1 - foreground.a) + foreground;
        }

        public static Color32[] Adjust(Color32[] pixels, float hue, float saturation, float lightness)
        {
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Adjust(pixels[i], hue, saturation, lightness);
            }

            return pixels;
        }

        public static Color[] Adjust(Color[] pixels, float hue, float saturation, float lightness)
        {
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Adjust(pixels[i], hue, saturation, lightness);
            }

            return pixels;
        }

        public static Color Adjust(Color color, float hue, float saturation, float lightness)
        {
            hue /= 180f;
            saturation /= 100f;
            lightness /= 100f;

            var a = color.a;

            Color.RGBToHSV(color, out var h, out var s, out var v);

            h += hue / 2f;

            if (h > 1) h -= 1;
            else if (h < 0) h += 1;

            color = Color.HSVToRGB(h, s, v);

            var grey = 0.3f * color.r + 0.59f * color.g + 0.11f * color.b;

            color.r = grey + (color.r - grey) * (saturation + 1);
            color.g = grey + (color.g - grey) * (saturation + 1);
            color.b = grey + (color.b - grey) * (saturation + 1);

            if (color.r < 0) color.r = 0;
            if (color.g < 0) color.g = 0;
            if (color.b < 0) color.b = 0;

            color.r += lightness;
            color.g += lightness;
            color.b += lightness;
            color.a = a;

            return color;
        }

        public static void DestroyTexture(Texture2D texture)
        {
            if (Application.isEditor)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
            else
            {
				UnityEngine.Object.Destroy(texture);
			}
		}
	}
    public static class Md5
    {
        /// <summary>
        /// Compute Md5-hash from string.
        /// </summary>
        public static string ComputeHash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            return ComputeHash(inputBytes);
        }

        public static string ComputeHash(byte[] inputBytes)
        {
            var hash = MD5.Create().ComputeHash(inputBytes);
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < hash.Length; i++)
            {
                stringBuilder.Append(hash[i].ToString("X2"));
            }

            return stringBuilder.ToString().ToLower();
        }
    }
}