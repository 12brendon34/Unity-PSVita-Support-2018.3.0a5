using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("UnityPrefix.h")]
	public class Diagnostics
	{
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool enableHUD
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::HudEnabled")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("PSP2::SetHudEnabled")]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("PSP2::GetFreeMemoryLPDDR")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int GetFreeMemoryLPDDR();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("PSP2::GetFreeMemoryCDRAM")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int GetFreeMemoryCDRAM();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("PSP2::GetFreeMemoryPHYSCONT")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int GetFreeMemoryPHYSCONT();
	}
}
