using UnityEditor.Modules;

namespace UnityEditor.PSP2
{
	internal class PSP2BuildPostprocessor : DefaultBuildPostprocessor
	{
		public override void PostProcess(BuildPostProcessArgs args)
		{
			PostProcessPSP2Player.PostProcessArgs = args;
			PostProcessPSP2Player.PostProcess(args.target, args.options, args.installPath, args.stagingAreaData, args.stagingArea, args.playerPackage, args.stagingAreaDataManaged, args.usedClassRegistry);
		}

		public override bool SupportsLz4Compression()
		{
			return false;
		}

		public override string PrepareForBuild(BuildOptions options, BuildTarget target)
		{
			return PostProcessPSP2Player.PrepareForBuild(options, target);
		}

		public override bool SupportsInstallInBuildFolder()
		{
			return true;
		}
	}
}
