using System;
using System.Text;
using UnityEditor.Modules;
using UnityEngine;

namespace UnityEditor
{
	internal class PSP2SettingsEditorExtension : DefaultPlayerSettingsEditorExtension
	{
		private SerializedProperty m_PSP2SplashScreen;

		private SerializedProperty m_PSP2NPSupportsGBMorGJP;

		private SerializedProperty m_PSP2NPAgeRating;

		private SerializedProperty m_PSP2NPTitleDatPath;

		private SerializedProperty m_PSP2NPCommunicationsID;

		private SerializedProperty m_PSP2NPCommsPassphrase;

		private SerializedProperty m_PSP2NPCommsSig;

		private SerializedProperty m_PSP2TrophyPackPath;

		private SerializedProperty m_PSP2ParamSfxPath;

		private SerializedProperty m_PSP2ManualPath;

		private SerializedProperty m_PSP2LiveAreaGatePath;

		private SerializedProperty m_PSP2LiveAreaBackgroundPath;

		private SerializedProperty m_PSP2LiveAreaPath;

		private SerializedProperty m_PSP2LiveAreaTrialPath;

		private SerializedProperty m_PSP2PatchChangeInfoPath;

		private SerializedProperty m_PSP2PatchOriginalPackage;

		private SerializedProperty m_PSP2PackagePassword;

		private SerializedProperty m_PSP2KeystoneFile;

		private SerializedProperty m_PSP2MemoryExpansionMode;

		private SerializedProperty m_PSP2DRMType;

		private SerializedProperty m_PSP2StorageType;

		private SerializedProperty m_PSP2MediaCapacity;

		private SerializedProperty m_PSP2PowerMode;

		private SerializedProperty m_PSP2AcquireBGM;

		private SerializedProperty m_PSPTVBootMode;

		private SerializedProperty m_PSPTVDisableEmu;

		private SerializedProperty m_PSP2EnterButtonAssignment;

		private SerializedProperty m_PSP2SaveDataQuota;

		private SerializedProperty m_PSP2ParentalLevel;

		private SerializedProperty m_PSP2ShortTitle;

		private SerializedProperty m_PSP2ContentID;

		private SerializedProperty m_PSP2Category;

		private SerializedProperty m_PSP2MasterVersion;

		private SerializedProperty m_PSP2AppVersion;

		private SerializedProperty m_PSP2Upgradable;

		private SerializedProperty m_PSP2HealthWarning;

		private SerializedProperty m_PSP2InfoBarOnStartup;

		private SerializedProperty m_PSP2InfoBarColor;

		private SerializedProperty m_PSP2ScriptOptimizationLevel;

		private static GUIContent[] kPowerModeNames = new GUIContent[3]
		{
			EditorGUIUtility.TrTextContent("Mode A (Normal)"),
			EditorGUIUtility.TrTextContent("Mode B (GPU High - No WLAN or COM)"),
			EditorGUIUtility.TrTextContent("Mode C (GPU High - No Camera, OLED Low brightness)")
		};

		private static int[] kPowerModeVals = new int[3] { 0, 1, 2 };

		private static GUIContent[] kCategoryNames = new GUIContent[2]
		{
			EditorGUIUtility.TrTextContent("PS Vita Application"),
			EditorGUIUtility.TrTextContent("PS Vita Application Patch")
		};

		private static int[] kCategoryVals = new int[2] { 0, 1 };

		private static GUIContent[] kDRMTypeNames = new GUIContent[2]
		{
			EditorGUIUtility.TrTextContent("Paid-for content (Local)"),
			EditorGUIUtility.TrTextContent("Free content (Free)")
		};

		private static int[] kDRMTypeVals = new int[2] { 0, 1 };

		private static GUIContent[] kTVBootModeNames = new GUIContent[3]
		{
			EditorGUIUtility.TrTextContent("Default (Managed by System Software) (SCEE or SCEA)"),
			EditorGUIUtility.TrTextContent("PS Vita Bootable, PS Vita TV Bootable"),
			EditorGUIUtility.TrTextContent("PS Vita Bootable, PS Vita TV Not Bootable")
		};

		private static int[] kTVBootModeVals = new int[3] { 0, 1, 2 };

		private static GUIContent[] kEnterButtonAssignmentNames = new GUIContent[2]
		{
			EditorGUIUtility.TrTextContent("Circle Button"),
			EditorGUIUtility.TrTextContent("Cross Button")
		};

