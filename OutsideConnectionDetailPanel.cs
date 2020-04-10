
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    using fw = framework;

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

                base.Awake();

                m_DeleteButton = fw.ui.UIHelper.AddButton(m_MainPanel).
                    SetName("Delete").
                    SetBackgroundSprites(fw.PreparedSpriteSets.IconClose).
                    MoveInnerRightOf(m_MainPanel).
                    SetTooltip("Delete entry from list.").
                    SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.CenterVertical).
                    GetButton();
                m_DeleteButton.eventClick += delegate (UIComponent comp, UIMouseEventParameter mouseParam)
                {
                    eventDeleteClicked?.Invoke(m_MainPanel, mouseParam);
                };

                m_NameTextField = fw.ui.UIHelper.AddTextField(m_MainPanel).
                    SetHeight(35).
                    SetRelativePosition(new Vector3(5, 0)).
                    ExpandHorizontallyUntil(m_DeleteButton, 5).
                    SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.CenterVertical).
                    GetTextField(true);
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
        private UIMultiStateButton m_LocationMarkerButton = null;
        private UIMultiStateButton m_ShowHideRoutesButton = null;
        private UIPanel m_NameGenerationPanel = null;
        private UIResizeHandle m_BottomResizeHandle = null;
        private UIScrollablePanel m_RandomNameContainer = null;
        private UIScrollbar m_RandomNameContainerScrollbar = null;
        private UILabel m_RandomNameCountLabel = null;
        private UITextField m_SingleNameTextFrame = null;
        private UIPanel m_SettingsPanel = null;
        private UILabel m_DirectionLabel = null;
        private UICheckBox m_DirectionInCheckbox = null;
        private UICheckBox m_DirectionOutCheckbox = null;
        private UILabel m_TransportTypeLabel = null;
        private UITextField m_DummyTrafficFactorTextfield = null;
        private UICheckBox[] m_NameModeCheckBoxes = null;
        private UITextField[] m_TouristFactorTextFields = null;
        private UILabel[] m_TouristFactorLabels = null;

        private bool m_Initialized = false;
        private bool m_IsRefreshing = false;

        private static readonly string m_MainPanelName = "AOC_DetailMainPanel";

        public event OutsideConnectionPropertyChanged<string> eventNameChanged;
        public event OutsideConnectionPropertyChanged<Building.Flags> eventDirectionChanged;

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

            m_MainPanel = fw.ui.UIHelper.MakeMainPanel(gameObject).
                SetName(m_MainPanelName).
                SetSize(new Vector2(500, 500)).
                SetMinimumSize(new Vector2(500, 150)).
                SetClipChildren(false).
                GetPanel();

            InitCaptionArea();
            InitSettingsPanel();

            m_BottomResizeHandle = fw.ui.UIHelper.AddResizeHandle(m_MainPanel, UIResizeHandle.ResizeEdge.Bottom).GetResizeHandle();
            InitNameGenerationPanel();

            m_MainPanel.minimumSize = new Vector2(m_MainPanel.width, m_NameGenerationPanel.relativePosition.y + m_NameGenerationPanel.minimumSize.y + m_BottomResizeHandle.height);
            m_MainPanel.Hide();

            m_Initialized = true;
        }

        private void InitCaptionArea()
        {
            m_PanelIcon = fw.ui.UIHelper.AddSprite(m_MainPanel).
               SetSize(fw.ui.UIHelper.DefaultPanelIconSize).
               SetSpriteName(fw.CommonSprites.IconOutsideConnections.normal).
               MoveInnerTopLeftOf(m_MainPanel, new Vector3(8, 2)).
               SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
               GetSprite(true);

            var dragHandle = fw.ui.UIHelper.AddDragHandle(m_MainPanel).
                SetTarget(m_MainPanel).
                SetWidth(m_MainPanel.width).
                SetHeight(fw.ui.UIHelper.DefaultDragHandleSize).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Left).
                GetDragHandle();

            var m_PanelClose = fw.ui.UIHelper.AddButton(m_MainPanel).
                SetName("Close").
                SetBackgroundSprites(fw.PreparedSpriteSets.IconClose).
                MoveInnerTopRightOf(m_MainPanel, new Vector3(5, 2)).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right).
                GetButton();
            m_PanelClose.eventClick += delegate { OnClose(); };

            m_ShowHideRoutesButton = fw.ui.UIHelper.AddMultiStateButton(m_MainPanel).
                SetName("ShowHideTrafficRoutesButton").
                SetSpriteSets(new fw.SpriteSet[] { fw.PreparedSpriteSets.ShowHideTrafficRoutesBg0, fw.PreparedSpriteSets.ShowHideTrafficRoutesBg1 },
                    new fw.SpriteSet[] { fw.PreparedSpriteSets.ShowHideTrafficRoutesFg0, fw.PreparedSpriteSets.ShowHideTrafficRoutesFg1 }).
                MoveLeftOf(m_PanelClose, 2).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right).
                GetMultiStateButton();

            m_LocationMarkerButton = fw.ui.UIHelper.AddMultiStateButton(m_MainPanel).
                SetName("LocationMarkerButton").
                SetSpriteSets(new fw.SpriteSet[] { fw.PreparedSpriteSets.LocationMarker, fw.PreparedSpriteSets.LocationMarkerActive }).
                MoveLeftOf(m_ShowHideRoutesButton, 2).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right).
                GetMultiStateButton();

            m_ConnectionNameTextfield = fw.ui.UIHelper.AddTextField(m_MainPanel).
                SetName("ConnectionNameTextField").
                SetHeight(32).
                ClampHorizontallyBetween(m_PanelIcon, m_LocationMarkerButton, 15, 15).
                SetRelativeY(5).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Left).
                GetTextField(true);

            m_ShowHideRoutesButton.eventClick += OnShowHideRoutesClicked;
            m_LocationMarkerButton.eventClick += OnLocationMarkerClicked;
            m_MainPanel.eventVisibilityChanged += OnVisibilityChanged;
            m_ConnectionNameTextfield.eventTextSubmitted += delegate (UIComponent component, string newName)
            {
                if (m_BuildingID != 0 && m_CachedSettings != null)
                {
                    m_CachedSettings.Name = newName;
                    eventNameChanged?.Invoke(m_BuildingID, newName);
                }
            };
        }

        private void InitSettingsPanel()
        {
            var textScale = 0.7f;

            m_SettingsPanel = fw.ui.UIHelper.AddPanel(m_MainPanel).
                SetName("SettingsPanel").
                SetBackgroundSprite(fw.CommonSprites.GenericPanel).
                SetColor(fw.ui.UIHelper.contenPanelColor).
                SetSize(new Vector2(m_MainPanel.width - 10, 120)).
                SetRelativeX(5).
                MoveBottomOf(m_PanelIcon, 2).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top | UIAnchorStyle.Right).
                GetPanel();

            m_TransportTypeLabel = fw.ui.UIHelper.AddLabel(m_SettingsPanel, false).
                SetName("TransportTypeLabel").
                SetAutoSize(true).
                SetTextScale(textScale).
                SetRelativePosition(new Vector3(10, 10)).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
                GetLabel(true);

            m_DirectionLabel = fw.ui.UIHelper.AddLabel(m_SettingsPanel, false).
                SetName("DirectionLabel").
                SetText("Direction").
                SetAutoSize(true).
                SetTextScale(textScale).
                SetTextAlignment(UIHorizontalAlignment.Center).
                MoveInnerTopRightOf(m_SettingsPanel, new Vector3(25, 10)).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top).
                GetLabel();

            m_DirectionOutCheckbox = fw.ui.UIHelper.AddCheckBox(m_SettingsPanel).
                SetName("DirectionOutCheckBox").
                SetText("Out").
                SetTextScale(textScale).
                SetWidth(50).
                MoveInnerRightOf(m_SettingsPanel, 5).
                MoveBottomOf(m_DirectionLabel, 5).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top).
                SetObjectUserData(Building.Flags.Incoming).
                GetCheckBox();
            m_DirectionOutCheckbox.eventCheckChanged += OnDirectionCheckboxChanged;

            m_DirectionInCheckbox = fw.ui.UIHelper.AddCheckBox(m_SettingsPanel).
                SetName("DirectionInCheckBox").
                SetText("In").
                SetTextScale(textScale).
                SetWidth(40).
                SetRelativeY(m_DirectionOutCheckbox.relativePosition.y).
                MoveLeftOf(m_DirectionOutCheckbox, 5).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top).
                SetObjectUserData(Building.Flags.Outgoing).
                GetCheckBox();
            m_DirectionInCheckbox.eventCheckChanged += OnDirectionCheckboxChanged;

            var dummyTrafficFactorLabel = fw.ui.UIHelper.AddLabel(m_SettingsPanel).
                SetName("DummyTrafficFactorLabel").
                SetPrefix("Dummy Traffic Factor: ").
                SetAutoSize(true).
                SetTextScale(textScale).
                MoveBottomOf(m_TransportTypeLabel, 20).
                SetRelativeX(m_TransportTypeLabel.relativePosition.x).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
                GetLabel();

            m_DummyTrafficFactorTextfield = fw.ui.UIHelper.AddTextField(m_SettingsPanel).
                SetName("DummyTrafficFactorTextfield").
                SetNumericalOnly(true).
                SetTextScale(textScale).
                SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
                SetColor(Color.black).
                SetTextColor(Color.white).
                SetPadding(new RectOffset(0, 0, 5, 5)).
                SetTooltip("Modifies the factor of the dummy traffic. Value is clamped between 0 and 1.000.000.").
                SetSize(new Vector2(75, 20)).
                MoveRightOf(dummyTrafficFactorLabel, 5).
                SetRelativeY(dummyTrafficFactorLabel.relativePosition.y - 5).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
                GetTextField(true);
            m_DummyTrafficFactorTextfield.eventTextSubmitted += OnDummyTrafficFactorChanged;

            InitTouristSubPanel();
        }

        private void InitTouristSubPanel()
        {
            var touristPanel = fw.ui.UIHelper.AddPanel(m_SettingsPanel).
                SetName("TouristSubPanel").
                SetBackgroundSprite(fw.CommonSprites.GenericPanel).
                SetColor(Color.white).
                SetRelativeX(5).
                MoveBottomOf(m_DummyTrafficFactorTextfield, 5).
                SpanInnerBottom(m_SettingsPanel, 5).
                SpanInnerRight(m_DummyTrafficFactorTextfield).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Bottom).
                GetPanel();

            var touristIcon = fw.ui.UIHelper.AddSprite(touristPanel).
                SetName("TouristIcon").
                SetSpriteName(fw.CommonSprites.IconTourist).
                SetColor(Color.gray).
                SetSize(new Vector2(36, 36)).
                MoveInnerTopLeftOf(touristPanel, new Vector3(- 5, -5)).
                GetSprite();

            var wealthyIconSize = new Vector2(24, 24);

            var lowWealthyIcon = fw.ui.UIHelper.AddSprite(touristPanel).
                SetName("LowWealthyIcon").
                SetSpriteName(fw.CommonSprites.InfoIconLandValue.disabled).
                SetSize(wealthyIconSize).
                MoveRightOf(touristIcon).
                SetRelativeY(0).
                GetSprite();

            var mediumWealthyIcon = fw.ui.UIHelper.AddSprite(touristPanel).
                SetName("MediumWealthyIcon").
                SetSpriteName(fw.CommonSprites.InfoIconLandValue.focused).
                SetSize(wealthyIconSize).
                SetRelativeX(lowWealthyIcon.relativePosition.x).
                MoveBottomOf(lowWealthyIcon, -3).
                GetSprite();

            var highWealthyIcon = fw.ui.UIHelper.AddSprite(touristPanel).
                SetName("HighWealthyIcon").
                SetSpriteName(fw.CommonSprites.InfoIconLandValue.normal).
                SetSize(wealthyIconSize).
                SetRelativeX(mediumWealthyIcon.relativePosition.x).
                MoveBottomOf(mediumWealthyIcon, -3).
                GetSprite();

            var wealthyTextFieldPadding = new RectOffset(0, 0, 5, 5);
            var wealthyTextScale = 0.7f;
            var lowWealthyTextField = fw.ui.UIHelper.AddTextField(touristPanel).
                SetName("LowWealthySlider").
                SetTooltip("Adjust low wealthy tourist factor. Value is clamped between 0 and 1.000.000.").
                SetTextScale(wealthyTextScale).
                SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
                SetColor(Color.black).
                SetTextColor(Color.white).
                SetPadding(wealthyTextFieldPadding).
                SetNumericalOnly(true).
                MoveRightOf(lowWealthyIcon, 5).
                SetRelativeY(lowWealthyIcon.relativePosition.y + 2).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
                GetTextField(true);

            var mediumWealthyTextField = fw.ui.UIHelper.AddTextField(touristPanel).
                SetName("MediumWealthySlider").
                SetTooltip("Adjust medium wealthy tourist factor. Value is clamped between 0 and 1.000.000.").
                SetTextScale(wealthyTextScale).
                SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
                SetColor(Color.black).
                SetTextColor(Color.white).
                SetPadding(wealthyTextFieldPadding).
                SetNumericalOnly(true).
                MoveRightOf(mediumWealthyIcon, 5).
                SetRelativeY(mediumWealthyIcon.relativePosition.y + 2).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
                GetTextField(true);

            var highWealthyTextField = fw.ui.UIHelper.AddTextField(touristPanel).
                SetName("HighWealthySlider").
                SetTooltip("Adjust high wealthy tourist factor. Value is clamped between 0 and 1.000.000.").
                SetTextScale(wealthyTextScale).
                SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
                SetColor(Color.black).
                SetTextColor(Color.white).
                SetPadding(wealthyTextFieldPadding).
                SetNumericalOnly(true).
                MoveRightOf(highWealthyIcon, 5).
                SetRelativeY(highWealthyIcon.relativePosition.y + 2).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
                GetTextField(true);

            lowWealthyTextField.eventTextSubmitted += OnTouristFactorChanged;
            mediumWealthyTextField.eventTextSubmitted += OnTouristFactorChanged;
            highWealthyTextField.eventTextSubmitted += OnTouristFactorChanged;

            var labelPadding = new RectOffset(5, 5, 2, 0);
            var lowWealthyLabel = fw.ui.UIHelper.AddLabel(touristPanel).
                SetName("LowWealthyLabel").
                SetTooltip("Percentage rate about the likeliness to spawn a tourist of this wealthness.").
                SetBackgroundSprite(fw.CommonSprites.GenericPanel).
                SetColor(Color.gray).
                SetSuffix(" %").
                SetPadding(labelPadding).
                MoveRightOf(lowWealthyTextField, 5).
                SetRelativeY(lowWealthyTextField.relativePosition.y + 2).
                SetTextScale(wealthyTextScale).
                SpanInnerRight(touristPanel, 5).
                SetTextAlignment(UIHorizontalAlignment.Right).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
                GetLabel();

            var mediumWealthyLabel = fw.ui.UIHelper.AddLabel(touristPanel).
                SetName("MediumWealthyLabel").
                SetTooltip("Percentage rate about the likeliness to spawn a tourist of this wealthness.").
                SetBackgroundSprite(fw.CommonSprites.GenericPanel).
                SetColor(Color.gray).
                SetSuffix(" %").
                SetPadding(labelPadding).
                MoveRightOf(mediumWealthyTextField, 5).
                SetRelativeY(mediumWealthyTextField.relativePosition.y + 2).
                SetTextScale(wealthyTextScale).
                SpanInnerRight(touristPanel, 5).
                SetTextAlignment(UIHorizontalAlignment.Right).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
                GetLabel();

            var highWealthyLabel = fw.ui.UIHelper.AddLabel(touristPanel).
                SetName("HighWealthyLabel").
                SetTooltip("Percentage rate about the likeliness to spawn a tourist of this wealthness.").
                SetBackgroundSprite(fw.CommonSprites.GenericPanel).
                SetColor(Color.gray).
                SetSuffix(" %").
                SetPadding(labelPadding).
                MoveRightOf(highWealthyTextField, 5).
                SetRelativeY(highWealthyTextField.relativePosition.y + 2).
                SetTextScale(wealthyTextScale).
                SpanInnerRight(touristPanel, 5).
                SetTextAlignment(UIHorizontalAlignment.Right).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
                GetLabel();

            m_TouristFactorTextFields = new UITextField[] { lowWealthyTextField, mediumWealthyTextField, highWealthyTextField };
            m_TouristFactorLabels = new UILabel[] { lowWealthyLabel, mediumWealthyLabel, highWealthyLabel };
        }

        private void InitNameGenerationPanel()
        {
            m_NameGenerationPanel = fw.ui.UIHelper.AddPanel(m_MainPanel).
                SetName("NameGenerationGroupPanel").
                SetBackgroundSprite(fw.CommonSprites.GenericPanel).
                SetSize(new Vector2(m_MainPanel.width - 10, 300)).
                SetColor(fw.ui.UIHelper.contenPanelColor).
                MoveTopOf(m_BottomResizeHandle).
                SetRelativeX(5).
                SetAnchor(UIAnchorStyle.All).
                GetPanel();

            var label = fw.ui.UIHelper.AddLabel(m_NameGenerationPanel, true).
                SetName("NameGenerationHeadlineLabel").
                SetText("Name Generation").
                SetTextAlignment(UIHorizontalAlignment.Center).
                SetWidth(m_NameGenerationPanel.width).
                SetRelativePosition(new Vector3(100, 15)).
                SetAnchor(UIAnchorStyle.CenterHorizontal | UIAnchorStyle.Top).
                GetLabel();

            m_RandomNameContainer = fw.ui.UIHelper.AddScrollablePanel(m_NameGenerationPanel, UIOrientation.Vertical).
                SetName("RandomNameContainer").
                SetBackgroundSprite(fw.CommonSprites.EmptySprite).
                SetColor(Color.grey).
                MoveInnerBottomLeftOf(m_NameGenerationPanel, new Vector3(5, 5)).
                ExpandVerticallyUntil(label, 15).
                SetWidth(300).
                SetAnchor(UIAnchorStyle.All).
                GetScrollabelPanel(true);

            m_RandomNameContainerScrollbar = fw.ui.UIHelper.AddScrollbarWithTrack(m_NameGenerationPanel, UIOrientation.Vertical).
                SetHeight(m_RandomNameContainer.height).
                MoveRightOf(m_RandomNameContainer).
                SetRelativeY(m_RandomNameContainer.relativePosition.y).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top | UIAnchorStyle.Bottom).
                GetScrollbar();
            m_RandomNameContainer.verticalScrollbar = m_RandomNameContainerScrollbar;

            m_NameModeCheckBoxes = new UICheckBox[Utils.MaxEnumValue<OutsideConnectionSettings.NameModeType>() + 1];
            m_NameModeCheckBoxes[0] = fw.ui.UIHelper.AddCheckBox(m_NameGenerationPanel).
                SetName("VanillaCheckBox").
                SetText("Vanilla").
                SetTextColor(Color.white).
                SetObjectUserData(OutsideConnectionSettings.NameModeType.Vanilla).
                SetGroup(m_RandomNameContainer).
                SetRelativeY(m_RandomNameContainer.relativePosition.y).
                MoveRightOf(m_RandomNameContainerScrollbar, 5).
                GetCheckBox();
            m_NameModeCheckBoxes[0].eventCheckChanged += OnNameGenerationModeChanged;

            m_NameModeCheckBoxes[1] = fw.ui.UIHelper.AddCheckBox(m_NameGenerationPanel).
                SetName("CustomSingleCheckBox").
                SetText("Custom Single").
                SetTextColor(Color.white).
                SetObjectUserData(OutsideConnectionSettings.NameModeType.CustomSingle).
                SetGroup(m_RandomNameContainer).
                MoveBottomOf(m_NameModeCheckBoxes[0], 10).
                SetRelativeX(m_NameModeCheckBoxes[0].relativePosition.x).
                GetCheckBox();
            m_NameModeCheckBoxes[1].eventCheckChanged += OnNameGenerationModeChanged;

            m_NameModeCheckBoxes[2] = fw.ui.UIHelper.AddCheckBox(m_NameGenerationPanel).
                SetName("CustomRandomCheckBox").
                SetText("Custom Random").
                SetTextColor(Color.white).
                SetObjectUserData(OutsideConnectionSettings.NameModeType.CustomRandom).
                SetGroup(m_RandomNameContainer).
                MoveBottomOf(m_NameModeCheckBoxes[1], 10).
                SetRelativeX(m_NameModeCheckBoxes[1].relativePosition.x).
                GetCheckBox();
            m_NameModeCheckBoxes[2].eventCheckChanged += OnNameGenerationModeChanged;

            m_SingleNameTextFrame = fw.ui.UIHelper.AddTextField(m_NameGenerationPanel).
                SetName("SingleNameTextField").
                SetSize(new Vector2(m_RandomNameContainer.width, 45)).
                SetRelativePosition(m_RandomNameContainer.relativePosition).
                SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
                SetColor(Color.black).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Top).
                GetTextField(true);
            m_SingleNameTextFrame.eventTextSubmitted += OnSingleNameSubmitted;

            m_RandomNameCountLabel = fw.ui.UIHelper.AddLabel(m_MainPanel, true).
                SetName("NameGenerationRandomCountLabel").
                SetPrefix("Random Names-Count: ").
                SetAutoSize(true).
                MoveInnerBottomLeftOf(m_MainPanel, new Vector3(10, 7)).
                SwapZOrder(m_BottomResizeHandle).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Bottom).
                GetLabel();

            m_NameGenerationPanel.minimumSize = new Vector2(m_NameGenerationPanel.width, m_RandomNameContainer.relativePosition.y + 100);
        }

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

        private void RefreshDirectionCheckbox(UICheckBox checkbox)
        {
            var oldIsRefreshing = m_IsRefreshing;
            m_IsRefreshing = true;

            var building = Utils.QueryBuilding(m_BuildingID);
            checkbox.isChecked = (building.m_flags & (Building.Flags)checkbox.objectUserData) != 0;
            checkbox.isEnabled = (m_CachedSettings.OriginalDirectionFlags & (Building.Flags)checkbox.objectUserData) != 0;

            m_IsRefreshing = oldIsRefreshing;
        }

        private void RefreshDummyTrafficFactorTextfield()
        {
            var oldIsRefreshing = m_IsRefreshing;
            m_IsRefreshing = true;

            var connectionAI = Utils.QueryBuildingAI(buildingID) as OutsideConnectionAI;
            var dummyTrafficFactor = m_CachedSettings.DummyTrafficFactor < 0 ? connectionAI.m_dummyTrafficFactor : m_CachedSettings.DummyTrafficFactor;
            m_DummyTrafficFactorTextfield.text = dummyTrafficFactor.ToString();

            m_IsRefreshing = oldIsRefreshing;
        }

        private void RefreshTouristPanel()
        {
            var oldIsRefreshing = m_IsRefreshing;
            m_IsRefreshing = true;

            float accumulated = m_CachedSettings.TouristFactors.Aggregate((a, b) => b + a);

            for (int i = 0; i < m_CachedSettings.TouristFactors.Length; ++i)
            {
                m_TouristFactorTextFields[i].text = m_CachedSettings.TouristFactors[i].ToString();
                m_TouristFactorLabels[i].text = accumulated == 0 ? "0" : (m_CachedSettings.TouristFactors[i] * 100 / accumulated).ToString("0.0");
            }

            // Even if it looks strange on the overview panel, this is how the actual game code works. ;)
            if (accumulated == 0)
                m_TouristFactorLabels[2].text = (100f).ToString("0.0");

            m_IsRefreshing = oldIsRefreshing;
        }

        public void RefreshData()
        {
            if (m_BuildingID == 0 || m_CachedSettings == null)
                return;

            var connectionAI = Utils.QueryBuildingAI(buildingID) as OutsideConnectionAI;
            if (connectionAI == null)
            {
                m_BuildingID = 0;
                m_CachedSettings = null;
                return;
            }

            m_IsRefreshing = true;
            m_PanelIcon.spriteName = Utils.GetSpriteNameForTransferReason(connectionAI.m_dummyTrafficReason);
            m_ConnectionNameTextfield.text = m_CachedSettings.Name;
            m_TransportTypeLabel.text = Utils.GetNameForTransferReason(connectionAI.m_dummyTrafficReason);
            RefreshDirectionCheckbox(m_DirectionInCheckbox);
            RefreshDirectionCheckbox(m_DirectionOutCheckbox);
            RefreshDummyTrafficFactorTextfield();
            RefreshTouristPanel();

            foreach (var comp in m_NameModeCheckBoxes)
            {
                if (comp)
                    comp.isChecked = (OutsideConnectionSettings.NameModeType)comp.objectUserData == m_CachedSettings.NameMode;
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

        private void ZoomToLocation()
        {
            if (m_BuildingID == 0)
                return;

            var building = BuildingManager.instance.m_buildings.m_buffer[m_BuildingID];
            var instanceID = default(InstanceID);
            instanceID.Building = m_BuildingID;
            ToolsModifierControl.cameraController.SetTarget(instanceID, building.m_position, false);
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

            if (m_LocationMarkerButton.activeStateIndex == 1)
                ZoomToLocation();
        }

        private void OnTouristFactorChanged(UIComponent component, string text)
        {
            if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
                return;

            var buildingAI = Utils.QueryBuildingAI(m_BuildingID) as OutsideConnectionAI;

            int index = -1;
            for (int i = 0; i < m_TouristFactorTextFields.Length; ++i)
            {
                if (m_TouristFactorTextFields[i] == component)
                {
                    index = i;
                    break;
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                m_CachedSettings.TouristFactors[index] = Utils.GetTouristFactorsFromOutsideConnection(buildingID)[index];
            }
            else
            {
                if (int.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out int value))
                    m_CachedSettings.TouristFactors[index] = Mathf.Clamp(value, 0, 1000000);
            }

            RefreshTouristPanel();
        }

        private void OnDummyTrafficFactorChanged(UIComponent component, string newText)
        {
            if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
                return;

            var buildingAI = Utils.QueryBuildingAI(m_BuildingID) as OutsideConnectionAI;
            if (string.IsNullOrEmpty(newText))
            {
                m_CachedSettings.DummyTrafficFactor = buildingAI.m_dummyTrafficFactor;
            }
            else
            {
                if (int.TryParse(newText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out int value))
                    m_CachedSettings.DummyTrafficFactor = Mathf.Clamp(value, 0, 1000000);
            }

            RefreshDummyTrafficFactorTextfield();
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

            m_CachedSettings.CurrentDirectionFlags = buildingFlags & Building.Flags.IncomingOutgoing;
            BuildingManager.instance.m_buildings.m_buffer[m_BuildingID].m_flags = buildingFlags;     // need to push back the copy

            eventDirectionChanged?.Invoke(m_BuildingID, m_CachedSettings.CurrentDirectionFlags);
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
