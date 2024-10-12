using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("UnityPrefix.h")]
	public class PSVitaDRM
	{
		public struct DrmContentFinder
		{
			public int dirHandle;

			public string contentDir;
		}

		private static DrmContentFinder DrmContentFinderOpen()
		{
			return default(DrmContentFinder);
		}

		private static DrmContentFinder DrmContentFinderNext(DrmContentFinder finder)
		{
			return finder;
		}

		//[FreeFunction]
		//[NativeConditional("PLATFORM_PSVITA")]
		private static bool DrmContentFinderClose(DrmContentFinder finder)
		{
			return DrmContentFinderClose_Injected(ref finder);
		}

		public static bool ContentFinderOpen(ref DrmContentFinder finder)
		{
			finder = DrmContentFinderOpen();
			return finder.contentDir.Length != 0;
		}

		public static bool ContentFinderNext(ref DrmContentFinder finder)
		{
			finder = DrmContentFinderNext(finder);
			return finder.contentDir.Length != 0;
		}

		public static bool ContentFinderClose(ref DrmContentFinder finder)
		{
			return DrmContentFinderClose(finder);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("DrmContentOpen")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool ContentOpen(string contentDir);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction("DrmContentClose")]
		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool ContentClose(string contentDir);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool DrmContentFinderClose_Injected(ref DrmContentFinder finder);
	}
}
