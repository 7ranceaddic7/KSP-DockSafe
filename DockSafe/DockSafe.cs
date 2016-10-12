using System;
using UnityEngine;
using KSP.UI.Screens;

namespace DockSafe
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class DockSafe : MonoBehaviour
	{
		private ApplicationLauncherButton AppLauncherButton;
		private bool isActive;

		void Start()
		{
			isActive = false;
			GameEvents.onGUIApplicationLauncherReady.Add (OnAppLauncherReady);
			GameEvents.onVesselChange.Add (OnVesselChange);
		}

		void Update()
		{
			if (!isActive) {
				if (FlightGlobals.ActiveVessel.targetObject is ModuleDockingNode) {
					AppLauncherButton.SetTrue ();
				}
			}
		}

		void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
			GameEvents.onVesselChange.Remove (OnVesselChange);
			if (AppLauncherButton != null) {
				ApplicationLauncher.Instance.RemoveModApplication (AppLauncherButton);
			}
		}

		public void OnAppLauncherReady()
		{
			if (AppLauncherButton == null) {
				AppLauncherButton = ApplicationLauncher.Instance.AddModApplication (
					Activate,
					Deactivate,
					null,
					null,
					null,
					null,
					ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
					GameDatabase.Instance.GetTexture ("DockSafe/Textures/ds_off", false)
				);
			}
			// If vessel has disabled engines, then DS was active previously, reinitiate override
			if (CheckActive ()) {
				AppLauncherButton.SetTrue ();
			}
		}

		// Activate app
		private void Activate() {
			if (!isActive) {
				isActive = true;
				ShutdownEngines ();
				AppLauncherButton.SetTexture (
					GameDatabase.Instance.GetTexture("DockSafe/Textures/ds_active", false)
				);
				ScreenMessages.PostScreenMessage ("DockSafe: Engines safety override active");
			}
		}

		// Deactivate app
		private void Deactivate() {
			isActive = false;
			ActivateEngines ();
			ScreenMessages.PostScreenMessage ("DockSafe: Deactivated");
			AppLauncherButton.SetTexture (
				GameDatabase.Instance.GetTexture("DockSafe/Textures/ds_off", false)
			);
		}

		// Shutdown engines and override control
		private void ShutdownEngines() {
			foreach (Part p in FlightGlobals.ActiveVessel.Parts) {
				foreach(PartModule pm in p.Modules) {
					if (pm is ModuleEngines) {
						ModuleEngines eng = (ModuleEngines)pm;
						if (eng.engineType == EngineType.LiquidFuel | eng.engineType == EngineType.Nuclear) {
							eng.Shutdown ();
							eng.Events ["Activate"].guiActive = false;
						}
					}
				}
			}
		}

		// Restore engines control without reactivating them
		private void ActivateEngines() {
			foreach (Part p in FlightGlobals.ActiveVessel.Parts) {
				foreach(PartModule pm in p.Modules) {
					if (pm is ModuleEngines) {
						ModuleEngines eng = (ModuleEngines)pm;
						if (eng.engineType == EngineType.LiquidFuel | eng.engineType == EngineType.Nuclear) {
							eng.Events ["Activate"].guiActive = true;
						}
					}
				}
			}
		}

		// Check if there are disabled engines
		private bool CheckActive() {
			foreach (Part p in FlightGlobals.ActiveVessel.Parts) {
				foreach(PartModule pm in p.Modules) {
					if (pm is ModuleEngines) {
						ModuleEngines eng = (ModuleEngines)pm;
						if (eng.Events ["Activate"].guiActive == false) {
							return true;
						}
					}
				}
			}
			return false;
		}

		// If vessel has disabled engines, then DS was active previously, reinitiate override
		private void OnVesselChange (Vessel v) {
			if (CheckActive ()) {
				AppLauncherButton.SetTrue ();
			} else {
				AppLauncherButton.SetFalse ();
			}
		}
	}
}
