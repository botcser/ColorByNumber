using UnityEngine;

namespace Assets.Scripts.UI
{
    public abstract class BaseInterface : MonoBehaviour
    {
        public GameObject Panel;
        public BaseInterface ReturnTo;

        private static BaseInterface _openedWindow;


        public void Open()
        {
            Panel.SetActive(true);
            if (_openedWindow)
            {
                _openedWindow.Panel.SetActive(false);
                _openedWindow = this;
            }
            else
            {
                _openedWindow = this;
            }

            OnOpen();
        }

        public void Close()
        {
            Panel.SetActive(false);
            ReturnTo?.Open();
            OnClose();
        }

        protected virtual void OnOpen()
        {

        }

        protected virtual void OnClose()
        {

        }
    }
}