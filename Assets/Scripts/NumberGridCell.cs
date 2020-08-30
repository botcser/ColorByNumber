using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class NumberGridCell : MonoBehaviour, IPointerClickHandler
    {
        public Text Text;
        public int index;
        public Color pixel;
        public Image Image;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Main.CurrentHandColor == pixel)
            {
                Image.color = pixel;
            }
        }
    }
}
