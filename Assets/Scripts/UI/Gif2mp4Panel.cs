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
using Random = System.Random;

namespace Assets.Scripts.UI
{
    public class Gif2mp4Panel : BaseInterface
    {
        public Texture2D Image1;
        public Texture2D Image2;
        public Texture2D Image3;
        public Texture2D Image4;
        public AudioSource InputAudioSource;
        public AudioSource CurrAudioSource;
        public AudioClip CloneAudioClip;
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
        public List<TimeBorder> BordersTimeBorder;
        public List<InputField> BordersInputFields;
        public GameObject ReturnBordersButton;
        public TimeBorder CenterLabel;

        public HorizontalLayoutGroup Gifgram;
        public GameObject GifFramePrefab;
        public Text StartTimeBorder;
        public Text EndTimeBorder;
        public InputField StartTimeBorderInput;
        public InputField EndTimeBorderInput;
        public string InputMp3Path = "";

        public static Gif2mp4Panel Instance;
        public static float CurrentAudioLenght;
        public static float GifDuractionTimeSec = 0f;
        public static float AudioRegionDuractionTimeSec = 0f;
        public static float StartTime;
        public static float EndTime;
        public static int BordersOutCount = 2; // enter twice from start
        public static int LoopAudioX = 1;

        private float _histogramFrameWidthInSeconds = 10f;
        private int _loopGifX = 1;
        private string _newAudio;
        private string _outAudioName;
        private Texture2D _currentTexture2D;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
#if UNITY_EDITOR
            StartDo();
#elif UNITY_ANDROID
            OpenReadAudioFile();
#endif
        }

