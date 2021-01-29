using UnityEngine;

namespace Assets.Scripts.Data
{
    public class MyGifFrame
    {
        public byte[] Encoded;
        public int Width;
        public int Height;
        public float Delay;
        public AudioSource Audio;

        public MyGifFrame(byte[] encoded, int width, int height, float delay, AudioSource audio)
        {
            Encoded = encoded;
            Width = width;
            Height = height;
            Delay = Mathf.Max(0.02f, delay);
            Audio = audio;
        }
    }
}
