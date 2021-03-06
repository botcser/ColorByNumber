using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class GifFrame
    {
        public byte[] Encoded;
        public int Width;
        public int Height;
        public float Delay;
        public AudioSource Audio;

        public GifFrame(byte[] encoded, int width, int height, float delay, AudioSource audio)
        {
            Encoded = encoded;
            Width = width;
            Height = height;
            Delay = Mathf.Max(0.02f, delay);
            Audio = audio;
        }
    }

    public class Gif
    {
        private static bool _free = false;

        /// <summary>
        /// List of GIF frames
        /// </summary>
        public List<GifFrame> Frames;

        /// <summary>
        /// Create a new instance from GIF frames.
        /// </summary>
        public Gif(List<GifFrame> frames)
        {
            Frames = frames;
        }
    }
}
