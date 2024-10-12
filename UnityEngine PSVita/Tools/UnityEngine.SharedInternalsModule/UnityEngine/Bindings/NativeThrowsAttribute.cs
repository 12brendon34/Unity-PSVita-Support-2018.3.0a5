using System;

namespace UnityEngine.Bindings
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	[VisibleToOtherModules]
	internal class NativeThrowsAttribute : Attribute, IBindingsThrowsProviderAttribute, IBindingsAttribute
	{
		public bool ThrowsException { get; set; }

		public NativeThrowsAttribute()
		{
			ThrowsException = true;
		}

		public NativeThrowsAttribute(bool throwsException)
		{
			ThrowsException = throwsException;
		}
	}
}
