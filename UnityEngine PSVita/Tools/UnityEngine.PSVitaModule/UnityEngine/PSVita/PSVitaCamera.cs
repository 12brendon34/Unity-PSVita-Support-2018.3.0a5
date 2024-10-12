using System.Runtime.CompilerServices;
//using UnityEngine.Bindings;

namespace UnityEngine.PSVita
{
	//[NativeHeader("UnityPrefix.h")]
	public class PSVitaCamera
	{
		public enum Device
		{
			Front,
			Back
		}

		public enum Effect
		{
			Normal,
			Nega,
			Bw,
			Sepia,
			Bluish,
			Reddish,
			Greenish
		}

		public enum Saturation
		{
			sat0 = 0,
			sat0_5 = 5,
			sat1 = 10,
			sat2 = 20,
			sat3 = 30,
			sat4 = 40
		}

		public enum Sharpness
		{
			percent100 = 1,
			percent200,
			percent300,
			percent400
		}

		public enum Reverse
		{
			off,
			mirror,
			flip,
			mirror_flip
		}

		public enum EV
		{
			plus2 = 20,
			plus1_7 = 17,
			plus1_5 = 15,
			plus1_3 = 13,
			plus1_0 = 10,
			plus0_7 = 7,
			plus0_5 = 5,
			plus0_3 = 3,
			plus0 = 0,
			minus0_3 = -3,
			minus0_5 = -5,
			minus0_7 = -7,
			minus1_0 = -10,
			minus1_3 = -13,
			minus1_5 = -15,
			minus1_7 = -17,
			minus2_0 = -20
		}

		public enum AntiFlicker
		{
			auto,
			hz50,
			hz60
		}

		public enum ISO
		{
			auto = 1,
			iso100 = 100,
			iso200 = 200,
			iso400 = 400
		}

		public enum WhiteBalance
		{
			auto,
			day,
			cwf,
			a
		}

		public enum Nightmode
		{
			off,
			less10,
			less100,
			over100
		}

		public enum ExposureCeiling
		{
			normal,
			half
		}

		public enum CameraFormat
		{
			YUV422_PLANE = 1,
			YUV422_PACKED,
			YUV420_PLANE,
			YUV422_TO_ARGB,
			YUV422_TO_ABGR,
			RAW8
		}

		public enum ShutterSound
		{
			IMAGE,
			VIDEO_START,
			VIDEO_STOP
		}

