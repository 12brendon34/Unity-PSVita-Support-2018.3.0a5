using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor.Utils;
using UnityEngine;

namespace UnityEditor.PSP2
{
	public class VitaSDKTools
	{
		public class RunToolExParams
		{
			public string tool;

			public string args;

			public string workingDirectory;

			public bool warnError;

			public bool waitForExit;

			public RunToolExParams()
			{
				tool = "";
				args = "";
				workingDirectory = "";
				warnError = false;
				waitForExit = true;
			}
		}

		public struct BuildPatchPackageParams
		{
			public string project_gp4p;

			public string content_id;

			public string masterVersion;

			public string drm_type;

			public string passcode;

			public string patchOriginalPackage;

			public List<string> filePairs;

			public string sceModulePath;

			public bool needSubmissionMaterials;

			public string stagingArea;

			public bool Validate()
			{
				if (passcode.Length != 32)
				{
					UnityEngine.Debug.LogError(kBuildFailed_InvalidPasscode);
					return false;
				}
				return true;
			}
		}

		public struct BuildAppPackageParams
		{
			public string project_gp4p;

			public string content_id;

			public string masterVersion;

			public string drm_type;

			public string capacity;

			public string passcode;

			public List<string> filePairs;

			public string sceModulePath;

			public bool needSubmissionMaterials;

			public string stagingArea;

			public bool Validate()
			{
				if (passcode.Length != 32)
				{
					UnityEngine.Debug.LogError(kBuildFailed_InvalidPasscode);
					return false;
				}
				return true;
			}
		}

		private static Dictionary<string, string> SDKTools = new Dictionary<string, string>();

		private static Dictionary<string, string> DeviceInfo = new Dictionary<string, string>();

		private static Dictionary<string, string> TrophyInfo = new Dictionary<string, string>();

		private static bool hasPublishTools = true;

		private static string kBuildFailed_InvalidPasscode = "Build Failed: The package passcode has not been set or its length is incorrect (must be 32 characters)\nPlease go to Player Settings and set the passcode.";

		private static string kBuildFailed_CantPurge = "Build Failed: Failed to purge build target folder.\n";

		private static string kBuildFailed_SubmissionPackageError = "Build Failed: Failed to generate package with submission materials...\n";

		private static string kBuildFailed_PackageError = "Build Failed: Failed to generate package...\n";

		private static string kBuildFailed_PackageFile = "Build Failed: Failed creating package file!\n";

		private static string kBuildFailed_ExtractPackage = "Build Failed: Failed to extract files from original package...\n";

		private static string kBuildWarning_VersionParseFailed = "Failed to parse dev-kit firmware version!\n";

		public static bool PSP2Ctrl(string args)
		{
			return RunTool("psp2ctrl", args);
		}

		public static bool RunToolEx(RunToolExParams runParams)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool(runParams.tool);
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.CreateNoWindow = true;
			if (runParams.workingDirectory.Length != 0)
			{
				processStartInfo.WorkingDirectory = runParams.workingDirectory;
			}
			return RunCommand(processStartInfo, runParams.args, runParams.warnError, runParams.waitForExit);
		}

		public static bool RunTool(string tool, string args, string workingDirectory = "")
		{
			RunToolExParams runToolExParams = new RunToolExParams();
			runToolExParams.tool = tool;
			runToolExParams.args = args;
			runToolExParams.workingDirectory = workingDirectory;
			return RunToolEx(runToolExParams);
		}

		public static bool HasPublishTools()
		{
			return hasPublishTools;
		}

		public static string GetTool(string name)
		{
			if (SDKTools.ContainsKey(name))
			{
				return SDKTools[name];
			}
			throw new Exception("Vita SDK tool '" + name + "' not found");
		}

		private static string GetFullPath(string file)
		{
			if (File.Exists(file))
			{
				return Path.GetFullPath(file);
			}
			string fileName = Path.GetFileName(file);
			string environmentVariable = Environment.GetEnvironmentVariable("PATH");
			string[] array = environmentVariable.Split(';');
			foreach (string path in array)
			{
				string text = Path.Combine(path, fileName);
				if (File.Exists(text))
				{
					return text;
				}
			}
			return null;
		}

		internal static bool FindAndStoreTrophyValue(string line, string search, string key)
		{
			int num = line.IndexOf(search);
			if (num >= 0)
			{
				string text = line.Substring(num + search.Length).Trim();
				text = text.TrimEnd('.');
				TrophyInfo[key] = text;
				Console.WriteLine("### trophy pack: " + key + " = " + text);
				return true;
			}
			return false;
		}

		internal static string GetTrophyInfo(string key)
		{
			if (TrophyInfo.ContainsKey(key))
			{
				return TrophyInfo[key];
			}
			return null;
		}

