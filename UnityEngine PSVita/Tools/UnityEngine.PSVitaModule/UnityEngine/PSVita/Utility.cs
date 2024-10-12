using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("UnityPrefix.h")]
	public class Utility
	{
		public enum SkuFlags
		{
			None = 0,
			Trial = 1,
			Full = 3
		}

		public enum PowerTickType
		{
			Default = 0,
			DisableAutoSuspend = 1,
			DisableDisplayOff = 4,
			DisableDisplayDimming = 6
		}

		public enum MountableContent
		{
			Music,
			Photos
		}

		public enum SystemLanguage
		{
			JAPANESE,
			ENGLISH_US,
			FRENCH,
			SPANISH,
			GERMAN,
			ITALIAN,
			DUTCH,
			PORTUGUESE_PT,
			RUSSIAN,
			KOREAN,
			CHINESE_T,
			CHINESE_S,
			FINNISH,
			SWEDISH,
			DANISH,
			NORWEGIAN,
			POLISH,
			PORTUGUESE_BR,
			ENGLISH_GB,
			TURKISH
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern SkuFlags skuFlags
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::GetSkuFlags")]
			get;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool gcEnable
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::IsGCEnabled")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::SetGCEnabled")]
			set;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int gcDisableMaxHeapSize
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::GetGcDisableMaxMonoHeapSize")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::SetGcDisableMaxMonoHeapSize")]
			set;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern SystemLanguage systemLanguage
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::GetSCESystemLanguage")]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("SetMonoHeapBehaviours")]
		public static extern bool SetMonoHeapBehaviours(bool constrain, bool tight);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("PSP2::CommonDialogIsRunning")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool commonDialogIsRunning();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("PSP2::SetInfoBarState")]
		public static extern bool SetInfoBarState(bool visible, bool white, bool translucent);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("PSP2::PowerTick")]
		public static extern bool PowerTick(PowerTickType tickType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("PSP2::MountContent")]
		public static extern int MountContent(MountableContent content);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("PSP2::UnmountContent")]
		public static extern int UnmountContent(MountableContent content);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("PSP2::EnableHeapBlockSorting")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern void EnableHeapBlockSorting();
	}
}
