using System;

namespace UnityEngine.Bindings
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	[VisibleToOtherModules]
	internal class NativeNameAttribute : Attribute, IBindingsNameProviderAttribute, IBindingsAttribute
	{
		public string Name { get; set; }

		public NativeNameAttribute()
		{
		}

		public NativeNameAttribute(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name == "")
			{
				throw new ArgumentException("name cannot be empty", "name");
			}
			Name = name;
		}
	}
}
