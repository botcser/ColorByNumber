using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ColorByNumber : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public Image Image;
        public RectTransform RectTransform;

        public void OnPointerDown(PointerEventData eventData)
        {
            SetPixel(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetPixel(eventData.position);
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

            texture.SetPixel(x, y, Color.black);
            texture.Apply();
        }
    }
}