		internal static bool VerifyTrophyFile(string fileName)
		{
			TrophyInfo = new Dictionary<string, string>();
			if (fileName.Length > 0 && File.Exists(fileName))
			{
				string processFileName = SDKTools["psp2pubcmd"];
				string inputArguments = " file_verify --iformat psp2_trp " + fileName;
				StreamReader streamReader = ExternalTool.StartProcess(processFileName, inputArguments, "", waitForExit: false, "");
				while (!streamReader.EndOfStream)
				{
					string line = streamReader.ReadLine();
					if (!FindAndStoreTrophyValue(line, "NP Comm ID =", "npID") && !FindAndStoreTrophyValue(line, "Platform =", "platform") && !FindAndStoreTrophyValue(line, "Number of Groups =", "numGroups") && !FindAndStoreTrophyValue(line, "Number of Trophies =", "numTrophies") && !FindAndStoreTrophyValue(line, "Trophy Set Version = ", "version"))
					{
					}
				}
			}
			return true;
		}

		public static bool CheckSDKToolsExist(bool pcHosted)
		{
			string environmentVariable = Environment.GetEnvironmentVariable("SCE_PSP2_SDK_DIR");
			string environmentVariable2 = Environment.GetEnvironmentVariable("SCE_ROOT_DIR");
			SDKTools = new Dictionary<string, string>();
			if (environmentVariable != null && environmentVariable2 != null)
			{
				string[] array = new string[8]
				{
					Path.Combine(environmentVariable2, "PSP2/Tools/Target Manager Server/bin/psp2ctrl.exe"),
					Path.Combine(environmentVariable2, "PSP2/Tools/Target Manager Server/bin/psp2run.exe"),
					Path.Combine(environmentVariable2, "PSP2/Tools/Publishing Tools/bin/psp2pubcmd.exe"),
					Path.Combine(environmentVariable, "host_tools/build/bin/psp2ld.exe"),
					Path.Combine(environmentVariable, "host_tools/build/bin/psp2snc.exe"),
					Path.Combine(environmentVariable, "host_tools/build/bin/arm-eabi-as.exe"),
					Path.Combine(environmentVariable, "host_tools/build/bin/psp2snarl.exe"),
					Path.Combine(environmentVariable, "host_tools/bin/psp2psarc.exe")
				};
				int num = 0;
				string[] array2 = array;
				foreach (string text in array2)
				{
					Console.WriteLine("### Checking for SDK file: " + text);
					string fullPath = GetFullPath(text);
					if (fullPath != null)
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
						SDKTools[fileNameWithoutExtension] = fullPath;
						num++;
						Console.WriteLine("### found: " + fileNameWithoutExtension + " @ " + fullPath);
					}
					else if (text.Contains("psp2pubcmd"))
					{
						hasPublishTools = false;
						if (pcHosted)
						{
							num++;
						}
						else
						{
							UnityEngine.Debug.LogError("Failed to find Vita SDK tool: " + text + "\nPackage builds will not be possible, Please install the Vita publishing tools or change the build type to 'PC Hosted'");
						}
					}
					else
					{
						UnityEngine.Debug.LogError("Failed to find Vita SDK tool: " + text + "\n");
					}
				}
				return num == array.Length;
			}
			return false;
		}

		public static void RefreshDevKitInfo()
		{
			DeviceInfo = new Dictionary<string, string>();
			string processFileName = SDKTools["psp2ctrl"];
			string inputArguments = " info";
			StreamReader streamReader = ExternalTool.StartProcess(processFileName, inputArguments, "", waitForExit: false, "");
			string text = "";
			while (!streamReader.EndOfStream)
			{
				string text2 = streamReader.ReadLine();
				int num = text2.IndexOf(':');
				bool flag = true;
				if (num >= 0)
				{
					string text3 = text2.Substring(0, num);
					string text4 = text2.Substring(num + 1, text2.Length - (num + 1));
					text3 = text + text3.Trim();
					text4 = text4.Trim();
					if (text3.Length > 0 && text4.Length > 0)
					{
						Console.WriteLine("### DeviceInfo[" + text3 + "] = " + text4);
						DeviceInfo[text3] = text4;
						flag = false;
					}
				}
				if (flag)
				{
					text = text2.Trim();
					if (text.CompareTo("CP:") != 0 && text.CompareTo("USB(CP):") != 0 && text.CompareTo("USB(Direct):") != 0)
					{
						text = "";
					}
				}
			}
		}

		public static VitaPowerStatus DevKitGetPowerStatus()
		{
			RefreshDevKitInfo();
			if (DeviceInfo.ContainsKey("PowerStatus"))
			{
				string text = DeviceInfo["PowerStatus"];
				if (text.Contains("STATUS_OFF"))
				{
					return VitaPowerStatus.OFF;
				}
				if (text.Contains("STATUS_ON"))
				{
					return VitaPowerStatus.ON;
				}
				if (text.Contains("STATUS_NO_SUPPLY"))
				{
					return VitaPowerStatus.NO_SUPPLY;
				}
			}
			return VitaPowerStatus.UNKNOWN;
		}

		public static bool DevKitPowerUp()
		{
			PSP2Ctrl("on");
			RefreshDevKitInfo();
			return true;
		}

		public static bool DevKitIsConnected()
		{
			if (DeviceInfo.ContainsKey("ConnectionState"))
			{
				string text = DeviceInfo["ConnectionState"];
				if (text.CompareTo("CONNECTION_AVAILABLE") == 0 || text.CompareTo("CONNECTION_CONNECTED") == 0)
				{
					return true;
				}
			}
			return false;
		}

		public static float DevKitKernelVersion()
		{
			if (DeviceInfo.ContainsKey("SDKVersion"))
			{
				string text = DeviceInfo["SDKVersion"];
				try
				{
					string[] array = text.Split('.');
					if (array.Length >= 2)
					{
						text = array[0] + "." + array[1];
						return Convert.ToSingle(text);
					}
				}
				catch (Exception)
				{
					UnityEngine.Debug.LogWarning(kBuildWarning_VersionParseFailed + " version string = \"" + text + "\"");
				}
			}
			return 0f;
		}

		public static MemCardStatus DevKitHasMemoryCard()
		{
			StringBuilder stringBuilder = new StringBuilder("");
			StringBuilder stringBuilder2 = new StringBuilder("");
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2ctrl");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.RedirectStandardError = true;
			RunCommand2(processStartInfo, "devices", stringBuilder, stringBuilder2);
			string[] array = stringBuilder.ToString().Split('\n');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.ToLower().Contains("memory card"))
				{
					return MemCardStatus.Available;
				}
			}
			array = stringBuilder2.ToString().Split('\n');
			string[] array3 = array;
			foreach (string text2 in array3)
			{
				if (text2.ToLower().Contains("error"))
				{
					return MemCardStatus.Unknown;
				}
			}
			return MemCardStatus.NotAvailable;
		}

		public static int SDKVersion()
		{
			int result = 0;
			string environmentVariable = Environment.GetEnvironmentVariable("SCE_PSP2_SDK_DIR");
			if (environmentVariable != null)
			{
				string text = Path.Combine(environmentVariable, "target/include/sdk_version.h");
				if (File.Exists(text))
				{
					string[] array = File.ReadAllLines(text);
					string[] array2 = array;
					foreach (string text2 in array2)
					{
						if (text2.Contains("SCE_PSP2_SDK_VERSION") && text2.Contains("#define"))
						{
							Console.WriteLine("### Found SDK version: " + text + ": " + text2);
							string[] array3 = text2.Split();
							if (array3[2].StartsWith("0x"))
							{
								result = Convert.ToInt32(array3[2].Substring(2), 16);
								break;
							}
						}
					}
				}
			}
			return result;
		}

		public static void DeleteDirectory(string target_dir)
		{
			string[] files = Directory.GetFiles(target_dir);
			string[] directories = Directory.GetDirectories(target_dir);
			string[] array = files;
			foreach (string path in array)
			{
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
			string[] array2 = directories;
			foreach (string target_dir2 in array2)
			{
				DeleteDirectory(target_dir2);
			}
			Directory.Delete(target_dir, recursive: false);
		}

		public static void PurgeBuildDirectory(string target_dir, bool preserveSaves)
		{
			string[] files = Directory.GetFiles(target_dir);
			string[] directories = Directory.GetDirectories(target_dir);
			string[] array = files;
			foreach (string path in array)
			{
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
			string[] array2 = directories;
			foreach (string text in array2)
			{
				if (!preserveSaves || !text.EndsWith("savedata"))
				{
					DeleteDirectory(text);
				}
			}
		}

		internal static void LogPackageError(string heading, StringBuilder details)
		{
			string text = heading;
			string[] array = details.ToString().Split('\n');
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2.ToLower().Contains("error"))
				{
					text = text + text2 + "\n";
				}
			}
			UnityEngine.Debug.LogError(text);
		}

		public static bool ModuleReferencesNpWebAPI(string modulePath)
		{
			List<string> moduleLibraryDependencies = GetModuleLibraryDependencies(modulePath);
			foreach (string item in moduleLibraryDependencies)
			{
				if (item.ToLower().Contains("npwebapi_stub"))
				{
					return true;
				}
			}
			return false;
		}

		public static string GetModuleSDKVersion(string modulePath)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2pubcmd");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
			processStartInfo.RedirectStandardError = true;
			StringBuilder stringBuilder = new StringBuilder("");
			RunCommand2(processStartInfo, $" file_verify --iformat psp2_self \"{modulePath}\"", stringBuilder, null);
			string[] array = stringBuilder.ToString().Split('\n');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.ToLower().Contains("ps vita self file"))
				{
					int num = text.LastIndexOf('=');
					if (num >= 0)
					{
						string text2 = text.Substring(num + 1, text.Length - (num + 1));
						text2 = text2.Trim();
						return text2.TrimEnd('.');
					}
				}
			}
			return "";
		}

		public static List<string> GetModuleLibraryDependencies(string modulePath)
		{
			List<string> list = new List<string>();
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2pubcmd");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
			processStartInfo.RedirectStandardError = true;
			StringBuilder stringBuilder = new StringBuilder("");
			RunCommand2(processStartInfo, $" file_verify --iformat psp2_self \"{modulePath}\"", stringBuilder, null);
			string[] array = stringBuilder.ToString().Split('\n');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.ToLower().Contains("lib"))
				{
					int num = text.LastIndexOf(':');
					if (num >= 0)
					{
						string text2 = text.Substring(num + 1, text.Length - (num + 1));
						text2 = text2.TrimStart(' ');
						list.Add(text2);
					}
				}
			}
			return list;
		}

		public static bool BuildPatchPackage(BuildPatchPackageParams buildParams, string installPath, string packageFilePath, bool createZip)
		{
			if (!buildParams.Validate())
			{
				return false;
			}
			string text = buildParams.stagingArea + "/PackageSource";
			string text2 = text + "/Files";
			string item = $"\"{buildParams.sceModulePath}\" sce_module";
			List<string> filePairs = buildParams.filePairs;
			filePairs.Add(item);
			CopyFilePairs(filePairs, text2);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2pubcmd");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
			processStartInfo.RedirectStandardError = true;
			StringBuilder stringBuilder = new StringBuilder("");
			string text3 = "psp2_patch";
			RunCommand(processStartInfo, string.Format("-gc --proj_type {1} --app_pkg_path \"{6}\" --content_id {2} --pub_ver {3} --drm_type {4} --passcode {5} \"{0}\"", buildParams.project_gp4p, text3, buildParams.content_id, buildParams.masterVersion, buildParams.drm_type, buildParams.passcode, buildParams.patchOriginalPackage));
			AddFilesToPackage(processStartInfo, text2, buildParams.project_gp4p);
			if (buildParams.needSubmissionMaterials)
			{
				try
				{
					PurgeBuildDirectory(installPath, preserveSaves: false);
				}
				catch (Exception)
				{
					UnityEngine.Debug.LogError(kBuildFailed_CantPurge);
					return false;
				}
				if (!RunCommand2(processStartInfo, $"-c --oformat all \"{buildParams.project_gp4p}\" \"{installPath}\"", stringBuilder, null))
				{
					LogPackageError(kBuildFailed_SubmissionPackageError, stringBuilder);
					return false;
				}
			}
			else if (!RunCommand2(processStartInfo, $"-c \"{buildParams.project_gp4p}\" \"{packageFilePath}\"", stringBuilder, null))
			{
				LogPackageError(kBuildFailed_PackageError, stringBuilder);
				return false;
			}
			if (!File.Exists(packageFilePath))
			{
				UnityEngine.Debug.LogError(kBuildFailed_PackageFile);
				return false;
			}
			if (createZip)
			{
				string path = buildParams.project_gp4p + ".backup";
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				string zipFilePath = Path.ChangeExtension(packageFilePath, ".source.zip");
				Zip(text, zipFilePath);
			}
			return true;
		}

		public static void AddFilesToPackage(ProcessStartInfo psi, string sourceFilesPath, string projectFile)
		{
			string[] files = Directory.GetFiles(sourceFilesPath, "*", SearchOption.AllDirectories);
			string[] array = files;
			foreach (string text in array)
			{
				string text2 = text.Substring(sourceFilesPath.Length);
				text2 = text2.Trim('\\');
				text2 = text2.Replace('\\', '/');
				RunCommand(psi, $"-gfa --force \"{text}\" \"{text2}\" \"{projectFile}\"");
			}
		}

		public static bool BuildAppPackage(BuildAppPackageParams buildParams, string installPath, string packageFilePath, bool createZip)
		{
			if (!buildParams.Validate())
			{
				return false;
			}
			string text = buildParams.stagingArea + "/PackageSource";
			string text2 = text + "/Files";
			string item = $"\"{buildParams.sceModulePath}\" sce_module";
			List<string> filePairs = buildParams.filePairs;
			filePairs.Add(item);
			CopyFilePairs(filePairs, text2);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2pubcmd");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
			processStartInfo.RedirectStandardError = true;
			StringBuilder stringBuilder = new StringBuilder("");
			string text3 = "psp2_app";
			RunCommand(processStartInfo, string.Format("-gc --proj_type {1} --capacity {2} --content_id {3} --pub_ver {4} --drm_type {5} --passcode {6} {0}", buildParams.project_gp4p, text3, buildParams.capacity, buildParams.content_id, buildParams.masterVersion, buildParams.drm_type, buildParams.passcode));
			AddFilesToPackage(processStartInfo, text2, buildParams.project_gp4p);
			if (buildParams.needSubmissionMaterials)
			{
				try
				{
					PurgeBuildDirectory(installPath, preserveSaves: false);
				}
				catch (Exception)
				{
					UnityEngine.Debug.LogError(kBuildFailed_CantPurge);
					return false;
				}
				if (!RunCommand2(processStartInfo, $"-c --oformat all \"{buildParams.project_gp4p}\" \"{installPath}\"", stringBuilder, null))
				{
					LogPackageError(kBuildFailed_SubmissionPackageError, stringBuilder);
					return false;
				}
			}
			else if (!RunCommand2(processStartInfo, $"-c \"{buildParams.project_gp4p}\" \"{packageFilePath}\"", stringBuilder, null))
			{
				LogPackageError(kBuildFailed_PackageError, stringBuilder);
				return false;
			}
			if (!File.Exists(packageFilePath))
			{
				UnityEngine.Debug.LogError(kBuildFailed_PackageFile);
				return false;
			}
			if (createZip)
			{
				string path = buildParams.project_gp4p + ".backup";
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				string zipFilePath = Path.ChangeExtension(packageFilePath, ".source.zip");
				Zip(text, zipFilePath);
			}
			return true;
		}

		public static bool CopyFilePairs(List<string> filePairs, string destPath)
		{
			Directory.CreateDirectory(destPath);
			foreach (string filePair in filePairs)
			{
				string input = filePair.Replace('\\', '/');
				List<string> list = (from Match m in Regex.Matches(input, "[\\\"].+?[\\\"]|[^ ]+")
					select m.Value).ToList();
				if (list.Count == 2)
				{
					string text = list[0].Replace("\"", "");
					string text2 = destPath + "/" + list[1].Replace("\"", "");
					FileAttributes attributes = File.GetAttributes(text);
					if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
					{
						Console.WriteLine("### copyDir: " + text + " > " + text2);
						FileUtil.CopyDirectoryRecursive(text, text2, overwrite: true);
						continue;
					}
					Console.WriteLine("### copyFile: " + text + " > " + text2);
					string directoryName = Path.GetDirectoryName(text2);
					Directory.CreateDirectory(directoryName);
					File.Copy(text, text2);
				}
				else
				{
					Console.WriteLine("### copyFailed: " + filePair);
				}
			}
			return true;
		}

		private static bool Zip(string sourcePath, string zipFilePath)
		{
			string text = EditorApplication.applicationContentsPath + "/Tools/7z.exe";
			if (File.Exists(text))
			{
				zipFilePath = Path.GetFullPath(zipFilePath);
				File.Delete(zipFilePath);
				string currentDirectory = Directory.GetCurrentDirectory();
				Directory.SetCurrentDirectory(sourcePath);
				string inputArguments = "a -tzip -mx3 \"" + zipFilePath + "\"";
				ExternalTool.StartProcess(text, inputArguments, "", waitForExit: true, "");
				Directory.SetCurrentDirectory(currentDirectory);
				return true;
			}
			return false;
		}

		public static bool UnZip(string zipFilePath, string destPath)
		{
			string text = EditorApplication.applicationContentsPath + "/Tools/7z.exe";
			if (File.Exists(text))
			{
				string inputArguments = "x -o\"" + destPath + "\" \"" + zipFilePath + "\"";
				ExternalTool.StartProcess(text, inputArguments, "", waitForExit: true, "");
			}
			return true;
		}

		public static void CompressWithPSArc(string controlFile, string outFile)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2psarc");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
			processStartInfo.RedirectStandardError = true;
			string args = ((0 == 0) ? string.Format("create -I \"{1}\" -o \"{2}\"", Directory.GetCurrentDirectory(), controlFile, outFile) : $"create -s \"{Directory.GetCurrentDirectory()}\" -I \"{controlFile}\" -o \"{outFile}\"");
			RunCommand(processStartInfo, args);
			StringBuilder stringBuilder = new StringBuilder("");
			RunCommand2(processStartInfo, $"list  \"{outFile}\"", stringBuilder, null);
			Console.WriteLine(stringBuilder.ToString());
		}

		public static void CompressFolderWithPSArc(string sourcePath, string outFile, string stagingArea)
		{
			outFile = Directory.GetCurrentDirectory() + "/" + outFile;
			string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
			string text = "archive.txt";
			string currentDirectory = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(sourcePath);
			StreamWriter streamWriter = new StreamWriter(text);
			string[] array = files;
			foreach (string text2 in array)
			{
				streamWriter.WriteLine(text2.Substring(sourcePath.Length + 1));
			}
			streamWriter.Close();
			CompressWithPSArc(text, outFile);
			Directory.SetCurrentDirectory(currentDirectory);
		}

		public static void DecompressWithPSArc(string inFile, string outPath)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2psarc");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
			processStartInfo.RedirectStandardError = true;
			RunCommand(processStartInfo, $"extract --to=\"{outPath}\" \"{inFile}\"");
		}

		public static bool ExtractPackage(string packagePath, string packagePassword, string destPath)
		{
			try
			{
				packagePath = packagePath.Replace('/', '\\');
				destPath = destPath.Replace('/', '\\');
				StringBuilder stringBuilder = new StringBuilder("");
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = GetTool("psp2pubcmd");
				processStartInfo.UseShellExecute = false;
				processStartInfo.CreateNoWindow = true;
				processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
				processStartInfo.RedirectStandardError = true;
				string args = " -x --passcode " + packagePassword + " \"" + packagePath + "\" \"" + destPath + "\"";
				if (!RunCommand2(processStartInfo, args, stringBuilder, null))
				{
					LogPackageError(kBuildFailed_ExtractPackage, stringBuilder);
					return false;
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int memcmp(byte[] b1, byte[] b2, long count);

		private static bool CompareByteArray(byte[] b1, byte[] b2)
		{
			return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
		}

		public static bool CompareFileForPatch(string oldFile, string newFile, bool log)
		{
			FileInfo fileInfo = new FileInfo(newFile);
			FileInfo fileInfo2 = new FileInfo(oldFile);
			if (fileInfo.LastWriteTime != fileInfo2.LastWriteTime)
			{
				if (fileInfo.Length != fileInfo2.Length)
				{
					if (log)
					{
						Console.WriteLine("#### repFile (diff size, " + fileInfo.Length + " != " + fileInfo2.Length + "): " + newFile);
					}
					return true;
				}
				byte[] b = File.ReadAllBytes(oldFile);
				byte[] b2 = File.ReadAllBytes(newFile);
				if (!CompareByteArray(b, b2))
				{
					if (log)
					{
						Console.WriteLine("#### repFile (diff bytes): " + newFile);
					}
					return true;
				}
				if (log)
				{
					Console.WriteLine("#### ignoreFile (same bytes): " + newFile);
				}
			}
			else if (log)
			{
				Console.WriteLine("#### ignoreFile (same time): " + newFile);
			}
			return false;
		}

		public static List<string> CompareDirectoriesForPatch(string oldPath, string newPath, List<string> ignoreStrings, bool log)
		{
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(oldPath, "*.*", SearchOption.AllDirectories);
			string[] files2 = Directory.GetFiles(newPath, "*.*", SearchOption.AllDirectories);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < files.Length; i++)
			{
				string text = files[i].Replace('\\', '/');
				text = text.Replace(oldPath + "/", "");
				dictionary[text] = text;
			}
			for (int j = 0; j < files2.Length; j++)
			{
				bool flag = true;
				if (ignoreStrings != null)
				{
					foreach (string ignoreString in ignoreStrings)
					{
						if (files2[j].Contains(ignoreString))
						{
							if (log)
							{
								Console.WriteLine("#### not checking: " + files2[j]);
							}
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					continue;
				}
				files2[j] = files2[j].Replace('\\', '/');
				files2[j] = files2[j].Replace(newPath + "/", "");
				string text2 = newPath + "/" + files2[j];
				string oldFile = oldPath + "/" + files2[j];
				if (dictionary.ContainsKey(files2[j]))
				{
					if (CompareFileForPatch(oldFile, text2, log))
					{
						list.Add(text2);
					}
					continue;
				}
				if (log)
				{
					Console.WriteLine("#### newFile: " + files2[j]);
				}
				list.Add(text2);
			}
			return list;
		}

		public static bool InstallPackage(string workingDirectory, string packageFile, string titleID)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2ctrl");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = workingDirectory;
			processStartInfo.RedirectStandardError = true;
			RunCommand(processStartInfo, $"pkg-install {packageFile}");
			Thread.Sleep(1000);
			RunCommand(processStartInfo, $"spawn-app {titleID}", warnError: true);
			return true;
		}

		public static bool UninstallPackage(string workingDirectory, string uninstallTitleID)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = GetTool("psp2ctrl");
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = workingDirectory;
			processStartInfo.RedirectStandardError = true;
			RunCommand(processStartInfo, "reboot");
			RunCommand(processStartInfo, $"pkg-uninstall {uninstallTitleID}", warnError: true);
			Thread.Sleep(5000);
			return true;
		}

		public static float PubCmdVersion()
		{
			if (!hasPublishTools)
			{
				return 0f;
			}
			string tool = GetTool("psp2pubcmd");
			try
			{
				string inputArguments = " version";
				StreamReader streamReader = ExternalTool.StartProcess(tool, inputArguments, "", waitForExit: false, "");
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					string text2 = "ver.";
					int num = text.IndexOf(text2);
					if (num >= 0)
					{
						Console.WriteLine("### Found psp2pubcmd version string: " + text);
						string value = text.Substring(num + text2.Length);
						return Convert.ToSingle(value);
					}
				}
			}
			catch
			{
				Console.WriteLine("### Failed to get psp2pubcmd version, bad version string.");
				return 0f;
			}
			Console.WriteLine("### Failed to get psp2pubcmd version, version not found.");
			return 0f;
		}

		public static void PSP2KillRunningProcesses()
		{
			string tool = GetTool("psp2ctrl");
			Console.WriteLine("PSP2KillRunningProcesses: " + tool);
			string inputArguments = " plist";
			StreamReader streamReader = ExternalTool.StartProcess(tool, inputArguments, "", waitForExit: false, "");
			while (!streamReader.EndOfStream)
			{
				string text = streamReader.ReadLine();
				Console.WriteLine("PSP2KillRunningProcesses: " + text);
				string[] array = text.Split();
				if (array.Length > 3 && array[2].Length > 0 && array[2] != "Name" && array[2] != "SceShell")
				{
					string text2 = array[2];
					inputArguments = " pkill " + text2;
					Console.WriteLine("PSP2KillRunningProcesses: " + inputArguments);
					ExternalTool.StartProcess(tool, inputArguments, "", waitForExit: false, "");
				}
			}
		}

		public static void RestartPSP2(bool reset)
		{
			if (reset)
			{
				PSP2KillRunningProcesses();
				Thread.Sleep(1000);
				return;
			}
			try
			{
				Process process = new Process();
				process.StartInfo.FileName = GetTool("psp2ctrl");
				process.StartInfo.Arguments = "reboot";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.Start();
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Device reboot failed, unable to run 'psp2ctrl.exe'");
				UnityEngine.Debug.LogError("Process exception: " + ex);
			}
		}

		public static void RunApp(string installPath, string installName)
		{
			Process process = new Process();
			process.StartInfo.FileName = GetTool("psp2run");
			process.StartInfo.Arguments = " /console:all /log:\"" + installPath + "/" + FileUtil.UnityGetFileNameWithoutExtension(installName) + ".log\" /fsroot \"" + installPath + "\" /elf \"" + installPath + "/" + installName + "\"";
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.CreateNoWindow = true;
			process.Start();
		}

		public static bool RunCommand(ProcessStartInfo psi, string args, bool warnError = false, bool waitForExit = true)
		{
			Console.WriteLine("### RunCommand: " + psi.FileName + " " + args);
			psi.Arguments = args;
			Program program = new Program(psi);
			program.Start();
			if (waitForExit)
			{
				program.WaitForExit();
				if (program.ExitCode != 0)
				{
					StringBuilder stringBuilder = new StringBuilder("");
					StringBuilder stringBuilder2 = new StringBuilder("");
					string[] standardOutput = program.GetStandardOutput();
					foreach (string text in standardOutput)
					{
						stringBuilder2.Append(text + Environment.NewLine);
					}
					string[] errorOutput = program.GetErrorOutput();
					foreach (string text2 in errorOutput)
					{
						stringBuilder.Append(text2 + Environment.NewLine);
					}
					if (!warnError)
					{
						throw new Exception("Failed running command: " + psi.FileName + " " + psi.Arguments + ".\nError output:\n " + stringBuilder2.ToString() + stringBuilder.ToString());
					}
					UnityEngine.Debug.LogWarning(Path.GetFileName(psi.FileName) + " " + psi.Arguments + "\n" + stringBuilder2.ToString() + stringBuilder.ToString());
					return false;
				}
			}
			return true;
		}

		public static bool RunCommand2(ProcessStartInfo psi, string args, StringBuilder output, StringBuilder errors)
		{
			Console.WriteLine("### RunCommand2: " + psi.FileName + " with " + args);
			psi.Arguments = args;
			Program program = new Program(psi);
			program.Start();
			program.WaitForExit();
			if (output != null)
			{
				string[] standardOutput = program.GetStandardOutput();
				foreach (string text in standardOutput)
				{
					output.Append(text + Environment.NewLine);
				}
			}
			if (errors != null)
			{
				string[] errorOutput = program.GetErrorOutput();
				foreach (string text2 in errorOutput)
				{
					errors.Append(text2 + Environment.NewLine);
				}
			}
			return program.ExitCode == 0;
		}

		internal static bool ValidateNPHexArray(string valueString, int numVals)
		{
			string[] array = valueString.Split(',');
			if (array.Length != numVals)
			{
				return false;
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim();
				if (!text2.StartsWith("0x"))
				{
					return false;
				}
				if (!uint.TryParse(text2.Substring(2), NumberStyles.HexNumber, null, out var result))
				{
					return false;
				}
				if (result > 255)
				{
					return false;
				}
			}
			return true;
		}

		public static bool ValidateNPCommsID(string NPCommsID)
		{
			if (NPCommsID.Length != 12)
			{
				return false;
			}
			char[] array = NPCommsID.ToCharArray();
			for (int i = 0; i < 4; i++)
			{
				if (array[i] < 'A' || array[i] > 'Z')
				{
					return false;
				}
			}
			for (int j = 4; j < 9; j++)
			{
				if (array[j] < '0' || array[j] > '9')
				{
					return false;
				}
			}
			if (array[9] != '_')
			{
				return false;
			}
			for (int k = 10; k < 12; k++)
			{
				if (array[k] < '0' || array[k] > '9')
				{
					return false;
				}
			}
			return true;
		}

		public static bool ValidateVersionString(string version)
		{
			if (version.Length != 5)
			{
				return false;
			}
			char[] array = version.ToCharArray();
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				if (array[num] < '0' || array[num] > '9')
				{
					return false;
				}
				num++;
			}
			if (array[num] != '.')
			{
				return false;
			}
			num++;
			for (int j = 0; j < 2; j++)
			{
				if (array[num] < '0' || array[num] > '9')
				{
					return false;
				}
				num++;
			}
			return true;
		}

		public static bool ValidateContentID(string contentID)
		{
			if (contentID.Length != 36)
			{
				return false;
			}
			char[] array = contentID.ToCharArray();
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				if (array[num] < 'A' || array[num] > 'Z')
				{
					return false;
				}
				num++;
			}
			for (int j = 0; j < 4; j++)
			{
				if (array[num] < '0' || array[num] > '9')
				{
					return false;
				}
				num++;
			}
			if (array[num] != '-')
			{
				return false;
			}
			num++;
			for (int k = 0; k < 4; k++)
			{
				if (array[num] < 'A' || array[num] > 'Z')
				{
					return false;
				}
				num++;
			}
			for (int l = 0; l < 5; l++)
			{
				if (array[num] < '0' || array[num] > '9')
				{
					return false;
				}
				num++;
			}
			if (array[num] != '_')
			{
				return false;
			}
			num++;
			for (int m = 0; m < 2; m++)
			{
				if (array[num] < '0' || array[num] > '9')
				{
					return false;
				}
				num++;
			}
			if (array[num++] != '-')
			{
				return false;
			}
			return true;
		}

		public static bool ValidateTitleID(string titleID)
		{
			if (titleID.Length != 9)
			{
				return false;
			}
			char[] array = titleID.ToCharArray();
			for (int i = 0; i < 4; i++)
			{
				if (array[i] < 'A' || array[i] > 'Z')
				{
					return false;
				}
			}
			for (int j = 4; j < 9; j++)
			{
				if (array[j] < '0' || array[j] > '9')
				{
					return false;
				}
			}
			return true;
		}

		internal static string CreateDummyTitleID()
		{
			return "ABCD12345";
		}

		internal static string CreateDummyServiceID(string titleID)
		{
			return "IV0000-" + titleID + "_00";
		}

		internal static string CreateDummyContentID(string serviceID)
		{
			return serviceID + "-0123456789ABCDEF";
		}

		internal static string CreateDummyNPCommsID()
		{
			return "DCBA54321_00";
		}

		internal static string CreateDummyNPPassphrase()
		{
			string text = "";
			for (int i = 0; i < 16; i++)
			{
				text += "0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00";
				if (i != 15)
				{
					text += ",";
				}
			}
			return text;
		}

		internal static string CreateDummyNPSignature()
		{
			string text = "";
			for (int i = 0; i < 20; i++)
			{
				text += "0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00";
				if (i != 19)
				{
					text += ",";
				}
			}
			return text;
		}

		public static bool CompileAsmFile(string input, string output, string workingDirectory)
		{
			string args = "\"" + input + ".s\" -o \"" + output + "\"";
			return RunTool("arm-eabi-as", args, workingDirectory);
		}

		public static bool CompileCppFile(string input, string output, string workingDirectory)
		{
			string args = " -DNDEBUG -DSN_TARGET_PSP2=1 -O3 -Xc-=exceptions -Xc-=rtti -Xthumb=0 -Xblxcall=1 -c \"" + input + "\" -o \"" + output + "\"";
			return RunTool("psp2snc", args, workingDirectory);
		}

		public static bool BuildLibrary(List<string> inputs, string output, string workingDirectory = "")
		{
			string text = "crs \"" + output + "\"";
			foreach (string input in inputs)
			{
				text = text + " \"" + input + "\"";
			}
			return RunTool("psp2snarl", text, workingDirectory);
		}

		public static bool LinkDynamicLibrary(List<string> inputs, string outputPath, string mapPath = "", string workingDirectory = "")
		{
			string text = " --warn-once --oformat=prx --prx-loose-stub";
			if (mapPath.Length != 0)
			{
				text = text + " --Map=\"" + mapPath + "\"";
			}
			foreach (string input in inputs)
			{
				text = text + " \"" + input + "\"";
			}
			text = text + " -o \"" + outputPath + "\"";
			return RunTool("psp2ld", text, workingDirectory);
		}
	}
}
