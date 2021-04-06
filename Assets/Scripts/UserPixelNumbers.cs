using Assets.Scripts.UI;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class UserPixelNumbers : BaseInterface
    {
        public Image Image;

        public static UserPixelNumbers Instance;

        public void OnEnable()
        {
            Instance = this;
        }

        protected override void OnOpen()
        {
            ColorByNumberSavedGame.Load();
            if (ColorByNumberSavedGame.DraftBytes != null)
            {
                ColorByNumberInit.Continue = true;
            }
            else
            {
                ColorByNumberInit.Continue = false;
            }
        }
        
        public void Back()
        {
            ColorByNumberSavedGame.Instance.Save();
            Close();
            
            /*if (ReturnTo is UserPage)                                                 // PixelStudio
            {
                UserPage.Instance.Open();
            }*/
        }

        public void ResetDraft()
        {
            var colorByNumberInit = new ColorByNumberInit();

            Image.sprite.texture.CopyFrom(ColorByNumberInit.TextureBackup);
            Image.sprite.texture.Apply();

            ColorByNumberSavedGame.DraftBytes = new byte[Image.sprite.texture.width * Image.sprite.texture.height];
            colorByNumberInit.NumbersInit(ColorByNumberInit.GGridNumbers, ColorByNumberInit.PaletteColors, ColorByNumberInit.GNumberPrefab, ColorByNumberInit.Pixels);
        }
    }
}