		private enum CameraStates
		{
			kCamState_ExposureCeiling,
			kCamState_AutoControlHold,
			kCamState_NightMode,
			kCamState_BackLight,
			kCamState_WhiteBalance,
			kCamState_ISO,
			kCamState_AntiFlicker,
			kCamState_Zoom,
			kCamState_EV,
			kCamState_Effect,
			kCamState_Reverse,
			kCamState_Sharpness,
			kCamState_Contrast,
			kCamState_Brightness,
			kCamState_Saturation
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("GetCameraState")]
		private static extern int GetCameraState(Device devnum, CameraStates state);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("SetCameraState")]
		private static extern void SetCameraState(Device devnum, CameraStates state, int value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("SetCameraFormat")]
		public static extern void SetFormat(CameraFormat value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		//[NativeConditional("PLATFORM_PSVITA")]
		//[FreeFunction("DoCameraShutterSound")]
		public static extern void DoShutterSound(ShutterSound value);

		public static Saturation GetSaturation(Device devnum)
		{
			return (Saturation)GetCameraState(devnum, CameraStates.kCamState_Saturation);
		}

		public static void GetSaturation(Device devnum, Saturation value)
		{
			SetCameraState(devnum, CameraStates.kCamState_Saturation, (int)value);
		}

		public static int GetBrightness(Device devnum)
		{
			return GetCameraState(devnum, CameraStates.kCamState_Brightness);
		}

		public static void SetBrightness(Device devnum, int value)
		{
			SetCameraState(devnum, CameraStates.kCamState_Brightness, value);
		}

		public static int GetContrast(Device devnum)
		{
			return GetCameraState(devnum, CameraStates.kCamState_Contrast);
		}

		public static void SetContrast(Device devnum, int value)
		{
			SetCameraState(devnum, CameraStates.kCamState_Contrast, value);
		}

		public static Sharpness GetSharpness(Device devnum)
		{
			return (Sharpness)GetCameraState(devnum, CameraStates.kCamState_Sharpness);
		}

		public static void SetSharpness(Device devnum, Sharpness value)
		{
			SetCameraState(devnum, CameraStates.kCamState_Sharpness, (int)value);
		}

		public static Reverse GetReverse(Device devnum)
		{
			return (Reverse)GetCameraState(devnum, CameraStates.kCamState_Reverse);
		}

		public static void SetReverse(Device devnum, Reverse value)
		{
			SetCameraState(devnum, CameraStates.kCamState_Reverse, (int)value);
		}

		public static Effect GetEffect(Device devnum)
		{
			return (Effect)GetCameraState(devnum, CameraStates.kCamState_Effect);
		}

		public static void SetEffect(Device devnum, Effect value)
		{
			SetCameraState(devnum, CameraStates.kCamState_Effect, (int)value);
		}

		public static EV GetEV(Device devnum)
		{
			return (EV)GetCameraState(devnum, CameraStates.kCamState_EV);
		}

		public static void SetEV(Device devnum, EV value)
		{
			SetCameraState(devnum, CameraStates.kCamState_EV, (int)value);
		}

		public static int GetZoom(Device devnum)
		{
			return GetCameraState(devnum, CameraStates.kCamState_Zoom);
		}

		public static void SetZoom(Device devnum, int value)
		{
			SetCameraState(devnum, CameraStates.kCamState_Zoom, value);
		}

		public static AntiFlicker GetAntiFlicker(Device devnum)
		{
			return (AntiFlicker)GetCameraState(devnum, CameraStates.kCamState_AntiFlicker);
		}

		public static void SetAntiFlicker(Device devnum, AntiFlicker value)
		{
			SetCameraState(devnum, CameraStates.kCamState_AntiFlicker, (int)value);
		}

		public static ISO GetISO(Device devnum)
		{
			return (ISO)GetCameraState(devnum, CameraStates.kCamState_ISO);
		}

		public static void SetISO(Device devnum, ISO value)
		{
			SetCameraState(devnum, CameraStates.kCamState_ISO, (int)value);
		}

		public static WhiteBalance GetWhiteBalance(Device devnum)
		{
			return (WhiteBalance)GetCameraState(devnum, CameraStates.kCamState_WhiteBalance);
		}

		public static void SetWhiteBalance(Device devnum, WhiteBalance value)
		{
			SetCameraState(devnum, CameraStates.kCamState_WhiteBalance, (int)value);
		}

		public static bool GetBacklight(Device devnum)
		{
			return (GetCameraState(devnum, CameraStates.kCamState_BackLight) != 0) ? true : false;
		}

		public static void SetBacklight(Device devnum, bool value)
		{
			SetCameraState(devnum, CameraStates.kCamState_BackLight, value ? 1 : 0);
		}

		public static Nightmode GetNightmode(Device devnum)
		{
			return (Nightmode)GetCameraState(devnum, CameraStates.kCamState_NightMode);
		}

		public static void SetNightmode(Device devnum, Nightmode value)
		{
			SetCameraState(devnum, CameraStates.kCamState_NightMode, (int)value);
		}

		public static bool GetAutoControlHold(Device devnum)
		{
			return (GetCameraState(devnum, CameraStates.kCamState_AutoControlHold) != 0) ? true : false;
		}

		public static void SetAutoControlHold(Device devnum, bool value)
		{
			SetCameraState(devnum, CameraStates.kCamState_AutoControlHold, value ? 1 : 0);
		}

		public static ExposureCeiling GetExposureCeiling(Device devnum)
		{
			return (ExposureCeiling)GetCameraState(devnum, CameraStates.kCamState_ExposureCeiling);
		}

		public static void SetExposureCeiling(Device devnum, ExposureCeiling value)
		{
			SetCameraState(devnum, CameraStates.kCamState_ExposureCeiling, (int)value);
		}
	}
}
