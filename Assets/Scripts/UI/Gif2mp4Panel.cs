using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.AudioCrop;
using Assets.Scripts.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class Gif2mp4Panel : BaseInterface
    {
        public Texture2D Image1;
        public Texture2D Image2;
        public Texture2D Image3;
        public Texture2D Image4;
        public AudioSource SourceAudio;
        public AudioSource CutedAudio;
        public Image HistogramImage;
        public RectTransform HistogramWindowRectTransform;
        public RectTransform HistogramParentRectTransform;
        public RectTransform HistogramRectTransform;
        public RectTransform HistogramRegionRectTransform;
        public RectTransform HistogramBoarderLRectTransform;
        public RectTransform HistogramBoarderRRectTransform;
        public ResizableRect HistogramRegionResizableRec;
        public Text GifDuractionTimeText;
        public Text AudioRegionDuractionTimeText;
        public List<DraggableRect> DraggableRects;
        public List<MyGifFrame> InputGif;
        public List<InputField> BordersInputFields;
        public GameObject ReturnBordersButton;

        public HorizontalLayoutGroup Gifgram;
        public GameObject GifFramePrefab;
        public string InputMp3 = "Half-Life13";
        public Text StartTimeBorder;
        public Text EndTimeBorder;
        public InputField StartTimeBorderInput;
        public InputField EndTimeBorderInput;

        public static Gif2mp4Panel Instance;
        public static float CurrentAudioLenght;
        public static float GifDuractionTimeSec = 0f;
        public static float AudioRegionDuractionTimeSec = 0f;
        public static float StartTime;
        public static float EndTime;
        public static int BordersOutCount = 2;     // enter twice from start
        public static int LoopAudioX = 1;

        private float _histogramFrameWidthInSeconds = 10f;
        private int _loopGifX = 1;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            InitGifsGram(_loopGifX);                               // INPUT GIF

            MakeHistogramImage(SourceAudio.clip, LoopAudioX);                                  // Audio Histogram

            CurrentAudioLenght = SourceAudio.clip.length;
        }

        public void Update()
        {
            if (SourceAudio.isPlaying && SourceAudio.time > EndTime)
            {
                SourceAudio.Stop();
            }

            if (BordersOutCount > 1)
            {
                ReturnBordersButton.SetActive(true);
            }
            else if (ReturnBordersButton.activeSelf)
            {
                ReturnBordersButton.SetActive(false);
            }
        }

        public void InitGifsGram(int loop)
        {
            InputGif = new List<MyGifFrame>();
            InitInputGif(InputGif);
            GifsGramMake(InputGif, Gifgram, GifFramePrefab, loop);
        }

        public void DecreaseGifLoop()
        {
            if (_loopGifX > 1)
            {
                GifsGramMake(InputGif, Gifgram, GifFramePrefab, --_loopGifX);
            }
        }

        public void IncreaseAudioRegionLoop()
        {
            var newAudioLenght = AudioRegionDuractionTimeSec * (LoopAudioX + 1);
            if (newAudioLenght < SourceAudio.clip.length || newAudioLenght < CutedAudio?.clip.length)
            {
                if (LoopAudioX < 20)
                {
                    AudioRegionExtend(AudioRegionDuractionTimeSec * ++LoopAudioX);
                }
            }
        }

        public void IncreaseGifLoop()
        {
            if (_loopGifX < 20)
            {
                GifsGramMake(InputGif, Gifgram, GifFramePrefab, ++_loopGifX);
            }
        }

        public void AudioRegionExtend(float newDuractionTime)
        {
            var deltaWidth = newDuractionTime / AudioRegionDuractionTimeSec;
            HistogramRegionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HistogramRegionRectTransform.rect.width * deltaWidth);
            HistogramRegionRectTransform.localPosition = new Vector3(HistogramRegionRectTransform.localPosition.x + HistogramRegionRectTransform.rect.width / 2 -
                                                                     HistogramRegionRectTransform.rect.width / (2 * deltaWidth), 
                HistogramRegionRectTransform.localPosition.y, 0);
            HistogramRegionResizableRec.UpdateBorders();
            HistogramRegionResizableRec.UpdateBordersTime();
        }


        public Texture2D PaintWaveformSpectrum(AudioClip inAudio, int width, int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float[] samples = new float[inAudio.samples];
            float[] waveform = new float[width];
            inAudio.GetData(samples, 0);
            int packSize = (inAudio.samples / width) + 1;
            int s = 0;
            for (int i = 0; i < inAudio.samples; i += packSize)
            {
                waveform[s] = Mathf.Abs(samples[i]);
                s++;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, Color.black);
                }
            }

            for (int x = 0; x < waveform.Length; x++)
            {
                for (int y = 0; y <= waveform[x] * ((float) height * .75f); y++)
                {
                    tex.SetPixel(x, (height / 2) + y, col);
                    tex.SetPixel(x, (height / 2) - y, col);
                }
            }

            tex.Apply();

            return tex;
        }

        public void GifsGramMake(List<MyGifFrame> myGif, HorizontalLayoutGroup gifsgram, GameObject gifFramePrefab, int loop)
        {
            GifDuractionTimeSec = 0f;
            foreach (var gif in myGif)
            {
                GifDuractionTimeSec += gif.Delay;
            }

            if (gifsgram.transform.childCount > 0)
            {
                GridLayoutGroupRelease(gifsgram.transform);
            }

            GifDuractionTimeSec *= loop;
            GifDuractionTimeText.text = (GifDuractionTimeSec).ToString("00.0");

            do
            {
                foreach (var gif in myGif)
                {
                    //var scale = gif.Delay / GifHistogramLenghtSec;
                    var size = (int) Mathf.Sqrt((float) gif.Encoded.Length / 3);
                    //var sizeDelay = GifgramRect.rect.width * gif.Delay / GifHistogramLenghtSec;
                    var tex = new Texture2D((int)size, (int)size, TextureFormat.RGB24, false)
                        {filterMode = FilterMode.Point};
                    var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one / 2);
                    tex.LoadRawTextureData(gif.Encoded);
                    tex.Apply();
                    sprite.texture.LoadImage(tex.EncodeToPNG());
                    gifFramePrefab.GetComponent<Image>().sprite = sprite;
                    Instantiate(gifFramePrefab, gifsgram.transform);
                }
                loop--;
            } while (loop > 0);
        }

        void GridLayoutGroupRelease(Transform gridLayoutGroup)
        {
            for (int i = 0; i < gridLayoutGroup.childCount; i++)
            {
                var child = gridLayoutGroup.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        public bool MakeHistogramImage(AudioClip inputClip, int loop)
        {
            HistogramImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, HistogramWindowRectTransform.rect.height);
            var histogramTexture = PaintWaveformSpectrum(inputClip, (int) GetHistogramWidth(inputClip),
                (int) HistogramImage.rectTransform.rect.height, Color.green);
            if (histogramTexture == null)
            {
                return false;
            }

            HistogramImage.sprite = Sprite.Create(histogramTexture,
                new Rect(0, 0, histogramTexture.width, histogramTexture.height), Vector2.one / 2);
            if (HistogramImage.sprite == null)
            {
                return false;
            }

            if (!HistogramImage.sprite.texture.LoadImage(histogramTexture.EncodeToPNG()))
            {
                return false;
            }

            HistogramImage.sprite.texture.Apply();

            return true;
        }

        public void ResetDraggableRects()
        {
            foreach (var rect in DraggableRects)
            {
                rect.ResetDraggableRect();
            }
        }

        public void ToEndDraggableRects()
        {
            foreach (var rect in DraggableRects)
            {
                rect.ToEndDraggableRect();
            }
        }

        public float GetHistogramWidth(AudioClip music)
        {
            if (music.length >= _histogramFrameWidthInSeconds)
            {
                var delta = music.length - _histogramFrameWidthInSeconds;
                var secondWidth = HistogramParentRectTransform.rect.width / _histogramFrameWidthInSeconds;
                HistogramImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    HistogramParentRectTransform.rect.width + secondWidth * delta);
                return HistogramImage.rectTransform.rect.width;
            }
            else
            {
                HistogramImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    HistogramParentRectTransform.rect.width);
                return HistogramImage.rectTransform.rect.width;
            }
        }

        public static void UpdateStartEndTimes()
        {
            var time1 = float.Parse(Instance.StartTimeBorderInput.text);
            var time2 = float.Parse(Instance.EndTimeBorderInput.text);
            if (time1 > time2)
            {
                StartTime = time2;
                EndTime = time1;
            }
            else
            {
                StartTime = time1;
                EndTime = time2;
            }
            AudioRegionDuractionTimeSec = EndTime - StartTime;
            Instance.AudioRegionDuractionTimeText.text = AudioRegionDuractionTimeSec.ToString("00.00");
        }

        public void PlayRegion()
        {
            UpdateStartEndTimes();
            SourceAudio.time = StartTime;
            SourceAudio.Play();
        }

        public void CutButton()
        {
            UpdateStartEndTimes();
            Debug.Log("CCCCCCCCCCCCC");
            WWW www;
#if UNITY_EDITOR
            www = new WWW("file://" + EditorUtility.OpenFilePanel("Select a short Song", "", ""));
#elif UNITY_ANDROID
            www = new WWW("file://" + Mp3Cut(StartTime, EndTime));
#else
            return;
#endif
            if (www == null || www.url == null)
            {
                return;
            }
            CutedAudio.clip = www.GetAudioClip(false, false);
            Debug.Log("www = " + www.url);
            Debug.Log(" CutedAudio clip lenght = " + (CutedAudio.clip.length));
            CutedAudio.Play();

            LoopAudioX = 1;
            CurrentAudioLenght = AudioRegionDuractionTimeSec = CutedAudio.clip.length;
            LoopAudioX = 1;
            MakeHistogramImage(CutedAudio.clip, LoopAudioX);
            Debug.Log("DONE: " + StartTime + " - " + EndTime + " - " + CurrentAudioLenght);
            ResetDraggableRects();
        }

        public string Mp3Cut(float start, float end)
        {
            var inPath = Path.Combine(Application.persistentDataPath, InputMp3 + ".mp3");
            var outPath = Path.Combine(Application.persistentDataPath, "out" + InputMp3 + ".mp3");
            var _start = TimeSpan.FromSeconds(start).ToString(@"hh\:mm\:ss\.ff");
            var _length = TimeSpan.FromSeconds(end - start).ToString(@"hh\:mm\:ss\.ff");
            Debug.Log("Start = " + _start + ", Length = " + _length);
            File.Delete(outPath);

            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Debug.Log("CurrentDirectory" + Environment.CurrentDirectory);
            Debug.Log("persistentDataPath" + Application.persistentDataPath);
            Debug.Log("inPath = " + inPath);
            Debug.Log("outPath = " + outPath);
            Debug.Log("File exists = " + File.Exists(inPath));
            var player = new AndroidJavaClass("com.unity3d.player.UnityPlayerActivity");
            var scannery = new AndroidJavaClass("com.arthenica.mobileffmpeg.FFmpeg");
            var cmd = $"-ss {_start} -t {_length} -i {inPath} -acodec copy {outPath}";
            Debug.Log("command is = " + cmd);
            var y = scannery.CallStatic<int>("execute", cmd);
            Debug.Log("result: " + y.ToString());
            Debug.Log("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            return outPath;
        }

        public string Img2Mp4(string imgLabel, int frameRate)
        {
            //var y = scannery.CallStatic<int>("execute",  $"-f image2 -framerate 9 -i image%d.jpg video.mp4

            return "";
        }

        public void InitInputGif(List<MyGifFrame> myGif)
        {
            myGif.Add(new MyGifFrame(Image1.GetRawTextureData(), Image1.width, Image1.height, 1f, null));
            myGif.Add(new MyGifFrame(Image2.GetRawTextureData(), Image2.width, Image2.height, 4f, null));
            myGif.Add(new MyGifFrame(Image3.GetRawTextureData(), Image3.width, Image3.height, 1f, null));
            myGif.Add(new MyGifFrame(Image4.GetRawTextureData(), Image3.width, Image3.height, 1f, null));
        }

        public void RegionAutoSynth()
        {
            if (GifDuractionTimeSec > SourceAudio.clip.length || GifDuractionTimeSec > CutedAudio?.clip.length)
            {
                Debug.Log("GifDuractionTimeSec > Audio.clip.length! This auto feature is TODO!");             // TODO
            }
            else
            {
                var deltaTime = GifDuractionTimeSec / AudioRegionDuractionTimeSec;
                HistogramRegionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HistogramRegionRectTransform.rect.width * deltaTime);
                HistogramRegionResizableRec.UpdateBorders();
            }
        }

        public void AutoAudioExtend()
        {
            // расширяет аудио до размера гиф через ffmpeg
        }

        public void AudioExtendX2()
        {
            // дублирует аудио в два раза через ffmpeg
        }

        public void ResetRegion()
        {
            foreach (var borderInputField in BordersInputFields)
            {
                borderInputField.text = "11,11";
            }
        }

    }
}
