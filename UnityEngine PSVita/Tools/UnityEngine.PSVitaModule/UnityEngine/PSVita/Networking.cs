using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("UnityPrefix.h")]
	public class Networking
	{
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool enableUDPP2P
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("getP2Pflag")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("setP2Pflag")]
			set;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int npSignalingPort
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GetNpSignalingPort")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("SetNpSignalingPort")]
			set;
		}
	}
}
