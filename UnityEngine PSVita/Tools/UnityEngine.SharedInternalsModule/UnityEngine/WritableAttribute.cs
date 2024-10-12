using System;
using UnityEngine.Bindings;

namespace UnityEngine
{
	[VisibleToOtherModules]
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	internal class WritableAttribute : Attribute
	{
	}
}
