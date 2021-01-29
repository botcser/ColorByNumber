using System.Collections;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AudioCrop
{
    public class TimeBorder : MonoBehaviour
    {
        public RectTransform HistogramRectTransform;
        public RectTransform ParentRectTransform;
        public Text Time;

        private bool _oneTime = true;

        public void Update()
        {
            if (_oneTime && Gif2mp4Panel.CurrentAudioLenght > 0f)
            {
                UpdateTime();
                _oneTime = false;
            }
        }

        public void UpdateTime()
        {
            var time = Gif2mp4Panel.CurrentAudioLenght * (ParentRectTransform.anchoredPosition.x 
                                                          + HistogramRectTransform.rect.width / 2) / HistogramRectTransform.rect.width;
            this.Time.text = time.ToString("00.00");

            Gif2mp4Panel.UpdateStartEndTimes();
            Gif2mp4Panel.AudioRegionDuractionTimeSecOrig = Gif2mp4Panel.AudioRegionDuractionTimeSec;
        }
    }
}
