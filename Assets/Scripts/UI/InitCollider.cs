using UnityEngine;

namespace Assets.Scripts.UI
{
    public class InitCollider : MonoBehaviour
    {
        public RectTransform MyRectTransform;

        public void Start()
        {
            GetComponent<BoxCollider2D>().size = new Vector2(MyRectTransform.rect.width, MyRectTransform.rect.height);
        }
    }
}
