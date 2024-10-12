using UnityEngine.Bindings;

namespace UnityEngine
{
	[VisibleToOtherModules]
	internal sealed class UnityString
	{
		public static string Format(string fmt, params object[] args)
		{
			return string.Format(fmt, args);
		}
	}
}
