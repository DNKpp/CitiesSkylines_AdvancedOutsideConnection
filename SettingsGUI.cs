using System.Collections.Generic;
using UnityEngine;

namespace ImprovedOutsideConnection
{
    public class SettingsGUI : MonoBehaviour
    {
        private Texture2D m_BGTexture;
        private GUISkin m_Skin;
        private bool m_ShowRequest;
        private Vector2 m_ScrollPosition = new Vector2(0,0);

        private Dictionary<ushort, OutsideConnectionSettings> m_TempSettingsDict;

#if DEBUG
        private Rect m_WindowRect = new Rect(64, 64, 500, 500);
#else
        private Rect windowRect = new Rect(64, 64, 350, 170);
#endif

        public void Awake()
        {
            m_BGTexture = new Texture2D(1, 1);
            m_BGTexture.SetPixel(0, 0, Color.grey);
            m_BGTexture.Apply();
        }

        public void Update()
        {
            if (ImprovedOutsideConnectionMod.InGame)
            {
                if (Input.GetKeyDown(KeyCode.F10))
                {
                    OutsideConnectionSettingsManager.instance.SyncWithBuildingManager();
                    m_TempSettingsDict = new Dictionary<ushort, OutsideConnectionSettings>(OutsideConnectionSettingsManager.instance.m_SettingsDict);
                    m_ShowRequest = !m_ShowRequest;
                }
            }
        }

        public void OnGUI()
        {
            if (!m_Skin)
            {
                Setup();
            }

            if (m_ShowRequest)
            {
                var oldSkin = GUI.skin;
                GUI.skin = m_Skin;
                m_WindowRect = GUI.Window(1337, m_WindowRect, DoConfigWindow, "Improved Outside Connections");
                GUI.skin = oldSkin;
            }
        }

        private void DoConfigWindow(int wnd)
        {
            int i = 0;
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            foreach(var setting in m_TempSettingsDict)
            {
                var values = setting.Value;
                GUILayout.Label("Outside Connection " + (i + 1));
                GUILayout.Label("Type: " + values.Type.ToString());
                GUILayout.Label("Position: " + values.Position.x + " / " + values.Position.z);
                GUILayout.BeginHorizontal();
                /*GUILayout.Label("Name Mode: " + values.NameMode.ToString());
                values.NameMode = (OutsideConnectionSettings.NameModeType)GUILayout.HorizontalSlider((int)values.NameMode,
                    Utils.MinEnumValue<OutsideConnectionSettings.NameModeType>(), Utils.MaxEnumValue<OutsideConnectionSettings.NameModeType>(), GUILayout.Width(150));*/
                for (int j = Utils.MinEnumValue<OutsideConnectionSettings.NameModeType>(); j <= Utils.MaxEnumValue<OutsideConnectionSettings.NameModeType>(); ++j)
                {
                    if (GUILayout.Toggle(j == (int)values.NameMode, ((OutsideConnectionSettings.NameModeType)j).ToString()))
                        values.NameMode = (OutsideConnectionSettings.NameModeType)j;
                }
                GUILayout.EndHorizontal();
                values.NameText = GUILayout.TextField(values.NameText);
                GUILayout.Space(20);

                ++i;
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                m_TempSettingsDict = new Dictionary<ushort, OutsideConnectionSettings>(OutsideConnectionSettingsManager.instance.m_SettingsDict);
            }

            if (GUILayout.Button("Cancel"))
            {
                m_TempSettingsDict = null;
                m_ShowRequest = false;
            }

            if (GUILayout.Button("Apply"))
            {
                OutsideConnectionSettingsManager.instance.m_SettingsDict = new Dictionary<ushort, OutsideConnectionSettings>(m_TempSettingsDict);
                m_ShowRequest = false;
            }

            GUILayout.EndHorizontal();
        }

        public void Setup()
        {
            m_Skin = ScriptableObject.CreateInstance<GUISkin>();
            m_Skin.box = new GUIStyle(GUI.skin.box);
            m_Skin.button = new GUIStyle(GUI.skin.button);
            m_Skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
            m_Skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
            m_Skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
            m_Skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
            m_Skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
            m_Skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            m_Skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
            m_Skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
            m_Skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            m_Skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
            m_Skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
            m_Skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
            m_Skin.label = new GUIStyle(GUI.skin.label);
            m_Skin.scrollView = new GUIStyle(GUI.skin.scrollView);
            m_Skin.textArea = new GUIStyle(GUI.skin.textArea);
            m_Skin.textField = new GUIStyle(GUI.skin.textField);
            m_Skin.toggle = new GUIStyle(GUI.skin.toggle);
            m_Skin.window = new GUIStyle(GUI.skin.window);
            m_Skin.window.normal.background = m_BGTexture;
            m_Skin.window.onNormal.background = m_BGTexture;
        }
    }
}
