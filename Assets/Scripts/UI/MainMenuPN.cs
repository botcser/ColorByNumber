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
            Profile.Load();
            //LocalizationManager.Read();
            //LocalizationManager.Language = Profile.Instance.Settings.Language;
        }

        public void Start()
        {
            ContinueButton.SetActive(PlayerPrefs.GetInt("Continue") == 1);
        }

        public void Continue()
        {
            if (Profile.Draft != null)
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