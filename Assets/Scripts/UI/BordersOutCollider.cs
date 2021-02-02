using UnityEngine;

namespace Assets.Scripts.UI
{
    public class BordersOutCollider : MonoBehaviour
    {
        public Collider2D HistogramWindow;

        void OnTriggerExit2D(Collider2D collisionInfo)
        {
            if (collisionInfo == HistogramWindow)
            {
                Gif2mp4Panel.BordersOutCount++;
            }
        }

        void OnTriggerEnter2D(Collider2D collisionInfo)
        {
            if (collisionInfo == HistogramWindow)
            {
                Gif2mp4Panel.BordersOutCount--;
            }
        }
    }
}
