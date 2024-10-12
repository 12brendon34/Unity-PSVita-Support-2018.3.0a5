using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("UnityPrefix.h")]
	public class PSVitaPlayerPrefs
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("LoadPlayerPrefsFromByteArray")]
		public static extern void LoadFromByteArray(byte[] bytes);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("SavePlayerPrefsToByteArray")]
		public static extern byte[] SaveToByteArray();
	}
}
