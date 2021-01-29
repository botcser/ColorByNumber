using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class Profile
    {
        public int Progress;
        public Settings Settings = new Settings();
        public long AdTimeTicks;
        public static Texture2D Draft = new Texture2D(2, 2);
        public string DraftString;

        public static byte[] DraftBytes;
        public static Profile Instance;

        private const string ProfKey = "profile";


        public void Save()
        {
            //var buf = Draft.EncodeToPNG();
            //DraftString = Convert.ToBase64String(buf, 0, buf.Length);
            DraftString = Convert.ToBase64String(DraftBytes);
            var json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(ProfKey, json);
            PlayerPrefs.Save();
        }


        public static void Load()
        {
            if (PlayerPrefs.HasKey(ProfKey))
            {
                var json = PlayerPrefs.GetString(ProfKey);
                Instance = JsonUtility.FromJson<Profile>(json);
                //Draft.LoadImage(Convert.FromBase64String(Instance.DraftString));
                DraftBytes = Convert.FromBase64String(Instance.DraftString);
            }
            else
            {
                Instance = new Profile();

                switch (Application.systemLanguage)
                {
                    case SystemLanguage.German:
                    case SystemLanguage.Russian:
                        Instance.Settings.Language = Application.systemLanguage.ToString();
                        break;
                    default:
                        Instance.Settings.Language = "English";
                        break;
                }
            }
        }



    }
}