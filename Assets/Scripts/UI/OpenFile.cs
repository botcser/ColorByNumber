using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class OpenFile : MonoBehaviour
    {
        public RawImage image;

        string path;
        private int _width;
        private int _height;

        public void OnClick()
        {
            path = EditorUtility.OpenFilePanel("Open pixel studio image", "", "png");

            if (path != null)
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] imageBytes = br.ReadBytes((int)fs.Length);

                _width = (int)Math.Sqrt(imageBytes.Length);
                _height = _width;

                var imageTexture = new Texture2D(_width, _height);
                imageTexture.LoadImage(imageBytes);
                image.texture = imageTexture;

            }
        }
    }
}
