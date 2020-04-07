using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    class OutsideConnectionDetailPanel : UICustomControl
    {
        class NameRowComponent : TableRowComponent
        {
            private UIPanel m_MainPanel = null;
            private UITextField m_NameTextField = null;
            private UIButton m_DeleteButton = null;

            private new void Awake()
            {
                m_MainPanel = gameObject.AddComponent<UIPanel>();
                m_MainPanel.name = "MainPanel";
                m_MainPanel.size = new Vector2(200, 45);
                m_MainPanel.clipChildren = false;

                base.Awake();

                m_DeleteButton = WidgetsFactory.AddButton(m_MainPanel, CommonSpriteNames.IconClose, "Delete");
                m_DeleteButton.relativePosition = new Vector3(m_MainPanel.width - m_DeleteButton.width, 0);
                m_DeleteButton.tooltip = "Delete name from random list.";
                m_DeleteButton.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
                m_DeleteButton.eventClick += delegate (UIComponent comp, UIMouseEventParameter mouseParam)
                {
                    eventDeleteClicked?.Invoke(m_MainPanel, mouseParam);
                };

                m_NameTextField = WidgetsFactory.AddTextField(m_MainPanel);
                m_NameTextField.relativePosition = new Vector3(2, 0);
                m_NameTextField.size = new Vector2(m_DeleteButton.relativePosition.x - m_NameTextField.relativePosition.x - 2, 30);
                m_NameTextField.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
                m_NameTextField.eventTextSubmitted += delegate (UIComponent comp, string newText)
                {
                    eventNameSubmitted?.Invoke(m_MainPanel, newText);
                };
            }

            public void SetText(string text)
            {
                m_NameTextField.text = text;
            }

            public void EnableDelete(bool enable)
            {
                m_DeleteButton.enabled = enable;
            }

            public event PropertyChangedEventHandler<string> eventNameSubmitted;
            public event MouseEventHandler eventDeleteClicked;
        }

            private ushort m_BuildingID = 0;
        public ushort buildingID => m_BuildingID;
        private OutsideConnectionSettings m_CachedSettings = null;
        private UIPanel m_MainPanel = null;
        private UISprite m_PanelIcon = null;
        private UITextField m_ConnectionNameTextfield = null;
        private UIButton m_PanelClose = null;
        private UIMultiStateButton m_LocationMarkerButton = null;
        private UIPanel m_NameGenerationPanel = null;
        private UIResizeHandle m_BottomResizeHandle = null;
        private UIPanel m_NameModeSubPanel = null;
        private UIScrollablePanel m_RandomNameContainer = null;
        private UIScrollbar m_RandomNameContainerScrollbar = null;
        private UILabel m_RandomNameCountLabel = null;
        private UITextField m_SingleNameTextFrame = null;

        private bool m_Initialized = false;

        private static readonly string m_MainPanelName = "AOC_DetailMainPanel";

        private void LateUpdate()
        {
            if (m_MainPanel.isVisible && m_LocationMarkerButton.isVisible &&
                m_LocationMarkerButton.activeStateIndex != 0 && ToolsModifierControl.cameraController.GetTarget() == InstanceID.Empty)
            {
                m_LocationMarkerButton.activeStateIndex = 0;
            }
        }

        private void OnEnable()
        {
            if (m_Initialized)
                return;

#if DEBUG
            m_MainPanel = UIView.Find<UIPanel>(m_MainPanelName);
            if (m_MainPanel)
            {
                UnityEngine.Object.Destroy(m_MainPanel.gameObject);
                UnityEngine.Object.Destroy(m_MainPanel);
                m_MainPanel = null;

                Utils.Log("Cleared " + m_MainPanelName + " from previous sessions.");
            }
#endif

            var objectOfType = UnityEngine.Object.FindObjectOfType<UIView>();
            transform.parent = objectOfType.transform;

            m_MainPanel = WidgetsFactory.MakeMainPanel(gameObject, m_MainPanelName, new Vector2(500, 500));
            m_MainPanel.name = m_MainPanelName;
            m_MainPanel.relativePosition = new Vector3(900, 0f);
            m_MainPanel.minimumSize = new Vector2(m_MainPanel.width, 150);

            InitCaptionArea();
            m_BottomResizeHandle = WidgetsFactory.AddPanelBottomResizeHandle(m_MainPanel);
            InitNameGenerationPanel();

            m_MainPanel.minimumSize = new Vector2(m_MainPanel.width, m_NameGenerationPanel.relativePosition.y + m_NameGenerationPanel.minimumSize.y + m_BottomResizeHandle.height);
            m_MainPanel.Hide();

            m_Initialized = true;
        }

        private void InitCaptionArea()
        {
            m_PanelIcon = WidgetsFactory.AddPanelIcon(m_MainPanel, CommonSpriteNames.IconOutsideConnections.normal);
            WidgetsFactory.AddPanelDragHandle(m_MainPanel);
            m_PanelClose = WidgetsFactory.AddPanelCloseButton(m_MainPanel);
            m_LocationMarkerButton = WidgetsFactory.AddMultistateButton(m_MainPanel, CommonSpriteNames.LocationMarkerActive, CommonSpriteNames.LocationMarker, "LocationMarkerButton");
            m_LocationMarkerButton.relativePosition = new Vector3(m_PanelClose.relativePosition.x - m_LocationMarkerButton.width - 2, 4);

            m_ConnectionNameTextfield = WidgetsFactory.AddTextField(m_MainPanel, "Testtext", true);
            m_ConnectionNameTextfield.size = new Vector2(300, 32);
            m_ConnectionNameTextfield.relativePosition = new Vector3((m_MainPanel.width - m_ConnectionNameTextfield.width) / 2, 5);
            m_ConnectionNameTextfield.anchor = UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Left;

            m_LocationMarkerButton.eventClick += OnLocationMarkerClicked;
            m_MainPanel.eventVisibilityChanged += OnVisibilityChanged;
            m_ConnectionNameTextfield.eventTextSubmitted += delegate (UIComponent component, string newText)
            {
                if (m_CachedSettings != null && m_CachedSettings.Name != newText)
                {
                    m_CachedSettings.Name = newText;
                    eventNameChanged?.Invoke(m_BuildingID, newText);
                }
            };
        }

        private void InitNameGenerationPanel()
        {
            m_NameGenerationPanel = m_MainPanel.AddUIComponent<UIPanel>();
            m_NameGenerationPanel.name = "NameGenerationGroupPanel";
            m_NameGenerationPanel.backgroundSprite = CommonSpriteNames.GenericPanel;
            m_NameGenerationPanel.width = m_MainPanel.width - 10;
            m_NameGenerationPanel.height = 300;
            m_NameGenerationPanel.color = new Color32(206, 206, 206, 255);
            m_NameGenerationPanel.relativePosition = new Vector3(5, m_BottomResizeHandle.relativePosition.y - m_NameGenerationPanel.height);
            m_NameGenerationPanel.anchor = UIAnchorStyle.All;

            var label = WidgetsFactory.AddLabel(m_NameGenerationPanel, "Name Generation", true);
            label.width = m_NameGenerationPanel.width;
            label.relativePosition = new Vector3(100, 15);
            label.textAlignment = UIHorizontalAlignment.Center;
            label.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.Top;

            var checkboxDefs = new KeyValuePair<string, int>[Utils.MaxEnumValue<OutsideConnectionSettings.NameModeType>() + 1];
            for (var i = Utils.MinEnumValue<OutsideConnectionSettings.NameModeType>(); i <= Utils.MaxEnumValue<OutsideConnectionSettings.NameModeType>(); ++i)
            {
                var enumVal = (OutsideConnectionSettings.NameModeType)i;
                checkboxDefs[i] = new KeyValuePair<string, int>( enumVal.ToString(), i );
            };
            m_NameModeSubPanel = WidgetsFactory.AddGroupedCheckBoxes(m_NameGenerationPanel, checkboxDefs, 20, true,
                new PropertyChangedEventHandler<bool>(OnNameGenerationModeChanged), "NameGenerationModeCheckBox");
            m_NameModeSubPanel.size = new Vector2(160, 110);
            //m_NameModeSubPanel.backgroundSprite = CommonSpriteNames.EmptySprite;
            m_NameModeSubPanel.relativePosition = new Vector3(m_NameGenerationPanel.width - m_NameModeSubPanel.width - 5, label.relativePosition.y + label.height + 40);
            foreach (var comp in m_NameModeSubPanel.components)
            {
                var checkbox = comp as UICheckBox;
                if (checkbox && checkbox.label)
                {
                    checkbox.label.color = Color.white;
                }
            }

            m_RandomNameContainer = WidgetsFactory.AddScrollablePanel(m_NameGenerationPanel);
            m_RandomNameContainer.relativePosition = new Vector3(5, label.relativePosition.y + label.height + 15);
            m_RandomNameContainer.width = m_NameModeSubPanel.relativePosition.x - m_RandomNameContainer.relativePosition.x - WidgetsFactory.DefaultVScrollbarWidth - 5;
            m_RandomNameContainer.height = m_NameGenerationPanel.height - m_RandomNameContainer.relativePosition.y - 5;
            m_RandomNameContainer.backgroundSprite = CommonSpriteNames.EmptySprite;
            m_RandomNameContainer.color = Color.grey;
            m_RandomNameContainerScrollbar = WidgetsFactory.AddVerticalScrollbar(m_NameGenerationPanel, m_RandomNameContainer);

            m_SingleNameTextFrame = WidgetsFactory.AddTextField(m_NameGenerationPanel);
            m_SingleNameTextFrame.size = new Vector2(m_RandomNameContainer.width, 40);
            m_SingleNameTextFrame.relativePosition = m_RandomNameContainer.relativePosition;
            m_SingleNameTextFrame.normalBgSprite = CommonSpriteNames.EmptySprite;
            m_SingleNameTextFrame.color = Color.black;
            m_SingleNameTextFrame.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Top;
            m_SingleNameTextFrame.eventTextSubmitted += OnSingleNameSubmitted;

            m_RandomNameCountLabel = WidgetsFactory.AddLabel(m_MainPanel, "", true, "NameGenerationRandomCountLabel");
            m_RandomNameCountLabel.relativePosition = m_BottomResizeHandle.relativePosition + new Vector3(10, 10);
            m_RandomNameCountLabel.anchor = UIAnchorStyle.Bottom | UIAnchorStyle.Left;
            m_RandomNameCountLabel.prefix = "Random Names-Count: ";
            int oldZ = m_RandomNameCountLabel.zOrder;
            m_RandomNameCountLabel.zOrder = m_BottomResizeHandle.zOrder;
            m_BottomResizeHandle.zOrder = oldZ;

            m_NameGenerationPanel.minimumSize = new Vector2(m_NameGenerationPanel.width, m_NameModeSubPanel.relativePosition.y + m_NameModeSubPanel.height);
        }

        private void OnSingleNameSubmitted(UIComponent component, string text)
        {
            if (m_BuildingID == 0 || m_CachedSettings == null)
                return;

            m_CachedSettings.SingleGenerationName = text;
        }

        private void OnNameGenerationModeChanged(UIComponent component, bool isChecked)
        {
            if (!isChecked || m_BuildingID == 0 || m_CachedSettings == null)
                return;

            var checkbox = component as UICheckBox;
            if (checkbox)
            {
                m_CachedSettings.NameMode = (OutsideConnectionSettings.NameModeType)checkbox.objectUserData;
                AdjustNameGenerationComponentVisibility();
            }
        }

        private void AdjustNameGenerationComponentVisibility()
        {
            m_RandomNameCountLabel.Hide();
            m_RandomNameContainerScrollbar.Hide();
            m_RandomNameContainer.Hide();
            m_SingleNameTextFrame.Hide();

            switch (m_CachedSettings.NameMode)
            {
                case OutsideConnectionSettings.NameModeType.CustomRandom:
                    m_RandomNameContainerScrollbar.Show();
                    m_RandomNameContainer.Show();
                    m_RandomNameCountLabel.Show();
                    break;
                case OutsideConnectionSettings.NameModeType.CustomSingle:
                    m_SingleNameTextFrame.Show();
                    break;
                default:
                    break;
            }
        }

        public event OutsideConnectionPropertyChanged<string> eventNameChanged;

        public void RefreshData()
        {
            if (m_CachedSettings == null)
                return;

            m_ConnectionNameTextfield.text = m_CachedSettings.Name;
            foreach (var comp in m_NameModeSubPanel.components)
            {
                var checkbox = comp as UICheckBox;
                if (checkbox)
                    checkbox.isChecked = (int)checkbox.objectUserData == (int)m_CachedSettings.NameMode;
            }

            m_SingleNameTextFrame.text = m_CachedSettings.SingleGenerationName;

            //// add one additional row for inserting
            while (m_RandomNameContainer.components.Count <= m_CachedSettings.RandomGenerationNames.Count)
            {
                var gameObject = new GameObject("AOCRandomNamePanelComponent", new System.Type[] { typeof(NameRowComponent) });
                m_RandomNameContainer.AttachUIComponent(gameObject);
                var nameRowComponent = gameObject.GetComponent<NameRowComponent>();
                nameRowComponent.component.width = m_RandomNameContainer.width - 4;
                nameRowComponent.eventDeleteClicked += OnRandomGenerationNameDeleted;
                nameRowComponent.eventNameSubmitted += OnRandomGenerationNameSubmitted;
                Utils.Log("added name row");
            }

            while (m_RandomNameContainer.components.Count > m_CachedSettings.RandomGenerationNames.Count + 1)
            {
                var component = m_RandomNameContainer.components[0];
                var nameRowComponent = component.gameObject.GetComponent<NameRowComponent>();
                nameRowComponent.eventDeleteClicked -= OnRandomGenerationNameDeleted;
                nameRowComponent.eventNameSubmitted -= OnRandomGenerationNameSubmitted;
                m_RandomNameContainer.RemoveUIComponent(component);
                UnityEngine.Object.Destroy(component.gameObject);
                Utils.Log("deleted name row");
            }

            var textCount = 0;
            foreach (var text in m_CachedSettings.RandomGenerationNames)
            {
                var nameComponent = m_RandomNameContainer.components[textCount].GetComponent<NameRowComponent>();
                nameComponent.SetText(text);
                nameComponent.EnableDelete(true);
                ++textCount;
            }

            var lastComponent = m_RandomNameContainer.components[textCount].GetComponent<NameRowComponent>();
            lastComponent.SetText(string.Empty);
            lastComponent.EnableDelete(false);

            m_RandomNameCountLabel.text = (m_RandomNameContainer.components.Count - 1).ToString();
        }

        private void OnRandomGenerationNameDeleted(UIComponent comp, UIMouseEventParameter mouseParam)
        {
            int i = 0;
            foreach (var panel in m_RandomNameContainer.components)
            {
                //var panelComp = panel.gameObject.GetComponent<NameRowComponent>();
                //if (panelComp != null)
                //{
                if (panel == comp)
                {
                    if (i + 1 == m_RandomNameContainer.components.Count)
                    {
                        // just ignore
                    }
                    else
                        m_CachedSettings.RandomGenerationNames.RemoveAt(i);
                    break;
                }
                ++i;
                //}
            }
            RefreshData();
        }

        private void OnRandomGenerationNameSubmitted(UIComponent comp, string newText)
        {
            int i = 0;
            foreach (var panel in m_RandomNameContainer.components)
            {
                //var panelComp = panel.gameObject.GetComponent<NameRowComponent>();
                //if (panelComp != null)
                //{
                    if (panel == comp)
                    {
                        if (i + 1 == m_RandomNameContainer.components.Count)
                        {
                            m_CachedSettings.RandomGenerationNames.Add(newText);
                        }
                        else
                            m_CachedSettings.RandomGenerationNames[i] = newText;
                    break;
                    }
                    ++i;
                //}
            }
            RefreshData();
        }

        public void ChangeTarget(ushort buildingID)
        {
            if (buildingID == 0 || !OutsideConnectionSettingsManager.instance.SettingsDict.TryGetValue(buildingID, out m_CachedSettings))
            {
                m_BuildingID = 0;
                m_CachedSettings = null;
                return;
            }

            m_BuildingID = buildingID;

            RefreshData();
            AdjustNameGenerationComponentVisibility();
        }

        private void ZoomToLocation()
        {
            if (m_BuildingID == 0)
                return;

            var building = BuildingManager.instance.m_buildings.m_buffer[m_BuildingID];
            var instanceID = default(InstanceID);
            instanceID.Building = m_BuildingID;
            ToolsModifierControl.cameraController.SetTarget(instanceID, building.m_position, true);
        }

        private void OnLocationMarkerClicked(UIComponent component, UIMouseEventParameter mouseParam)
        {
            if (mouseParam.source == m_LocationMarkerButton)
            {
                if (m_LocationMarkerButton.activeStateIndex != 1)
                {
                    Utils.Log("reset cam");
                    ToolsModifierControl.cameraController.SetTarget(InstanceID.Empty, Vector3.zero, false);
                }
                else
                {
                    ZoomToLocation();
                }
            }
        }

        private void OnVisibilityChanged(UIComponent component, bool isVisibly)
        {
            enabled = isVisibly;
            if (isVisibly)
            {
                if (!ToolsModifierControl.cameraController.m_unlimitedCamera)
                    m_LocationMarkerButton.Hide();
                else
                {
                    m_LocationMarkerButton.Show();
                    //m_LocationMarkerButton.SimulateClick();
                }
            }
            else
                m_LocationMarkerButton.activeStateIndex = 0;
        }
    }
}
