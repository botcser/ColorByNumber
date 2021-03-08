using System;
using System.Collections.Generic;
using System.Globalization;
using Assets.Scripts.Common;
using Assets.Scripts.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Client
{
    public class RemoteConvert : MonoBehaviour
    {
        public static string Url = "https://localhost:5001/api/";
        public static string Url2 = "http://hippogames.dev/api/";
        public static string Token = "cac464deb2b2bd3dc39218b649769f4b";
        private Dictionary<int, Coroutine> _coroutines = new Dictionary<int, Coroutine>();


        public void ConvertToMovie(Gif gif, byte[] audio, int scale, int loop, Action<bool, string, byte[]> callback, Action<float> onProgress)
        {
            var data = new Dictionary<string, object>
            {
                { "json", JsonConvert.SerializeObject(gif) },
                { "audio", audio },
                { "scale", scale },
                { "loop", loop }
            };

            HttpPost($"{Url}convert/ConvertToMovie", data, (success, error, bytes, json) => callback(success, error, bytes), onProgress);
        }

        private void HttpPost(string url, Dictionary<string, object> data, Action<bool, string, string> callback, Action<float> onProgress = null)
        {
            HttpPost(url, data, (success, error, bytes, json) => callback(success, error, json));
        }

        private void HttpPost(string url, Dictionary<string, object> data, Action<bool, string, byte[], string> callback, Action<float> onProgress = null)
        {
            var form = new WWWForm();

            foreach (var key in data.Keys)
            {
                switch (data[key])
                {
                    case byte[] _:
                        form.AddField(key, Convert.ToBase64String((byte[])data[key]));
                        break;
                    case float _:
                        form.AddField(key, ((float)data[key]).ToString(CultureInfo.InvariantCulture));
                        break;
                    case string _:
                        form.AddField(key, (string)data[key]);
                        break;
                    case DateTime _:
                        form.AddField(key, ((DateTime)data[key]).ToString(CultureInfo.InvariantCulture));
                        break;
                    default:
                        form.AddField(key, data[key] == null ? "" : data[key].ToString());
                        break;
                }
            }

            form.AddField("token", Token);
            form.AddField("hash", 127 * form.data.Length);

            var id = UnityEngine.Random.Range(0, int.MaxValue);

            _coroutines.Add(id, Downloader.Download(url, form, webRequest =>
            {
                _coroutines.Remove(id);

                if (string.IsNullOrEmpty(webRequest.error))
                {
                    callback(true, null, webRequest.downloadHandler.data, webRequest.downloadHandler.text);
                }
                else
                {
                    callback(false, webRequest.responseCode == 500 && !webRequest.downloadHandler.text.StartsWith("<!DOCTYPE html>") ? webRequest.downloadHandler.text : webRequest.error, webRequest.downloadHandler.data, null);
                }
            }, onProgress));
        }




    }
}
