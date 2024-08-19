using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.PSP2.Il2Cpp
{
	public class PSP2Il2CppNativeCodeBuilder : Il2CppNativeCodeBuilder
	{
		private readonly int m_scriptOptimizationLevel = 0;

		private readonly string m_extraLinkerOptions;

		public override string CompilerPlatform => "PSP2";

		public override string CompilerArchitecture => "ARMv7";

		public override bool SetsUpEnvironment => true;

		public override string CompilerFlags
		{
			get
			{
				switch (m_scriptOptimizationLevel)
				{
				default:
					return "-O0";
				case 1:
				case 2:
					return "-O3";
				}
			}
		}

		public override string LinkerFlags
		{
			get
			{
				string text = "";
				int scriptOptimizationLevel = m_scriptOptimizationLevel;
				if (scriptOptimizationLevel == 2)
				{
					text += "--strip-unused-data --strip-duplicates ";
					Debug.Log(text);
                }
                Debug.Log(text + m_extraLinkerOptions);
                return text + m_extraLinkerOptions;
            }
		}

		public override string PluginPath => Path.Combine(BuildPipeline.GetBuildToolsDirectory(BuildTarget.PSP2).Replace('/', '\\'), "PSP2Il2CppPlugin.dll");

		public override string CacheDirectory => Application.dataPath + "/../Library";

		public PSP2Il2CppNativeCodeBuilder(int scriptOptimizationLevel, string extraLinkerOptions)
		{
			m_scriptOptimizationLevel = scriptOptimizationLevel;
			m_extraLinkerOptions = extraLinkerOptions;
		}

		public override IEnumerable<string> ConvertIncludesToFullPaths(IEnumerable<string> relativeIncludePaths)
		{
			return relativeIncludePaths;
		}

		public override string ConvertOutputFileToFullPath(string outputFileRelativePath)
		{
			string text = Application.dataPath + "/../";
			return text + outputFileRelativePath;
		}
	}
}
