using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Gestures : MonoBehaviour
    {
        public bool MoveWithTwoTouches;
        public RectTransform ImageRectTransform;
        public GridLayoutGroup GridNumbers;
        public Image SourceImage;

        private int _touchCount;
        private Vector3 _pointerStartPosition;
        private Vector3 _imageRectTransformStartPosition;
        private float _fingerDistance;
        private float _orthographicSize;
        private float _size;
        private float _zoomAccelerate = 3;

        public void Update()
        {
            Zoom();
            Move();
        }

        private void Move()
        {
            if (Input.GetMouseButtonDown(0) &&
                Input.touchCount == 0 &&
                ToolbarToolsPixelNumbers.CurrentTool == (int)ToolbarToolsPixelNumbers.Tools.MoveCamera &&
                RectTransformUtility.RectangleContainsScreenPoint(ImageRectTransform, Input.mousePosition, Camera.main))
            {
                _touchCount = 0;
                _pointerStartPosition = Input.mousePosition;
                _imageRectTransformStartPosition = ImageRectTransform.position;
            }

            if (MoveWithTwoTouches)
            {
                if (Input.touchCount == 2 && Input.touches.Any(i => i.phase == TouchPhase.Began))
                {
                    _touchCount = 2;
                    _pointerStartPosition = (Input.touches[0].position + Input.touches[1].position) / 2;
                    _imageRectTransformStartPosition = ImageRectTransform.position;
                }
            }
            else
            {
                if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began &&
                    RectTransformUtility.RectangleContainsScreenPoint(ImageRectTransform, Input.touches[0].position, Camera.main))
                {
                    _touchCount = 1;
                    _pointerStartPosition = Input.touches[0].position;
                    _imageRectTransformStartPosition = ImageRectTransform.position;
                }
            }

            if ((Input.GetMouseButton(0) &&
                                         ToolbarToolsPixelNumbers.CurrentTool == (int)ToolbarToolsPixelNumbers.Tools.MoveCamera &&
                                         RectTransformUtility.RectangleContainsScreenPoint(ImageRectTransform, Input.mousePosition, Camera.main)) ||
                 Input.touches.Any(i => i.phase == TouchPhase.Moved) &&
                                                                                 RectTransformUtility.RectangleContainsScreenPoint(ImageRectTransform, Input.touches[0].position, Camera.main))
            {
                Vector3 pointerPosition;

                switch (_touchCount)
                {
                    case 0:
                        pointerPosition = Input.mousePosition;
                        break;
                    case 1:
                        if (Input.touchCount != 1) return;

                        pointerPosition = Input.touches[0].position;
                        break;
                    case 2:
                        pointerPosition = (Input.touches[0].position + Input.touches[1].position) / 2;
                        break;
                    default:
                        return;
                }

                var deltaViewport = Camera.main.ScreenToViewportPoint(pointerPosition - _pointerStartPosition);

                if (deltaViewport.magnitude > 0.01f)
                {
                    var deltaWorld = Camera.main.ScreenToWorldPoint(pointerPosition) - Camera.main.ScreenToWorldPoint(_pointerStartPosition);
                    var position = (Vector2)(_imageRectTransformStartPosition + deltaWorld);

                    if (position.magnitude > 6)
                    {
                        position = 6 * position.normalized;
                    }

                    ImageRectTransform.position = new Vector3(position.x, position.y, ImageRectTransform.position.z);
                }
            }
        }

        private void Zoom()
        {
            if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0))
            {
                _size = _zoomAccelerate * Input.mouseScrollDelta.y;
                ImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ImageRectTransform.rect.size.x + _size);
                ImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ImageRectTransform.rect.size.y + _size);
                //GridNumbers.cellSize += new Vector2(_size / UserImagePage.Instance.UserImage.Width, _size / UserImagePage.Instance.UserImage.Width);              // PixelStudio
                GridNumbers.cellSize += new Vector2(_size / ColorByNumber.SourceTexture.width, _size / ColorByNumber.SourceTexture.width);                      // PixelStudio

            }

            if (Input.touchCount == 2 && (Input.touches[0].phase == TouchPhase.Began || Input.touches[1].phase == TouchPhase.Began))
            {
                _touchCount = -1; // Break Move();
                _fingerDistance = (Input.touches[1].position - Input.touches[0].position).magnitude;
                _orthographicSize = Camera.main.orthographicSize;
            }

            if (Input.touchCount == 2 && Input.touches.Any(i => i.phase == TouchPhase.Moved))
            {
                var fingerDistance = (Input.touches[1].position - Input.touches[0].position).magnitude;

                _size = _orthographicSize * _fingerDistance / fingerDistance;
                ImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ImageRectTransform.rect.size.x + _size);
                ImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ImageRectTransform.rect.size.y + _size);
                //GridNumbers.cellSize += new Vector2(_size / SourceImage.sprite.texture.width, _size / SourceImage.sprite.texture.width);                          // PixelStudio
                GridNumbers.cellSize += new Vector2(_size / ColorByNumber.SourceTexture.width, _size / ColorByNumber.SourceTexture.width);                      // PixelStudio
            }
        }
    }
}