		private static int[] kEnterButtonAssignmentVals = new int[2] { 1, 2 };

		private static GUIContent[] kMemoryExpansionModeNames = new GUIContent[4]
		{
			EditorGUIUtility.TrTextContent("None"),
			EditorGUIUtility.TrTextContent("29MiB - Only this app can be run"),
			EditorGUIUtility.TrTextContent("77MiB - Only this app can be run, the internet browser cannot be used"),
			EditorGUIUtility.TrTextContent("109MiB - Only this app can be run, the internet browser and title store cannot be used")
		};

		private static int[] kMemoryExpansionModeVals = new int[4] { 0, 1, 2, 3 };

		private static GUIContent[] kStorageTypeNames = new GUIContent[3]
		{
			EditorGUIUtility.TrTextContent("No VC\\MC-MC"),
			EditorGUIUtility.TrTextContent("VC-VC"),
			EditorGUIUtility.TrTextContent("VC-MC")
		};

		private static int[] kStorageTypeNamesVals = new int[3] { 0, 1, 2 };

		private static GUIContent[] kNoVCMediaCapacity = new GUIContent[1] { EditorGUIUtility.TrTextContent("No Media") };

		private static int[] kNoVCMediaCapacityVals = new int[1];

		private static GUIContent[] kVCVCMediaCapacity = new GUIContent[6]
		{
			new GUIContent("VC 2GB (R\\O:1312Mib), R\\W:480Mib)"),
			new GUIContent("VC 2GB (R\\O:1504Mib), R\\W:288Mib)"),
			new GUIContent("VC 2GB (R\\O:1696Mib), R\\W:96Mib)"),
			new GUIContent("VC 4GB (R\\O:2624Mib), R\\W:960Mib)"),
			new GUIContent("VC 4GB (R\\O:3008Mib), R\\W:576Mib)"),
			new GUIContent("VC 4GB (R\\O:3392Mib), R\\W:192Mib)")
		};

		private static int[] kVCVCMediaCapacityVals = new int[6] { 1, 2, 3, 4, 5, 6 };

		private static GUIContent[] kVCMCMediaCapacity = new GUIContent[2]
		{
			new GUIContent("VC 2GB (R\\O:1792Mib, R\\W:On MC)"),
			new GUIContent("VC 4GB (R\\O:3584Mib, R\\W:On MC)")
		};

		private static int[] kVCMCMediaCapacityVals = new int[2] { 7, 8 };

		private static GUIContent[] kIL2CPPOptimizationLevelNames = new GUIContent[3]
		{
			EditorGUIUtility.TrTextContent("No Optimization"),
			EditorGUIUtility.TrTextContent("Optimized Compile"),
			EditorGUIUtility.TrTextContent("Optimized Compile, Unused code removed")
		};

		private static int[] kIL2CPPOptimizationLevelVals = new int[3] { 0, 1, 2 };

