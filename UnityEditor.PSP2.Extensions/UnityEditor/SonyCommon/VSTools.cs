using System;
using System.IO;

namespace UnityEditor.SonyCommon
{
	internal class VSTools
	{
		public enum VSPlatform
		{
			PSVita,
			PS4
		}

		public static void WriteVCSolution(VSPlatform platform, string sourcePath, string solutionPath, string projectName, string executablePath, string workingDirectory)
		{
			Guid guid = Guid.NewGuid();
			string platformName = "";
			string[] cppFiles = new string[0];
			string[] headerFiles = new string[0];
			switch (platform)
			{
			case VSPlatform.PSVita:
				platformName = "PSVita";
				break;
			case VSPlatform.PS4:
				platformName = "ORBIS";
				break;
			}
			if (sourcePath != null)
			{
				cppFiles = Directory.GetFiles(sourcePath, "*.cpp", SearchOption.TopDirectoryOnly);
				headerFiles = Directory.GetFiles(sourcePath, "*.h", SearchOption.TopDirectoryOnly);
			}
			if (!Directory.Exists(solutionPath))
			{
				Directory.CreateDirectory(solutionPath);
			}
			WriteVCSolution(platformName, Path.Combine(solutionPath, projectName + ".sln"), projectName, guid);
			WriteVCProject(platformName, Path.Combine(solutionPath, projectName + ".vcxproj"), cppFiles, headerFiles, guid);
			WriteVCProjectFilters(Path.Combine(solutionPath, projectName + ".vcxproj.filters"), cppFiles, headerFiles);
			WriteVCProjectUser(platformName, Path.Combine(solutionPath, projectName + ".vcxproj.user"), executablePath, workingDirectory);
		}

		private static void WriteVCSolution(string platformName, string fileName, string projectName, Guid projectGuid)
		{
			string text = projectGuid.ToString().ToUpper();
			string text2 = Guid.NewGuid().ToString().ToUpper();
			StreamWriter streamWriter = new StreamWriter(fileName);
			streamWriter.Write("Microsoft Visual Studio Solution File, Format Version 12.00\n");
			streamWriter.Write("# Visual Studio 2012\n");
			streamWriter.Write("Project(\"{" + text2 + "}\") = \"" + projectName + "\", \"" + projectName + ".vcxproj\", \"{" + text + "}\"\n");
			streamWriter.Write("EndProject\n");
			streamWriter.Write("Global\n");
			streamWriter.Write("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution\n");
			streamWriter.Write("\t\tNull|" + platformName + " = Null|" + platformName + "\n");
			streamWriter.Write("\tEndGlobalSection\n");
			streamWriter.Write("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution\n");
			streamWriter.Write("\t\t{" + text + "}.Null|" + platformName + ".ActiveCfg = Null|" + platformName + "\n");
			streamWriter.Write("\t\t{" + text + "}.Null|" + platformName + ".Build.0 = Null|" + platformName + "\n");
			streamWriter.Write("\tEndGlobalSection\n");
			streamWriter.Write("\tGlobalSection(SolutionProperties) = preSolution\n");
			streamWriter.Write("\t\tHideSolutionNode = FALSE\n");
			streamWriter.Write("\tEndGlobalSection\n");
			streamWriter.Write("EndGlobal\n");
			streamWriter.Close();
		}

