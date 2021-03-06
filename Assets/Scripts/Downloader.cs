using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Common
{
	/// <summary>
	/// HTTP downloader with WWW utility (creates instance automatically).
	/// </summary>
	[ExecuteInEditMode]
	public class Downloader : MonoBehaviour
	{
		private static Downloader _instance;

		public static Downloader Instance => _instance ?? (_instance = new GameObject("Downloader").AddComponent<Downloader>());

		public void OnDestroy()
		{
			_instance = null;
		}

		public static Coroutine Download(string url, Action<UnityWebRequest> callback)
		{
			Debug.LogFormat("Downloading: {0}", url);

			return Instance.StartCoroutine(Coroutine(url, callback));
		}

		public static Coroutine Download(string url, WWWForm formData, Action<UnityWebRequest> callback, Action<float> onProgress = null)
		{
			Debug.LogFormat("Downloading {0} with post data {1}", url, formData);

            return Instance.StartCoroutine(Coroutine(url, formData, callback, onProgress));
		}

		private static IEnumerator Coroutine(string url, Action<UnityWebRequest> callback)
		{
            using (var webRequest = UnityWebRequest.Get(url))
			{
				yield return webRequest.SendWebRequest();

				Debug.LogFormat("Downloaded: {0}, text = {1}, error = {2}", url, CleanupText(webRequest.downloadHandler.text), webRequest.error);

				callback(webRequest);
			}
		}

		private static IEnumerator Coroutine(string url, WWWForm formData, Action<UnityWebRequest> callback, Action<float> onProgress = null)
        {
			using (var webRequest = UnityWebRequest.Post(url, formData))
			{
				if (onProgress == null)
				{
					yield return webRequest.SendWebRequest();
				}
				else
				{
					webRequest.SendWebRequest();

					while (!webRequest.isDone)
					{
						onProgress(webRequest.downloadProgress);
						yield return null;
					}
				}

                var data = CleanupText(webRequest.downloadHandler.text);

                if (data.Length > 128) data = data.Substring(0, 128);

				Debug.LogFormat("Downloaded: {0}, text={1}, error={2}, bytes={3}.", url, data, webRequest.error, webRequest.downloadHandler.data?.Length);

				callback(webRequest);
			}
        }

		private static string CleanupText(string text)
		{
			return text.Replace("\n", " ").Replace("\t", null);
		}
	}
}