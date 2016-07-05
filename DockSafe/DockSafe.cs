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
		}

		public void Activate() {
			if (!isActive) {
				isActive = true;
				ShutdownEngines ();
				AppLauncherButton.SetTexture (
					GameDatabase.Instance.GetTexture("DockSafe/Textures/ds_active", false)
				);
				ScreenMessages.PostScreenMessage ("DockSafe: Engines safety override active");
			}
		}

		private void Deactivate() {
			isActive = false;
			ActivateEngines ();
			ScreenMessages.PostScreenMessage ("DockSafe: Deactivated");
			AppLauncherButton.SetTexture (
				GameDatabase.Instance.GetTexture("DockSafe/Textures/ds_off", false)
			);
		}

		public void ShutdownEngines() {
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

		public void ActivateEngines() {
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
	}
}
