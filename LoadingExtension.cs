
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using ICities;
using Harmony;
using UnityEngine;
using ColossalFramework.Plugins;
using System.Collections.Generic;

namespace AdvancedOutsideConnection
{
	public class LoadingExtension : LoadingExtensionBase
	{
		public static bool InGame { get; internal set; } = false;

		private static string m_HarmonyIdentifier = "connection.outside.advanced";

		HarmonyInstance m_HarmonyInstance = null;

		private GameObject m_GameObject = null;

		public override void OnCreated(ILoading loading)
		{
			base.OnCreated(loading);

			m_HarmonyInstance = HarmonyInstance.Create(m_HarmonyIdentifier);

			if (loading.loadingComplete)
			{
				switch (loading.currentMode)
				{
					case AppMode.Game:
						Init(true);
						break;
				}
			}
		}

		public override void OnLevelLoaded(LoadMode mode)
		{
			base.OnLevelLoaded(mode);

			switch (mode)
			{
				case LoadMode.NewGame:
				case LoadMode.LoadGame:
					Init(false);
					break;
			}
		}

		public override void OnReleased()
		{
			base.OnReleased();
			if (InGame)
			{
				Deinit();
				m_HarmonyInstance = null;
			}
		}

		public override void OnLevelUnloading()
		{
			base.OnLevelUnloading();

			if (!InGame)
				return;

			Deinit();
		}

		private void Init(bool late)
		{
			InGame = true;

			// This is needed in cases where the mod is hot reloaded. Due to its implementation, the SerializableDataExtension OnCreated function will only be called during game loads.
			// This way we manually force the call, so we can relay on correct behavior afterwards.
			// Fails unfortunatly OnSave. Don't know whats going wrong.
#if DEBUG
			if (late)
			{
				Utils.Log("LoadingExtension Init late.");
				m_HarmonyInstance.UnpatchAll(m_HarmonyIdentifier);

				var serDataWrapper = SimulationManager.instance.m_SerializableDataWrapper;
				if (serDataWrapper != null)
				{
					var serializableDataExtensions = PluginManager.instance.GetImplementations<ISerializableDataExtension>();
					Utils.Log("serializableDataExtensions count: " + serializableDataExtensions.Count);
					foreach (var extension in serializableDataExtensions)
					{
						var own = extension as SerializableDataExtension;
						own?.OnCreated(serDataWrapper);
					}
					Traverse.Create(serDataWrapper).Field<List<ISerializableDataExtension>>("m_SerializableDataExtensions").Value = serializableDataExtensions;
				}
			}
#endif

			m_HarmonyInstance.PatchAll();
			SerializableDataExtension.instance.Loaded = true;
 
			var outConMgr = OutsideConnectionSettingsManager.instance;
			outConMgr.Init();
			outConMgr.SyncWithBuildingManager();

			//var objectOfType = UnityEngine.Object.FindObjectOfType<UIView>();
			m_GameObject = new GameObject("AOCGameObject");
			//m_GameObject.transform.parent = objectOfType.transform;
			m_GameObject.AddComponent<OverviewPanelExtension>();
		}

		private void Deinit()
		{
			try
			{
				Utils.Log("LoadingExtension Deinit.");
				m_HarmonyInstance.UnpatchAll(m_HarmonyIdentifier);

				SerializableDataExtension.instance.Loaded = false;

				if (m_GameObject)
					UnityEngine.Object.Destroy(m_GameObject);

				OutsideConnectionSettingsManager.instance.Deinit();
			}
			finally
			{
				InGame = false;
			}
		}
	}
}
