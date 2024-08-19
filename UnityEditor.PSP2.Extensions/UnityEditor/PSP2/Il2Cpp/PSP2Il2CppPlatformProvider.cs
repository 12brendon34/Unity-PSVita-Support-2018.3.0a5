using System.IO;
using UnityEditorInternal;

namespace UnityEditor.PSP2.Il2Cpp
{
	internal class PSP2Il2CppPlatformProvider : BaseIl2CppPlatformProvider
	{
		private readonly bool m_emitNullChecks = false;

		private readonly bool m_emitArrayBoundsChecks = false;

		private readonly int m_scriptOptimizationLevel = 2;

		private readonly string m_extraLinkerOptions = "";

		public override bool emitNullChecks => m_emitNullChecks;

		public override bool enableArrayBoundsCheck => m_emitArrayBoundsChecks;

		public override string nativeLibraryFileName => "Il2CppAssemblies.suprx";

		public string il2CppFolder => Path.GetFullPath(Path.Combine(EditorApplication.applicationContentsPath, "il2cpp"));

		public override string[] includePaths => new string[0];

		public PSP2Il2CppPlatformProvider(BuildTarget target, bool explicitNullChecks, bool arrayBoundsChecks, int scriptOptimizationLevel, string extraLinkerOptions)
			: base(target, null)
		{
			m_emitNullChecks = explicitNullChecks;
			m_emitArrayBoundsChecks = arrayBoundsChecks;
			m_scriptOptimizationLevel = scriptOptimizationLevel;
			m_extraLinkerOptions = extraLinkerOptions;
		}

		public override INativeCompiler CreateNativeCompiler()
		{
			return null;
		}

		public override Il2CppNativeCodeBuilder CreateIl2CppNativeCodeBuilder()
		{
			return new PSP2Il2CppNativeCodeBuilder(m_scriptOptimizationLevel, m_extraLinkerOptions);
		}
	}
}
