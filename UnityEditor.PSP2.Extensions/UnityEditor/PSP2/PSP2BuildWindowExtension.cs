using System;
using UnityEditor.Modules;
using UnityEngine;

namespace UnityEditor.PSP2
{
	internal class PSP2BuildWindowExtension : DefaultBuildWindowExtension
	{
		private PSP2BuildSubtarget[] psp2Subtargets = new PSP2BuildSubtarget[2]
		{
			PSP2BuildSubtarget.PCHosted,
			PSP2BuildSubtarget.Package
		};

		private GUIContent[] psp2SubtargetStrings = new GUIContent[2]
		{
			EditorGUIUtility.TrTextContent("PC Hosted"),
			EditorGUIUtility.TrTextContent("PSVita Package")
		};

		private GUIContent psp2TargetStyle = EditorGUIUtility.TrTextContent("Build Type");

		private GUIContent compressWithPsArc = EditorGUIUtility.TrTextContent("Compress with PSArc");

		private GUIContent needSubmissionMaterials = EditorGUIUtility.TrTextContent("Submission Materials");

		public override void ShowPlatformBuildOptions()
		{
			int num = Array.IndexOf(psp2Subtargets, EditorUserBuildSettings.psp2BuildSubtarget);
			if (num == -1)
			{
				num = 0;
			}
			num = EditorGUILayout.Popup(psp2TargetStyle, num, psp2SubtargetStrings);
			EditorUserBuildSettings.psp2BuildSubtarget = psp2Subtargets[num];
			EditorUserBuildSettings.compressWithPsArc = EditorGUILayout.Toggle(compressWithPsArc, EditorUserBuildSettings.compressWithPsArc);
			if (EditorUserBuildSettings.psp2BuildSubtarget != 0 && !EditorUserBuildSettings.development)
			{
				EditorUserBuildSettings.needSubmissionMaterials = EditorGUILayout.Toggle(needSubmissionMaterials, EditorUserBuildSettings.needSubmissionMaterials);
			}
		}

		public override bool ShouldDrawProfilerCheckbox()
		{
			return EditorUserBuildSettings.development;
		}

		public override bool ShouldDrawExplicitNullCheckbox()
		{
			return true;
		}

		public override bool ShouldDrawExplicitArrayBoundsCheckbox()
		{
			return true;
		}

		public override bool ShouldDrawForceOptimizeScriptsCheckbox()
		{
			return true;
		}
	}
}
