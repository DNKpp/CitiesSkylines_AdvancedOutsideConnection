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

                m_DeleteButton = WidgetsFactory.AddButton(m_MainPanel, CommonSprites.IconClose, "Delete");
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
        private UIMultiStateButton m_ShowHideRoutesButton = null;
        private UIPanel m_NameGenerationPanel = null;
        private UIResizeHandle m_BottomResizeHandle = null;
        private UIPanel m_NameModeSubPanel = null;
        private UIScrollablePanel m_RandomNameContainer = null;
        private UIScrollbar m_RandomNameContainerScrollbar = null;
        private UILabel m_RandomNameCountLabel = null;
        private UITextField m_SingleNameTextFrame = null;
        private UIPanel m_SettingsPanel = null;
        private UILabel m_DirectionLabel = null;
        private UICheckBox m_DirectionInCheckbox = null;
        private UICheckBox m_DirectionOutCheckbox = null;
        private UILabel m_TransportTypeLabel = null;

        private bool m_Initialized = false;
        private bool m_IsRefreshing = false;

        private static readonly string m_MainPanelName = "AOC_DetailMainPanel";

        private void LateUpdate()
        {
            if (!m_MainPanel.isVisible)
                return;
            if (m_LocationMarkerButton && m_LocationMarkerButton.isVisible)
            {
                m_LocationMarkerButton.activeStateIndex = ToolsModifierControl.cameraController.GetTarget() == InstanceID.Empty ? 0 : 1;
            }
            if (m_ShowHideRoutesButton)
            {
                m_ShowHideRoutesButton.activeStateIndex = (Utils.IsRoutesViewOn() ? 1 : 0);
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
            InitSettingsPanel();

            m_BottomResizeHandle = WidgetsFactory.AddPanelBottomResizeHandle(m_MainPanel);
            InitNameGenerationPanel();

            m_MainPanel.minimumSize = new Vector2(m_MainPanel.width, m_NameGenerationPanel.relativePosition.y + m_NameGenerationPanel.minimumSize.y + m_BottomResizeHandle.height);
            m_MainPanel.Hide();

            m_Initialized = true;
        }

        private void InitSettingsPanel()
        {
            m_SettingsPanel = m_MainPanel.AddUIComponent<UIPanel>();
            m_SettingsPanel.name = "SettingsPanel";
            m_SettingsPanel.backgroundSprite = CommonSprites.GenericPanel;
            m_SettingsPanel.width = m_MainPanel.width - 10;
            m_SettingsPanel.height = 100;
            m_SettingsPanel.color = new Color32(206, 206, 206, 255);
            m_SettingsPanel.relativePosition = new Vector3(5, m_PanelIcon.relativePosition.y + m_PanelIcon.height + 5);
            m_SettingsPanel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top | UIAnchorStyle.Right;

            m_TransportTypeLabel = WidgetsFactory.AddLabel(m_SettingsPanel, "", false, "TransportTypeLabel");
            m_TransportTypeLabel.autoSize = true;
            m_TransportTypeLabel.textScale = 0.7f;
            m_TransportTypeLabel.relativePosition = new Vector3(10, 10);

            m_DirectionLabel = WidgetsFactory.AddLabel(m_SettingsPanel, "", false, "DirectionLabel");
            m_DirectionLabel.text = "Direction";
            m_DirectionLabel.autoSize = false;
            m_DirectionLabel.width = 100;
            m_DirectionLabel.autoHeight = true;
            m_DirectionLabel.textAlignment = UIHorizontalAlignment.Center;
            m_DirectionLabel.textScale = 0.7f;
            m_DirectionLabel.relativePosition = new Vector3(m_SettingsPanel.width - m_DirectionLabel.width - 10, 10);
            //m_DirectionLabel.backgroundSprite = CommonSpriteNames.EmptySprite;

            m_DirectionInCheckbox = WidgetsFactory.AddCheckBox(m_SettingsPanel, "In", Building.Flags.Outgoing, OnDirectionCheckboxChanged, "DirectionInCheckBox");
            m_DirectionInCheckbox.relativePosition = m_DirectionLabel.relativePosition + new Vector3(0, 20);
            m_DirectionInCheckbox.label.textScale = 0.7f;
            m_DirectionOutCheckbox = WidgetsFactory.AddCheckBox(m_SettingsPanel, "Out", Building.Flags.Incoming, OnDirectionCheckboxChanged, "DirectionOutCheckBox");
            m_DirectionOutCheckbox.relativePosition = m_DirectionInCheckbox.relativePosition + new Vector3(50, 0);
            m_DirectionOutCheckbox.label.textScale = 0.7f;
        }

        private void InitCaptionArea()
        {
            m_PanelIcon = WidgetsFactory.AddPanelIcon(m_MainPanel, CommonSprites.IconOutsideConnections.normal);
            WidgetsFactory.AddPanelDragHandle(m_MainPanel);
            m_PanelClose = WidgetsFactory.AddPanelCloseButton(m_MainPanel);
            m_PanelClose.eventClick += delegate { OnClose(); };

            var backgroundSprites = new CommonSprites.SpriteSet[2];
            backgroundSprites[0] = new CommonSprites.SpriteSet(CommonSprites.InfoIconBase);
            backgroundSprites[0].focused = "";
            backgroundSprites[1] = new CommonSprites.SpriteSet(CommonSprites.InfoIconBase);
            backgroundSprites[1].focused = "";
            backgroundSprites[1].normal = CommonSprites.InfoIconBase.focused;
            var foregroundSprites = new CommonSprites.SpriteSet[2];
            foregroundSprites[0] = new CommonSprites.SpriteSet(CommonSprites.InfoIconTrafficRoutes);
            foregroundSprites[0].focused = "";
            foregroundSprites[1] = new CommonSprites.SpriteSet(CommonSprites.InfoIconTrafficRoutes);
            foregroundSprites[1].focused = "";
            foregroundSprites[1].normal = CommonSprites.InfoIconTrafficRoutes.focused;
            m_ShowHideRoutesButton = WidgetsFactory.AddMultistateButton(m_MainPanel, foregroundSprites, backgroundSprites, "ShowHideRoutesButton");
            m_ShowHideRoutesButton.relativePosition = new Vector3(m_PanelClose.relativePosition.x - m_ShowHideRoutesButton.width - 2, 4);

            backgroundSprites = new CommonSprites.SpriteSet[2];
            backgroundSprites[0] = new CommonSprites.SpriteSet(CommonSprites.LocationMarker);
            backgroundSprites[0].focused = "";
            backgroundSprites[1] = new CommonSprites.SpriteSet(CommonSprites.LocationMarkerActive);
            backgroundSprites[1].focused = "";
            m_LocationMarkerButton = WidgetsFactory.AddMultistateButton(m_MainPanel, null, backgroundSprites, "LocationMarkerButton");
            m_LocationMarkerButton.relativePosition = new Vector3(m_ShowHideRoutesButton.relativePosition.x - m_LocationMarkerButton.width - 2, 4);

            m_ConnectionNameTextfield = WidgetsFactory.AddTextField(m_MainPanel, "Testtext", true);
            m_ConnectionNameTextfield.size = new Vector2(290, 32);
            m_ConnectionNameTextfield.relativePosition = new Vector3((m_MainPanel.width - m_ConnectionNameTextfield.width) / 2, 5);
            m_ConnectionNameTextfield.anchor = UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Left;

            m_ShowHideRoutesButton.eventClick += OnShowHideRoutesClicked;
            m_LocationMarkerButton.eventClick += OnLocationMarkerClicked;
            m_MainPanel.eventVisibilityChanged += OnVisibilityChanged;
            m_ConnectionNameTextfield.eventTextSubmitted += delegate (UIComponent component, string newName)
            {
                if (m_BuildingID != 0 && m_CachedSettings != null)
                    eventNameChanged?.Invoke(m_BuildingID, newName);
            };
        }

        private void OnShowHideRoutesClicked(UIComponent component, UIMouseEventParameter mouseParam)
        {
            if (Utils.IsRoutesViewOn())
            {
                InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            }
            else
            {
                SelectOutsideConnectionBuilding();
                InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.TrafficRoutes, InfoManager.SubInfoMode.Default);
            }
        }

        private void OnClose()
        {
            m_MainPanel.Hide();
        }

        private void InitNameGenerationPanel()
        {
            m_NameGenerationPanel = m_MainPanel.AddUIComponent<UIPanel>();
            m_NameGenerationPanel.name = "NameGenerationGroupPanel";
            m_NameGenerationPanel.backgroundSprite = CommonSprites.GenericPanel;
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
            m_RandomNameContainer.backgroundSprite = CommonSprites.EmptySprite;
            m_RandomNameContainer.color = Color.grey;
            m_RandomNameContainerScrollbar = WidgetsFactory.AddVerticalScrollbar(m_NameGenerationPanel, m_RandomNameContainer);

            m_SingleNameTextFrame = WidgetsFactory.AddTextField(m_NameGenerationPanel);
            m_SingleNameTextFrame.size = new Vector2(m_RandomNameContainer.width, 45);
            m_SingleNameTextFrame.relativePosition = m_RandomNameContainer.relativePosition;
            m_SingleNameTextFrame.normalBgSprite = CommonSprites.InfoViewPanel;//CommonSpriteNames.EmptySprite;
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

        private void OnDirectionCheckboxChanged(UIComponent component, bool isChecked)
        {
            if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
                return;

            var buildingFlags = Utils.QueryBuilding(m_BuildingID).m_flags;
            var flag = (Building.Flags)(component as UICheckBox).objectUserData;

            // do not allow to apply/remove flags, which weren't set in the original
            if ((m_CachedSettings.OriginalDirectionFlags & flag) == 0)
                return;
            Utils.Log("flag before: " + buildingFlags);
            if (isChecked)
                buildingFlags |= flag;
            else
                buildingFlags &= ~flag;
            Utils.Log("flag after: " + buildingFlags);

            BuildingManager.instance.m_buildings.m_buffer[m_BuildingID].m_flags = buildingFlags;     // need to push back the copy

            eventDirectionChanged?.Invoke(m_BuildingID, buildingFlags & Building.Flags.IncomingOutgoing);
        }

        private void OnSingleNameSubmitted(UIComponent component, string text)
        {
            if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
                return;

            m_CachedSettings.SingleGenerationName = text;
        }

        private void OnNameGenerationModeChanged(UIComponent component, bool isChecked)
        {
            if (m_IsRefreshing || !isChecked || m_BuildingID == 0 || m_CachedSettings == null)
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
        public event OutsideConnectionPropertyChanged<Building.Flags> eventDirectionChanged;

        public void RefreshData()
        {
            if (m_BuildingID == 0 || m_CachedSettings == null)
                return;

            m_IsRefreshing = true;

            var transportInfo = Utils.QueryTransportInfo(m_BuildingID);
            var building = Utils.QueryBuilding(m_BuildingID);
            m_PanelIcon.spriteName = CommonSprites.SubBarPublicTransport[(int)transportInfo.m_transportType];
            m_ConnectionNameTextfield.text = BuildingManager.instance.GetBuildingName(m_BuildingID, InstanceID.Empty);
            m_TransportTypeLabel.text = transportInfo.m_transportType.ToString();
            m_DirectionInCheckbox.isChecked = (building.m_flags & Building.Flags.Incoming) != 0;
            m_DirectionInCheckbox.readOnly = (m_CachedSettings.OriginalDirectionFlags & Building.Flags.Incoming) == 0;
            m_DirectionOutCheckbox.isChecked = (building.m_flags & Building.Flags.Outgoing) != 0;
            m_DirectionOutCheckbox.readOnly = (m_CachedSettings.OriginalDirectionFlags & Building.Flags.Outgoing) == 0;

            foreach (var comp in m_NameModeSubPanel.components)
            {
                var checkbox = comp as UICheckBox;
                if (checkbox)
                    checkbox.isChecked = (int)checkbox.objectUserData == (int)m_CachedSettings.NameMode;
            }

            m_SingleNameTextFrame.text = m_CachedSettings.SingleGenerationName;

            // add one additional row for inserting
            while (m_RandomNameContainer.components.Count <= m_CachedSettings.RandomGenerationNames.Count)
            {
                var gameObject = new GameObject("AOCRandomNamePanelComponent", new System.Type[] { typeof(NameRowComponent) });
                m_RandomNameContainer.AttachUIComponent(gameObject);
                var nameRowComponent = gameObject.GetComponent<NameRowComponent>();
                nameRowComponent.component.width = m_RandomNameContainer.width - 4;
                nameRowComponent.eventDeleteClicked += OnRandomGenerationNameDeleted;
                nameRowComponent.eventNameSubmitted += OnRandomGenerationNameSubmitted;
            }

            while (m_RandomNameContainer.components.Count > m_CachedSettings.RandomGenerationNames.Count + 1)
            {
                var component = m_RandomNameContainer.components[0];
                var nameRowComponent = component.gameObject.GetComponent<NameRowComponent>();
                nameRowComponent.eventDeleteClicked -= OnRandomGenerationNameDeleted;
                nameRowComponent.eventNameSubmitted -= OnRandomGenerationNameSubmitted;
                m_RandomNameContainer.RemoveUIComponent(component);
                UnityEngine.Object.Destroy(component.gameObject);
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

            m_IsRefreshing = false;
        }

        private void OnRandomGenerationNameDeleted(UIComponent comp, UIMouseEventParameter mouseParam)
        {
            int i = 0;
            foreach (var panel in m_RandomNameContainer.components)
            {
                if (panel == comp)
                {
                    if (i < m_RandomNameContainer.components.Count)
                        m_CachedSettings.RandomGenerationNames.RemoveAt(i);
                    break;
                }
                ++i;
            }
            RefreshData();
        }

        private void OnRandomGenerationNameSubmitted(UIComponent comp, string newText)
        {
            if (m_IsRefreshing)
                return;

            int i = 0;
            foreach (var panel in m_RandomNameContainer.components)
            {
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
            }
            RefreshData();
        }

        private void SelectOutsideConnectionBuilding()
        {
            m_BuildingID = buildingID;
            var instance = new InstanceID();
            instance.Building = m_BuildingID;
            InstanceManager.instance.SelectInstance(instance);
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

            if (m_ShowHideRoutesButton.activeStateIndex == 1)
                SelectOutsideConnectionBuilding();
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