		public override void OnEnable(PlayerSettingsEditor settingsEditor)
		{
			base.OnEnable(settingsEditor);
			m_PSP2SplashScreen = settingsEditor.FindPropertyAssert("psp2Splashimage");
			m_PSP2NPSupportsGBMorGJP = settingsEditor.FindPropertyAssert("psp2NPSupportGBMorGJP");
			m_PSP2NPAgeRating = settingsEditor.FindPropertyAssert("psp2NPAgeRating");
			m_PSP2NPTitleDatPath = settingsEditor.FindPropertyAssert("psp2NPTitleDatPath");
			m_PSP2NPCommunicationsID = settingsEditor.FindPropertyAssert("psp2NPCommunicationsID");
			m_PSP2NPCommsPassphrase = settingsEditor.FindPropertyAssert("psp2NPCommsPassphrase");
			m_PSP2NPCommsSig = settingsEditor.FindPropertyAssert("psp2NPCommsSig");
			m_PSP2TrophyPackPath = settingsEditor.FindPropertyAssert("psp2NPTrophyPackPath");
			m_PSP2ParamSfxPath = settingsEditor.FindPropertyAssert("psp2ParamSfxPath");
			m_PSP2ManualPath = settingsEditor.FindPropertyAssert("psp2ManualPath");
			m_PSP2LiveAreaGatePath = settingsEditor.FindPropertyAssert("psp2LiveAreaGatePath");
			m_PSP2LiveAreaBackgroundPath = settingsEditor.FindPropertyAssert("psp2LiveAreaBackroundPath");
			m_PSP2LiveAreaPath = settingsEditor.FindPropertyAssert("psp2LiveAreaPath");
			m_PSP2LiveAreaTrialPath = settingsEditor.FindPropertyAssert("psp2LiveAreaTrialPath");
			m_PSP2PatchChangeInfoPath = settingsEditor.FindPropertyAssert("psp2PatchChangeInfoPath");
			m_PSP2PatchOriginalPackage = settingsEditor.FindPropertyAssert("psp2PatchOriginalPackage");
			m_PSP2PackagePassword = settingsEditor.FindPropertyAssert("psp2PackagePassword");
			m_PSP2KeystoneFile = settingsEditor.FindPropertyAssert("psp2KeystoneFile");
			m_PSP2MemoryExpansionMode = settingsEditor.FindPropertyAssert("psp2MemoryExpansionMode");
			m_PSP2DRMType = settingsEditor.FindPropertyAssert("psp2DRMType");
			m_PSP2StorageType = settingsEditor.FindPropertyAssert("psp2StorageType");
			m_PSP2MediaCapacity = settingsEditor.FindPropertyAssert("psp2MediaCapacity");
			m_PSP2PowerMode = settingsEditor.FindPropertyAssert("psp2PowerMode");
			m_PSP2AcquireBGM = settingsEditor.FindPropertyAssert("psp2AcquireBGM");
			m_PSPTVBootMode = settingsEditor.FindPropertyAssert("psp2TVBootMode");
			m_PSPTVDisableEmu = settingsEditor.FindPropertyAssert("psp2TVDisableEmu");
			m_PSP2EnterButtonAssignment = settingsEditor.FindPropertyAssert("psp2EnterButtonAssignment");
			m_PSP2SaveDataQuota = settingsEditor.FindPropertyAssert("psp2SaveDataQuota");
			m_PSP2ParentalLevel = settingsEditor.FindPropertyAssert("psp2ParentalLevel");
			m_PSP2ShortTitle = settingsEditor.FindPropertyAssert("psp2ShortTitle");
			m_PSP2ContentID = settingsEditor.FindPropertyAssert("psp2ContentID");
			m_PSP2Category = settingsEditor.FindPropertyAssert("psp2Category");
			m_PSP2MasterVersion = settingsEditor.FindPropertyAssert("psp2MasterVersion");
			m_PSP2AppVersion = settingsEditor.FindPropertyAssert("psp2AppVersion");
			m_PSP2Upgradable = settingsEditor.FindPropertyAssert("psp2Upgradable");
			m_PSP2HealthWarning = settingsEditor.FindPropertyAssert("psp2HealthWarning");
			m_PSP2InfoBarOnStartup = settingsEditor.FindPropertyAssert("psp2InfoBarOnStartup");
			m_PSP2InfoBarColor = settingsEditor.FindPropertyAssert("psp2InfoBarColor");
			m_PSP2ScriptOptimizationLevel = settingsEditor.FindPropertyAssert("psp2ScriptOptimizationLevel");
		}

		private void BeginGroup(GUIContent groupLabel)
		{
			GUILayout.Label(groupLabel, EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		}

		private void EndGroup()
		{
			EditorGUILayout.EndVertical();
		}

		public override bool HasIdentificationGUI()
		{
			return false;
		}

		public override bool HasPublishSection()
		{
			return true;
		}

		public override bool SupportsDynamicBatching()
		{
			return false;
		}

		public override void SplashSectionGUI()
		{
			m_PSP2SplashScreen.objectReferenceValue = EditorGUILayout.ObjectField(EditorGUIUtility.TrTextContent("960x544, 8bit palette"), (Texture2D)m_PSP2SplashScreen.objectReferenceValue, typeof(Texture2D), false);
			EditorGUILayout.Space();
		}

		public override void ConfigurationSectionGUI()
		{
			if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.PSP2) == ScriptingImplementation.IL2CPP)
			{
				EditorGUILayout.IntPopup(m_PSP2ScriptOptimizationLevel, kIL2CPPOptimizationLevelNames, kIL2CPPOptimizationLevelVals, EditorGUIUtility.TrTextContent("IL2CPP optimization level"));
			}
			EditorGUILayout.IntPopup(m_PSP2PowerMode, kPowerModeNames, kPowerModeVals, EditorGUIUtility.TrTextContent("Power Mode"));
			EditorGUILayout.PropertyField(m_PSP2AcquireBGM, EditorGUIUtility.TrTextContent("Override PS Vita Music", "Acquires the PS Vita's BGM port pausing any background music which might be playing."));
		}

