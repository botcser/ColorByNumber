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

        public static Texture2D SourceTexture;
        public static Color CurrentHandColor;


        public void OnPointerDown(PointerEventData eventData)
        {
            SetPixel(eventData.position);
            Profile.Instance.Save();
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetPixel(eventData.position);
            Profile.Instance.Save();
        }

        private void SetPixel(Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, position, Camera.main, out var p);

            var texture = Image.sprite.texture;
            var scale = RectTransform.rect.width / texture.width;
            var x = (int) ((p.x + RectTransform.rect.width / 2) / scale);
            var y = (int) ((p.y + RectTransform.rect.height / 2) / scale);

            if (x == texture.width) x = texture.width - 1;
            if (y == texture.height) x = texture.height - 1;

            if (SourceTexture.GetPixel(x, y) == CurrentHandColor)
            {
                texture.SetPixel(x, y, CurrentHandColor);
                texture.Apply();
                //Profile.Draft.SetPixel(x, y, CurrentHandColor);
                //Profile.Draft.Apply();
                Profile.DraftBytes[y * texture.height + x] = 1;
            }
        }
    }
}
