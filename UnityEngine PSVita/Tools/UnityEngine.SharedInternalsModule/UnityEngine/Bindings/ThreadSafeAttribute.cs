using System;

namespace UnityEngine.Bindings
{
	[VisibleToOtherModules]
	[AttributeUsage(AttributeTargets.Method)]
	internal class ThreadSafeAttribute : NativeMethodAttribute
	{
		public ThreadSafeAttribute()
		{
			base.IsThreadSafe = true;
		}
	}
}
