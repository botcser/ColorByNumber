using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.UI
{

    public class MainMenuPN : BaseInterface
    {
        public static MainMenuPN Instance;
        public GameObject ContinueButton;

        public void Awake()
        {
            Instance = this;
            ColorByNumberSavedGame.Load();
            //LocalizationManager.Read();
            //LocalizationManager.Language = ColorByNumberSavedGame.Instance.Settings.Language;
        }

        public void Start()
        {
            ContinueButton.SetActive(PlayerPrefs.GetInt("Continue") == 1);
        }

        public void Continue()
        {
            if (ColorByNumberSavedGame.Draft != null)
            {
                ColorByNumberInit.Continue = true;
            }
            this.Close();
        }

        public void New()
        {
            this.Close();
        }
    }
}