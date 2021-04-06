using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts
{
	public static class Constants
    {
        public const int PaletteMaxSize = 256;
		public const int MinImageSize = 1;
		public const int HistorySize = 50;
		public const float MinFrameDelay = 0.02f;
		public const float MaxFrameDelay = 60f;
		public const string SupportEmail = "hippogamesunity@gmail.com";
		public const int Kilobyte = 1024;
		public const int Megabyte = 1024 * 1024;
		public const string PngExtension = ".png";
        public const string BmpExtension = ".bmp";
		public const string JpgExtension = ".jpg";
		public const string JpegExtension = ".jpeg";
		public const string GifExtension = ".gif";
        public const string Mp4Extension = ".mp4";
		public const string PspExtension = ".psp";
	    public const string PsxExtension = ".psx";
        public const string AseExtension = ".ase";
        public const string AsepriteExtension = ".aseprite";

		public const string Pro = "Professional";

		#if UNITY_ANDROID

		#if PREMIUM

		public const string StoreLink = "market://details?id=com.pixelstudio.pro";
		public const string StoreLinkWeb = "https://play.google.com/store/apps/details?id=com.pixelstudio.pro";

		#elif SMARTPASS

        public const string StoreLink = "http://pass.auone.jp/app/detail?app_id=2254410000075";
		public const string StoreLinkWeb = "http://pass.auone.jp/app/detail?app_id=2254410000075";

		#elif APPGALLERY

        public const string StoreLink = "market://details?id=com.pixelstudio.pro.huawei";
		public const string StoreLinkWeb = "https://appgallery.huawei.com/#/app/C103064177";
    
        #else

		public const string StoreLink = "market://details?id=com.PixelStudio";
		public const string StoreLinkWeb = "https://play.google.com/store/apps/details?id=com.PixelStudio";

		#endif

		#elif UNITY_IOS

		#if PREMIUM

		public const string StoreLink = "https://apps.apple.com/app/id1476932307?action=write-review";
		public const string StoreLinkWeb = "https://apps.apple.com/app/id1476932307?action=write-review";

		#else

		public const string StoreLink = "https://apps.apple.com/app/id1404203859?action=write-review";
		public const string StoreLinkWeb = "https://apps.apple.com/app/id1404203859?action=write-review";

		#endif

        #elif UNITY_STANDALONE_WIN && STEAM

        public const string StoreLink = "steam://advertise/1204050";
		public const string StoreLinkWeb = "https://store.steampowered.com/app/1204050";

        #elif UNITY_STANDALONE_WIN || UNITY_WSA

        public const string StoreLink = "ms-windows-store://pdp/?ProductId=9P7XS7VH1R3J";
		public const string StoreLinkWeb = "https://www.microsoft.com/store/apps/9P7XS7VH1R3J";
        public const string ReviewLink = "ms-windows-store://pdp/?ProductId=9P7XS7VH1R3J?&activetab=pivot:reviewstab";

        #elif UNITY_STANDALONE_OSX

        public const string StoreLink = "https://apps.apple.com/app/id1477015249?action=write-review";
		public const string StoreLinkWeb = "https://apps.apple.com/app/id1477015249?action=write-review";

		#endif

		public const int MaxBrushSize = 128;
		public const int MaxObjectSize = 256;
	    public const int MaxGradientSteps = 256;

        public static Color32 EmptyColor = new Color32(0, 0, 0, 0);
        public static Color32 BlackColor = new Color32(0, 0, 0, 255);

		public const int MaxImageSizeEx = 4096;

		public static int MaxImageSize => 1024;

        public const int HeavyProjectInPixels = 10 * 512 * 512;
        public const int HeavyProjectInBytes = 1024 * 1024;
	}
}