        public void Update()
        {
            if (CurrAudioSource.isPlaying && CurrAudioSource.time > EndTime)
            {
                CurrAudioSource.Stop();
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

        public void StartDo()
        {
            InitGifsGram(_loopGifX);                                    // INPUT GIF

            MakeHistogramImage(InputAudioSource.clip, LoopAudioX);      // Audio Histogram

            CurrentAudioLenght = InputAudioSource.clip.length;
            CurrAudioSource.clip = InputAudioSource.clip;
            _outAudioName = InputAudioSource.clip.name;
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
            if (newAudioLenght < InputAudioSource.clip.length || newAudioLenght < CurrAudioSource.clip?.length)
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
            HistogramRegionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                HistogramRegionRectTransform.rect.width * deltaWidth);
            HistogramRegionRectTransform.localPosition = new Vector3(
                HistogramRegionRectTransform.localPosition.x + HistogramRegionRectTransform.rect.width / 2 -
                HistogramRegionRectTransform.rect.width / (2 * deltaWidth),
                HistogramRegionRectTransform.localPosition.y, 0);
            HistogramRegionResizableRec.UpdateBorders();
            HistogramRegionResizableRec.UpdateBordersTime();
        }

        public Texture2D PaintWaveformSpectrum(AudioClip inAudio, int width, int height, Color col)
        {
            if (_currentTexture2D != null)
            {
                Destroy(_currentTexture2D);
            }

            _currentTexture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
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

            var pixelsArr = _currentTexture2D.GetPixels32();
            for (int x = 0; x < width * height; x++)
            {
                    pixelsArr[x] = Color.black;
            }
            
            _currentTexture2D.SetPixels32(pixelsArr);
            for (int x = 0; x < waveform.Length; x++)
            {
                for (int y = 0; y <= waveform[x] * ((float) height * .5f); y++)
                {
                    var newYpos = (height / 2) + y;
                    var newYneg = (height / 2) - y;
                    pixelsArr[x + newYpos * waveform.Length] = col;
                    pixelsArr[x + newYneg * waveform.Length] = col;
                }
            }

            _currentTexture2D.SetPixels32(pixelsArr);
            _currentTexture2D.Apply();

            return _currentTexture2D;
        }

        public void GifsGramMake(List<MyGifFrame> myGif, HorizontalLayoutGroup gifsgram, GameObject gifFramePrefab,
            int loop)
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
                    var tex = new Texture2D((int) size, (int) size, TextureFormat.RGB24, false)
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
            HistogramImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                HistogramWindowRectTransform.rect.height);
            var x = GetHistogramWidth(inputClip);
            var histogramTexture = PaintWaveformSpectrum(inputClip, (int) x,
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
                var delta = music.length / _histogramFrameWidthInSeconds;
                HistogramImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    HistogramParentRectTransform.rect.width * delta);
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
            CurrAudioSource.time = StartTime;
            CurrAudioSource.Play();
        }

        public void HistorgamCutButton()                         
        {
            FfmpegCall(FfmpegCallDOAudioCut);
        }

        public void HistorgamExtendX2Button()                     // DEP
        {
            FfmpegCall(FfmpegCallDOAudioExtendX2);
        }

        public void FfmpegCall(Func<string> Do)
        {
            UpdateStartEndTimes();

#if UNITY_EDITOR
            InputMp3Path = EditorUtility.OpenFilePanel("Select a short Song", "", "");
#elif UNITY_ANDROID
            InputMp3Path = Do();                            // вызов ffmpeg
#else
            return;
#endif
            if (string.IsNullOrEmpty(InputMp3Path))
            {
                Debug.Log("FfmpegCall: Do returned InputMp3Path null");      // ffmpeg fail
            }

            Debug.Log("FfmpegCall: InputMp3Path = " + InputMp3Path);
            StartCoroutine(GetAudioClip());                 // загрузка результатов ffmpeg
        }

        public AudioClip MakeMonoAudioClip(AudioClip audioClip, string name)
        {
            AudioClip newAudioClip =
                AudioClip.Create(name, audioClip.samples, audioClip.channels, audioClip.frequency, false);
            float[] copyData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(copyData, 0);

            List<float> monoData = new List<float>();
            for (int i = 0; i < copyData.Length; i += 2)
            {
                monoData.Add(copyData[i]);
            }

            newAudioClip.SetData(monoData.ToArray(), 0);

            return newAudioClip;
        }

        private string FfmpegCallDOAudioCut()
        {
            var outPath = Path.Combine(Application.persistentDataPath, _outAudioName);

            if (outPath == InputMp3Path)
            {
                outPath = Path.Combine(Application.persistentDataPath, "out" + (int)UnityEngine.Random.Range(1, 100) + ".mp3");
            }

            Debug.Log("FfmpegCallDOAudioCut: inPath = " + InputMp3Path);
            Debug.Log("FfmpegCallDOAudioCut: outPath = " + outPath);

            File.Delete(outPath);

            var _start = TimeSpan.FromSeconds(StartTime).ToString(@"hh\:mm\:ss\.ff");
            var _length = TimeSpan.FromSeconds(EndTime - StartTime).ToString(@"hh\:mm\:ss\.ff");

            var cmd = $"-ss {_start} -t {_length} -i {InputMp3Path} -acodec copy {outPath}";

            return FfmpegExecute(cmd) == 0 ? outPath : "";
        }

        public int FfmpegExecute(string cmd)
        {
            Debug.Log("FfmpegExecute: CurrentDirectory" + Environment.CurrentDirectory);
            Debug.Log("FfmpegExecute: persistentDataPath" + Application.persistentDataPath);

            var player = new AndroidJavaClass("com.unity3d.player.UnityPlayerActivity");
            var scannery = new AndroidJavaClass("com.arthenica.mobileffmpeg.FFmpeg");

            Debug.Log("FfmpegExecute: command is = " + cmd);
            var res = scannery.CallStatic<int>("execute", cmd);
            Debug.Log("FfmpegExecute: result: " + res.ToString());

            return res;
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
            if (GifDuractionTimeSec > InputAudioSource.clip.length ||
                GifDuractionTimeSec > CurrAudioSource.clip?.length)
            {
                Debug.Log("GifDuractionTimeSec > Audio.clip.length! This auto feature is TODO!"); // TODO
            }
            else
            {
                var deltaTime = GifDuractionTimeSec / AudioRegionDuractionTimeSec;
                HistogramRegionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    HistogramRegionRectTransform.rect.width * deltaTime);
                HistogramRegionResizableRec.UpdateBorders();
            }
        }

        public void AutoAudioExtend()
        {
            // расширяет аудио до размера гиф через ffmpeg
        }

        private string FfmpegCallDOAudioExtendX2()
        {
            var outPath = Path.Combine(Application.persistentDataPath, _outAudioName);

            if (outPath == InputMp3Path)
            {
                outPath = Path.Combine(Application.persistentDataPath, "out" + (int)UnityEngine.Random.Range(1, 100) + ".mp3");
            }
            
            Debug.Log("FfmpegCallDOAudioExtendX2: inPath = " + InputMp3Path);
            Debug.Log("FfmpegCallDOAudioExtendX2: outPath = " + outPath);

            File.Delete(outPath);

            return FfmpegExecute("-i concat:" + InputMp3Path + "|" + InputMp3Path + " -acodec copy " + outPath) == 0
                ? outPath
                : "";
        }

        public void ResetRegion()
        {
            var deltaTime = -1f;
            for (int i = 0; i < 2; i++)
            {
                BordersInputFields[i].text = GifDuractionTimeSec <= 10f
                    ? (float.Parse(CenterLabel.TimeText.text) + deltaTime * GifDuractionTimeSec / 4f).ToString()
                    : (float.Parse(CenterLabel.TimeText.text) + deltaTime * 2f).ToString();

                BordersTimeBorder[i].SetTime(BordersInputFields[i].text);

                deltaTime *= deltaTime;
            }
        }

        public byte[] LoadBytesFromResoursesTxt(string path)
        {
            TextAsset file = Resources.Load(path) as TextAsset;
            if (file != null)
            {
                return file.bytes;
            }

            return null;
        }

        public void SaveFile(byte[] bytes, string prefix)
        {
            Stream stream = new MemoryStream(bytes);
            BinaryWriter binaryWriter;
#if UNITY_EDITOR
            binaryWriter = new BinaryWriter(File.Open(EditorUtility.SaveFilePanel("Select to save", "", "", "mp3"),
                FileMode.Create));
#elif UNITY_ANDROID
            var outPath = Path.Combine(Application.persistentDataPath, prefix + _outAudioName + ".mp3");
            File.Delete(outPath);
            Debug.Log("SaveFile: OutFile outPath exists = " + File.Exists(outPath) + " :: " + outPath);
            binaryWriter = new BinaryWriter(File.Open(outPath, FileMode.Create));
#endif
            binaryWriter.Write(bytes);
            binaryWriter.Close();
        }

        public void OpenReadAudioFile()
        {
            PickFile();
            StartCoroutine(GetAudioClip());
        }

        public void PickFile()
        {
            var mp3FileType = NativeFilePicker.ConvertExtensionToFileType("mp3");
            var permission = NativeFilePicker.PickFile(path =>
            {
                if (path == null)
                {
                    Debug.Log("PickFile: Operation cancelled");
                }
                else
                {
                    Debug.Log("PickFile: path = " + path);
                    InputMp3Path = path;
                }
            }, new string[] { mp3FileType });

            Debug.Log("PickFile: permission = " + permission);
        }

        private IEnumerator GetAudioClip()
        {
            yield return new WaitForSeconds(2f);


            if (!string.IsNullOrEmpty(InputMp3Path))
            {
                Debug.Log("GetAudioClip: InputMp3Path = " + InputMp3Path);
                using (var www = UnityWebRequestMultimedia.GetAudioClip($"file://{InputMp3Path}", AudioType.MPEG))
                {
                    yield return www.SendWebRequest();

                    if (www.isHttpError || www.isNetworkError)
                    {
                        Debug.Log("GetAudioClip: " + www.error);
                    }
                    else
                    {
                        var clip = DownloadHandlerAudioClip.GetContent(www);

                        Debug.Log("GetAudioClip: GetContent bytes " + clip.length);
                        _outAudioName = "out.mp3";
                        Debug.Log("GetAudioClip: _outAudioName = " + _outAudioName);

                        CloneAudioClip = MakeMonoAudioClip(clip, _outAudioName);

                        InputAudioSource.clip = clip;
                        CurrAudioSource.clip = clip;
                        LoopAudioX = 1;
                        LoopAudioX = 1;
                        CurrentAudioLenght = AudioRegionDuractionTimeSec = CurrAudioSource.clip.length;
                        if (MakeHistogramImage(CloneAudioClip, LoopAudioX))
                        {
                            ResetDraggableRects();

                            Debug.Log("GetAudioClip: CurrAudioSource clip lenght = " + (CurrAudioSource.clip.length));
                            Debug.Log("GetAudioClip done: " + StartTime + " - " + EndTime + " - " + CurrentAudioLenght);
                        }
                        else
                        {
                            Debug.Log("GetAudioClip: MakeHistogramImage error!");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("GetAudioClip: critical error: input path is null!!!!!!!!!!!!!!");
            }

        }
    }

}

