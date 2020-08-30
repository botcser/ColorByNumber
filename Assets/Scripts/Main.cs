using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Main : MonoBehaviour
    {
        public Image Image;
        public GridLayoutGroup GridNumbers;
        public GridLayoutGroup GridPalette;
        public NumberGridCell NumberPrefab;
        public ColorGridCell ColorPrefab;
        public Texture2D SourceTexture;

        public static Color CurrentHandColor;


        public void Start()
        {
        
            var grayscaleTexture = new Texture2D(SourceTexture.width, SourceTexture.height) { filterMode = FilterMode.Point };
            var pixels = SourceTexture.GetPixels();

            grayscaleTexture.SetPixels(pixels.Select(i => new Color(i.grayscale, i.grayscale, i.grayscale)).ToArray());
            grayscaleTexture.Apply();

            Image.sprite = Sprite.Create(grayscaleTexture, new Rect(0,0, grayscaleTexture.width, grayscaleTexture.height), Vector2.one/2);

            var paletteColors = pixels.Distinct().ToList();
            var imageRect = Image.GetComponent<RectTransform>().rect;

            GridNumbers.cellSize = Vector2.one * imageRect.width / SourceTexture.width;
            Image.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageRect.width * SourceTexture.height / SourceTexture.width);
            PaletteInit(GridPalette, paletteColors, ColorPrefab);
            NumbersInit(GridNumbers, paletteColors, NumberPrefab, pixels);
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

        void NumbersInit(GridLayoutGroup numbers, List<Color> colors, NumberGridCell numberPrefab, Color[] imagePixels)
        {
            for (int i = 0; i < imagePixels.Length; i++)
            {
                var numberCell = Instantiate(numberPrefab, numbers.transform);
                numberCell.index = i;
                numberCell.Text.text = colors.IndexOf(imagePixels[i]).ToString();
                numberCell.pixel = imagePixels[i];
            }
        }

 
    }
}
