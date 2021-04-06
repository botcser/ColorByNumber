using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ColorGridCell : MonoBehaviour
    {
        // Start is called before the first frame update
        public Text Text;
        public GameObject ThisSelectMarker;
        public Toggle ThisToggle;
        public Image ThisColor;

        public void ColorClick()
        {
            ColorByNumber.CurrentHandColor = ThisColor.color;

            if (GridPalette.PrevSelectMarker == null)
            {
                GridPalette.PrevSelectMarker = ThisSelectMarker;
                GridPalette.PrevText = Text;
            }
            else
            {
                GridPalette.PrevSelectMarker.SetActive(false);
                GridPalette.PrevText.fontStyle = FontStyle.Normal;

                GridPalette.PrevSelectMarker = ThisSelectMarker;
                GridPalette.PrevText = Text;
            }
            ThisSelectMarker.SetActive(true);
            Text.fontStyle = FontStyle.Bold;
        }
    }
}
