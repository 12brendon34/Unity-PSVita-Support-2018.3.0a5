using System;
using UnityEditor.Modules;

namespace UnityEditor.PSP2
{
	internal class TargetExtension : DefaultPlatformSupportModule
	{
		private static PSP2BuildWindowExtension buildWindow;

		public override string TargetName => "PSP2";

		public override string JamTarget => "PSP2EditorExtensions";

		public override string[] AssemblyReferencesForUserScripts => new string[0];

		public override IBuildPostprocessor CreateBuildPostprocessor()
		{
			return new PSP2BuildPostprocessor();
		}

		public override IScriptingImplementations CreateScriptingImplementations()
		{
			return new PSP2ScriptingImplementations();
		}

		public override ISettingEditorExtension CreateSettingsEditorExtension()
		{
			return new PSP2SettingsEditorExtension();
		}

		public override IDevice CreateDevice(string id)
		{
			throw new NotSupportedException();
		}

		public override IBuildWindowExtension CreateBuildWindowExtension()
		{
			return (buildWindow != null) ? buildWindow : (buildWindow = new PSP2BuildWindowExtension());
		}
	}
}
