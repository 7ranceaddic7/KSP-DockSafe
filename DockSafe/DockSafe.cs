using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;

namespace DockSafe
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class DockSafe : MonoBehaviour
	{
		private ApplicationLauncherButton appLauncherButton;
		private bool isActive;
		private readonly List<ModuleEngines> deactivatedEngines = new List<ModuleEngines>();

		private void Start()
		{
			isActive = false;
			GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
			GameEvents.onVesselChange.Add(OnVesselChange);
		}

		private void Update()
		{
			if (FlightGlobals.ActiveVessel.targetObject is ModuleDockingNode)
			{
				appLauncherButton.SetTrue();
			}
			else if(isActive) appLauncherButton.SetFalse();
		}

		private void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
			GameEvents.onVesselChange.Remove(OnVesselChange);
			if (appLauncherButton != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
			}
		}

		public void OnAppLauncherReady()
		{
			if (appLauncherButton == null)
			{
				appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
					Activate,
					Deactivate,
					null,
					null,
					null,
					null,
					ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
					GameDatabase.Instance.GetTexture("DockSafe/Textures/ds_off", false)
				);
			}

			// If vessel has disabled engines, then DS was active previously, re-activate override
			if (CheckActive())
			{
				appLauncherButton.SetTrue();
			}
		}

		// Activate app
		private void Activate()
		{
			if (isActive) return;
			isActive = true;
			ShutdownEngines();
			appLauncherButton.SetTexture(
				GameDatabase.Instance.GetTexture("DockSafe/Textures/ds_active", false)
			);
			ScreenMessages.PostScreenMessage("DockSafe: Engines safety override active");
		}

		// Deactivate app
		private void Deactivate()
		{
			isActive = false;
			ActivateEngines();
			ScreenMessages.PostScreenMessage("DockSafe: Deactivated");
			appLauncherButton.SetTexture(
				GameDatabase.Instance.GetTexture("DockSafe/Textures/ds_off", false)
			);
		}

		// Shutdown engines and override control
		private void ShutdownEngines()
		{
			foreach (Part p in FlightGlobals.ActiveVessel.Parts)
			{
				foreach (PartModule pm in p.Modules)
				{
					if (!(pm is ModuleEngines)) continue;
					ModuleEngines eng = (ModuleEngines) pm;
					if (!(eng.engineType == EngineType.LiquidFuel | eng.engineType == EngineType.Nuclear)) continue;
					if(!eng.Events["Activate"].active)deactivatedEngines.Add(eng);
					eng.Shutdown();
					eng.Events["Activate"].guiActive = false;
				}
			}
		}

		// Restore engines control without reactivating them
		private void ActivateEngines()
		{
			foreach (Part p in FlightGlobals.ActiveVessel.Parts)
			{
				foreach (PartModule pm in p.Modules)
				{
					if (!(pm is ModuleEngines)) continue;
					ModuleEngines eng = (ModuleEngines) pm;
					if (eng.engineType == EngineType.LiquidFuel | eng.engineType == EngineType.Nuclear)
					{
						eng.Events["Activate"].guiActive = true;
					}

					if (!deactivatedEngines.Contains(eng)) continue;
					eng.Activate();
					deactivatedEngines.Remove(eng);
				}
			}
		}

		// Check if there are disabled engines
		private bool CheckActive()
		{
			foreach (Part p in FlightGlobals.ActiveVessel.Parts)
			{
				foreach (PartModule pm in p.Modules)
				{
					if (!(pm is ModuleEngines)) continue;
					ModuleEngines eng = (ModuleEngines) pm;
					if (eng.Events["Activate"].guiActive == false)
					{
						return true;
					}
				}
			}

			return false;
		}

		// If vessel has disabled engines, then DS was active previously, re-activate override
		private void OnVesselChange(Vessel v)
		{
			if (CheckActive())
			{
				appLauncherButton.SetTrue();
			}
			else
			{
				appLauncherButton.SetFalse();
			}
		}
	}
}