		private static void WriteVCProject(string platformName, string XmlFileName, string[] cppFiles, string[] headerFiles, Guid projectGUID)
		{
			StreamWriter streamWriter = new StreamWriter(XmlFileName);
			streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
			streamWriter.Write("<Project DefaultTargets=\"Build\" ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n");
			streamWriter.Write("  <ItemGroup Label=\"ProjectConfigurations\">\n");
			streamWriter.Write("    <ProjectConfiguration Include=\"Null|" + platformName + "\">\n");
			streamWriter.Write("      <Configuration>Null</Configuration>\n");
			streamWriter.Write("      <Platform>" + platformName + "</Platform>\n");
			streamWriter.Write("    </ProjectConfiguration>\n");
			streamWriter.Write("  </ItemGroup>\n");
			streamWriter.Write("  <ItemGroup>\n");
			foreach (string path in cppFiles)
			{
				streamWriter.Write("    <None Include=\"" + Path.GetFullPath(path) + "\" />\n");
			}
			streamWriter.Write("  </ItemGroup>\n");
			streamWriter.Write("  <ItemGroup>\n");
			foreach (string path2 in headerFiles)
			{
				streamWriter.Write("    <None Include=\"" + Path.GetFullPath(path2) + "\" />\n");
			}
			streamWriter.Write("  </ItemGroup>\n");
			streamWriter.Write("  <PropertyGroup Label=\"Globals\">\n");
			streamWriter.Write("    <VCTargetsPath Condition=\"'$(VCTargetsPath11)' != '' and '$(VSVersion)' == '' and '$(VisualStudioVersion)' == ''\">$(VCTargetsPath11)</VCTargetsPath>\n");
			streamWriter.Write("    <ProjectGuid>{" + projectGUID.ToString().ToUpper() + "}</ProjectGuid>\n");
			streamWriter.Write("  </PropertyGroup>\n");
			streamWriter.Write("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />\n");
			streamWriter.Write("  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)'=='Null|" + platformName + "'\" Label=\"Configuration\">\n");
			streamWriter.Write("    <ConfigurationType>Utility</ConfigurationType>\n");
			streamWriter.Write("  </PropertyGroup>\n");
			streamWriter.Write("  <PropertyGroup Condition=\"'$(Platform)'=='" + platformName + "'\">\n");
			streamWriter.Write("    <ConfigurationType>Utility</ConfigurationType>\n");
			streamWriter.Write("    <IntDir>$(SolutionDir)\\</IntDir>\n");
			streamWriter.Write("    <OutDir>$(SolutionDir)\\</OutDir>\n");
			streamWriter.Write("    <OutputPath>$(SolutionDir)\\</OutputPath>\n");
			streamWriter.Write("  </PropertyGroup>\n");
			streamWriter.Write("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />\n");
			streamWriter.Write("  <PropertyGroup Condition=\"'$(DebuggerFlavor)'=='" + platformName + "Debugger'\" Label=\"OverrideDebuggerDefaults\">\n");
			streamWriter.Write("    <!--LocalDebuggerCommand>$(TargetPath)</LocalDebuggerCommand-->\n");
			streamWriter.Write("    <!--LocalDebuggerReboot>false</LocalDebuggerReboot-->\n");
			streamWriter.Write("    <!--LocalDebuggerCommandArguments></LocalDebuggerCommandArguments-->\n");
			streamWriter.Write("    <!--LocalDebuggerTarget></LocalDebuggerTarget-->\n");
			streamWriter.Write("    <!--LocalDebuggerWorkingDirectory>$(ProjectDir)</LocalDebuggerWorkingDirectory-->\n");
			streamWriter.Write("    <!--LocalMappingFile></LocalMappingFile-->\n");
			streamWriter.Write("    <!--LocalRunCommandLine></LocalRunCommandLine-->\n");
			streamWriter.Write("  </PropertyGroup>\n");
			streamWriter.Write("  <ImportGroup Label=\"ExtensionSettings\">\n");
			streamWriter.Write("  </ImportGroup>\n");
			streamWriter.Write("  <ImportGroup Label=\"PropertySheets\" Condition=\"'$(Configuration)|$(Platform)'=='Null|" + platformName + "'\">\n");
			streamWriter.Write("    <Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />\n");
			streamWriter.Write("  </ImportGroup>\n");
			streamWriter.Write("  <PropertyGroup Label=\"UserMacros\" />\n");
			streamWriter.Write("  <PropertyGroup />\n");
			streamWriter.Write("  <ItemDefinitionGroup>\n");
			streamWriter.Write("  </ItemDefinitionGroup>\n");
			streamWriter.Write("  <ItemGroup>\n");
			streamWriter.Write("  </ItemGroup>\n");
			streamWriter.Write("  <Import Condition=\"'$(ConfigurationType)' == 'Makefile' and Exists('$(VCTargetsPath)\\Platforms\\$(Platform)\\SCE.Makefile.$(Platform).targets')\" Project=\"$(VCTargetsPath)\\Platforms\\$(Platform)\\SCE.Makefile.$(Platform).targets\" />\n");
			streamWriter.Write("  <Import Condition=\"'$(Platform)'=='" + platformName + "'\" Project=\"$(VCTargetsPath)\\Platforms\\" + platformName + "\\SCE.DebugOnly." + platformName.ToLower() + ".targets\" />\n");
			streamWriter.Write("  <Import Condition=\"'$(Platform)'!='" + platformName + "'\" Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />\n");
			streamWriter.Write("  <ImportGroup Label=\"ExtensionTargets\">\n");
			streamWriter.Write("  </ImportGroup>\n");
			streamWriter.Write("</Project>\n");
			streamWriter.Close();
		}

		private static void WriteVCProjectUser(string platformName, string XmlFileName, string executablePath, string workingDirectory)
		{
			StreamWriter streamWriter = new StreamWriter(XmlFileName);
			streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
			streamWriter.Write("<Project ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n");
			streamWriter.Write("  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)'=='Null|" + platformName + "'\">\n");
			streamWriter.Write("    <LocalDebuggerCommand>" + executablePath + "</LocalDebuggerCommand>\n");
			streamWriter.Write("    <LocalDebuggerDebugOnly>true</LocalDebuggerDebugOnly>\n");
			streamWriter.Write("    <LocalDebuggerWorkingDirectory>" + workingDirectory + "</LocalDebuggerWorkingDirectory>\n");
			streamWriter.Write("  </PropertyGroup>\n");
			streamWriter.Write("</Project>\n");
			streamWriter.Close();
		}

		private static void WriteVCProjectFilters(string XmlFileName, string[] cppFiles, string[] headerFiles)
		{
			StreamWriter streamWriter = new StreamWriter(XmlFileName);
			streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
			streamWriter.Write("<Project ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n");
			streamWriter.Write("  <ItemGroup>\n");
			streamWriter.Write("    <Filter Include=\"Header Files\">\n");
			streamWriter.Write("    </Filter>\n");
			streamWriter.Write("    <Filter Include=\"Source Files\">\n");
			streamWriter.Write("    </Filter>\n");
			streamWriter.Write("  </ItemGroup>\n");
			streamWriter.Write("  <ItemGroup>\n");
			foreach (string path in cppFiles)
			{
				streamWriter.Write("  <None Include=\"" + Path.GetFullPath(path) + "\">\n");
				streamWriter.Write("    <Filter>Source Files</Filter>\n");
				streamWriter.Write("  </None>\n");
			}
			foreach (string path2 in headerFiles)
			{
				streamWriter.Write("  <None Include=\"" + Path.GetFullPath(path2) + "\">\n");
				streamWriter.Write("    <Filter>Header Files</Filter>\n");
				streamWriter.Write("  </None>\n");
			}
			streamWriter.Write("  </ItemGroup>\n");
			streamWriter.Write("</Project>\n");
			streamWriter.Close();
		}
	}
}
