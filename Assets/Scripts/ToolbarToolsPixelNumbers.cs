using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ToolbarToolsPixelNumbers : MonoBehaviour
    {
        public RectTransform ImageRectTransform;
        public GridLayoutGroup GridNumbers;

        public static int CurrentTool = 0;
        public enum Tools
        {
            Pen,
            MoveCamera,
            Fill,
            Brush,
            Eraser,
        }

        public void SelectMoveCameraToogle()
        {
            CurrentTool = CurrentTool == (int)Tools.MoveCamera ? (int)Tools.Pen : (int)Tools.MoveCamera;
        }

        public void ResetViewOn()
        {
            ImageRectTransform.position = ColorByNumberInit.ImageRectTransformOriginPosition;
            ImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ColorByNumberInit.ImageRectTransformOriginSize.x);
            ImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ColorByNumberInit.ImageRectTransformOriginSize.y);
            GridNumbers.cellSize = ColorByNumberInit.GridNumbersOriginSize;
        }
    }
}