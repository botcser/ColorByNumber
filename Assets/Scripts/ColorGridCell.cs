using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ColorGridCell : MonoBehaviour
    {
        // Start is called before the first frame update
        public Text Text;
        public Toggle Toggle;

        public void ColorClick()
        {
            Main.CurrentHandColor = Toggle.colors.normalColor;
        }
    }
}
