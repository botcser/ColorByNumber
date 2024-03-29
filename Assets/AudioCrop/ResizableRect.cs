﻿using System.Collections.Generic;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.AudioCrop
{
    public class ResizableRect : MonoBehaviour
    {
        public RectTransform RectTransform;
        public List<RectTransform> BordersRectTransform;
        public List<TimeBorder> BordersTimeBorder;
        public static ResizableRect Instance;

        private float _oldWidth = 0f;

        public void Awake()
        {
            Instance = this;
        }

        public void UpdateSize()
        {
            var width = Mathf.Abs(BordersRectTransform[0].localPosition.x - BordersRectTransform[1].localPosition.x);

            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _oldWidth = width;
            RectTransform.localPosition = new Vector2((BordersRectTransform[0].localPosition.x + BordersRectTransform[1].localPosition.x) / 2, RectTransform.localPosition.y);
            this.GetComponent<InitCollider>()?.Start();
        }

        public void UpdateBorders()
        {
            BordersRectTransform[0].localPosition = new Vector2(RectTransform.localPosition.x - RectTransform.rect.width / 2, BordersRectTransform[0].localPosition.y);
            BordersRectTransform[1].localPosition = new Vector2(RectTransform.localPosition.x + RectTransform.rect.width / 2, BordersRectTransform[1].localPosition.y);
            BordersTimeBorder[0].UpdateTime();
            BordersTimeBorder[1].UpdateTime();
            if (_oldWidth != RectTransform.rect.width)
            {
                this.GetComponent<InitCollider>()?.Start();
            }
        }

        public void UpdateBordersTime()
        {
            foreach (var border in BordersTimeBorder)
            {
                border.UpdateTime();
            }
        }
    }
}