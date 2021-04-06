using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ColorByNumberInit : MonoBehaviour
    {
        public Image Image;
        public Text NumberPrefab;
        public Texture2D SourceTexture;
        public ColorGridCell ColorPrefab;
        public GridLayoutGroup GridNumbers;
        public ToggleGroup GridPaletteNew;
        public RectTransform DraftRect;
        public RectTransform ParentRect;
        public RectTransform SelectMarker;

        public static RectTransform GParentRect;
        public static bool Continue;
        public static Texture2D TextureBackup;
        public static Vector2 ImageRectTransformOriginSize;
        public static Vector2 GridNumbersOriginSize;
        public static Vector2 ImageRectTransformOriginPosition;
        public static List<Color> PaletteColors;
        public static Color[] Pixels;
        public static GridLayoutGroup GGridNumbers;
        public static Text GNumberPrefab;

        public static List<Text> TextNumbers;

        public void Start()
        {
            //SourceTexture = UserImagePage.Instance.UserImage.ToTexture();

            ColorByNumber.SourceTexture = SourceTexture;                

            GParentRect = ParentRect;

            TransparentPixelReplace(SourceTexture, Color.grey);

            var grayscaleTexture = new Texture2D(SourceTexture.width, SourceTexture.height) { filterMode = FilterMode.Point };
            Pixels = SourceTexture.GetPixels();

            grayscaleTexture.SetPixels(Pixels.Select(i => new Color(i.grayscale, i.grayscale, i.grayscale)).ToArray());
            grayscaleTexture.Apply();

            Image.sprite = Sprite.Create(grayscaleTexture, new Rect(0, 0, grayscaleTexture.width, grayscaleTexture.height), Vector2.one / 2);

            TextureBackup = new Texture2D(SourceTexture.width, SourceTexture.height) { filterMode = FilterMode.Point };
            TextureBackup.CopyFrom(Image.sprite.texture);
            TextureBackup.Apply();

            DraftRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Math.Min(ParentRect.rect.size.x, ParentRect.rect.size.y));
            DraftRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Min(ParentRect.rect.size.x, ParentRect.rect.size.y));

            PaletteColors = Pixels.Distinct().ToList();
            var imageRect = Image.GetComponent<RectTransform>().rect;

            GridNumbers.cellSize = Vector2.one * imageRect.width / SourceTexture.width;
            Image.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageRect.width * SourceTexture.height / SourceTexture.width);
            
            PaletteInit(GridPaletteNew, PaletteColors, ColorPrefab);
            GNumberPrefab = NumberPrefab;
            GGridNumbers = GridNumbers;
            NumbersInit(GridNumbers, PaletteColors, NumberPrefab, Pixels);
            ImageRectTransformOriginSize = DraftRect.rect.size;
            ImageRectTransformOriginPosition = DraftRect.position;
            GridNumbersOriginSize = GridNumbers.cellSize;

            PlayerPrefs.SetInt("Continue", 1);
            if (Continue)
            {
                DraftToTexture(ColorByNumberSavedGame.DraftBytes, Image.sprite.texture, SourceTexture);           // Сохранить исходную картинку в ColorByNumberSavedGame!
            }
            else
            {
                ColorByNumberSavedGame.DraftBytes = new byte[Image.sprite.texture.width * Image.sprite.texture.height];
            }
        }

        void PaletteInit(ToggleGroup paletteToggleGroup, List<Color> colors, ColorGridCell colorPrefab)
        {
            if (paletteToggleGroup.transform.childCount > 0)
            {
                GridLayoutGroupRelease(paletteToggleGroup.transform);
            }

            foreach (var color in colors)
            {
                colorPrefab.ThisToggle.group = paletteToggleGroup;
                var colorCell = Instantiate(colorPrefab, paletteToggleGroup.transform);

                colorCell.ThisColor.color = color;
                if (colorCell.ThisColor.color.a == 0f)                         // transparent
                {
                        colorCell.ThisColor.color  = Color.grey;
                }

                colorCell.Text.text = colors.IndexOf(color).ToString();

                if (color.maxColorComponent > 0.38f)
                {
                    colorCell.Text.color = Color.black;
                }
                else
                {
                    colorCell.Text.color = Color.white;
                }
            }
        }

        void GridLayoutGroupRelease(Transform palette)
        {
            for (int i = 0; i < palette.childCount; i++)
            {
                var child = palette.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        void TransparentPixelReplace(Texture2D texture, Color replaceColor)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    if (texture.GetPixel(x, y).a == 0f)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
            }
            texture.Apply();
        }

        public void NumbersInit(GridLayoutGroup numbers, List<Color> colors, Text numberPrefab, Color[] imagePixels)
        {
            if (numbers.transform.childCount > 0)
            {
                GridLayoutGroupRelease(numbers.transform);
            }

            TextNumbers = new List<Text>(imagePixels.Length);
            for (int i = 0; i < imagePixels.Length; i++)
            {
                var numberCell = Instantiate(numberPrefab, numbers.transform);
                numberCell.text = colors.IndexOf(imagePixels[i]).ToString();
                TextNumbers.Add(numberCell);

                if (imagePixels[i].maxColorComponent < 0.48f)
                {
                    numberCell.color = Color.grey;
                }
                else if (imagePixels[i].maxColorComponent <= 0.60f)
                {
                    numberCell.color = Color.white;
                }
            }
        }

        public static void NumbersTextTransparentUpdate(float a)
        {
            foreach (var number in GGridNumbers.GetComponentsInChildren<Text>())
            {
                number.color = new Color(number.color.r, number.color.g, number.color.b, a);
            }
        }

        void DraftToTexture(byte[] draft, Texture2D dstTexture, Texture2D srcTexture)
        {
            var i = 0;
            foreach (var bite in draft)
            {
                if (bite == 1)
                {
                    var y = i / dstTexture.width;
                    var x = i - y * dstTexture.width;
                    dstTexture.SetPixel(x, y, srcTexture.GetPixel(x, y));
                    TextNumbers[x + srcTexture.width * y].text = "";
                }

                i++;
            }
            dstTexture.Apply();
        }
    }
}
