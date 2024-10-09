using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor.Modules;
using UnityEditor.PSP2.Il2Cpp;
using UnityEditor.SonyCommon;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.PSP2
{
	internal class PostProcessPSP2Player
	{
		public class ParamsAttribute
		{
			public static int kLibLocation = 2;

			public static int kInfoBar = 128;

			public static int kInfoWhite = 256;

			public static int kUpgradable = 1024;

			public static int kHealthWarning = 2097152;

			public static int kUseTwDialog = 33554432;

			public static int kTv_DisableTouchEmu = 268435456;
		}

		public class ParamsAttributeMinor
		{
			public static int kEnterButtonAssignment_Default = 0;

			public static int kEnterButtonAssignment_CircleButton = 1;

			public static int kEnterButtonAssignment_CrossButton = 2;

			public static int kEnterButtonAssignment_Mask = 3;

			public static int kTv_Default = 0;

			public static int kTv_VitaBootable_TvBootable = 16;

			public static int kTv_VitaBootable_TvNotBootable = 24;

			public static int kTv_Mask = 24;
		}

		public class ParamsAttribute2
		{
			public static int kMemExpansionNone = 0;

			public static int kMemExpansion29MiB = 4;

			public static int kMemExpansion77MiB = 8;

			public static int kMemExpansion109MiB = 12;

			public static int kMemExpansion_Mask = 12;
		}

		public enum PackageType
		{
			kApplication,
			kPatch,
			kAdditionalContent
		}

		private enum BuildFolderStatus
		{
			BuildFolderDoesNotExist,
			BuildFolderIsEmpty,
			BuildFolderContainsBuild,
			BuildFolderContainsUnknownFiles
		}

		internal static ScriptingImplementation s_ScriptingBackend;

		internal static string s_MonoVitaStubLibrary = "mono-vita_stub.a";

		internal static string s_MonoAssembliesSUPRX = "MonoAssembliesPSP2.suprx";

		internal static string s_Il2cppAssembliesSUPRX = "il2CppAssemblies.suprx";

		internal static string s_Il2cppDebugProjectName = "il2cppDebug";

		internal static string s_Il2cppDebugFolder = "il2cppDebug";

		internal static string s_Il2cppSymbolMap = "SymbolMap";

		private static ParamFile sfxParams = new ParamFile();

		internal static BuildPostProcessArgs PostProcessArgs { private get; set; }

		internal static void CreateRazorHUDSettingsFile(string stagingArea)
		{
			string path = Path.Combine(stagingArea, "hud_settings.ini");
			StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine("# Razor HUD settings");
			streamWriter.WriteLine("ENABLE = 1");
			streamWriter.WriteLine("GPU_HUD = 0");
			streamWriter.WriteLine("CPU_HUD = 0");
			streamWriter.WriteLine("HUD_CORES = 1");
			streamWriter.WriteLine("HUD_FIRMWARE = 1");
			streamWriter.WriteLine("HUD_DISPLAY_FRAMES = 1");
			streamWriter.WriteLine("HUD_SCENE_COLOR_MODE = 0");
			streamWriter.WriteLine("LIVE = 0");
			streamWriter.Close();
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

		internal static bool StringListContainsCaseInd(List<string> strings, string text)
		{
			foreach (string @string in strings)
			{
				if (@string.ToLower().Contains(text.ToLower()))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool ParamSfxVerifyAndSetDefaults(bool isPCHosted)
		{
			bool result = true;
			string text = VitaSDKTools.CreateDummyTitleID();
			string serviceID = VitaSDKTools.CreateDummyServiceID(text);
			string text2 = VitaSDKTools.CreateDummyContentID(serviceID);
			string text3 = "\nRequired for SCE submission packages.";
			sfxParams.GetWithWarningAndSetDefault("CATEGORY", "gd", "Param File - CATEGORY not found, using a default value 'gd'" + text3, !isPCHosted);
			string text4 = sfxParams.Get("CATEGORY", "gd");
			if (text4 != "gd" && text4 != "gp")
			{
				UnityEngine.Debug.LogError("Build Failed: Param File - Unsupported category, category must be 'gd' or 'gp'\n");
				result = false;
			}
			sfxParams.GetWithWarningAndSetDefault("APP_VER", "01.00", "Param File - APP_VER not found, using a default value '01.00'" + text3, !isPCHosted);
			if (!VitaSDKTools.ValidateVersionString(sfxParams.Get("APP_VER", "01.00")))
			{
				UnityEngine.Debug.LogError("Build Failed: Param File - Application version is invalid and must be in the form 01.00\n");
				result = false;
			}
			sfxParams.GetWithWarningAndSetDefault("VERSION", "01.00", "Param File - VERSION not found, using a default value '01.00'" + text3, !isPCHosted);
			if (!VitaSDKTools.ValidateVersionString(sfxParams.Get("VERSION", "01.00")))
			{
				UnityEngine.Debug.LogError("Build Failed: Param File - Master version is invalid and must be in the form 01.00\n");
				result = false;
			}
			sfxParams.GetWithWarningAndSetDefault("ATTRIBUTE", "0", "Param File - ATTRIBUTE not found, using a default value '0'" + text3, !isPCHosted);
			sfxParams.GetWithWarningAndSetDefault("ATTRIBUTE_MINOR", "2", "Param File - ATTRIBUTE_MINOR not found, using a default value '2'" + text3, !isPCHosted);
			sfxParams.GetWithWarningAndSetDefault("CONTENT_ID", text2, "Param File - CONTENT_ID not found, using a default value '" + text2 + "'" + text3, !isPCHosted);
			sfxParams.GetWithWarningAndSetDefault("PARENTAL_LEVEL", "1", "Param File - PARENTAL_LEVEL not found, using a default value '1'" + text3, !isPCHosted);
			sfxParams.GetWithWarningAndSetDefault("SAVEDATA_MAX_SIZE", "10240", "Param File - SAVEDATA_MAX_SIZE not found, using a default value '10240'" + text3, !isPCHosted);
			int @int = sfxParams.GetInt("SAVEDATA_MAX_SIZE", 10240);
			if (((uint)@int & 0x3FFu) != 0 || @int <= 0)
			{
				UnityEngine.Debug.LogError("Build Failed: Save data quota must be a multiple of 1024 and not 0.\n");
				result = false;
			}
			sfxParams.GetWithWarningAndSetDefault("TITLE", "Unity Application", "Param File - TITLE not found, using a default value 'Unity Application'" + text3, !isPCHosted);
			if (Encoding.UTF8.GetByteCount(sfxParams.Get("STITLE", "")) > 127)
			{
				UnityEngine.Debug.LogError("Build Failed: Param File - Title length exceeds " + 127 + " bytes after UTF-8 encoding\n");
				result = false;
			}
			sfxParams.GetWithWarningAndSetDefault("STITLE", "UnityApp", "Param File - STITLE not found, using a default value 'UnityApp'" + text3, !isPCHosted);
			if (Encoding.UTF8.GetByteCount(sfxParams.Get("STITLE", "")) > 51)
			{
				UnityEngine.Debug.LogError("Build Failed: Param File - Short title length exceeds " + 51 + " bytes after UTF-8 encoding\n");
				result = false;
			}
			sfxParams.GetWithWarningAndSetDefault("TITLE_ID", text, "Param File - TITLE_ID not found, using a default value '" + text + "'" + text3, !isPCHosted);
			int num = sfxParams.GetInt("ATTRIBUTE", 0);
			if ((num & ParamsAttribute.kUseTwDialog) == ParamsAttribute.kUseTwDialog)
			{
				num &= ~ParamsAttribute.kUseTwDialog;
				UnityEngine.Debug.LogWarning("param.sfx specified use of twitter dialog but this was deprecated in SDK 3.570, forcing flag to 0");
			}
			if ((num & ParamsAttribute.kLibLocation) == ParamsAttribute.kLibLocation)
			{
				num &= ~ParamsAttribute.kLibLocation;
				UnityEngine.Debug.LogWarning("param.sfx specified use of liblocation but this was deprecated in SDK 3.570, forcing flag to 0");
			}
			sfxParams.SetInt("ATTRIBUTE", num);
			sfxParams.Dump();
			return result;
		}

		private static string[] FindStringsContaining(string[] strings, bool dumpResults, string find)
		{
			string[] array = Array.FindAll(strings, (string s) => s.Contains(find));
			if (dumpResults)
			{
				Console.WriteLine("### Contains: '" + find + "'");
				string[] array2 = array;
				foreach (string text in array2)
				{
					Console.WriteLine(" Found: '" + text + "'");
				}
			}
			return array;
		}

		private static int CountProjectFiles(string stagingArea)
		{
			try
			{
				Console.WriteLine("### CountProjectFiles:  " + stagingArea);
				string[] files = Directory.GetFiles(stagingArea + "", "*.*", SearchOption.AllDirectories);
				return files.Length;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return 0;
		}

		private static bool ArchiveFiles(string stagingArea)
		{
			string text = stagingArea + "\\Data";
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(text, "mainData*.*", SearchOption.TopDirectoryOnly);
			string[] array = files;
			foreach (string item in array)
			{
				list.Add(item);
			}
			files = Directory.GetFiles(text, "level*.*", SearchOption.TopDirectoryOnly);
			string[] array2 = files;
			foreach (string item2 in array2)
			{
				list.Add(item2);
			}
			files = Directory.GetFiles(text, "resources*.*", SearchOption.TopDirectoryOnly);
			string[] array3 = files;
			foreach (string item3 in array3)
			{
				list.Add(item3);
			}
			files = Directory.GetFiles(text, "sharedassets*.assets", SearchOption.TopDirectoryOnly);
			string[] array4 = files;
			foreach (string item4 in array4)
			{
				list.Add(item4);
			}
			files = Directory.GetFiles(text + "\\Resources", "*.*", SearchOption.TopDirectoryOnly);
			string[] array5 = files;
			foreach (string item5 in array5)
			{
				list.Add(item5);
			}
			string text2 = stagingArea + "\\Media";
			CreateDirectory(text2);
			foreach (string item6 in list)
			{
				Console.WriteLine("### Archiving: " + item6.Substring(text.Length + 1));
				string text3 = text2 + item6.Substring(text.Length);
				CreateDirectory(Path.GetDirectoryName(text3));
				FileUtil.CopyFileOrDirectory(item6, text3);
			}
			string text4 = "archive.txt";
			string outFile = "archive.psarc";
			string currentDirectory = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(stagingArea);
			StreamWriter streamWriter = new StreamWriter(text4);
			foreach (string item7 in list)
			{
				streamWriter.WriteLine("Media" + item7.Substring(text.Length));
			}
			streamWriter.Close();
			VitaSDKTools.CompressWithPSArc(text4, outFile);
			FileUtil.DeleteFileOrDirectory(text4);
			Directory.SetCurrentDirectory(currentDirectory);
			Console.WriteLine("### Curr Dir: " + Directory.GetCurrentDirectory());
			FileUtil.DeleteFileOrDirectory(text2);
			foreach (string item8 in list)
			{
				FileUtil.DeleteFileOrDirectory(item8);
			}
			files = Directory.GetFiles(text + "\\Resources", "*.*", SearchOption.AllDirectories);
			if (files.Length == 0)
			{
				FileUtil.DeleteFileOrDirectory(text + "\\Resources");
			}
			return true;
		}

		private static BuildFolderStatus CheckContentsOfBuildFolder(string buildFolder, bool isPCHosted, bool isSubmission)
		{
			if (!Directory.Exists(buildFolder))
			{
				return BuildFolderStatus.BuildFolderDoesNotExist;
			}
			string[] files = Directory.GetFiles(buildFolder, "*", SearchOption.AllDirectories);
			if (isPCHosted)
			{
				if (files.Length == 0)
				{
					return BuildFolderStatus.BuildFolderIsEmpty;
				}
				if (Array.FindAll(files, (string s) => s.Contains("Media")).Length < 1)
				{
					Console.WriteLine("### Unexpected folder Media has wrong number of files: " + Array.FindAll(files, (string s) => s.Contains("Media")).Length);
					return BuildFolderStatus.BuildFolderContainsUnknownFiles;
				}
				if (Array.FindAll(files, (string s) => s.Contains("sce_sys")).Length < 1)
				{
					Console.WriteLine("### Unexpected folder sce_sys has wrong number of files: " + Array.FindAll(files, (string s) => s.Contains("sce_sys")).Length);
					return BuildFolderStatus.BuildFolderContainsUnknownFiles;
				}
				files = Array.FindAll(files, (string s) => !s.Contains("\\Media\\"));
				files = Array.FindAll(files, (string s) => !s.Contains("\\sce_sys\\"));
				files = Array.FindAll(files, (string s) => !s.Contains("\\sce_module\\"));
				files = Array.FindAll(files, (string s) => !s.Contains("\\savedata\\"));
				files = Array.FindAll(files, (string s) => !s.Contains(".bat"));
				files = Array.FindAll(files, (string s) => !s.Contains(".map"));
				files = Array.FindAll(files, (string s) => !s.Contains(".log"));
				files = Array.FindAll(files, (string s) => !s.Contains(".self"));
				files = Array.FindAll(files, (string s) => !s.Contains(".psp2path"));
				files = Array.FindAll(files, (string s) => !s.Contains(".bin"));
				files = Array.FindAll(files, (string s) => !s.Contains(".psp2dmp"));
				files = Array.FindAll(files, (string s) => !s.Contains(".spsp2dmp"));
				files = Array.FindAll(files, (string s) => !s.Contains(".sgx"));
				files = Array.FindAll(files, (string s) => !s.Contains(".txt"));
				files = Array.FindAll(files, (string s) => !s.Contains(".psarc"));
				files = Array.FindAll(files, (string s) => !s.Contains(".ini"));
				files = Array.FindAll(files, (string s) => !s.Contains("SymbolFiles"));
				files = Array.FindAll(files, (string s) => !s.Contains(s_Il2cppDebugFolder));
				files = Array.FindAll(files, (string s) => !s.Contains(".png"));
				if (files.Length > 0)
				{
					string[] array = files;
					foreach (string text in array)
					{
						Console.WriteLine("### Unexpected: " + text);
					}
					return BuildFolderStatus.BuildFolderContainsUnknownFiles;
				}
			}
			else
			{
				files = Array.FindAll(files, (string s) => !s.Contains("\\SymbolFiles"));
				if (files.Length == 0)
				{
					return BuildFolderStatus.BuildFolderIsEmpty;
				}
				string[] array2 = new string[7] { "\\sce_sys\\", "\\sym_self\\", "\\sym_tab\\", ".pkg", "spec.xml", "verify.log", ".zip" };
				string[] array3 = array2;
				foreach (string item in array3)
				{
					files = Array.FindAll(files, (string s) => !s.Contains(item));
				}
				if (files.Length > 0)
				{
					string[] array4 = files;
					foreach (string text2 in array4)
					{
						Console.WriteLine("### Unexpected: " + text2);
					}
					return BuildFolderStatus.BuildFolderContainsUnknownFiles;
				}
			}
			return BuildFolderStatus.BuildFolderContainsBuild;
		}

		private static void CreateApplicationInfoCPP(string stagingAreaData, string titleID, string npCommsID, string serviceID)
		{
			string path = stagingAreaData + "/MiscCPP";
			path = Path.Combine(Directory.GetCurrentDirectory(), path);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string path2 = Path.Combine(path, "ApplicationInfo.cpp");
			StreamWriter streamWriter = new StreamWriter(path2);
			streamWriter.WriteLine("#include <stdio.h>");
			streamWriter.WriteLine("#include <np.h>");
			streamWriter.WriteLine("#define STORAGE_CLASS  __declspec(dllexport)");
			if (npCommsID.Length > 0 && !VitaSDKTools.ValidateNPCommsID(npCommsID))
			{
				npCommsID = "";
				UnityEngine.Debug.LogError("Invalid NP Communications ID, it should contain 12 characters in the form 'ABCD12345_00', or none if NP is not required.");
			}
			string text = PlayerSettings.PSVita.npCommsSig;
			if (text.Length > 0 && !VitaSDKTools.ValidateNPHexArray(text, 160))
			{
				text = "";
				UnityEngine.Debug.LogError("Invalid NP signature, it should contain 160 8-bit hex values separated by commas, or none if NP is not required.");
			}
			string text2 = PlayerSettings.PSVita.npCommsPassphrase;
			if (text2.Length > 0 && !VitaSDKTools.ValidateNPHexArray(text2, 128))
			{
				text2 = "";
				UnityEngine.Debug.LogError("Invalid NP pass phrase, it should contain 128 8-bit hex values separated by commas, or none if NP is not required.");
			}
			if ((npCommsID.Length > 0 || text.Length > 0 || text2.Length > 0) && (text.Length == 0 || text2.Length == 0) && npCommsID.CompareTo(VitaSDKTools.CreateDummyNPCommsID()) != 0)
			{
				UnityEngine.Debug.LogError("One or more of NP communications ID, NP signature or NP pass phrase are not set, all are required, or none.");
			}
			bool flag = true;
			if (npCommsID.Length == 0)
			{
				npCommsID = VitaSDKTools.CreateDummyNPCommsID();
				flag = false;
			}
			if (text2.Length == 0)
			{
				text2 = VitaSDKTools.CreateDummyNPPassphrase();
				flag = false;
			}
			if (text.Length == 0)
			{
				text = VitaSDKTools.CreateDummyNPSignature();
				flag = false;
			}
			streamWriter.WriteLine("STORAGE_CLASS SceNpCommunicationId g_PSP2TitleNPCommsId = {");
			string text3 = "";
			for (int i = 0; i < 9; i++)
			{
				string text4 = text3;
				text3 = text4 + "'" + npCommsID[i] + "'";
				if (i != 8)
				{
					text3 += ",";
				}
			}
			string text5 = npCommsID.Substring(10);
			streamWriter.WriteLine("{ " + text3 + " },");
			streamWriter.WriteLine("'\\0',");
			streamWriter.WriteLine(text5 + ", 0");
			streamWriter.WriteLine("};\n");
			streamWriter.WriteLine("STORAGE_CLASS SceNpCommunicationPassphrase g_PSP2TitleNPCommsPassphrase = {");
			streamWriter.WriteLine(text2 + "};\n");
			streamWriter.WriteLine("STORAGE_CLASS SceNpCommunicationSignature g_PSP2TitleNPCommsSig = {");
			streamWriter.WriteLine(text + "};\n");
			streamWriter.WriteLine("STORAGE_CLASS bool g_PSP2TitleNPHasTrophyPack = " + ((!flag || PlayerSettings.PSVita.npTrophyPackPath.Length <= 0) ? "false" : "true") + ";");
			streamWriter.WriteLine("STORAGE_CLASS int g_PSP2TitleNPAgeRating = " + PlayerSettings.PSVita.npAgeRating + ";");
			streamWriter.WriteLine("STORAGE_CLASS const char* g_PSP2TitleNPServiceId = \"" + serviceID + "\";");
			streamWriter.WriteLine("STORAGE_CLASS const char* g_PSP2TitleId = \"" + titleID + "\";");
			streamWriter.WriteLine("STORAGE_CLASS bool g_PSP2TitleTrial = false;");
			streamWriter.WriteLine("STORAGE_CLASS bool g_PSP2HasPSArc = " + ((!EditorUserBuildSettings.compressWithPsArc) ? "false" : "true") + ";");
			streamWriter.WriteLine("STORAGE_CLASS int g_PSP2FileCount = " + CountProjectFiles(stagingAreaData) + ";");
			streamWriter.Close();
		}

		private static void CreateDirectory(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		private static void CopyFiles(string destPath, string sourcePath, string[] files, bool ignoreMissing = false)
		{
			CreateDirectory(destPath);
			foreach (string path in files)
			{
				string dest = Path.Combine(destPath, path);
				string text = Path.Combine(sourcePath, path);
				if (!ignoreMissing || Directory.Exists(text) || File.Exists(text))
				{
					FileUtil.CopyFileOrDirectory(text, dest);
				}
			}
		}

		private static void MoveFiles(string destPath, string sourcePath, string[] files)
		{
			CreateDirectory(destPath);
			foreach (string path in files)
			{
				string dest = Path.Combine(destPath, path);
				string source = Path.Combine(sourcePath, path);
				FileUtil.MoveFileOrDirectory(source, dest);
			}
		}

		private static void CopyFile(string destFile, string sourceFile)
		{
			Console.WriteLine("### CopyFile(" + destFile + ", " + sourceFile + ")");
			string directoryName = Path.GetDirectoryName(destFile);
			CreateDirectory(directoryName);
			FileUtil.CopyFileOrDirectory(sourceFile, destFile);
		}

		private static void CopyFileWithDefault(string destFile, string sourceFile, string defaultFile)
		{
			Console.WriteLine("### CopyFileWithDefault(" + destFile + ", " + sourceFile + ", " + defaultFile + ")");
			string directoryName = Path.GetDirectoryName(destFile);
			CreateDirectory(directoryName);
			if (sourceFile.Length > 0)
			{
				if (File.Exists(sourceFile))
				{
					FileUtil.CopyFileOrDirectory(sourceFile, destFile);
					return;
				}
				UnityEngine.Debug.LogWarning("File not found: " + sourceFile + "\n");
			}
			FileUtil.CopyFileOrDirectory(defaultFile, destFile);
		}

		private static void CopyFilesWithDefault(string destPath, string sourcePath, string defaultPath, string[] files)
		{
			CreateDirectory(destPath);
			foreach (string path in files)
			{
				string text = Path.Combine(sourcePath, path);
				string dest = Path.Combine(destPath, path);
				if (File.Exists(text))
				{
					FileUtil.CopyFileOrDirectory(text, dest);
					continue;
				}
				string source = Path.Combine(defaultPath, path);
				FileUtil.CopyFileOrDirectory(source, dest);
			}
		}

		private static void MoveFilesWithDefault(string destPath, string sourcePath, string defaultPath, string[] files)
		{
			CreateDirectory(destPath);
			foreach (string path in files)
			{
				string text = Path.Combine(sourcePath, path);
				string dest = Path.Combine(destPath, path);
				if (File.Exists(text))
				{
					FileUtil.MoveFileOrDirectory(text, dest);
					continue;
				}
				string source = Path.Combine(defaultPath, path);
				FileUtil.CopyFileOrDirectory(source, dest);
			}
		}

		private static bool CopyDirectory(string destPath, string sourcePath)
		{
			return CopyDirectoryWithFilter(destPath, sourcePath, "*.*");
		}

		private static bool CopyDirectoryWithFilter(string destPath, string sourcePath, string filters)
		{
			if (!Directory.Exists(sourcePath))
			{
				return false;
			}
			string[] directories = Directory.GetDirectories(sourcePath);
			string[] array = directories;
			foreach (string text in array)
			{
				string path = text.Substring(sourcePath.Length + 1);
				string path2 = Path.Combine(destPath, path);
				CreateDirectory(path2);
			}
			string[] array2 = filters.Split(';');
			string[] array3 = array2;
			foreach (string searchPattern in array3)
			{
				string[] files = Directory.GetFiles(sourcePath, searchPattern, SearchOption.AllDirectories);
				string[] array4 = files;
				foreach (string text2 in array4)
				{
					string path3 = text2.Substring(sourcePath.Length + 1);
					string text3 = Path.Combine(destPath, path3);
					string directoryName = Path.GetDirectoryName(text3);
					CreateDirectory(directoryName);
					FileUtil.CopyFileOrDirectory(text2, text3);
				}
			}
			return true;
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
				if (preserveSaves && text.EndsWith("savedata"))
				{
					continue;
				}
				try
				{
					DeleteDirectory(text);
				}
				catch (Exception ex)
				{
					if (!text.EndsWith(s_Il2cppDebugFolder))
					{
						throw ex;
					}
				}
			}
		}

		private static void RemovePreviousBuild(string installPath, string installName)
		{
			int num = 0;
			bool flag = false;
			while (!flag)
			{
				try
				{
					EditorUtility.DisplayProgressBar("Building Player", "Removing previous build", 0.95f);
					PurgeBuildDirectory(installPath, preserveSaves: true);
					flag = true;
				}
				catch (Exception)
				{
					switch (num)
					{
					case 0:
						EditorUtility.DisplayProgressBar("Building Player", "Killing " + installName, 0.95f);
						VitaSDKTools.PSP2KillRunningProcesses();
						Thread.Sleep(1000);
						break;
					case 1:
					case 2:
					case 3:
						switch (EditorUtility.DisplayDialogComplex("PSVita Build : Unable to remove previous build data!", installPath + " may be locked by an open explorer window or command prompt", "Retry", "Abort", "Reset"))
						{
						case 0:
							EditorUtility.DisplayProgressBar("Building Player", "Stopping PSVita", 0.95f);
							VitaSDKTools.PSP2KillRunningProcesses();
							Thread.Sleep(1000);
							break;
						case 1:
							throw;
						case 2:
							EditorUtility.DisplayProgressBar("Building Player", "Resetting PSVita", 0.95f);
							VitaSDKTools.PSP2Ctrl("reboot");
							break;
						}
						break;
					case 4:
						if (!EditorUtility.DisplayDialog("PSVita Build : Unable to remove " + installName + " previous data!", installPath + " may be locked by an open explorer window or command prompt", "Ignore", "Abort"))
						{
							throw;
						}
						flag = true;
						break;
					default:
						throw;
					}
					num++;
				}
			}
		}

		private static bool ValidateBuildEnvironment(bool autoRunPlayer, bool pcHosted)
		{
			float num = 3.55f;
			int num2 = 56033297;
			int num3 = 0;
			float num4 = 2.25f;
			bool flag = !File.Exists("NoSDKCheck.txt");
			bool flag2 = !File.Exists("NoKernelCheck.txt");
			bool flag3 = !File.Exists("NoDeviceCheck.txt");
			if (!VitaSDKTools.CheckSDKToolsExist(pcHosted))
			{
				UnityEngine.Debug.LogError("Build failed; The PSVita SDK is not installed or is missing some components, please install the PSVita SDK, see below for supported versions...\nMinumum supported version: " + num2.ToString("X8") + ((num3 <= 0) ? "" : (", Maximum supported version: " + num3.ToString("X8"))));
				return false;
			}
			if (flag)
			{
				int num5 = VitaSDKTools.SDKVersion();
				Console.WriteLine("### SDK Version: " + num5.ToString("X8"));
				if (num5 < num2)
				{
					if (num5 == 0)
					{
						UnityEngine.Debug.LogError("Build failed; Unable to determine SDK version, some components are missing or corrupt, please install the PSVita SDK, see below for supported versions...\nMinimum supported version: " + num2.ToString("X8") + ((num3 <= 0) ? "" : (", Maximum supported version: " + num3.ToString("X8"))));
						return false;
					}
					UnityEngine.Debug.LogError("Build failed; PSVita SDK version is invalid, see below for supported versions...\nMinimum supported version: " + num2.ToString("X8") + ((num3 <= 0) ? "" : (", Maximum supported version: " + num3.ToString("X8"))));
					return false;
				}
				float num6 = VitaSDKTools.PubCmdVersion();
				if (num6 > 0f && num6 < num4)
				{
					UnityEngine.Debug.LogError("Build failed; The SCE tool 'psp2pubcmd.exe' is an old unsupported version...\nFound version: " + num6 + " Minimum supported version: " + num4);
					return false;
				}
			}
			else
			{
				UnityEngine.Debug.LogWarning("PSVita SDK version check has been disabled.");
			}
			if (!flag2)
			{
				UnityEngine.Debug.LogWarning("PSVita Kernel version check has been disabled.");
			}
			if (!flag3)
			{
				UnityEngine.Debug.LogWarning("PSVita device state check has been disabled.");
			}
			if (autoRunPlayer)
			{
				VitaPowerStatus vitaPowerStatus = VitaSDKTools.DevKitGetPowerStatus();
				Console.WriteLine("### vitaPowerStatus: " + vitaPowerStatus);
				if (vitaPowerStatus == VitaPowerStatus.NO_SUPPLY)
				{
					UnityEngine.Debug.LogError("Unable to run: Please connect the power supply to the PSVita dev-kit and try again.\n");
					return false;
				}
				bool flag4 = VitaSDKTools.DevKitIsConnected();
				Console.WriteLine("### vitaIsConnected: " + flag4);
				if (flag3 && !flag4)
				{
					UnityEngine.Debug.LogError("Unable to run: Please connect a PSVita dev-kit to the PC and ensure that it is set as the default in 'Neighborhood for Playstation(R)Vita'.\n");
					return false;
				}
				if (vitaPowerStatus != VitaPowerStatus.ON)
				{
					VitaSDKTools.DevKitPowerUp();
				}
				float num7 = VitaSDKTools.DevKitKernelVersion();
				MemCardStatus memCardStatus = VitaSDKTools.DevKitHasMemoryCard();
				Console.WriteLine("### vitaSystemVersion: " + num7);
				Console.WriteLine("### memCardState: " + memCardStatus);
				if (flag2 && num7 > 0f && num7 < num)
				{
					UnityEngine.Debug.LogError("Unable to run: Invalid PSVita system software ( >= " + num + " required) , please update your dev-kit.\n");
					return false;
				}
				if (flag3 && !pcHosted)
				{
					switch (memCardStatus)
					{
					case MemCardStatus.NotAvailable:
						UnityEngine.Debug.LogError("Unable to install & run package; Please insert a memory card into the PSVita.\n");
						return false;
					default:
						UnityEngine.Debug.LogWarning("Failed to retrieve memory card status from dev-kit, attempting to continue package installation.\n");
						break;
					case MemCardStatus.Available:
						break;
					}
				}
			}
			return true;
		}

		private static bool BuildMonoScript(BuildTarget target, BuildOptions options, string installPath, string stagingAreaData, string stagingArea, string playerPackage, string stagingAreaDataManaged, RuntimeClassRegistry usedClassRegistry, bool scriptOnly = false)
		{
			Console.WriteLine(">>> BuildMonoScript: scriptOnly=" + scriptOnly + ", stagingAreaData=" + stagingAreaData + ", stagingArea=" + stagingArea + ", stagingAreaDataManaged=" + stagingAreaDataManaged + ", playerPackage=" + playerPackage);
			CrossCompileOptions crossCompileOptions = CrossCompileOptions.Static;
			crossCompileOptions = (((options & BuildOptions.AllowDebugging) == 0) ? crossCompileOptions : (crossCompileOptions | CrossCompileOptions.Debugging));
			crossCompileOptions = ((!EditorUserBuildSettings.explicitNullChecks) ? crossCompileOptions : (crossCompileOptions | CrossCompileOptions.ExplicitNullChecks));
			CopyModules(stagingAreaData, playerPackage);
			string text = Path.Combine(stagingAreaData, "AOTCompileStep");
			FileUtil.CopyFileOrDirectory(stagingAreaDataManaged, text);
			MonoCrossCompile.CrossCompileAOTDirectoryParallel(target, crossCompileOptions, stagingAreaDataManaged, text, PlayerSettings.aotOptions);
			AssemblyReferenceChecker assemblyReferenceChecker = new AssemblyReferenceChecker();
			assemblyReferenceChecker.CollectReferences(text, collectMethods: false, 0f, ignoreSystemDlls: false);
			string file = Path.Combine(text, "RegisterMonoModules.cpp");
			MonoAOTRegistration.WriteCPlusPlusFileForStaticAOTModuleRegistration(target, file, CrossCompileOptions.Static, advancedLic: true, "Unknown", stripping: true, usedClassRegistry, assemblyReferenceChecker, stagingAreaDataManaged);
			string source = Path.Combine(playerPackage, "Data/Modules/" + s_MonoVitaStubLibrary);
			FileUtil.CopyFileOrDirectory(source, Path.Combine(text, s_MonoVitaStubLibrary));
			if (!LinkMonoAssemblies(options, stagingAreaData, stagingArea, assemblyReferenceChecker))
			{
				return false;
			}
			if (!ProcessNativePlugins(target, stagingAreaData, text, playerPackage))
			{
				return false;
			}
			if (scriptOnly)
			{
				bool flag = (options & BuildOptions.InstallInBuildFolder) != 0;
				string text2 = ((!flag) ? installPath : Path.Combine(Unsupported.GetBaseUnityDeveloperFolder(), "build/PSP2Player"));
				FileUtil.DeleteFileOrDirectory(stagingArea + "/Data/Modules/" + s_MonoAssembliesSUPRX);
				FileUtil.CopyFileOrDirectory(Path.Combine(text, s_MonoAssembliesSUPRX), stagingArea + "/Data/Modules/" + s_MonoAssembliesSUPRX);
				if (flag)
				{
					FileUtil.DeleteFileOrDirectory(Path.Combine(text2, "Data/Modules/MonoAssembliesPSP2_stub_weak.a"));
					FileUtil.MoveFileOrDirectory(Path.Combine(text, "MonoAssembliesPSP2_stub_weak.a"), Path.Combine(text2, "Data/Modules/MonoAssembliesPSP2_stub_weak.a"));
				}
				FileUtil.DeleteFileOrDirectory(text2 + "/Media/Modules");
				FileUtil.DeleteFileOrDirectory(text2 + "/Media/Managed");
				FileUtil.DeleteFileOrDirectory(text2 + "/Media/Plugins");
				FileUtil.MoveFileOrDirectory(stagingAreaData + "/Modules/", text2 + "/Media/Modules");
				FileUtil.MoveFileOrDirectory(stagingAreaData + "/Managed/", text2 + "/Media/Managed");
				FileUtil.MoveFileOrDirectory(stagingAreaData + "/Plugins/", text2 + "/Media/Plugins");
			}
			return true;
		}

		private static bool BuildIL2CPPScript(BuildTarget target, BuildOptions options, string installPath, string stagingAreaData, string stagingArea, string playerPackage, string stagingAreaDataManaged, RuntimeClassRegistry usedClassRegistry, bool scriptOnly = false)
		{
			Console.WriteLine(">>> BuildIL2CPPScript: scriptOnly=" + scriptOnly + ", stagingAreaData=" + stagingAreaData + ", stagingArea=" + stagingArea + ", stagingAreaDataManaged=" + stagingAreaDataManaged + ", playerPackage=" + playerPackage);
			string path = "..\\..\\..";

			string currentDirectory = Directory.GetCurrentDirectory();
			UnityEngine.Debug.Log("Current working directory: " + currentDirectory);
			UnityEngine.Debug.Log("Application.dataPath directory: " + Application.dataPath);


			string workingDirectory = "temp\\stagingarea\\data\\MiscCPP";
			string text = Path.Combine(path, "ApplicationInfo.o");

			UnityEngine.Debug.Log("ApplicationInfo.o Path: " + text);
			UnityEngine.Debug.Log("VitaSDKTools.CompileCppFile working directory: " + workingDirectory);




			string input = "ApplicationInfo.cpp";
			CopyModules(stagingAreaData, playerPackage);

			DateTime yourBirthday = new DateTime(DateTime.Today.Year, 8, 8);
			if (System.DateTime.Today == yourBirthday)
			{
				UnityEngine.Debug.Log("12brendon34 was here");
			}

			if (!VitaSDKTools.CompileCppFile(input, text, workingDirectory))
			{
				return false;
			}
			string extraLinkerOptions = text;
			EditorUtility.DisplayProgressBar("Building Player", "Running IL2CPP", 0f);
			PSP2Il2CppPlatformProvider platformProvider = new PSP2Il2CppPlatformProvider(target, EditorUserBuildSettings.explicitNullChecks, EditorUserBuildSettings.explicitArrayBoundsChecks, PlayerSettings.PSVita.scriptOptimizationLevel, extraLinkerOptions);
			FileUtil.CopyFileOrDirectory(playerPackage + "/Tools/native_link.xml", stagingAreaData + "/platform_native_link.xml");
			IL2CPPUtils.RunIl2Cpp(stagingArea, stagingAreaData, platformProvider, null, usedClassRegistry);
			string text2 = Path.Combine(stagingAreaData, "AOTCompileStep");
			Directory.CreateDirectory(text2);
			if (!ProcessNativePlugins(target, stagingAreaData, text2, playerPackage))
			{
				return false;
			}
			if (scriptOnly)
			{
				string text3 = (((options & BuildOptions.InstallInBuildFolder) == 0) ? installPath : Path.Combine(Unsupported.GetBaseUnityDeveloperFolder(), "build/PSP2Player"));
				FileUtil.DeleteFileOrDirectory(stagingArea + "/Data/Modules/" + s_Il2cppAssembliesSUPRX);
				FileUtil.CopyFileOrDirectory("Temp/StagingArea/Data/Native/" + s_Il2cppAssembliesSUPRX, stagingArea + "/Data/Modules/" + s_Il2cppAssembliesSUPRX);
				FileUtil.DeleteFileOrDirectory(text3 + "/Media/Modules");
				FileUtil.DeleteFileOrDirectory(text3 + "/Media/Managed");
				FileUtil.DeleteFileOrDirectory(text3 + "/Media/Metadata");
				FileUtil.DeleteFileOrDirectory(text3 + "/Media/Plugins");
				FileUtil.MoveFileOrDirectory(stagingArea + "/Data/Modules/", text3 + "/Media/Modules");
				FileUtil.MoveFileOrDirectory(stagingArea + "/Data/Managed/", text3 + "/Media/Managed");
				FileUtil.MoveFileOrDirectory(stagingArea + "/Data/Plugins/", text3 + "/Media/Plugins");
				FileUtil.MoveFileOrDirectory(stagingArea + "/il2cppOutput/Data/Metadata/", text3 + "/Media/Metadata");
			}
			else
			{
				string destinationFolder = Path.Combine(stagingAreaData, "Resources");
				IL2CPPUtils.CopyEmbeddedResourceFiles(stagingArea, destinationFolder);
				string text4 = Path.Combine(stagingAreaData, "Metadata");
				FileUtil.CreateOrCleanDirectory(text4);
				IL2CPPUtils.CopyMetadataFiles(stagingArea, text4);
			}
			return true;
		}

		public static void PostProcessScriptsOnly(ScriptingImplementation scriptingBackend, BuildTarget target, BuildOptions options, string installPath, string stagingAreaData, string stagingArea, string playerPackage, string stagingAreaDataManaged, RuntimeClassRegistry usedClassRegistry)
		{
			Console.WriteLine(">>> PostProcessScriptsOnly");
			VitaSDKTools.PSP2KillRunningProcesses();
			Thread.Sleep(1000);
			string text = sfxParams.Get("CONTENT_ID", PlayerSettings.PSVita.contentID);
			string titleID = text.Substring(7, 9);
			string serviceID = text.Substring(0, 19);
			string npCommunicationsID = PlayerSettings.PSVita.npCommunicationsID;
			CreateApplicationInfoCPP(stagingAreaData, titleID, npCommunicationsID, serviceID);
			if (scriptingBackend == ScriptingImplementation.Mono2x)
			{
				if ((options & BuildOptions.InstallInBuildFolder) != 0)
				{
					installPath = Path.Combine(Unsupported.GetBaseUnityDeveloperFolder(), "build/PSP2Player");
				}
				if (!BuildMonoScript(target, options, installPath, stagingAreaData, stagingArea, playerPackage, stagingAreaDataManaged, usedClassRegistry, scriptOnly: true))
				{
					return;
				}
			}
			else if (!BuildIL2CPPScript(target, options, installPath, stagingAreaData, stagingArea, playerPackage, stagingAreaDataManaged, usedClassRegistry, scriptOnly: true))
			{
				return;
			}
			bool flag = PSP2BuildSubtarget.PCHosted == EditorUserBuildSettings.psp2BuildSubtarget;
			bool flag2 = (options & BuildOptions.AutoRunPlayer) != 0;
			string installName = FileUtil.UnityGetFileNameWithoutExtension(installPath) + ".self";
			if (flag2 && flag)
			{
				VitaSDKTools.RunApp(installPath, installName);
			}
		}

		private static void CopyModules(string stagingAreaData, string playerPackage)
		{
			CreateDirectory(stagingAreaData + "/Modules");
			if (s_ScriptingBackend == ScriptingImplementation.Mono2x)
			{
				FileUtil.CopyFileOrDirectory(playerPackage + "/Data/Modules/mono-vita.suprx", stagingAreaData + "/Modules/mono-vita.suprx");
				FileUtil.CopyFileOrDirectory(playerPackage + "/Data/Modules/SUPRXManager.suprx", stagingAreaData + "/Modules/SUPRXManager.suprx");
				if (!Directory.Exists(stagingAreaData + "/Managed/mono/2.0"))
				{
					Directory.CreateDirectory(stagingAreaData + "/Managed/mono/2.0");
				}
				FileUtil.CopyFileOrDirectory(playerPackage + "/machine.config", stagingAreaData + "/Managed/mono/2.0/machine.config");
				if (!Directory.Exists(stagingAreaData + "/Managed/mono/4.0"))
				{
					Directory.CreateDirectory(stagingAreaData + "/Managed/mono/4.0");
				}
				FileUtil.CopyFileOrDirectory(playerPackage + "/machine.config", stagingAreaData + "/Managed/mono/4.0/machine.config");
			}
			FileUtil.CopyFileOrDirectory(playerPackage + "/Data/Modules/pthread.suprx", stagingAreaData + "/Modules/pthread.suprx");
		}

		private static void MoveStagingFolder(string buildsFolder, string stagingArea, string stagingAreaData, bool scriptOnly = false)
		{
			string text = Path.Combine(buildsFolder, "Media");
			string path = Path.Combine(stagingAreaData, "AOTCompileStep");
			string path2 = Path.Combine(stagingAreaData, "MiscCPP");
			FileUtil.DeleteFileOrDirectory(path);
			FileUtil.DeleteFileOrDirectory(path2);
			bool flag = false;
			while (!flag)
			{
				try
				{
					EditorUtility.DisplayProgressBar("Building Player", "create build", 0.95f);
					FileUtil.DeleteFileOrDirectory(text);
					FileUtil.MoveFileOrDirectory(stagingAreaData, text);
					flag = true;
				}
				catch (Exception)
				{
					switch (EditorUtility.DisplayDialogComplex("PSP2 Build : Unable to create build", text + " may be locked by an open explorer window or command prompt", "Retry", "Abort", "Kill"))
					{
					case 1:
						throw;
					case 2:
						EditorUtility.DisplayProgressBar("Building Player", "Killing processes", 0.95f);
						VitaSDKTools.PSP2KillRunningProcesses();
						Thread.Sleep(1000);
						break;
					}
				}
			}
		}

		public static string PrepareForBuild(BuildOptions options, BuildTarget target)
		{
			bool pcHosted = PSP2BuildSubtarget.PCHosted == EditorUserBuildSettings.psp2BuildSubtarget;
			bool autoRunPlayer = (options & BuildOptions.AutoRunPlayer) != 0;
			if (!ValidateBuildEnvironment(autoRunPlayer, pcHosted))
			{
				return "There was a problem with the PS Vita build environment";
			}
			if (PlayerSettings.colorSpace == ColorSpace.Linear)
			{
				return "Linear color space is not supported on PS Vita, please set the color space to Gamma";
			}
			return null;
		}

		private static void WriteIl2cppDebugSolution(string installPath, string stagingArea)
		{
			string sourcePath = Path.Combine(stagingArea, "il2cppOutput");
			string solutionPath = Path.Combine(installPath, s_Il2cppDebugFolder);
			string text = FileUtil.UnityGetFileNameWithoutExtension(installPath) + ".self";
			VSTools.WriteVCSolution(VSTools.VSPlatform.PSVita, sourcePath, solutionPath, s_Il2cppDebugProjectName, installPath + "\\SymbolFiles\\" + text, installPath);
		}

		public static void PostProcess(BuildTarget target, BuildOptions options, string installPath, string stagingAreaData, string stagingArea, string playerPackage, string stagingAreaDataManaged, RuntimeClassRegistry usedClassRegistry)
		{
			bool flag = true;
			s_ScriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.PSP2);
			bool flag2 = PSP2BuildSubtarget.PCHosted == EditorUserBuildSettings.psp2BuildSubtarget;
			bool flag3 = true;
			bool flag4 = (options & BuildOptions.AutoRunPlayer) != 0;
			bool flag5 = (options & BuildOptions.BuildScriptsOnly) != 0;
			bool flag6 = (options & BuildOptions.InstallInBuildFolder) != 0;
			bool flag7 = (options & BuildOptions.Development) != 0;
			bool flag8 = EditorUserBuildSettings.needSubmissionMaterials && !flag7;
			string text = FileUtil.UnityGetFileNameWithoutExtension(installPath) + ".self";
			Console.WriteLine("POSTPROCESS>>> installName: {0}, installPath: {1}, UnityGetFileNameWithoutExtension: {2}", text.ToString(), installPath.ToString(), FileUtil.UnityGetFileNameWithoutExtension(installPath));
			if (InternalEditorUtility.inBatchMode)
			{
				flag = false;
			}
			if (flag5)
			{
				PostProcessScriptsOnly(s_ScriptingBackend, target, options, installPath, stagingAreaData, stagingArea, playerPackage, stagingAreaDataManaged, usedClassRegistry);
				return;
			}
			BuildFolderStatus buildFolderStatus = CheckContentsOfBuildFolder(installPath, flag2, flag8);
			if (flag && buildFolderStatus == BuildFolderStatus.BuildFolderContainsUnknownFiles)
			{
				Console.WriteLine("### " + buildFolderStatus);
				EditorUtility.DisplayDialog("PSVita Build : Invalid Build Folder", installPath + " contains unexpected files!!\n\nThe build will be aborted.\n\nPlease select a valid builds folder or manually delete the files in this folder and try again.", "OK");
				string[] files = Directory.GetFiles(installPath, "*", SearchOption.TopDirectoryOnly);
				if (files.Length > 0)
				{
					EditorUtility.RevealInFinder(files[0]);
				}
				else
				{
					string[] directories = Directory.GetDirectories(installPath, "*", SearchOption.TopDirectoryOnly);
					if (directories.Length > 0)
					{
						EditorUtility.RevealInFinder(directories[0]);
					}
				}
				UnityEngine.Debug.LogError("Build Failed: The target folder contains un-expected files.\nTo avoid deleting files that you may want to keep the build was aborted, please cleanup the build folder or select a different one.");
				return;
			}
			PostprocessBuildPlayer.InstallStreamingAssets(stagingAreaData);
			CreateDirectory(stagingAreaData + "/Resources");
			FileUtil.CopyFileOrDirectory(playerPackage + "/Data/Resources/unity default resources", stagingAreaData + "/Resources/unity default resources");
			string[] files2 = new string[2] { "icon0.png", "pic0.png" };
			string text2 = Path.Combine(stagingArea, "sce_sys");
			string defaultPath = Path.Combine(playerPackage, "sce_sys");
			MoveFilesWithDefault(text2, stagingArea, defaultPath, files2);
			string path = Path.Combine(stagingArea, "savedata");
			CreateDirectory(path);
			files2 = new string[1] { "configuration.psp2path" };
			CopyFiles(stagingArea, playerPackage, files2);
			string path2 = Path.Combine(stagingArea, "savedata/persistentdata");
			CreateDirectory(path2);
			bool flag9 = false;
			string text3 = Path.Combine(stagingArea, "sce_sys/manual");
			if (PlayerSettings.PSVita.manualPath.Length > 0)
			{
				if (CopyDirectoryWithFilter(text3, PlayerSettings.PSVita.manualPath, "*.png"))
				{
					flag9 = true;
				}
				else
				{
					UnityEngine.Debug.LogWarning("Software Manual source folder does not exist.");
				}
			}
			string environmentVariable = Environment.GetEnvironmentVariable("SCE_PSP2_SDK_DIR");
			string paramSfxPath = PlayerSettings.PSVita.paramSfxPath;
			sfxParams.Clear();
			if (File.Exists(paramSfxPath))
			{
				Console.WriteLine("### Setting package params from file: " + paramSfxPath);
				sfxParams.Read(paramSfxPath);
			}
			else
			{
				Console.WriteLine("### Setting package params from player settings");
				string[] array = new string[3] { "gd", "gp", "ac" };
				sfxParams.Set("CATEGORY", array[(int)PlayerSettings.PSVita.category]);
				sfxParams.Set("VERSION", PlayerSettings.PSVita.masterVersion);
				sfxParams.Set("APP_VER", PlayerSettings.PSVita.appVersion);
				sfxParams.Set("CONTENT_ID", PlayerSettings.PSVita.contentID);
				if (VitaSDKTools.ValidateContentID(PlayerSettings.PSVita.contentID))
				{
					sfxParams.Set("TITLE_ID", PlayerSettings.PSVita.contentID.Substring(7, 9));
				}
				sfxParams.Set("TITLE", PlayerSettings.productName);
				sfxParams.Set("STITLE", PlayerSettings.PSVita.shortTitle);
				sfxParams.SetInt("SAVEDATA_MAX_SIZE", PlayerSettings.PSVita.saveDataQuota);
				sfxParams.SetInt("PARENTAL_LEVEL", PlayerSettings.PSVita.parentalLevel);
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int[] array2 = new int[3]
				{
					ParamsAttributeMinor.kTv_Default,
					ParamsAttributeMinor.kTv_VitaBootable_TvBootable,
					ParamsAttributeMinor.kTv_VitaBootable_TvNotBootable
				};
				num &= ~ParamsAttributeMinor.kTv_Mask;
				num |= array2[(int)PlayerSettings.PSVita.tvBootMode];
				int[] array3 = new int[3]
				{
					ParamsAttributeMinor.kEnterButtonAssignment_Default,
					ParamsAttributeMinor.kEnterButtonAssignment_CircleButton,
					ParamsAttributeMinor.kEnterButtonAssignment_CrossButton
				};
				num &= ~ParamsAttributeMinor.kEnterButtonAssignment_Mask;
				num |= array3[(int)PlayerSettings.PSVita.enterButtonAssignment];
				num2 &= ~ParamsAttribute.kTv_DisableTouchEmu;
				num2 |= (PlayerSettings.PSVita.tvDisableEmu ? ParamsAttribute.kTv_DisableTouchEmu : 0);
				num2 &= ~ParamsAttribute.kUpgradable;
				num2 |= (PlayerSettings.PSVita.upgradable ? ParamsAttribute.kUpgradable : 0);
				num2 &= ~ParamsAttribute.kHealthWarning;
				num2 |= (PlayerSettings.PSVita.healthWarning ? ParamsAttribute.kHealthWarning : 0);
				num2 &= ~ParamsAttribute.kInfoBar;
				num2 |= (PlayerSettings.PSVita.infoBarOnStartup ? ParamsAttribute.kInfoBar : 0);
				num2 &= ~ParamsAttribute.kInfoWhite;
				num2 |= (PlayerSettings.PSVita.infoBarColor ? ParamsAttribute.kInfoWhite : 0);
				int[] array4 = new int[4]
				{
					ParamsAttribute2.kMemExpansionNone,
					ParamsAttribute2.kMemExpansion29MiB,
					ParamsAttribute2.kMemExpansion77MiB,
					ParamsAttribute2.kMemExpansion109MiB
				};
				num3 &= ~ParamsAttribute2.kMemExpansion_Mask;
				num3 |= array4[(int)PlayerSettings.PSVita.memoryExpansionMode];
				sfxParams.SetInt("ATTRIBUTE", num2);
				sfxParams.SetInt("ATTRIBUTE_MINOR", num);
				sfxParams.SetInt("ATTRIBUTE2", num3);
			}
			if (!ParamSfxVerifyAndSetDefaults(flag2))
			{
				return;
			}
			string text4 = sfxParams.Get("CONTENT_ID", PlayerSettings.PSVita.contentID);
			string text5 = text4.Substring(7, 9);
			string serviceID = text4.Substring(0, 19);
			if (!VitaSDKTools.ValidateContentID(text4))
			{
				UnityEngine.Debug.LogError("Build Failed: Content ID is invalid, content ID should be 36 characters in the form IV0000-ABCD12345_00-0123456789ABCDEF");
				return;
			}
			if (!VitaSDKTools.ValidateTitleID(text5))
			{
				UnityEngine.Debug.LogError("Build Failed: Title ID is invalid, title ID should be 9 characters in the form ABCD12345");
				return;
			}
			string text6 = PlayerSettings.PSVita.npCommunicationsID;
			Console.WriteLine("###  NP communications ID (from player settings): " + text6);
			if (PlayerSettings.PSVita.npSupportGBMorGJP)
			{
				if (!VitaSDKTools.ValidateNPCommsID(text6))
				{
					if (!InternalEditorUtility.inBatchMode)
					{
						UnityEngine.Debug.LogError("NP communications ID is not set or is invalid.\nRequired for Playstation(R)Network, trophies, etc. Also requires a matching NP Signature and NP Pass-phrase");
					}
					text6 = VitaSDKTools.CreateDummyNPCommsID();
				}
				sfxParams.Set("NP_COMMUNICATION_ID", text6);
			}
			else
			{
				sfxParams.Remove("NP_COMMUNICATION_ID");
				if (!VitaSDKTools.ValidateNPCommsID(text6))
				{
					if (!InternalEditorUtility.inBatchMode)
					{
						UnityEngine.Debug.LogWarning("NP communications ID is not set or is invalid.\nRequired for Playstation(R)Network, trophies, etc. Also requires a matching NP Signature and NP Pass-phrase");
					}
					text6 = VitaSDKTools.CreateDummyNPCommsID();
				}
			}
			string text7 = "Temp/params.sfx";
			sfxParams.Write(text7);
			string text8 = sfxParams.Get("CATEGORY", "gd");
			PackageType packageType;
			if (text8 == "gd")
			{
				packageType = PackageType.kApplication;
			}
			else
			{
				if (!(text8 == "gp"))
				{
					packageType = PackageType.kAdditionalContent;
					UnityEngine.Debug.LogError("Build Failed: Creating 'additional content' packages is not yet supported!\n");
					return;
				}
				packageType = PackageType.kPatch;
			}
			if (flag7)
			{
				CreateRazorHUDSettingsFile(stagingArea);
			}
			string text9 = Path.Combine(stagingArea, "sce_sys/livearea/contents");
			string text10 = Path.Combine(stagingArea, "sce_sys/retail/livearea/contents");
			string destPath = Path.Combine(stagingArea, "sce_sys/changeinfo");
			bool flag10 = false;
			if (PlayerSettings.PSVita.liveAreaPath.Length > 0)
			{
				if (PlayerSettings.PSVita.upgradable)
				{
					if (CopyDirectoryWithFilter(text10, PlayerSettings.PSVita.liveAreaPath, "*.xml;*.png"))
					{
						flag10 = true;
					}
					else
					{
						UnityEngine.Debug.LogWarning("Live Area source folder does not exist.");
					}
					if (!CopyDirectoryWithFilter(text9, PlayerSettings.PSVita.liveAreaTrialPath, "*.xml;*.png"))
					{
						UnityEngine.Debug.LogError("Build Failed: Live Area Trial source folder is unspecified or does not exist, this path must be valid for upgradable applications.\n");
						return;
					}
				}
				else if (CopyDirectoryWithFilter(text9, PlayerSettings.PSVita.liveAreaPath, "*.xml;*.png"))
				{
					flag10 = true;
				}
				else
				{
					UnityEngine.Debug.LogWarning("Live Area source folder does not exist.");
				}
			}
			if (!flag10)
			{
				if ((sfxParams.GetInt("ATTRIBUTE", 0) & ParamsAttribute.kUpgradable) != 0)
				{
					files2 = new string[3] { "bg0.png", "default_gate.png", "template.xml" };
					string path3 = Path.Combine(playerPackage, "sce_sys/livearea/contents");
					CopyFileWithDefault(Path.Combine(text10, "bg0.png"), PlayerSettings.PSVita.liveAreaBackroundPath, Path.Combine(path3, "bg0.png"));
					CopyFileWithDefault(Path.Combine(text10, "default_gate.png"), PlayerSettings.PSVita.liveAreaGatePath, Path.Combine(path3, "default_gate.png"));
					CopyFile(Path.Combine(text10, "template.xml"), Path.Combine(path3, "template.xml"));
					if (!CopyDirectoryWithFilter(text9, PlayerSettings.PSVita.liveAreaTrialPath, "*.xml;*.png"))
					{
						UnityEngine.Debug.LogError("Build Failed: Live Area Trial source folder is unspecified or does not exist, this path must be valid for upgradable applications.\n");
						return;
					}
				}
				else
				{
					files2 = new string[3] { "bg0.png", "default_gate.png", "template.xml" };
					string path4 = Path.Combine(playerPackage, "sce_sys/livearea/contents");
					CopyFileWithDefault(Path.Combine(text9, "bg0.png"), PlayerSettings.PSVita.liveAreaBackroundPath, Path.Combine(path4, "bg0.png"));
					CopyFileWithDefault(Path.Combine(text9, "default_gate.png"), PlayerSettings.PSVita.liveAreaGatePath, Path.Combine(path4, "default_gate.png"));
					CopyFile(Path.Combine(text9, "template.xml"), Path.Combine(path4, "template.xml"));
				}
			}
			if (packageType == PackageType.kPatch)
			{
				if (PlayerSettings.PSVita.patchChangeInfoPath.Length <= 0)
				{
					UnityEngine.Debug.LogError("Build Failed: Trying to build a patch package but the change info folder has not been specified.");
					return;
				}
				if (!CopyDirectoryWithFilter(destPath, PlayerSettings.PSVita.patchChangeInfoPath, "*.xml"))
				{
					UnityEngine.Debug.LogError("Build Failed: Failed to find/copy change info directory for patch package.");
					return;
				}
			}
			paramSfxPath = text7;
			string text11 = Path.Combine(text2, "param.sfo");
			string text12 = Path.Combine(text2, "keystone");
			string text13 = Path.Combine(text2, "nptitle.dat");
			paramSfxPath = paramSfxPath.Replace("/", "\\");
			text11 = text11.Replace("/", "\\");
			text12 = text12.Replace("/", "\\");
			text13 = text13.Replace("/", "\\");
			bool flag11 = false;
			bool flag12 = false;
			string text14 = Path.Combine(Path.Combine(stagingArea, "sce_sys/trophy"), text6);
			string text15 = text6;
			if (VitaSDKTools.HasPublishTools())
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = VitaSDKTools.GetTool("psp2pubcmd");
				processStartInfo.UseShellExecute = false;
				processStartInfo.CreateNoWindow = true;
				processStartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
				processStartInfo.RedirectStandardError = true;
				VitaSDKTools.RunCommand(processStartInfo, $"-sc \"{paramSfxPath}\" \"{text11}\"");
				if (!File.Exists(text11))
				{
					throw new Exception("Failed creating param.sfo!");
				}
				Console.WriteLine("### Created params.sfo...");
				Console.WriteLine("###  read from: " + paramSfxPath);
				Console.WriteLine("###  copied to: " + text11);
				if (PlayerSettings.PSVita.npTrophyPackPath.Length > 0)
				{
					VitaSDKTools.VerifyTrophyFile(PlayerSettings.PSVita.npTrophyPackPath);
					text15 = VitaSDKTools.GetTrophyInfo("npID");
					if (text15 != null)
					{
						if (text15 != text6)
						{
							UnityEngine.Debug.LogWarning("The Trophy file's NP Communications ID is different to the NP Communications ID in the player settings, this is OK if the intention is to have cross-platform trophies.\nTrophy File = " + text15 + " , Player Settings = " + text6);
						}
						text14 = Path.Combine(Path.Combine(stagingArea, "sce_sys/trophy"), text15);
					}
					else
					{
						UnityEngine.Debug.LogWarning("Unable to retrieve NP comms ID from trophy file " + PlayerSettings.PSVita.npTrophyPackPath);
						text15 = text6;
					}
					if (VitaSDKTools.ValidateNPCommsID(text15))
					{
						CreateDirectory(text14);
						FileUtil.CopyFileOrDirectory(PlayerSettings.PSVita.npTrophyPackPath, text14 + "/TROPHY.TRP");
						flag11 = true;
					}
					else
					{
						string text16 = "Trophy NP Communications ID '" + text15 + "' is not valid\n\nThe ID must be 12 characters long and in the form 'ABCD12345_00'.\nTrophy package will be excluded from the build.";
						UnityEngine.Debug.LogWarning("Failed to build trophy package.\n" + text16);
						EditorUtility.DisplayDialog("Invalid Trophy NP Communications ID", text16, "OK");
					}
				}
			}
			if (PlayerSettings.PSVita.npTitleDatPath.Length > 0)
			{
				FileUtil.CopyFileOrDirectory(PlayerSettings.PSVita.npTitleDatPath, text2 + "/nptitle.dat");
				flag12 = true;
			}
			if (PlayerSettings.PSVita.keystoneFile.Length > 0)
			{
				FileUtil.CopyFileOrDirectory(PlayerSettings.PSVita.keystoneFile, text2 + "/keystone");
			}
			CreateApplicationInfoCPP(stagingAreaData, text5, text6, serviceID);
			if (flag2)
			{
				EditorUtility.DisplayProgressBar("Building Player", "Resetting target", 0f);
				VitaSDKTools.RestartPSP2(reset: true);
			}
			string text17 = Path.Combine(stagingAreaData, "AOTCompileStep");
			string path5 = Path.Combine(stagingAreaData, "MiscCPP");
			if (s_ScriptingBackend == ScriptingImplementation.Mono2x)
			{
				if (!BuildMonoScript(target, options, installPath, stagingAreaData, stagingArea, playerPackage, stagingAreaDataManaged, usedClassRegistry))
				{
					return;
				}
			}
			else if (!BuildIL2CPPScript(target, options, installPath, stagingAreaData, stagingArea, playerPackage, stagingAreaDataManaged, usedClassRegistry))
			{
				return;
			}
			if (EditorUserBuildSettings.compressWithPsArc)
			{
				EditorUtility.DisplayProgressBar("Building Player", "Creating Archive", 0.96f);
				ArchiveFiles(stagingArea);
			}
			if (flag6)
			{
				string text18 = Path.Combine(Unsupported.GetBaseUnityDeveloperFolder(), "build/PSP2Player");
				string text19 = Path.Combine(stagingArea, "hud_settings.ini");
				Console.WriteLine("PostProcess: buildsFolder={0}", text18);
				Console.WriteLine("PostProcess: aotCompileStepFolder={0}", text17);
				if (s_ScriptingBackend == ScriptingImplementation.Mono2x)
				{
					FileUtil.MoveFileOrDirectory(Path.Combine(text17, s_MonoAssembliesSUPRX), stagingAreaData + "/Modules/" + s_MonoAssembliesSUPRX);
					FileUtil.DeleteFileOrDirectory(Path.Combine(text18, "Data/Modules/MonoAssembliesPSP2_stub_weak.a"));
					FileUtil.MoveFileOrDirectory(Path.Combine(text17, "MonoAssembliesPSP2_stub_weak.a"), Path.Combine(text18, "Data/Modules/MonoAssembliesPSP2_stub_weak.a"));
				}
				else
				{
					string path6 = stagingAreaData + "/Modules";
					FileUtil.DeleteFileOrDirectory(Path.Combine(path6, s_Il2cppAssembliesSUPRX));
					FileUtil.MoveFileOrDirectory("Temp/StagingArea/Data/Native/" + s_Il2cppAssembliesSUPRX, Path.Combine(path6, s_Il2cppAssembliesSUPRX));
					if (File.Exists("Temp/StagingArea/Data/Native/Data/" + s_Il2cppSymbolMap))
					{
						FileUtil.DeleteFileOrDirectory(Path.Combine(stagingAreaData, s_Il2cppSymbolMap));
						FileUtil.MoveFileOrDirectory("Temp/StagingArea/Data/Native/Data/" + s_Il2cppSymbolMap, Path.Combine(stagingAreaData, s_Il2cppSymbolMap));
					}
				}
				MoveStagingFolder(text18, stagingArea, stagingAreaData);
				if (File.Exists(text18 + "\\archive.psarc"))
				{
					FileUtil.DeleteFileOrDirectory(text18 + "\\archive.psarc");
				}
				if (EditorUserBuildSettings.compressWithPsArc)
				{
					FileUtil.CopyFileOrDirectory(stagingArea + "\\archive.psarc", text18 + "\\archive.psarc");
				}
				if (flag9)
				{
					string text20 = Path.Combine(text18, "sce_sys/manual");
					FileUtil.DeleteFileOrDirectory(text20);
					CopyDirectoryWithFilter(text20, PlayerSettings.PSVita.manualPath, "*.png");
				}
				if (flag11)
				{
					string path7 = Path.Combine(text18, "sce_sys/trophy");
					path7 = Path.Combine(path7, text15);
					FileUtil.DeleteFileOrDirectory(path7);
					Directory.CreateDirectory(path7);
					FileUtil.CopyFileOrDirectory(PlayerSettings.PSVita.npTrophyPackPath, path7 + "/TROPHY.TRP");
				}
				string text21 = Path.Combine(text18, "sce_sys");
				FileUtil.DeleteFileOrDirectory(text21 + "/param.sfo");
				if (File.Exists(text11))
				{
					FileUtil.ReplaceFile(text11, text21 + "/param.sfo");
				}
				if (File.Exists(text12))
				{
					FileUtil.ReplaceFile(text12, text21 + "/keystone");
				}
				if (File.Exists(text13))
				{
					FileUtil.ReplaceFile(text13, text21 + "/nptitle.dat");
				}
				if (File.Exists(text19))
				{
					FileUtil.ReplaceFile(text19, text18 + "/hud_settings.ini");
				}
				return;
			}
			string text22;
			string text23;
			string path8;
			if (s_ScriptingBackend == ScriptingImplementation.Mono2x)
			{
				text22 = Path.Combine(playerPackage, (!flag7) ? "PSP2Player_mono.self" : "PSP2PlayerDevelopment_mono.self");
				text23 = s_MonoAssembliesSUPRX;
				path8 = text17;
			}
			else
			{
				text22 = Path.Combine(playerPackage, (!flag7) ? "PSP2Player_il2cpp.self" : "PSP2PlayerDevelopment_il2cpp.self");
				text23 = s_Il2cppAssembliesSUPRX;
				path8 = text17 + "/../Native/";
			}
			string text24 = Path.Combine(stagingArea, text);
			Console.WriteLine("player:" + text22);
			Console.WriteLine("destExePath:" + text24);
			FileUtil.CopyFileOrDirectory(text22, text24);
			FileUtil.DeleteFileOrDirectory(Path.Combine(stagingArea, "Data/Modules/" + text23));
			FileUtil.MoveFileOrDirectory(Path.Combine(path8, text23), Path.Combine(stagingArea, "Data/Modules/" + text23));
			if (File.Exists("Temp/StagingArea/Data/Native/Data/" + s_Il2cppSymbolMap))
			{
				FileUtil.DeleteFileOrDirectory(Path.Combine(stagingAreaData, s_Il2cppSymbolMap));
				FileUtil.MoveFileOrDirectory("Temp/StagingArea/Data/Native/Data/" + s_Il2cppSymbolMap, Path.Combine(stagingAreaData, s_Il2cppSymbolMap));
			}
			FileUtil.DeleteFileOrDirectory(text17);
			FileUtil.DeleteFileOrDirectory(path5);
			string withWarningAndSetDefault = sfxParams.GetWithWarningAndSetDefault("VERSION", "01.00", "Param File - VERSION not found, using a default value '01.00'", enableWarning: true);
			if (s_ScriptingBackend == ScriptingImplementation.IL2CPP)
			{
				FileUtil.DeleteFileOrDirectory(stagingArea + "/Data/Managed");
				FileUtil.DeleteFileOrDirectory(stagingArea + "/Data/Native");
			}
			if (!flag2)
			{
				if (Directory.Exists(installPath))
				{
					RemovePreviousBuild(installPath, text);
				}
				switch (packageType)
				{
				case PackageType.kApplication:
					EditorUtility.DisplayProgressBar("Building Player", (!flag8) ? "Creating application package" : "Creating application package and submission materials", 0.97f);
					break;
				case PackageType.kPatch:
					EditorUtility.DisplayProgressBar("Building Player", (!flag8) ? "Creating patch package" : "Creating patch package and submission materials", 0.97f);
					break;
				}
				if (!File.Exists(paramSfxPath))
				{
					UnityEngine.Debug.LogError("No PARAM.SFX provided! See 'Publishing Settings'");
					return;
				}
				ProcessStartInfo processStartInfo2 = new ProcessStartInfo();
				processStartInfo2.FileName = VitaSDKTools.GetTool("psp2pubcmd");
				processStartInfo2.UseShellExecute = false;
				processStartInfo2.CreateNoWindow = true;
				processStartInfo2.WorkingDirectory = Directory.GetCurrentDirectory();
				processStartInfo2.RedirectStandardError = true;
				string arg = Path.Combine(text2, "icon0.png");
				string arg2 = Path.Combine(text2, "pic0.png");
				string sceModulePath = Path.Combine(environmentVariable, "target/sce_module");
				string text25 = Path.Combine(stagingArea, "archive.psarc");
				string project_gp4p = Path.Combine(Path.Combine(stagingArea, "PackageSource"), "project.gp4p");
				string text26 = Path.Combine(stagingArea, text);
				string text27;
				switch (PlayerSettings.PSVita.drmType)
				{
				case PlayerSettings.PSVita.PSVitaDRMType.PaidFor:
					text27 = "Local";
					break;
				case PlayerSettings.PSVita.PSVitaDRMType.Free:
					text27 = "Free";
					break;
				default:
					throw new Exception("Invalid DRM Type!\n");
				}
				Console.WriteLine("### DRM Type: " + text27);
				string[] array5 = new string[9] { "no_media", "gc2_14_05", "gc2_16_03", "gc2_18_01", "gc4_28_10", "gc4_32_06", "gc4_36_02", "gc2_gcrm", "gc4_gcrm" };
				string text28 = array5[PlayerSettings.PSVita.mediaCapacity];
				Console.WriteLine("### Media capacity: " + text28);
				string packagePassword = PlayerSettings.PSVita.packagePassword;
				string arg3 = Path.Combine(stagingArea, "Data");
				string arg4 = Path.Combine(stagingArea, "savedata");
				string arg5 = Path.Combine(text14, "TROPHY.TRP");
				string arg6 = Path.Combine(stagingArea, "hud_settings.ini");
				if (!File.Exists(text11))
				{
					UnityEngine.Debug.LogError("Build Failed: Creation of param.sfo failed during package build, please specify the location of the param.sfx file in Player Settings - Package Parameters!\n");
					return;
				}
				switch (packageType)
				{
				case PackageType.kPatch:
				{
					string text30 = Path.Combine(installPath, text4) + ".pkg";
					if (PlayerSettings.PSVita.patchOriginalPackage.Length > 0)
					{
						if (!File.Exists(PlayerSettings.PSVita.patchOriginalPackage))
						{
							UnityEngine.Debug.LogError("Build Failed: Trying to create a patch package but the path for the original package has not been specified.\n");
							return;
						}
						string text31 = Path.GetFullPath(installPath).Replace('\\', '/');
						string text32 = Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(PlayerSettings.PSVita.patchOriginalPackage)).Replace('\\', '/');
						Console.WriteLine("### Original package: " + PlayerSettings.PSVita.patchOriginalPackage);
						Console.WriteLine("### Original package path: " + text32);
						Console.WriteLine("### Patch package path: " + text31);
						if (string.Compare(text31, text32, ignoreCase: true) == 0)
						{
							UnityEngine.Debug.LogError("Build Failed: The target folder for the patch package is the same as the original package!\n");
							return;
						}
						if (File.Exists(text30))
						{
							File.Delete(text30);
						}
						string text33 = "";
						string text34 = Path.ChangeExtension(PlayerSettings.PSVita.patchOriginalPackage, ".source.zip");
						bool flag13 = false;
						if (File.Exists(text34))
						{
							Console.WriteLine("### Extracting package source zip: " + text34);
							text33 = stagingArea + "/Patch_OriginalFiles";
							CreateDirectory(text33);
							flag13 = VitaSDKTools.UnZip(text34, text33);
							if (flag13 && File.Exists(Path.Combine(text33, "project.gp4p")))
							{
								text33 += "/Files";
							}
						}
						if (!flag13)
						{
							UnityEngine.Debug.LogWarning("The original package zip \"" + text34 + "\" could not be found, extracting package instead.\nThis will cause all native modules (.SUPRX files) to be included in the patch even if they have not changed.");
							text33 = stagingArea + "/Patch_OriginalFiles";
							CreateDirectory(text33);
							VitaSDKTools.ExtractPackage(PlayerSettings.PSVita.patchOriginalPackage, packagePassword, text33);
						}
						if (text33.Length == 0)
						{
							UnityEngine.Debug.LogError("Build failed: Unable to extract original package files\n");
							return;
						}
						string text35 = text33 + "/Media";
						string text36 = stagingArea + "/Data";
						string moduleSDKVersion = VitaSDKTools.GetModuleSDKVersion(text33 + "/eboot.bin");
						string moduleSDKVersion2 = VitaSDKTools.GetModuleSDKVersion(text26);
						if (moduleSDKVersion != moduleSDKVersion2)
						{
							CreateDummyModulesForPatch(text35 + "/Modules", text36 + "/Modules", stagingAreaData);
						}
						List<string> list = VitaSDKTools.CompareDirectoriesForPatch(text35, text36, null, log: true);
						string text37 = stagingArea + "/Patch_NewFiles/Media";
						CreateDirectory(text37);
						for (int i = 0; i < list.Count; i++)
						{
							string text38 = list[i];
							string text39 = text38.Replace(text36, text37);
							string directoryName = Path.GetDirectoryName(text39);
							Directory.CreateDirectory(directoryName);
							File.Copy(text38, text39);
							list[i] = text39;
						}
						string oldPath = text33 + "/sce_sys";
						string text40 = stagingArea + "/sce_sys";
						List<string> list2 = new List<string>();
						list2.Add("livearea/");
						list2.Add("retail/");
						list2.Add("keystone");
						List<string> list3 = VitaSDKTools.CompareDirectoriesForPatch(oldPath, text40, list2, log: true);
						foreach (string item in list)
						{
							if (!item.Contains(".suprx") || !VitaSDKTools.ModuleReferencesNpWebAPI(item))
							{
								continue;
							}
							if (!StringListContainsCaseInd(list3, "nptitle.dat"))
							{
								string text41 = text40 + "/nptitle.dat";
								if (File.Exists(text41))
								{
									UnityEngine.Debug.Log("A patched module uses the NP Web API, adding " + text40 + "/nptitle.dat\n");
									list3.Add(text41);
								}
								else
								{
									UnityEngine.Debug.LogError("A patched module uses the NP Web API but the nptitle.dat file could not be found\n");
								}
							}
							break;
						}
						string text42 = stagingArea + "/Patch_NewFiles/sce_sys";
						CreateDirectory(text42);
						for (int j = 0; j < list3.Count; j++)
						{
							string text43 = list3[j];
							string text44 = text43.Replace(text40, text42);
							string directoryName2 = Path.GetDirectoryName(text44);
							Directory.CreateDirectory(directoryName2);
							File.Copy(text43, text44);
							list3[j] = text44;
						}
						string text45 = "archive.psarc";
						bool flag14 = false;
						if (File.Exists(text25))
						{
							string text46 = text33 + "/archive.psarc";
							if (File.Exists(text46))
							{
								Console.WriteLine("### Comparing archives: " + text46 + " -> " + text25);
								if (VitaSDKTools.CompareFileForPatch(text46, text25, log: true))
								{
									string text47 = stagingArea + "/Patch_OldArchive";
									CreateDirectory(text47);
									VitaSDKTools.DecompressWithPSArc(text46, text47);
									string text48 = stagingArea + "/Patch_NewArchive";
									CreateDirectory(text48);
									VitaSDKTools.DecompressWithPSArc(text25, text48);
									List<string> list4 = VitaSDKTools.CompareDirectoriesForPatch(text47, text48, null, log: true);
									if (list4.Count > 0)
									{
										string text49 = stagingArea + "/Patch_PatchArchive";
										CreateDirectory(text49);
										for (int k = 0; k < list4.Count; k++)
										{
											string text50 = list4[k];
											string text51 = text50.Replace(text48, text49);
											string directoryName3 = Path.GetDirectoryName(text51);
											Directory.CreateDirectory(directoryName3);
											File.Copy(text50, text51);
										}
										text45 = "archive_patch.psarc";
										text25 = stagingArea + "/" + text45;
										VitaSDKTools.CompressFolderWithPSArc(text49, text25, stagingArea);
										flag14 = true;
									}
								}
							}
							else
							{
								flag14 = true;
							}
						}
						VitaSDKTools.BuildPatchPackageParams buildParams2 = default(VitaSDKTools.BuildPatchPackageParams);
						buildParams2.project_gp4p = project_gp4p;
						buildParams2.content_id = text4;
						buildParams2.masterVersion = withWarningAndSetDefault;
						buildParams2.drm_type = text27;
						buildParams2.passcode = packagePassword;
						buildParams2.patchOriginalPackage = PlayerSettings.PSVita.patchOriginalPackage;
						buildParams2.sceModulePath = sceModulePath;
						buildParams2.needSubmissionMaterials = flag8;
						buildParams2.stagingArea = stagingArea;
						buildParams2.filePairs = new List<string>();
						buildParams2.filePairs.Add($"\"{text26}\" eboot.bin");
						foreach (string item2 in list3)
						{
							string arg7 = item2.Replace(text42 + "/", "");
							buildParams2.filePairs.Add($"\"{item2}\" sce_sys/{arg7}");
						}
						buildParams2.filePairs.Add($"\"{text37}\" Media");
						if (flag14)
						{
							buildParams2.filePairs.Add($"\"{text25}\" {text45}");
						}
						if (!VitaSDKTools.BuildPatchPackage(buildParams2, installPath, text30, createZip: true))
						{
							return;
						}
						break;
					}
					UnityEngine.Debug.LogError("Build Failed: Trying to create a patch package but the original package file does not exist.\n");
					return;
				}
				case PackageType.kApplication:
				{
					string text29 = Path.Combine(installPath, text4) + ".pkg";
					if (File.Exists(text29))
					{
						File.Delete(text29);
					}
					VitaSDKTools.BuildAppPackageParams buildParams = default(VitaSDKTools.BuildAppPackageParams);
					buildParams.project_gp4p = project_gp4p;
					buildParams.content_id = text4;
					buildParams.masterVersion = withWarningAndSetDefault;
					buildParams.drm_type = text27;
					buildParams.capacity = text28;
					buildParams.passcode = packagePassword;
					buildParams.sceModulePath = sceModulePath;
					buildParams.needSubmissionMaterials = flag8;
					buildParams.stagingArea = stagingArea;
					buildParams.filePairs = new List<string>();
					buildParams.filePairs.Add($"\"{text26}\" eboot.bin");
					buildParams.filePairs.Add($"\"{text11}\" sce_sys/param.sfo");
					buildParams.filePairs.Add($"\"{arg}\" sce_sys/icon0.png");
					buildParams.filePairs.Add($"\"{arg2}\" sce_sys/pic0.png");
					buildParams.filePairs.Add($"\"{arg3}\" Media");
					buildParams.filePairs.Add($"\"{arg4}\" savedata");
					if (File.Exists(text25))
					{
						buildParams.filePairs.Add($"\"{text25}\" archive.psarc");
					}
					if (flag7)
					{
						buildParams.filePairs.Add($"\"{arg6}\" hud_settings.ini");
					}
					if (flag11)
					{
						buildParams.filePairs.Add($"\"{arg5}\" sce_sys/trophy/{text15}/TROPHY.TRP");
					}
					if (flag12)
					{
						buildParams.filePairs.Add($"\"{PlayerSettings.PSVita.npTitleDatPath}\" sce_sys/nptitle.dat");
					}
					if (flag9)
					{
						buildParams.filePairs.Add($"\"{text3}\" sce_sys/manual");
					}
					if ((sfxParams.GetInt("ATTRIBUTE", 0) & ParamsAttribute.kUpgradable) != 0)
					{
						buildParams.filePairs.Add($"\"{text10}\" sce_sys/retail/livearea/contents");
						buildParams.filePairs.Add($"\"{text9}\" sce_sys/livearea/contents");
					}
					else
					{
						buildParams.filePairs.Add($"\"{text9}\" sce_sys/livearea/contents");
					}
					if (!VitaSDKTools.BuildAppPackage(buildParams, installPath, text29, createZip: true))
					{
						return;
					}
					break;
				}
				default:
					UnityEngine.Debug.LogError("Build Failed: Creating 'additional content' packages is not yet supported!\n");
					return;
				}
			}
			else
			{
				if (Directory.Exists(installPath))
				{
					RemovePreviousBuild(installPath, text);
				}
				CopyDirectory(installPath + "/Media", stagingArea + "/Data");
				if (flag7 && s_ScriptingBackend == ScriptingImplementation.IL2CPP)
				{
					WriteIl2cppDebugSolution(installPath, stagingArea);
				}
				CopyFiles(installPath, stagingArea, new string[5] { "sce_sys", text, "configuration.psp2path", "archive.psarc", "archive_patch.psarc" }, ignoreMissing: true);
				if (flag7)
				{
					CopyFiles(installPath, stagingArea, new string[1] { "hud_settings.ini" }, ignoreMissing: true);
				}
				if (!Directory.Exists(installPath + "/savedata"))
				{
					CopyDirectory(installPath + "/savedata", stagingArea + "/savedata");
				}
				string text52 = "vita_settings.ini";
				string text53 = Path.Combine(playerPackage, text52);
				if (File.Exists(text53))
				{
					FileUtil.CopyFileOrDirectory(text53, installPath + "/" + text52);
				}
				string environmentVariable2 = Environment.GetEnvironmentVariable("SCE_PSP2_SDK_DIR");
				FileUtil.CopyFileOrDirectory(environmentVariable2 + "/target/sce_module", installPath + "/sce_module");
				string contents = "psp2run /console:all /log:\"" + FileUtil.UnityGetFileNameWithoutExtension(text) + ".log\" /fsroot . /elf \"" + text + "\"";
				string path9 = installPath + "/" + FileUtil.UnityGetFileNameWithoutExtension(text) + ".bat";
				File.WriteAllText(path9, contents);
			}
			if (flag3)
			{
				string text54 = installPath + "/SymbolFiles";
				Console.WriteLine("!!!!! Writing symbol files to " + text54);
				Directory.CreateDirectory(text54);
				File.Copy(Path.Combine(stagingArea, text), Path.Combine(text54, text), overwrite: true);
				string[] files3 = Directory.GetFiles(stagingArea, "*.suprx", SearchOption.AllDirectories);
				string[] array6 = files3;
				foreach (string text55 in array6)
				{
					string text56 = Path.Combine(text54, Path.GetFileName(text55));
					if (!File.Exists(text56))
					{
						try
						{
							File.Copy(text55, text56, overwrite: true);
						}
						catch (Exception)
						{
							UnityEngine.Debug.LogWarning("Failed to copy symbol file " + text55 + " to " + text54);
						}
					}
				}
			}
			if (flag4 && flag2)
			{
				VitaSDKTools.RunApp(installPath, text);
			}
			else if (flag4)
			{
				string packageFile = text4 + ".pkg";
				string text57 = text5;
				int num4 = text57.IndexOf('_');
				if (num4 > 0)
				{
					text57 = text57.Remove(num4);
				}
				if (packageType == PackageType.kPatch)
				{
					EditorUtility.DisplayProgressBar("Running Player", "Installing patch package", 0.98f);
					VitaSDKTools.InstallPackage(installPath, packageFile, text5);
					return;
				}
				EditorUtility.DisplayProgressBar("Running Player", "Uninstalling old package", 0.97f);
				VitaSDKTools.UninstallPackage(installPath, text57);
				EditorUtility.DisplayProgressBar("Running Player", "Installing package", 0.98f);
				VitaSDKTools.InstallPackage(installPath, packageFile, text5);
			}
		}

		private static bool CreateDummyModulesForPatch(string basePath, string patchPath, string stagingAreaData)
		{
			string[] files = Directory.GetFiles(patchPath, "*.suprx", SearchOption.TopDirectoryOnly);
			string[] files2 = Directory.GetFiles(basePath, "*.suprx", SearchOption.TopDirectoryOnly);
			string[] array = files2;
			foreach (string path in array)
			{
				string fileName = Path.GetFileName(path);
				string text = "";
				bool flag = false;
				string[] array2 = files;
				foreach (string path2 in array2)
				{
					text = Path.GetFileName(path2);
					if (text == fileName)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
					if (!CreateDummyModule(fileNameWithoutExtension, patchPath, stagingAreaData))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool CreateDummyModule(string moduleName, string destPath, string stagingAreaData)
		{
			string text = Path.Combine(Directory.GetCurrentDirectory(), stagingAreaData + "/MiscCPP");
			string text2 = Path.Combine(text, moduleName + ".cpp");
			string text3 = Path.Combine(text, moduleName + ".o");
			string text4 = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(destPath, moduleName + ".suprx"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(destPath))
			{
				Directory.CreateDirectory(destPath);
			}
			StreamWriter streamWriter = new StreamWriter(text2);
			streamWriter.WriteLine("#include <kernel.h>");
			streamWriter.WriteLine("#include <moduleinfo.h>");
			streamWriter.WriteLine("");
			streamWriter.WriteLine("SCE_MODULE_INFO(" + SUPRXTools.MakeValidModuleName(moduleName) + ",SCE_MODULE_ATTR_NONE, 1, 1); ");
			streamWriter.WriteLine("");
			streamWriter.WriteLine("extern \"C\" int module_start(SceSize sz, const void* arg) {");
			streamWriter.WriteLine("  return SCE_KERNEL_START_SUCCESS;");
			streamWriter.WriteLine("}");
			streamWriter.Close();
			if (!VitaSDKTools.CompileCppFile(text2, text3, text))
			{
				UnityEngine.Debug.LogError("Failed to compile cpp file: " + text2);
				return false;
			}
			List<string> list = new List<string>();
			list.Add(text3);
			if (!VitaSDKTools.LinkDynamicLibrary(list, text4, "", text))
			{
				UnityEngine.Debug.LogError("Failed to link module: " + text4);
			}
			FileUtil.DeleteFileOrDirectory(text);
			return true;
		}

		private static bool ProcessNativePlugins(BuildTarget target, string stagingAreaData, string aotCompileStepFolder, string playerPackage)
		{
			string text = stagingAreaData + "/Plugins";
			Directory.CreateDirectory(text);
			PluginImporter[] importers = PluginImporter.GetImporters(target);
			foreach (PluginImporter pluginImporter in importers)
			{
				string fileName = Path.GetFileName(pluginImporter.assetPath);
				string extension = Path.GetExtension(pluginImporter.assetPath);
				if (string.Compare(extension, ".suprx", ignoreCase: true) == 0)
				{
					FileUtil.UnityFileCopy(pluginImporter.assetPath, Path.Combine(text, fileName));
					string path = Path.GetFileNameWithoutExtension(pluginImporter.assetPath) + "_stub.a";
					string text2 = Path.Combine(Path.GetDirectoryName(pluginImporter.assetPath), path);
					if (!File.Exists(text2))
					{
						UnityEngine.Debug.LogError("Build Failed: native plugin \"" + fileName + "\" does not have a matching stub file\n");
						return false;
					}
					FileUtil.UnityFileCopy(text2, Path.Combine(text, path));
				}
			}
			SUPRXTools.CreateNativePluginsPRX(aotCompileStepFolder, playerPackage, text);
			return true;
		}

		public static bool LinkMonoAssemblies(BuildOptions options, string stagingAreaData, string stagingArea, AssemblyReferenceChecker checker)
		{
			string path = stagingAreaData + "/AOTCompileStep";
			string path2 = stagingAreaData + "/MiscCPP";
			path = Path.Combine(Directory.GetCurrentDirectory(), path);
			path2 = Path.Combine(Directory.GetCurrentDirectory(), path2);
			string text = Path.Combine(path, "RegisterMonoModules.o");
			if (EditorUtility.DisplayCancelableProgressBar("Building Player", "AOT Module Registration", 0.95f))
			{
				return false;
			}
			string input = Path.Combine(path, "RegisterMonoModules.cpp");
			if (!VitaSDKTools.CompileCppFile(input, text, path))
			{
				return false;
			}
			string text2 = Path.Combine(path2, "ApplicationInfo.o");
			input = Path.Combine(path2, "ApplicationInfo.cpp");
			if (!File.Exists(input))
			{
				UnityEngine.Debug.LogError("Build Failed: Failed compiling application info, file does not exist");
				return false;
			}
			if (!VitaSDKTools.CompileCppFile(input, text2, path2))
			{
				return false;
			}
			string[] assemblyFileNames = checker.GetAssemblyFileNames();
			foreach (string text3 in assemblyFileNames)
			{
				if (EditorUtility.DisplayCancelableProgressBar("Building Player", "Generating native code " + text3, 0.95f))
				{
					return false;
				}
				input = Path.Combine(path, Path.GetFileName(text3));
				if (MonoCrossCompile.ArtifactsPath != null)
				{
					File.Copy(input + ".s", MonoCrossCompile.ArtifactsPath + Path.GetFileName(text3) + ".s", overwrite: true);
				}
				string output = Path.Combine(path, input) + ".o";
				if (!VitaSDKTools.CompileAsmFile(input, output, path))
				{
					return false;
				}
			}
			string outputPath = Path.Combine(path, s_MonoAssembliesSUPRX);
			List<string> list = new List<string>();
			string[] assemblyFileNames2 = checker.GetAssemblyFileNames();
			foreach (string text4 in assemblyFileNames2)
			{
				list.Add(Path.Combine(path, text4 + ".o"));
			}
			list.Add(text);
			list.Add(text2);
			list.Add(Path.Combine(path, s_MonoVitaStubLibrary));
			VitaSDKTools.LinkDynamicLibrary(list, outputPath, "", path);
			return true;
		}

		internal static string SetupInstallPath(BuildOptions options, string installPath, bool isIncremental)
		{
			return installPath;
		}

		private static string GetVariationPath(string folder)
		{
			return Path.Combine(PostProcessArgs.playerPackage, $"Variations\\PSVita_{folder}_{GetVariationName()}");
		}

		private static string GetVariationName()
		{
			string arg = "mono";
			ScriptingImplementation scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.PSP2);
			if (scriptingBackend == ScriptingImplementation.IL2CPP)
			{
				arg = "il2cpp";
			}
			return $"{arg}";
		}
	}
}
