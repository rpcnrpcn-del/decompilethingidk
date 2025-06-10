namespace Steamworks
{
	public static class SteamController
	{
		public static bool Init()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_Init();
		}

		public static bool Shutdown()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_Shutdown();
		}

		public static void RunFrame()
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamController_RunFrame();
		}

		public static int GetConnectedControllers(ControllerHandle_t[] handlesOut)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_GetConnectedControllers(handlesOut);
		}

		public static bool ShowBindingPanel(ControllerHandle_t controllerHandle)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_ShowBindingPanel(controllerHandle);
		}

		public static ControllerActionSetHandle_t GetActionSetHandle(string pszActionSetName)
		{
			InteropHelp.TestIfAvailableClient();
			using (InteropHelp.UTF8StringHandle pszActionSetName2 = new InteropHelp.UTF8StringHandle(pszActionSetName))
			{
				return (ControllerActionSetHandle_t)NativeMethods.ISteamController_GetActionSetHandle(pszActionSetName2);
			}
		}

		public static void ActivateActionSet(ControllerHandle_t controllerHandle, ControllerActionSetHandle_t actionSetHandle)
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamController_ActivateActionSet(controllerHandle, actionSetHandle);
		}

		public static ControllerActionSetHandle_t GetCurrentActionSet(ControllerHandle_t controllerHandle)
		{
			InteropHelp.TestIfAvailableClient();
			return (ControllerActionSetHandle_t)NativeMethods.ISteamController_GetCurrentActionSet(controllerHandle);
		}

		public static ControllerDigitalActionHandle_t GetDigitalActionHandle(string pszActionName)
		{
			InteropHelp.TestIfAvailableClient();
			using (InteropHelp.UTF8StringHandle pszActionName2 = new InteropHelp.UTF8StringHandle(pszActionName))
			{
				return (ControllerDigitalActionHandle_t)NativeMethods.ISteamController_GetDigitalActionHandle(pszActionName2);
			}
		}

		public static ControllerDigitalActionData_t GetDigitalActionData(ControllerHandle_t controllerHandle, ControllerDigitalActionHandle_t digitalActionHandle)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_GetDigitalActionData(controllerHandle, digitalActionHandle);
		}

		public static int GetDigitalActionOrigins(ControllerHandle_t controllerHandle, ControllerActionSetHandle_t actionSetHandle, ControllerDigitalActionHandle_t digitalActionHandle, EControllerActionOrigin[] originsOut)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_GetDigitalActionOrigins(controllerHandle, actionSetHandle, digitalActionHandle, originsOut);
		}

		public static ControllerAnalogActionHandle_t GetAnalogActionHandle(string pszActionName)
		{
			InteropHelp.TestIfAvailableClient();
			using (InteropHelp.UTF8StringHandle pszActionName2 = new InteropHelp.UTF8StringHandle(pszActionName))
			{
				return (ControllerAnalogActionHandle_t)NativeMethods.ISteamController_GetAnalogActionHandle(pszActionName2);
			}
		}

		public static ControllerAnalogActionData_t GetAnalogActionData(ControllerHandle_t controllerHandle, ControllerAnalogActionHandle_t analogActionHandle)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_GetAnalogActionData(controllerHandle, analogActionHandle);
		}

		public static int GetAnalogActionOrigins(ControllerHandle_t controllerHandle, ControllerActionSetHandle_t actionSetHandle, ControllerAnalogActionHandle_t analogActionHandle, EControllerActionOrigin[] originsOut)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamController_GetAnalogActionOrigins(controllerHandle, actionSetHandle, analogActionHandle, originsOut);
		}

		public static void StopAnalogActionMomentum(ControllerHandle_t controllerHandle, ControllerAnalogActionHandle_t eAction)
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamController_StopAnalogActionMomentum(controllerHandle, eAction);
		}

		public static void TriggerHapticPulse(ControllerHandle_t controllerHandle, ESteamControllerPad eTargetPad, ushort usDurationMicroSec)
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamController_TriggerHapticPulse(controllerHandle, eTargetPad, usDurationMicroSec);
		}

		public static void TriggerRepeatedHapticPulse(ControllerHandle_t controllerHandle, ESteamControllerPad eTargetPad, ushort usDurationMicroSec, ushort usOffMicroSec, ushort unRepeat, uint nFlags)
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamController_TriggerRepeatedHapticPulse(controllerHandle, eTargetPad, usDurationMicroSec, usOffMicroSec, unRepeat, nFlags);
		}
	}
}
