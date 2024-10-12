using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("Runtime/Mono/MonoBehaviour.h")]
	//[NativeHeader("Runtime/Input/GetInput.h")]
	//[NativeHeader("Runtime/Scripting/ScriptingUtility.h")]
	//[NativeHeader("UnityPrefix.h")]
	public class PSVitaInput
	{
		public enum CompassStability
		{
			CompassUnstable,
			CompassStable,
			CompassVeryStable
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool secondaryTouchIsScreenSpace
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GetBackTouchIsScreenSpace")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("SetBackTouchIsScreenSpace")]
			set;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int touchCountSecondary
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GetActiveBackTouchCount")]
			get;
		}

		public static Touch[] touchesSecondary
		{
			get
			{
				int num = touchCountSecondary;
				Touch[] array = new Touch[num];
				for (int i = 0; i < num; i++)
				{
					Touch reference = array[i];
					reference = GetSecondaryTouch(i);
				}
				return array;
			}
		}

		public static bool secondaryTouchEnabled => false;

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int secondaryTouchWidth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GetBackTouchWidth")]
			get;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern int secondaryTouchHeight
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GetBackTouchHeight")]
			get;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern CompassStability compassFieldStability
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GetCompassFieldStability")]
			get;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool gyroDeadbandFilterEnabled
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GyroIsDeadbandFilterEnabled")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GyroSetDeadbandFilterEnabled")]
			set;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool gyroTiltCorrectionEnabled
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GyroIsTiltCorrectionEnabled")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GyroSetTiltCorrectionEnabled")]
			set;
		}

		//[NativeConditional("PLATFORM_PSVITA")]
		public static extern bool fingerIdEqSceTouchId
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("GetFingerIdEqSceId")]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			//[FreeFunction("SetFingerIdEqSceId")]
			set;
		}

		private PSVitaInput()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[FreeFunction]
		private static extern int GetActiveBackTouchCount();

		public static Touch GetSecondaryTouch(int index)
		{
			return default(Touch);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction]
		public static extern void ResetMotionSensors();

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction]
		public static extern bool WirelesslyControlled();
	}
}
