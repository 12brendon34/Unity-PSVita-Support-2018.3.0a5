using System;
using UnityEngine.Bindings;

namespace UnityEngine.Scripting
{
	[VisibleToOtherModules]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, Inherited = false)]
	internal class UsedByNativeCodeAttribute : Attribute
	{
		public string Name { get; set; }

		public UsedByNativeCodeAttribute()
		{
		}

		public UsedByNativeCodeAttribute(string name)
		{
			Name = name;
		}
	}
}
