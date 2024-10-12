using System;

namespace UnityEngine.Bindings
{
	[AttributeUsage(AttributeTargets.Method)]
	[VisibleToOtherModules]
	internal sealed class NativeWritableSelfAttribute : Attribute, IBindingsWritableSelfProviderAttribute, IBindingsAttribute
	{
		public bool WritableSelf { get; set; }

		public NativeWritableSelfAttribute()
		{
			WritableSelf = true;
		}

		public NativeWritableSelfAttribute(bool writable)
		{
			WritableSelf = writable;
		}
	}
}
