using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("UnityPrefix.h")]
	public class PSVitaVideoPlayer
	{
		public enum Looping
		{
			None,
			Continuous
		}

		public enum Mode
		{
			FullscreenVideo,
			RenderToTexture
		}

		public enum MovieEvent
		{
			STOP = 1,
			READY = 2,
			PLAY = 3,
			PAUSE = 4,
			BUFFERING = 5,
			TIMED_TEXT_DELIVERY = 16,
			WARNING_ID = 32,
			ENCRYPTION = 48
		}

		public enum TrickSpeeds
		{
			Normal = 100,
			FF_2X = 200,
			FF_4X = 400,
			FF_8X = 800,
			FF_16X = 1600,
			FF_MAX = 3200,
			RW_8X = -800,
			RW_16X = -1600,
			RW_MAX = -3200
		}

		public struct PlayParams
		{
			public Looping loopSetting;

			public Mode modeSetting;

			public string audioLanguage;

			public int audioStreamIndex;

			public string textLanguage;

			public int textStreamIndex;

			public float volume;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern long subtitleTimeStamp
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("Video::GetSubtitleTime")]
			get;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern string subtitleText
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("Video::GetSubtitleText")]
			get;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern long videoDuration
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("Video::GetVideoLength")]
			get;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern long videoTime
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("Video::GetCurrentTime")]
			get;
		}

		//[FreeFunction("Video::PlayEx")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static bool PlayEx(string path, PlayParams vidParams)
		{
			return PlayEx_Injected(path, ref vidParams);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::Play")]
		public static extern bool Play(string path, Looping loop, Mode fullscreenvideo);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::SetVolume")]
		public static extern void SetVolume(float volume);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("Video::Stop")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern void Stop();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::Pause")]
		public static extern void Pause();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::TransferMemToHeap")]
		public static extern bool TransferMemToHeap();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::TransferMemToMonoHeap")]
		public static extern bool TransferMemToMonoHeap();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("Video::Resume")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool Resume();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::SetTrickSpeed")]
		public static extern bool SetTrickSpeed(TrickSpeeds speed);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::JumpToTime")]
		public static extern bool JumpToTime(ulong jumpTimeMsec);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("Video::GetCurrentTime")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern ulong GetCurrentTime();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("Video::GetVideoLength")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern ulong GetVideoLength();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("Video::Init")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern void Init(RenderTexture renderTexture);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("Video::Update")]
		public static extern void Update();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool PlayEx_Injected(string path, ref PlayParams vidParams);
	}
}
