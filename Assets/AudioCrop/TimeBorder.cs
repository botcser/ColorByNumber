using System.Collections;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.AudioCrop
{
    public class TimeBorder : MonoBehaviour
    {
        public RectTransform HistogramRectTransform;
        public RectTransform ParentRectTransform;
        public ResizableRect RegionResizableRect;
        public InputField TimeInput;
        public Text Time;

        private bool _oneTime = true;
        private bool isKeyReturnDawn = false;
        private string _oldText;

        public void Start()
        {
            if (TimeInput != null)
            {
                TimeInput.onValidateInput = (string text, int charIndex, char addedChar) =>
                {
                    return ValidateInput("1234567890,.", addedChar);
                };

                TimeInput.onEndEdit.AddListener(delegate { CheckIsEdited(TimeInput); });
            }
        }

        public void Update()
        {
            if (_oneTime && Gif2mp4Panel.CurrentAudioLenght > 0f)
            {
                UpdateTime();
                _oneTime = false;
            }
        }

        public void CheckIsEdited(InputField timeField)
        {
            if ((Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) && SetTime())
            {
                return;
            }
            timeField.text = _oldText;
        }



        public void UpdateTime()
        {
            if (TimeInput != null)     // as TimeBorder
            {
                var time = Gif2mp4Panel.CurrentAudioLenght * (ParentRectTransform.anchoredPosition.x
                                                              + HistogramRectTransform.rect.width / 2) / HistogramRectTransform.rect.width;
                TimeInput.text = time.ToString("00.00");
                _oldText = TimeInput.text;
                Gif2mp4Panel.UpdateStartEndTimes();
                Gif2mp4Panel.LoopAudioX = 1;
            }
            else     // as CenterLabel
            {
                var time = Gif2mp4Panel.CurrentAudioLenght * (-ParentRectTransform.anchoredPosition.x
                                                              + HistogramRectTransform.rect.width / 2) / HistogramRectTransform.rect.width;
                Time.text = time.ToString("00.00");
            }
        }

        public bool SetTime()
        {
            TimeInput.text = TimeInput.text.Replace('.', ',');
            var time = float.Parse(TimeInput.text);

            if (time < 0 || time > Gif2mp4Panel.CurrentAudioLenght)
            {
                return false;
            }
            var newPosX = time * HistogramRectTransform.rect.width / Gif2mp4Panel.CurrentAudioLenght -
                          HistogramRectTransform.rect.width / 2;

            ParentRectTransform.anchoredPosition = new Vector2(newPosX, ParentRectTransform.anchoredPosition.y);

            RegionResizableRect.UpdateSize();
            Gif2mp4Panel.UpdateStartEndTimes();
            Gif2mp4Panel.LoopAudioX = 1;
            return true;
        }

        public bool SetTime(string newTime)
        {
            newTime = newTime.Replace('.', ',');
            var time = float.Parse(newTime);

            if (time < 0 || time > Gif2mp4Panel.CurrentAudioLenght)
            {
                return false;
            }
            var newPosX = time * HistogramRectTransform.rect.width / Gif2mp4Panel.CurrentAudioLenght -
                                                     HistogramRectTransform.rect.width / 2;

            ParentRectTransform.anchoredPosition = new Vector2(newPosX, ParentRectTransform.anchoredPosition.y);

            RegionResizableRect.UpdateSize();
            Gif2mp4Panel.UpdateStartEndTimes();
            Gif2mp4Panel.LoopAudioX = 1;
            return true;
        }

        private char ValidateInput(string validChars, char inputChar)
        {

            if (validChars.IndexOf(inputChar) != -1)
            {
                return inputChar;
            }
            else
            {
                return '\0';
            }
        }
    }
}
