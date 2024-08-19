using UnityEditor.Modules;

namespace UnityEditor.PSP2
{
	internal class PSP2ScriptingImplementations : DefaultScriptingImplementations
	{
		public override ScriptingImplementation[] Enabled()
		{
			if (PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Legacy)
			{
				return new ScriptingImplementation[2]
				{
					ScriptingImplementation.Mono2x,
					ScriptingImplementation.IL2CPP
				};
			}
			return new ScriptingImplementation[1] { ScriptingImplementation.IL2CPP };
		}
	}
}
