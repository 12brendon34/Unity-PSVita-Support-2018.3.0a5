using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityEditor.PSP2
{
	internal class SUPRXTools
	{
		public class PRXModuleInfo
		{
			public string name;

			public string stub;

			public List<string> exports;

			public PRXModuleInfo()
			{
				exports = new List<string>();
			}
		}

		private static string scePath = Environment.GetEnvironmentVariable("SCE_PSP2_SDK_DIR");

		internal static string MakeValidModuleName(string longName)
		{
			longName = longName.Replace("-", "_");
			longName = longName.Replace(" ", "_");
			if (longName.Length < 26)
			{
				return longName;
			}
			UnityEngine.Debug.LogWarning("Warning: " + longName + " contains more than 27 characters which is the allowed limit for PRX module");
			return longName.Substring(0, 26);
		}

		internal static PRXModuleInfo ExtractGlobalSymbolsFromPRX(string filePath)
		{
			PRXModuleInfo pRXModuleInfo = new PRXModuleInfo();
			pRXModuleInfo.name = Path.GetFileNameWithoutExtension(filePath);
			pRXModuleInfo.name = pRXModuleInfo.name.Replace(".", "_");
			pRXModuleInfo.name = pRXModuleInfo.name.Replace("-", "_");
			pRXModuleInfo.name = pRXModuleInfo.name.Replace(" ", "_");
			string text = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath);
			pRXModuleInfo.stub = text + "_stub.a";
			string processFileName = Path.Combine(scePath, "host_tools/build/bin/psp2bin.exe");
			string inputArguments = " -ms \"" + filePath + "\"";
			StreamReader streamReader = ExternalTool.StartProcess(processFileName, inputArguments, filePath, waitForExit: true, "");
			while (!streamReader.EndOfStream)
			{
				string input = streamReader.ReadLine();
				Match match = Regex.Match(input, "0x(\\w{8})(\\s+)(\\w+)(\\s+)(\\w+)", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					pRXModuleInfo.exports.Add(match.Groups[5].Value);
				}
			}
			return pRXModuleInfo;
		}

		internal static void CreateBootstrapPRX(string targetCrossCompiledASMFolder, string sourceAssembliesFolder, string playerPackage, string bootstrapFile, PRXModuleInfo moduleInfo)
		{
			string text = "B_" + moduleInfo.name;
			string text2 = Path.Combine(targetCrossCompiledASMFolder, text);
			string text3 = text2 + ".cpp";
			string text4 = text2 + ".o";
			ScriptingImplementation scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.PSP2);
			StreamWriter streamWriter = new StreamWriter(text3);
			streamWriter.WriteLine("#include <stdio.h>");
			streamWriter.WriteLine("#include <stdlib.h>");
			streamWriter.WriteLine("#include <string.h>");
			streamWriter.WriteLine("#include <kernel.h>");
			streamWriter.WriteLine("#include <moduleinfo.h>");
			streamWriter.WriteLine("");
			streamWriter.WriteLine("SCE_MODULE_INFO(" + MakeValidModuleName(text) + ",SCE_MODULE_ATTR_NONE, 1, 1); ");
			streamWriter.WriteLine("");
			foreach (string export in moduleInfo.exports)
			{
				streamWriter.WriteLine("extern \"C\"  int " + export + "();");
			}
			streamWriter.WriteLine("");
			streamWriter.WriteLine("typedef int (*EXTERNFUNCTION)();");
			streamWriter.WriteLine("static struct FunctionMap { const char* name; EXTERNFUNCTION address;} s_FunctionNames [] =  {");
			streamWriter.WriteLine("");
			foreach (string export2 in moduleInfo.exports)
			{
				streamWriter.WriteLine("  { \"" + export2 + "\", &" + export2 + "},");
			}
			streamWriter.WriteLine("};");
			streamWriter.WriteLine("");
			streamWriter.WriteLine("extern \"C\" __declspec(dllimport) void RegisterModule(const char* module_name, void* exports, size_t exports_count, SceSize size, void* arg);");
			streamWriter.WriteLine("");
			streamWriter.WriteLine("extern \"C\" int module_start(SceSize sz, const void* arg) {");
			streamWriter.WriteLine("  RegisterModule(\"" + MakeValidModuleName(moduleInfo.name) + "\", s_FunctionNames, sizeof(s_FunctionNames)/sizeof(s_FunctionNames[0]), sz, (void*)arg);");
			streamWriter.WriteLine("  return SCE_KERNEL_START_SUCCESS;");
			streamWriter.WriteLine("}");
			streamWriter.Close();
			string processFileName = Path.Combine(scePath, "host_tools/build/bin/psp2snc.exe");
			string text5 = " -DNDEBUG -DSN_TARGET_PSP2 -O3 ";
			string text6 = text5;
			text5 = text6 + " -c \"" + text3 + "\" -o \"" + text4 + "\"";
			ExternalTool.StartProcess(processFileName, text5, text4, waitForExit: true, "");
			string processFileName2 = Path.Combine(scePath, "host_tools/build/bin/psp2ld.exe");
			string text7 = " -oformat=prx ";
			text7 = text7 + " \"" + moduleInfo.stub + "\" ";
			text7 = ((scriptingBackend != 0) ? (text7 + " \"" + Path.Combine(playerPackage, "Data/Modules/Il2CppAssemblies_stub_weak.a") + "\" ") : (text7 + " \"" + Path.Combine(playerPackage, "Data/Modules/SUPRXManager_stub_weak.a") + "\" "));
			text6 = text7;
			text7 = text6 + "\"" + text4 + "\" -o \"" + bootstrapFile + "\"";

            ExternalTool.StartProcess(processFileName2, text7, bootstrapFile, waitForExit: true, "");
		}

		internal static bool CreateNativePluginsPRX(string targetCrossCompiledASMFolder, string playerPackage, string pluginFolder)
		{
			targetCrossCompiledASMFolder = Path.Combine(Directory.GetCurrentDirectory(), targetCrossCompiledASMFolder);
			string strB = ".suprx";
			string[] fileSystemEntries = Directory.GetFileSystemEntries(pluginFolder);
			string[] array = fileSystemEntries;
			foreach (string path in array)
			{
				if (string.Compare(Path.GetExtension(path), strB, ignoreCase: true) == 0 || string.Compare(Path.GetFileName(path), strB, ignoreCase: true) == 0)
				{
					string filePath = Path.GetDirectoryName(path) + "/" + Path.GetFileName(path);
					PRXModuleInfo moduleInfo = ExtractGlobalSymbolsFromPRX(filePath);
					string bootstrapFile = Path.Combine(pluginFolder, Path.GetFileName(path) + ".b.suprx");
					CreateBootstrapPRX(targetCrossCompiledASMFolder, pluginFolder, playerPackage, bootstrapFile, moduleInfo);
				}
			}
			return true;
		}

		internal static string CreateMonoAssemblyPRX(string targetCrossCompiledASMFolder, string sourceFile, string sourceAssembliesFolder, string playerPackage)
		{
			string text = Path.Combine(targetCrossCompiledASMFolder, Path.GetFileName(sourceFile));
			string text2 = Path.Combine(sourceAssembliesFolder, Path.GetFileName(sourceFile));
			string text3 = text + ".s";
			string text4 = text + ".o";

			string processFileName = Path.Combine(scePath, "host_tools/build/bin/arm-eabi-as.exe");
			string inputArguments = "\"" + text3 + "\" -o \"" + text4 + "\"";


			ExternalTool.StartProcess(processFileName, inputArguments, text4, waitForExit: true, "");
			PRXModuleInfo moduleInfo = ExtractModuleInfoFromOBJ(text);


			string text5 = CreateExportsFile(targetCrossCompiledASMFolder, moduleInfo);
			string text6 = Path.Combine(targetCrossCompiledASMFolder, Path.GetFileNameWithoutExtension(text5)) + ".o";
			string text7 = text2 + ".suprx";
			string text8 = text + ".map";
			string bootstrapFile = text7 + ".b.suprx";
			string processFileName2 = Path.Combine(scePath, "host_tools/build/bin/psp2snc.exe");
			string text9 = " -DNDEBUG -DSN_TARGET_PSP2 -O3 ";
			text9 += $"-Xexternalas=1 -Xdbgcompresslines=0 -Y/a,\"{scePath}/host_tools/build/bin/arm-eabi-as\" -c \"{text5}\" -o \"{text6}\"";
			ExternalTool.StartProcess(processFileName2, text9, text6, waitForExit: true, "");
			string processFileName3 = Path.Combine(scePath, "host_tools/build/bin/psp2ld.exe");
			string text10 = "--disable-warning=0431 -oformat=prx ";
			text10 = text10 + "-prx-stub-output-dir=\"" + targetCrossCompiledASMFolder + "\" ";
			text10 = text10 + "\"" + text4 + "\" ";
			text10 = text10 + "\"" + text6 + "\" ";
			text10 = text10 + "-Wl,-Map=\"" + text8 + "\" ";
			text10 = text10 + "-o \"" + text7 + "\"";

            ExternalTool.StartProcess(processFileName3, text10, text7, waitForExit: true, "");
			CreateBootstrapPRX(targetCrossCompiledASMFolder, sourceAssembliesFolder, playerPackage, bootstrapFile, moduleInfo);
			return text7;
		}

		internal static PRXModuleInfo ExtractModuleInfoFromOBJ(string filePath)
		{
			PRXModuleInfo pRXModuleInfo = new PRXModuleInfo();
			pRXModuleInfo.name = Path.GetFileName(filePath);
			pRXModuleInfo.name = pRXModuleInfo.name.Replace(".", "_");
			pRXModuleInfo.name = pRXModuleInfo.name.Replace("-", "_");
			pRXModuleInfo.name = pRXModuleInfo.name.Replace(" ", "_");
			pRXModuleInfo.stub = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileName(filePath) + "_stub.a");
			string text = filePath + ".symbols";
			string text2 = Path.Combine(scePath, "host_tools/build/bin/psp2bin.exe");
			string text3 = "-dsy \"" + filePath + ".o\"";
			Process process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = " /C \"\"" + text2 + "\" " + text3 + " > \"" + text + "\"\"";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			process.WaitForExit();
			if (process.ExitCode != 0 || !File.Exists(text))
			{
				throw new Exception(process.StartInfo.FileName + process.StartInfo.Arguments + " : " + process.StandardError.ReadToEnd());
			}
			StreamReader streamReader = new StreamReader(text);
			while (!streamReader.EndOfStream)
			{
				string input = streamReader.ReadLine();
				Match match = Regex.Match(input, "0x\\w{8}\\s+Global\\s+\\w+\\s+\\.\\w+\\s+(\\w+)", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					pRXModuleInfo.exports.Add(match.Groups[1].Value);
				}
			}
			streamReader.Close();
			File.Delete(text);
			return pRXModuleInfo;
		}

		internal static string CreateExportsFile(string targetDir, PRXModuleInfo moduleInfo)
		{
			string text = Path.Combine(targetDir, moduleInfo.name + "exports.c");
			StreamWriter streamWriter = new StreamWriter(text);
			streamWriter.WriteLine("#define SYS_LIB_EXPORT( symbol )                      \\\n__asm__(                                              \\\n\".global \" #symbol \"\\n\"                               \\\n\".section .linker_cmd\\n\"                              \\\n\".short 4\\n\"                                          \\\n\".short .\" #symbol \"_end - .\" #symbol \"_start\\n\"      \\\n\".\" #symbol \"_start:\\n\"                               \\\n\".string \\\"\" #symbol \"\\\"\\n\"                           \\\n\".\" #symbol \"_end:\\n\"                                 \\\n)\n");
			foreach (string export in moduleInfo.exports)
			{
				streamWriter.WriteLine("SYS_LIB_EXPORT(" + export + ");");
			}
			streamWriter.Close();
			return text;
		}
	}
}
