using System;
using UnityEngine.Bindings;

namespace UnityEngine.Scripting
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, Inherited = false)]
	[VisibleToOtherModules]
	internal class RequiredByNativeCodeAttribute : Attribute
	{
		public string Name { get; set; }

		public bool Optional { get; set; }

		public bool GenerateProxy { get; set; }

		public RequiredByNativeCodeAttribute()
		{
		}

		public RequiredByNativeCodeAttribute(string name)
		{
			Name = name;
		}

		public RequiredByNativeCodeAttribute(bool optional)
		{
			Optional = optional;
		}

		public RequiredByNativeCodeAttribute(string name, bool optional)
		{
			Name = name;
			Optional = optional;
		}
	}
}
