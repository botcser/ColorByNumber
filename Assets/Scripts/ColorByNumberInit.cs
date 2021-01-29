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
        public GridLayoutGroup GridNumbers;
        public GridLayoutGroup GridPalette;
        public Text NumberPrefab;
        public ColorGridCell ColorPrefab;
        public Texture2D SourceTexture;

        public static bool Continue;

        public void Start()
        {
            ColorByNumber.SourceTexture = SourceTexture;
            var grayscaleTexture = new Texture2D(SourceTexture.width, SourceTexture.height) { filterMode = FilterMode.Point };
            var pixels = SourceTexture.GetPixels();

            grayscaleTexture.SetPixels(pixels.Select(i => new Color(i.grayscale, i.grayscale, i.grayscale)).ToArray());
            grayscaleTexture.Apply();

            Image.sprite = Sprite.Create(grayscaleTexture, new Rect(0, 0, grayscaleTexture.width, grayscaleTexture.height), Vector2.one / 2);

            var paletteColors = pixels.Distinct().ToList();
            var imageRect = Image.GetComponent<RectTransform>().rect;

            GridNumbers.cellSize = Vector2.one * imageRect.width / SourceTexture.width;
            Image.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageRect.width * SourceTexture.height / SourceTexture.width);
            PaletteInit(GridPalette, paletteColors, ColorPrefab);
            NumbersInit(GridNumbers, paletteColors, NumberPrefab, pixels);
            PlayerPrefs.SetInt("Continue", 1);
            if (Continue)
            {
                //Image.sprite.dstTexture.LoadImage(Profile.Draft.EncodeToPNG());
                //Image.sprite.dstTexture.Apply();
                DraftToTexture(Profile.DraftBytes, Image.sprite.texture, SourceTexture);           // Сохранить исходную картинку в Profile!
            }
            else
            {
                //Profile.Draft = new Texture2D(Image.sprite.dstTexture.width, Image.sprite.dstTexture.height) { filterMode = FilterMode.Point };
                //Profile.Draft.LoadImage(Image.sprite.dstTexture.EncodeToPNG());
                //Profile.Draft.Apply();
                Profile.DraftBytes = new byte[Image.sprite.texture.width * Image.sprite.texture.height];
            }
        }

        void PaletteInit(GridLayoutGroup palette, List<Color> colors, ColorGridCell colorPrefab)
        {
            foreach (var color in colors)
            {
                colorPrefab.Toggle.group = palette.GetComponent<ToggleGroup>();
                var colorCell = Instantiate(colorPrefab, palette.transform);
                var colorBlock = colorCell.Toggle.colors;
                colorBlock.normalColor = colorBlock.pressedColor = colorBlock.highlightedColor = colorBlock.selectedColor = color;
                colorCell.Toggle.colors = colorBlock;
                colorCell.Text.text = colors.IndexOf(color).ToString();
            }
        }

        void NumbersInit(GridLayoutGroup numbers, List<Color> colors, Text numberPrefab, Color[] imagePixels)
        {
            for (int i = 0; i < imagePixels.Length; i++)
            {
                var numberCell = Instantiate(numberPrefab, numbers.transform);
                numberCell.text = colors.IndexOf(imagePixels[i]).ToString();

                if (imagePixels[i].maxColorComponent < 0.47f)
                {
                    numberCell.color = Color.grey;
                }
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
                }

                i++;
            }
            dstTexture.Apply();
        }
    }
}
