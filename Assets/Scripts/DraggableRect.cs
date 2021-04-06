using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class DraggableRect : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public RectTransform RectTransform; // Don't user Preserve Aspect in Image!
        public RectTransform ParentRectTransform;
        public RectTransform AnchorRectTransform;
        public UnityEvent DragCallback;
        public UnityEvent TimeBorder1;
        public UnityEvent TimeBorder2;
        public UnityEvent UpdateBorders;
        public UnityEvent CenterLabel;

        private Vector2 _position, _pointer;

        private bool _onTriggerCheck = false;
        private float _anchorStartX;
        private float _deltaX;
        private float _startRegionPosX;
        private int _cycleSign = -1;
        private string _myTag;

        public static Vector2 _position_delta;

        public void Start()
        {
            _myTag = this.gameObject.tag;
        }

        public void Update()
        {
            if (_onTriggerCheck)
            {
                RectTransform.localPosition = new Vector3(_startRegionPosX + _cycleSign * RectTransform.rect.width, transform.localPosition.y);  // если быстро махнуть мышкой - он не догонит...
                _cycleSign *= -1;
            }

        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            _position = transform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRectTransform, eventData.position, null, out _pointer); // The cam parameter should be the camera associated with the screen point. For a RectTransform in a Canvas set to Screen Space - Overlay mode, the cam parameter should be null.
        }

        void OnTriggerExit2D(Collider2D collisionInfo)                          //
        {
            if (_myTag == "Region" || collisionInfo.tag == "HistogramWindow")
            {
                if (AnchorRectTransform != null)
                {
                    _startRegionPosX = RectTransform.localPosition.x;
                    RectTransform.localPosition = new Vector3(RectTransform.localPosition.x + RectTransform.rect.width,
                        transform.localPosition.y);
                    DragCallback?.Invoke();
                    _onTriggerCheck = true;
                }
            }
        }

        void OnTriggerEnter2D(Collider2D collisionInfo)                         // Region enter to HistogramWindow
        {
            if (_myTag == "Region" || collisionInfo.tag == "HistogramWindow")
            {
                DragCallback?.Invoke();
                _onTriggerCheck = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRectTransform, eventData.position, null, out var pointer);
            
            var max = Mathf.Abs(RectTransform.rect.width - ParentRectTransform.rect.width) / 2;
            _deltaX = pointer.x - _pointer.x;
            var x = _position.x + _deltaX;

            if (x > max)
            {
                x = max;
            }

            if (x < -max)
            {
                x = -max;
            }

            transform.localPosition = new Vector3(x, transform.localPosition.y);
            DragCallback?.Invoke();
            TimeBorder1?.Invoke();
            TimeBorder2?.Invoke();
            CenterLabel?.Invoke();
        }

        public void ResetDraggableRect()
        {
            RectTransform.localPosition = new Vector3(-ParentRectTransform.rect.width / 2 + RectTransform.rect.width / 2, transform.localPosition.y);
            UpdateBorders?.Invoke();
            CenterLabel?.Invoke();
        }

        public void ToEndDraggableRect()
        {
            RectTransform.localPosition = new Vector3(ParentRectTransform.rect.width / 2 - RectTransform.rect.width / 2, transform.localPosition.y);
            UpdateBorders?.Invoke();
            CenterLabel?.Invoke();
        }
    }
}