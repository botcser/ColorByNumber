using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ColorByNumber : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public Image Image;
        public RectTransform RectTransform;
        public GridLayoutGroup GridNumbers;

        public static Texture2D SourceTexture;
        public static Color CurrentHandColor;

        public static float AccentPixelGamma = -0.3f;

        public static ColorByNumber Instance;

        void Awake()
        {
            Instance = this;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ToolbarToolsPixelNumbers.CurrentTool != (int)ToolbarToolsPixelNumbers.Tools.MoveCamera)
            {
                SetPixel(eventData.position);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ToolbarToolsPixelNumbers.CurrentTool != (int)ToolbarToolsPixelNumbers.Tools.MoveCamera &&
                RectTransformUtility.RectangleContainsScreenPoint(RectTransform, Input.mousePosition, Camera.main))
            {
                SetPixel(eventData.position);
            }
        }

        private void SetPixel(Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, position, Camera.main, out var p);

            var texture = Image.sprite.texture;
            var scale = RectTransform.rect.width / texture.width;
            var x = (int)((p.x + RectTransform.rect.width / 2) / scale);
            var y = (int)((p.y + RectTransform.rect.height / 2) / scale);

            if (x == texture.width) x = texture.width - 1;
            if (y == texture.height) x = texture.height - 1;

            if (SourceTexture.GetPixel(x, y) == CurrentHandColor)
            {
                texture.SetPixel(x, y, CurrentHandColor);
                texture.Apply();
                ColorByNumberSavedGame.DraftBytes[y * texture.height + x] = 1;
                ColorByNumberInit.TextNumbers[x + SourceTexture.width * y].text = "";
            }
            else
            {
                texture.SetPixel(x, y, new Color(CurrentHandColor.r + AccentPixelGamma, CurrentHandColor.g + AccentPixelGamma, CurrentHandColor.b + AccentPixelGamma));
                texture.Apply();
                //Hanheld.Vibrate();
            }
        }

        void OnApplicationPause()
        {
            ColorByNumberSavedGame.Instance.Save();
        }

        public void NumbersAccent(int activate)
        {
            for (int x = 0; x < Image.sprite.texture.width; x++)
            {
                for (int y = 0; y < Image.sprite.texture.height; y++)
                {
                    var pixel = Image.sprite.texture.GetPixel(x, y);
                    if (pixel == CurrentHandColor)
                    {
                        Image.sprite.texture.SetPixel(x, y, new Color(pixel.r + AccentPixelGamma * activate, pixel.g + AccentPixelGamma * activate, pixel.b + AccentPixelGamma * activate));
                    }
                }
            }
            Image.sprite.texture.Apply();
        }
    }
}