		public override void PublishSectionGUI(float h, float midWidth, float maxWidth)
		{
			BeginGroup(EditorGUIUtility.TrTextContent("Live Area"));
			EditorGUILayout.HelpBox("Live area bitmaps must not exceed a total of 1 MB when uncompressed in memory, for this reason the use of 8 bit indexed PNGs is recommended.", MessageType.Info);
			using (new EditorGUI.DisabledScope(m_PSP2LiveAreaPath.stringValue.Length != 0))
			{
				PlayerSettingsEditor.BuildFileBoxButton(m_PSP2LiveAreaBackgroundPath, "Background Image|840x500, PNG, 8bit palette.", m_PSP2LiveAreaBackgroundPath.stringValue, "png");
				PlayerSettingsEditor.BuildFileBoxButton(m_PSP2LiveAreaGatePath, "Gate Image|280x158, PNG, 8bit palette.", m_PSP2LiveAreaGatePath.stringValue, "png");
			}
			GUILayout.Label(EditorGUIUtility.TrTextContent("Custom Live Area"), EditorStyles.boldLabel);
			PlayerSettingsEditor.BuildPathBoxButton(m_PSP2LiveAreaPath, "Live Area Folder|Folder containing live area PNG and XML files for full apps.", m_PSP2LiveAreaPath.stringValue);
			using (new EditorGUI.DisabledScope(!m_PSP2Upgradable.boolValue))
			{
				PlayerSettingsEditor.BuildPathBoxButton(m_PSP2LiveAreaTrialPath, "Live Area Folder (Trial)|Folder containing live area PNG and XML files for trial/upgradable apps.", m_PSP2LiveAreaTrialPath.stringValue);
			}
			EndGroup();
			BeginGroup(EditorGUIUtility.TrTextContent("Software Manual"));
			PlayerSettingsEditor.BuildPathBoxButton(m_PSP2ManualPath, "Manual Folder|Folder containing software manual PNG files.", m_PSP2ManualPath.stringValue);
			EndGroup();
			BeginGroup(EditorGUIUtility.TrTextContent("Param File"));
			using (new EditorGUI.DisabledScope(m_PSP2ParamSfxPath.stringValue.Length != 0))
			{
				EditorGUILayout.IntPopup(m_PSP2Category, kCategoryNames, kCategoryVals, EditorGUIUtility.TrTextContent("Category"));
				EditorGUILayout.PropertyField(m_PSP2MasterVersion, EditorGUIUtility.TrTextContent("Master Version"));
				EditorGUILayout.PropertyField(m_PSP2AppVersion, EditorGUIUtility.TrTextContent("Application Version"));
				EditorGUILayout.PropertyField(m_PSP2ContentID, EditorGUIUtility.TrTextContent("Content ID"));
				EditorGUILayout.PropertyField(m_PSP2ShortTitle, EditorGUIUtility.TrTextContent("Short Title"));
				EditorGUILayout.PropertyField(m_PSP2SaveDataQuota, EditorGUIUtility.TrTextContent("Save data quota (KB)"));
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_PSP2ParentalLevel, EditorGUIUtility.TrTextContent("Parental Level (0-11)"));
				if (EditorGUI.EndChangeCheck())
				{
					if (m_PSP2ParentalLevel.intValue < 0)
					{
						m_PSP2ParentalLevel.intValue = 0;
					}
					if (m_PSP2ParentalLevel.intValue > 11)
					{
						m_PSP2ParentalLevel.intValue = 11;
					}
				}
				m_PSP2Upgradable.boolValue = GUILayout.Toggle(m_PSP2Upgradable.boolValue, EditorGUIUtility.TrTextContent("Upgradable", "The application runs in trial mode unless an upgrade has been purchased."));
				m_PSP2HealthWarning.boolValue = GUILayout.Toggle(m_PSP2HealthWarning.boolValue, EditorGUIUtility.TrTextContent("Health warning", "Add a health warning to the first page of the software manual."));
				m_PSP2InfoBarOnStartup.boolValue = GUILayout.Toggle(m_PSP2InfoBarOnStartup.boolValue, EditorGUIUtility.TrTextContent("Information bar", "Display the PS Vita information bar when the applications starts"));
				m_PSP2InfoBarColor.boolValue = GUILayout.Toggle(m_PSP2InfoBarColor.boolValue, EditorGUIUtility.TrTextContent("Information bar color", "Sets the color of the information bar, ticked = white, un-ticked = black."));
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.IntPopup(m_PSPTVBootMode, kTVBootModeNames, kTVBootModeVals, EditorGUIUtility.TrTextContent("Vita TV Boot Mode"));
				if (EditorGUI.EndChangeCheck() && m_PSPTVBootMode.intValue == kTVBootModeVals[2])
				{
					m_PSPTVDisableEmu.boolValue = false;
				}
				using (new EditorGUI.DisabledScope(m_PSPTVBootMode.intValue == kTVBootModeVals[2]))
				{
					m_PSPTVDisableEmu.boolValue = GUILayout.Toggle(m_PSPTVDisableEmu.boolValue, EditorGUIUtility.TrTextContent("Vita TV, disable touch emulation", "Disable touch panel emulation by L3/R3 Buttons"));
				}
				EditorGUILayout.IntPopup(m_PSP2EnterButtonAssignment, kEnterButtonAssignmentNames, kEnterButtonAssignmentVals, EditorGUIUtility.TrTextContent("Enter Button Assignment"));
				EditorGUILayout.IntPopup(m_PSP2MemoryExpansionMode, kMemoryExpansionModeNames, kMemoryExpansionModeVals, EditorGUIUtility.TrTextContent("Memory Expansion Mode"));
			}
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Specifying a param file overrides all of the above values.", MessageType.Info);
			PlayerSettingsEditor.BuildFileBoxButton(m_PSP2ParamSfxPath, "Param file (.sfx)", m_PSP2ParamSfxPath.stringValue, "sfx");
			EndGroup();
			BeginGroup(EditorGUIUtility.TrTextContent("Package"));
			EditorGUILayout.PropertyField(m_PSP2PackagePassword, EditorGUIUtility.TrTextContent("Pass-code (32 chars)"));
			if (m_PSP2PackagePassword.stringValue.Length != 32)
			{
				System.Random random = new System.Random();
				StringBuilder stringBuilder = new StringBuilder();
				string text = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
				for (int i = 0; i < 32; i++)
				{
					int index = random.Next(text.Length);
					stringBuilder.Append(text[index]);
				}
				m_PSP2PackagePassword.stringValue = stringBuilder.ToString();
			}
			PlayerSettingsEditor.BuildFileBoxButton(m_PSP2KeystoneFile, "Keystone File", m_PSP2KeystoneFile.stringValue, "*");
			EditorGUILayout.HelpBox("Keystone file is required if your app supports additional content from PSN. Use psp2pubkeygen.exe to create the file. The keystone and additional content packages must be created using the pass-code specified above.", MessageType.Info);
			EndGroup();
			BeginGroup(EditorGUIUtility.TrTextContent("Cumulative Patch"));
			PlayerSettingsEditor.BuildPathBoxButton(m_PSP2PatchChangeInfoPath, "Change Info Folder", m_PSP2PatchChangeInfoPath.stringValue);
			PlayerSettingsEditor.BuildPathBoxButton(m_PSP2PatchOriginalPackage, "First Published Package", m_PSP2PatchOriginalPackage.stringValue);
			EndGroup();
			BeginGroup(EditorGUIUtility.TrTextContent("Digital Rights Management"));
			EditorGUILayout.IntPopup(m_PSP2DRMType, kDRMTypeNames, kDRMTypeVals, EditorGUIUtility.TrTextContent("DRM Type"));
			EditorGUILayout.Space();
			EndGroup();
			BeginGroup(EditorGUIUtility.TrTextContent("Media Type & Size"));
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.IntPopup(m_PSP2StorageType, kStorageTypeNames, kStorageTypeNamesVals, EditorGUIUtility.TrTextContent("Storage Type"));
			if (EditorGUI.EndChangeCheck())
			{
				switch (m_PSP2StorageType.intValue)
				{
				case 0:
					m_PSP2MediaCapacity.intValue = kNoVCMediaCapacityVals[0];
					break;
				case 1:
					m_PSP2MediaCapacity.intValue = kVCVCMediaCapacityVals[0];
					break;
				case 2:
					m_PSP2MediaCapacity.intValue = kVCMCMediaCapacityVals[0];
					break;
				}
			}
			switch (m_PSP2StorageType.intValue)
			{
			case 0:
				EditorGUILayout.HelpBox("[No VC/MC-MC]\nThis application is NOT distributed by PS Vita card(VC).\nThis application is distributed by network and installed\nonto memory card(MC).", MessageType.Info);
				EditorGUILayout.IntPopup(m_PSP2MediaCapacity, kNoVCMediaCapacity, kNoVCMediaCapacityVals, EditorGUIUtility.TrTextContent("Media Capacity"));
				break;
			case 1:
				EditorGUILayout.HelpBox("[VC-VC]\nThis application is distributed by PS Vita card(VC).\nThis application may be distributed by network and installed\nonto memory card(MC).\n\n[In case of VC distribution]\nThe VC has rewritable(R/W) area, and Patches/Additional\nContents/Save Data are stored on R/W area of the VC.\nMC is not required to run this application.", MessageType.Info);
				EditorGUILayout.IntPopup(m_PSP2MediaCapacity, kVCVCMediaCapacity, kVCVCMediaCapacityVals, EditorGUIUtility.TrTextContent("Media Capacity"));
				break;
			case 2:
				EditorGUILayout.HelpBox("[VC-MC]\nThis application is distributed by PS Vita card(VC).\nThis application may be distributed by network and installed\nonto memory card(MC).\n\n[In case of VC distribution]\nThe VC does NOT have rewritable(R/W) area, and Patches/Additional\nContents/Save Data are stored on MC.\nMC is REQUIRED to run this application.", MessageType.Info);
				EditorGUILayout.IntPopup(m_PSP2MediaCapacity, kVCMCMediaCapacity, kVCMCMediaCapacityVals, EditorGUIUtility.TrTextContent("Media Capacity"));
				break;
			}
			EditorGUILayout.Space();
			EndGroup();
			BeginGroup(EditorGUIUtility.TrTextContent("PlayStation®Network"));
			m_PSP2NPSupportsGBMorGJP.boolValue = GUILayout.Toggle(m_PSP2NPSupportsGBMorGJP.boolValue, EditorGUIUtility.TrTextContent("Supports Game Boot Message and/or Game Joining Presence"));
			PlayerSettingsEditor.BuildFileBoxButton(m_PSP2TrophyPackPath, "Trophy Pack (trophy.trp)", m_PSP2TrophyPackPath.stringValue, "trp");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(m_PSP2NPAgeRating, EditorGUIUtility.TrTextContent("NP Age Rating (0-99)"));
			if (EditorGUI.EndChangeCheck())
			{
				if (m_PSP2NPAgeRating.intValue < 0)
				{
					m_PSP2NPAgeRating.intValue = 0;
				}
				if (m_PSP2NPAgeRating.intValue > 99)
				{
					m_PSP2NPAgeRating.intValue = 99;
				}
			}
			PlayerSettingsEditor.BuildFileBoxButton(m_PSP2NPTitleDatPath, "NP Title ID (nptitle.dat)|This file is mandatory if you intend to publish a title using NP features. It overrides the NP Title secret and NP Title ID. It is obtained from PlayStation Vita Developer Network", m_PSP2NPTitleDatPath.stringValue, "dat");
			EditorGUILayout.PropertyField(m_PSP2NPCommunicationsID, EditorGUIUtility.TrTextContent("NP Communication ID"));
			m_PSP2NPCommsPassphrase.stringValue = EditorGUILayout.TextField(EditorGUIUtility.TrTextContent("NP Communication Passphrase"), m_PSP2NPCommsPassphrase.stringValue, GUILayout.Height(280f));
			m_PSP2NPCommsSig.stringValue = EditorGUILayout.TextField(EditorGUIUtility.TrTextContent("NP Communication Signature"), m_PSP2NPCommsSig.stringValue, GUILayout.Height(280f));
			EditorGUILayout.Space();
			EndGroup();
		}

		public override bool SupportsMultithreadedRendering()
		{
			return true;
		}
	}
}
