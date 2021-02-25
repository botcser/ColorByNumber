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
        public List<Texture2D> Images;
        public List<float> ImagesDuractions = new List<float>(){1, 1, 1, 1};
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
        public GameObject GifFramePrefab;
        public TimeBorder CenterLabel;

        public HorizontalLayoutGroup Gifgram;
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
            InitGifsGram(_loopGifX, 1);                                    // INPUT GIF

            MakeHistogramImage(InputAudioSource.clip, LoopAudioX);      // Audio Histogram

            CurrentAudioLenght = InputAudioSource.clip.length;
            CurrAudioSource.clip = InputAudioSource.clip;
            
#if UNITY_EDITOR
#elif UNITY_ANDROID
            InputMp3Path = SaveFile(LoadBytesFromResoursesTxt(InputAudioSource.clip.name), _outAudioName, "mp3");
            
            _outAudioName = "out.mp3";
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

        public void InitGifsGram(int loop, float ratio)
        {
            InputGif = new List<MyGifFrame>();
            InitInputGif(InputGif, ratio);
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
            FfmpegCall(FfmpegCallDOAudioCut, false);
        }

        public void HistorgamExtendX2Button()                     // DEP
        {
            FfmpegCall(FfmpegCallDOAudioExtendX2, false);
        }

        public void FfmpegCall(Func<string> Do, bool AR)
        {
            UpdateStartEndTimes();

#if UNITY_EDITOR
            var tmp = EditorUtility.OpenFilePanel("Select a short Song", "", "");
#elif UNITY_ANDROID
            var tmp = Do();                            // вызов ffmpeg
#else
            return;
#endif

            if (string.IsNullOrEmpty(tmp))
            {
                Debug.Log("FfmpegCall: Do result is null");      // ffmpeg fail
                return;
            }
            else
            {
                if (InputMp3Path.Contains(Application.persistentDataPath))
                {
                    File.Delete(InputMp3Path);
                }

                InputMp3Path = tmp;
            }

            Debug.Log("FfmpegCall: InputMp3Path = " + InputMp3Path);
            StartCoroutine(GetAudioClip(AR));                 // загрузка результатов ffmpeg
        }

        public AudioClip MakeMonoAudioClip(AudioClip audioClip, string name, bool AR)
        {
            float[] copyData;
            AudioClip newAudioClip;

            if (AR)
            {
                newAudioClip =
                    AudioClip.Create(name, audioClip.samples / 2, audioClip.channels, audioClip.frequency, false);
                copyData = new float[audioClip.samples / 2 * audioClip.channels];
            }
            else
            {
                newAudioClip =
                    AudioClip.Create(name, audioClip.samples, audioClip.channels, audioClip.frequency, false);
                copyData = new float[audioClip.samples * audioClip.channels];
            }


            audioClip.GetData(copyData, 0);

            List<float> monoData = new List<float>();
            for (int i = 0; i < copyData.Length; i += 2)
            {
                monoData.Add(copyData[i]);
            }

            newAudioClip.SetData(monoData.ToArray(), 0);

            return newAudioClip;
        }

        public int FfmpegExecute(string cmd)
        {
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


        private void InitInputGif(List<MyGifFrame> myGif, float ratio)
        {
            foreach (var image in Images)
            {
                myGif.Add(new MyGifFrame(image.GetRawTextureData(), image.width, image.height, ImagesDuractions[Images.IndexOf(image)] * ratio, null));
            }
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

        public void AutoAudioExtendButton()
        {
            FfmpegCall(FfmpegCallDOAutoAudioExtend, true);
        }
        
        public void AutoGifExtendButton()
        {
            var fDuractionRatio = CurrentAudioLenght / GifDuractionTimeSec;

            GifDuractionTimeSec *= fDuractionRatio;

            InitGifsGram(1, fDuractionRatio);

            for (int i = 0; i < ImagesDuractions.Count; i++)
            {
                ImagesDuractions[i] *= fDuractionRatio;
            }
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

        private string FfmpegCallDOAutoAudioExtend()
        {
            // расширяет аудио до размера гиф через ffmpeg
            var outPath = Path.Combine(Application.persistentDataPath, _outAudioName);

            if (outPath == InputMp3Path)
            {
                outPath = Path.Combine(Application.persistentDataPath,
                    "out" + (int) UnityEngine.Random.Range(1, 100) + ".mp3");
            }

            var fDuractionRatio = CurrentAudioLenght / GifDuractionTimeSec;
            if (2.00f <= fDuractionRatio && fDuractionRatio <= 2.03f)
            {
                fDuractionRatio = 2.0f;
            }

            if (0.47f <= fDuractionRatio && fDuractionRatio <= 0.50f)
            {
                fDuractionRatio = 0.50f;
            }

            if (2.0 < fDuractionRatio || fDuractionRatio < 0.5f)
            {
                Debug.Log("FfmpegCallDOAutoAudioExtend: duractionRatio if incorrect (0.5<x<2.0) = " +
                          fDuractionRatio); // RESTRICTION
                return "";
            }

            var sDuractionRatio = fDuractionRatio.ToString().Replace(",", ".");

            Debug.Log("FfmpegCallDOAutoAudioExtend: inPath = " + InputMp3Path);
            Debug.Log("FfmpegCallDOAutoAudioExtend: outPath = " + outPath);
            Debug.Log("FfmpegCallDOAutoAudioExtend: duractionRatio = " + sDuractionRatio);

            File.Delete(outPath);


            //return FfmpegExecute("-codecs") == 0 ? outPath : "";
            return FfmpegExecute(" -i " + InputMp3Path + " -af atempo=" + sDuractionRatio + " -codec:a libmp3lame " + outPath) == 0
                ? outPath
                : "";
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

        public void MakeGif2Mp4()
        {
            var outPath = Path.Combine(Application.persistentDataPath, "gif2mp4" + ".mp4");
            var myGif = MakeGif();

            Debug.Log("MakeGif2Mp4: InputMp3Path = " + InputMp3Path);
            Debug.Log("MakeGif2Mp4: MyGif = " + myGif);
            Debug.Log("MakeGif2Mp4: outPath = " + outPath);

            if (!string.IsNullOrEmpty(myGif))
            {
                FfmpegExecute("-i " + myGif + " -i " + InputMp3Path +
                                     @" -movflags faststart -pix_fmt yuv420p -vf ""scale=trunc(iw/2)*2:trunc(ih/2)*2"" -c:a copy -b:a 128k " +
                                     outPath);
            }
        }

        private string MakeGif()
        {
            var imageName = Path.Combine(Application.persistentDataPath, Images[0].name.Substring(0, Images[0].name.Length - 1));
            var outPath = Path.Combine(Application.persistentDataPath, "out.gif");

            foreach (var image in Images)
            {
                SaveFile(image.EncodeToPNG(), image.name, "png");
            }

            return FfmpegExecute("-f image2 -framerate 1 -i " + imageName + "%d.png -vf scale=" + Images[0].width +
                                 "x" + Images[0].height +
                                 ",transpose=1 " + outPath) == 0
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
            TextAsset file = Resources.Load(path, typeof(TextAsset)) as TextAsset;
            if (file != null)
            {
                Debug.Log("LoadBytesFromResoursesTxt: loaded " + file.bytes.Length);
                return file.bytes;
            }
            Debug.Log("LoadBytesFromResoursesTxt: fail load " + path);
            return null;
        }

        public string SaveFile(byte[] bytes, string outAudioName, string postfix)
        {
            Stream stream = new MemoryStream(bytes);
            BinaryWriter binaryWriter;
            string outPath;
#if UNITY_EDITOR
            outPath = EditorUtility.SaveFilePanel("Select to save", "", "", postfix);
            binaryWriter = new BinaryWriter(File.Open(outPath, FileMode.Create));
#elif UNITY_ANDROID
            outPath = Path.Combine(Application.persistentDataPath, outAudioName + "." + postfix);
            File.Delete(outPath);
            Debug.Log("SaveFile: OutFile outPath exists = " + File.Exists(outPath) + " :: " + outPath);
            binaryWriter = new BinaryWriter(File.Open(outPath, FileMode.Create));
            
#endif
            binaryWriter.Write(bytes);
            binaryWriter.Close();

            return outPath;
        }

        public void OpenReadAudioFile()
        {
            PickFile();
            StartCoroutine(GetAudioClip(false));
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

        private IEnumerator GetAudioClip(bool AR)
        {
            yield return new WaitForSeconds(2f);                    // жду, тк до этого мог выполняться ffmpeg


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

                        CloneAudioClip = MakeMonoAudioClip(clip, _outAudioName, AR);
                        if (AR)
                        {
                            var newAudioClip =
                                AudioClip.Create(name, clip.samples / 2, clip.channels, clip.frequency, false);
                            var copyData = new float[clip.samples / 2 * clip.channels];
                            clip.GetData(copyData, 0);
                            newAudioClip.SetData(copyData, 0);
                            clip = newAudioClip;
                        }

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

