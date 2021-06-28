
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.UI;
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
				m_DeleteButton.isVisible = enable;
			}

			public event PropertyChangedEventHandler<string> eventNameSubmitted;
			public event MouseEventHandler eventDeleteClicked;
		}

		class MaterialRowComponent : TableRowComponent
		{
			private bool m_IsRefreshing = false;
			private UIPanel m_MainPanel = null;
			private UISprite m_Icon = null;
			private UISlider m_Slider = null;
			private UILabel m_ValueLabel = null;
			private TransferManager.TransferReason m_Material = TransferManager.TransferReason.None;

			public event PropertyChangedEventHandler<float> eventRatioChanged;

			public TransferManager.TransferReason Material
			{ 
				get { return m_Material; }
				set
				{
					m_Material = value;
					Utils.SetupSpriteForMaterial(m_Icon, m_Material);
					m_Slider.objectUserData = m_Material;
					m_Slider.tooltip = m_Icon.tooltip;
					m_ValueLabel.tooltip = m_Icon.tooltip;
				}
			}

			private new void Awake()
			{
				m_MainPanel = gameObject.AddComponent<UIPanel>();
				m_MainPanel.name = "MainPanel";
				m_MainPanel.size = new Vector2(200, 30);

				base.Awake();

				m_Icon = fw.ui.UIHelper.AddSprite(m_MainPanel).
					SetRelativePosition(new Vector3(2, 0)).
					SetName("Icon").
					SetSize(new Vector2(24, 24)).
					SetAnchor(UIAnchorStyle.CenterVertical | UIAnchorStyle.Left).
					GetSprite(true);

				Utils.SetupSpriteForMaterial(m_Icon, TransferManager.TransferReason.Coal);

				var labelPadding = new RectOffset(5, 5, 2, 0);
				m_ValueLabel = fw.ui.UIHelper.AddLabel(m_MainPanel).
					SetName("ValueLabel").
					SetTooltip("Factor for the likeliness offering this good type.").
					SetSuffix("%").
					SetBackgroundSprite(fw.CommonSprites.GenericPanel).
					SetColor(Color.gray).
					SetPadding(labelPadding).
					SetWidth(55).
					MoveInnerRightOf(m_MainPanel, 15).
					SetRelativeY(0).
					SetTextScale(0.7f).
					SetTextAlignment(UIHorizontalAlignment.Right).
					SetAnchor(UIAnchorStyle.CenterVertical | UIAnchorStyle.Right).
					GetLabel(true);

				m_Slider = fw.ui.UIHelper.AddSlider(m_MainPanel, UIOrientation.Horizontal).
					SetMinValue(0).
					SetMaxValue(100).
					SetStepSize(0.2f).
					MoveRightOf(m_Icon, 15).
					SpanRight(m_ValueLabel, 15).
					SetRelativeY(10).
					SetHeight(15).
					SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Left).
					GetSlider();
				m_Slider.eventValueChanged += OnSliderValueChanged;
			}

			public void RefreshData(int value, int totalValue)
			{
				if (m_IsRefreshing || m_Material == TransferManager.TransferReason.None)
					return;

				m_IsRefreshing = true;

				float percent = (float)(value * 100) / totalValue;
				m_Slider.value = value / 100;
				m_ValueLabel.text = percent.ToString("0.00");

				m_IsRefreshing = false;
			}

			private void OnSliderValueChanged(UIComponent component, float value)
			{
				eventRatioChanged?.Invoke(component, value);
			}
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
		private UIPanel m_NameGenerationFooterPanel = null;
		private UIPanel m_RandomNameButtonPanel = null;
		private UILabel m_RandomNameCountLabel = null;
		private UITextField m_SingleNameTextFrame = null;
		private UIPanel m_SettingsPanel = null;
		//private UILabel m_DirectionLabel = null;
		private UICheckBox m_DirectionInCheckbox = null;
		private UICheckBox m_DirectionOutCheckbox = null;
		private UILabel m_TransportTypeLabel = null;
		private UITextField m_DummyTrafficFactorTextfield = null;
		private UITextField m_ResidentCapacityTextfield = null;
		private UITextField m_CargoCapacityTextfield = null;
		private UICheckBox[] m_NameModeCheckBoxes = null;

		private UIPanel m_TouristSubPanel = null;
		private UITextField[] m_TouristFactorTextFields = null;

		private UIPanel m_GoodsSubPanel = null;
		private UIScrollablePanel m_ResourceImportScrollPanel = null;
		private UIScrollablePanel m_ResourceExportScrollPanel = null;
		private UITabstrip m_ResourceTabstrip = null;

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
				SetSize(new Vector2(600, 600)).
				SetMinimumSize(new Vector2(600, 1200)).
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
				SetSize(new Vector2(m_MainPanel.width - 10, 305)).
				SetRelativeX(5).
				MoveBottomOf(m_PanelIcon, 2).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top | UIAnchorStyle.Right).
				GetPanel();

			m_TransportTypeLabel = fw.ui.UIHelper.AddLabel(m_SettingsPanel, false).
				SetName("TransportTypeLabel").
				//SetAutoSize(true).
				SetWidth(50).
				SetTextScale(textScale).
				SetRelativePosition(new Vector3(10, 10)).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
				GetLabel(true);

			m_DirectionOutCheckbox = fw.ui.UIHelper.AddCheckBox(m_SettingsPanel).
				SetName("DirectionOutCheckBox").
				SetText("Out").
				SetTextScale(textScale).
				SetWidth(50).
				MoveRightOf(m_TransportTypeLabel, 5).
				SetRelativeY(5).
				SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top).
				SetObjectUserData(Building.Flags.Incoming).
				GetCheckBox();
			m_DirectionOutCheckbox.eventCheckChanged += OnDirectionCheckboxChanged;

			m_DirectionInCheckbox = fw.ui.UIHelper.AddCheckBox(m_SettingsPanel).
				SetName("DirectionInCheckBox").
				SetText("In").
				SetTextScale(textScale).
				SetWidth(40).
				MoveRightOf(m_DirectionOutCheckbox, 5).
				SetRelativeY(m_DirectionOutCheckbox.relativePosition.y).
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

			var cargoCapacityFactorLabel = fw.ui.UIHelper.AddLabel(m_SettingsPanel).
				SetName("CargoCapacityLabel").
				SetPrefix("Cargo Capacity: ").
				SetAutoSize(true).
				SetTextScale(textScale).
				MoveBottomOf(dummyTrafficFactorLabel, 10).
				SetRelativeX(dummyTrafficFactorLabel.relativePosition.x).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
				GetLabel();

			m_CargoCapacityTextfield = fw.ui.UIHelper.AddTextField(m_SettingsPanel).
				SetName("CargoCapacityTextfield").
				SetNumericalOnly(true).
				SetTextScale(textScale).
				SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
				SetColor(Color.black).
				SetTextColor(Color.white).
				SetPadding(new RectOffset(0, 0, 5, 5)).
				SetTooltip("Modifies the cargo capacity. Value is clamped between 0 and 1.000.000.").
				SetSize(new Vector2(75, 20)).
				SetRelativeX(m_DummyTrafficFactorTextfield.relativePosition.x).
				SetRelativeY(cargoCapacityFactorLabel.relativePosition.y - 5).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
				GetTextField(true);
			m_CargoCapacityTextfield.eventTextSubmitted += OnCargoCapacityChanged;

			var residentCapacityFactorLabel = fw.ui.UIHelper.AddLabel(m_SettingsPanel).
				SetName("ResidentCapacityLabel").
				SetPrefix("Resident Capacity: ").
				SetAutoSize(true).
				SetTextScale(textScale).
				MoveBottomOf(cargoCapacityFactorLabel, 10).
				SetRelativeX(cargoCapacityFactorLabel.relativePosition.x).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
				GetLabel();

			m_ResidentCapacityTextfield = fw.ui.UIHelper.AddTextField(m_SettingsPanel).
				SetName("ResidentCapacityTextfield").
				SetNumericalOnly(true).
				SetTextScale(textScale).
				SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
				SetColor(Color.black).
				SetTextColor(Color.white).
				SetPadding(new RectOffset(0, 0, 5, 5)).
				SetTooltip("Modifies the resident capacity. Value is clamped between 0 and 1.000.000.").
				SetSize(new Vector2(75, 20)).
				SetRelativeX(m_CargoCapacityTextfield.relativePosition.x).
				SetRelativeY(residentCapacityFactorLabel.relativePosition.y - 5).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
				GetTextField(true);
			m_ResidentCapacityTextfield.eventTextSubmitted += OnResidentCapacityChanged;

			InitTouristSubPanel();
			InitGoodsSubPanel();
		}

		private void InitGoodsSubPanel()
		{
			var textScale = 0.7f;

			m_GoodsSubPanel = fw.ui.UIHelper.AddPanel(m_SettingsPanel).
				SetName("GoodsSubPanel").
				SetBackgroundSprite(fw.CommonSprites.GenericPanel).
				SetColor(Color.white).
				MoveRightOf(m_TouristSubPanel, 5).
				SetRelativeY(5).
				SpanInnerBottomRight(m_SettingsPanel, new Vector2(5, 5)).
				SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Bottom).
				GetPanel();

			var goodsIcon = fw.ui.UIHelper.AddSprite(m_GoodsSubPanel).
				SetName("GoodsIcon").
				SetSpriteName(fw.CommonSprites.IconOutsideConnections.normal).
				SetColor(Color.gray).
				SetSize(new Vector2(28, 28)).
				MoveInnerTopRightOf(m_GoodsSubPanel, new Vector3(5, 0)).
				GetSprite();

			m_ResourceTabstrip = fw.ui.UIHelper.AddTabstripWithButtons(m_GoodsSubPanel, new string[] { "Import", "Export" }, textScale).
				SetHeight(20).
				MoveInnerLeftOf(m_GoodsSubPanel).
				SetRelativeY(5).
				GetTabtrip();

			var tabContainer = fw.ui.UIHelper.AddTabContainer(m_GoodsSubPanel).
				SetName("GoodsTabContainer").
				MoveBottomOf(m_ResourceTabstrip, 3).
				SetRelativeX(m_ResourceTabstrip.relativePosition.x).
				SpanInnerBottomRight(m_GoodsSubPanel, new Vector2(5, 5)).
				GetTabContainer();

			m_ResourceImportScrollPanel = InitResourcePanel("Impport", tabContainer, Utils.ImportResources, OnImportResourceRatioChanged, OnResetImportResourceRatios);
			m_ResourceExportScrollPanel = InitResourcePanel("Export", tabContainer, Utils.ExportResources, OnExportResourceRatioChanged, OnResetExportResourceRatios);

			// really don't know why I have to do this. Both tab pages are rendered at once, so this is the workaround to solve this. (I fucking hate this shitty UI...)
			tabContainer.selectedIndex = 1;
			m_ResourceTabstrip.tabPages = tabContainer;
			m_ResourceTabstrip.startSelectedIndex = 0;
			m_ResourceTabstrip.selectedIndex = 0;
			tabContainer.selectedIndex = 0;
		}

		private UIScrollablePanel InitResourcePanel(string prefix, UITabContainer tabContainer, TransferManager.TransferReason[] reasons, PropertyChangedEventHandler<float> onRatioChanged, MouseEventHandler onResetButtonClicked)
		{
			var tabPanel = (UIPanel)tabContainer.AddTabPage(prefix + "Page");
			var resetButton = fw.ui.UIHelper.AddTextButton(tabPanel, "Reset Ratios").
				SetName(prefix + "ResetButton").
				SetTooltip("Resets all " + prefix + " resource ratios to default.").
				MoveInnerTopRightOf(tabPanel, new Vector2(25, 0)).
				GetButton();

			resetButton.eventClicked += onResetButtonClicked;

			var exportScrollBar = fw.ui.UIHelper.AddScrollbarWithTrack(tabPanel, UIOrientation.Vertical).
				SetName(prefix + "ScrollBar").
				MoveBottomOf(resetButton, 5).
				MoveInnerRightOf(tabPanel).
				SpanInnerBottom(tabPanel).
				SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top | UIAnchorStyle.All).
				GetScrollbar();

			var scrollPanel = fw.ui.UIHelper.AddScrollablePanel(tabPanel, UIOrientation.Vertical).
				SetName(prefix + "ScrollPanel").
				SetClipChildren(true).
				SetAutoLayout(true).
				SetAutoLayoutDirection(LayoutDirection.Vertical).
				SetAutoLayoutPadding(new RectOffset(0, 0, 2, 2)).
				SetRelativeY(exportScrollBar.relativePosition.y).
				MoveLeftOf(exportScrollBar).
				SpanInnerBottomLeft(tabPanel).
				SetAnchor(UIAnchorStyle.All).
				SetVerticalScrollbar(exportScrollBar).
				GetScrollabelPanel();

			foreach (var material in reasons)
			{
				var gameObject = new GameObject(prefix + "MaterialRowComponent", new System.Type[] { typeof(MaterialRowComponent) });
				scrollPanel.AttachUIComponent(gameObject);
				var materialRowComponent = gameObject.GetComponent<MaterialRowComponent>();
				materialRowComponent.AlwaysBrightBgColor = true;
				materialRowComponent.component.width = scrollPanel.width;
				materialRowComponent.Material = material;
				materialRowComponent.eventRatioChanged += onRatioChanged;
			}

			return scrollPanel;
		}

		private void InitTouristSubPanel()
		{
			m_TouristSubPanel = fw.ui.UIHelper.AddPanel(m_SettingsPanel).
				SetName("TouristSubPanel").
				SetBackgroundSprite(fw.CommonSprites.GenericPanel).
				SetColor(Color.white).
				SetRelativeX(5).
				MoveBottomOf(m_ResidentCapacityTextfield, 5).
				SpanInnerBottom(m_SettingsPanel, 5).
				SpanInnerRight(m_ResidentCapacityTextfield).
				SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Bottom).
				GetPanel();

			var touristIcon = fw.ui.UIHelper.AddSprite(m_TouristSubPanel).
				SetName("TouristIcon").
				SetSpriteName(fw.CommonSprites.IconTourist).
				SetColor(Color.gray).
				SetSize(new Vector2(36, 36)).
				MoveInnerTopLeftOf(m_TouristSubPanel, new Vector3(- 5, -5)).
				GetSprite();

			float firstY =  touristIcon.relativePosition.y + touristIcon.height + 2;
			m_TouristFactorTextFields = new UITextField[]
			{
				InitTouristRow("Low", fw.CommonSprites.InfoIconLandValue.disabled, m_TouristSubPanel, firstY),
				InitTouristRow("Medium", fw.CommonSprites.InfoIconLandValue.focused, m_TouristSubPanel, firstY + 25),
				InitTouristRow("High", fw.CommonSprites.InfoIconLandValue.normal, m_TouristSubPanel, firstY + 50)
			};
		}

		private UITextField InitTouristRow(string typeString, string spriteName, UIComponent parent, float relY)
		{
			var wealthynessIcon = fw.ui.UIHelper.AddSprite(m_TouristSubPanel).
				SetName(typeString + "WealthinessIcon").
				SetTooltip(typeString + " wealthy tourists").
				SetSpriteName(spriteName).
				SetSize(new Vector2(24, 24)).
				MoveInnerLeftOf(parent, 5).
				SetRelativeY(relY).
				GetSprite();

			var highWealthyTextField = fw.ui.UIHelper.AddTextField(m_TouristSubPanel).
				SetName(typeString + "WealthinessTextField").
				SetTooltip("Adjust " + typeString + " wealthy tourist factor. Value is clamped between 0 and 1.000.000. Leaving blank will reset to default.").
				SetTextScale(0.7f).
				SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
				SetColor(Color.black).
				SetTextColor(Color.white).
				SetPadding(new RectOffset(0, 0, 2, 2)).
				SetNumericalOnly(true).
				MoveRightOf(wealthynessIcon, 5).
				SetRelativeY(relY).
				SpanInnerRight(parent, 15).
				SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
				GetTextField(true);
			highWealthyTextField.eventTextSubmitted += OnTouristFactorChanged;
			return highWealthyTextField;
		}

		private void InitNameGenerationPanel()
		{
			m_NameGenerationPanel = fw.ui.UIHelper.AddPanel(m_MainPanel).
				SetName("NameGenerationGroupPanel").
				SetBackgroundSprite(fw.CommonSprites.GenericPanel).
				SetColor(fw.ui.UIHelper.contenPanelColor).
				MoveBottomOf(m_SettingsPanel, 5).
				SetRelativeX(5).
				SpanBottom(m_BottomResizeHandle).
				SpanInnerRight(m_MainPanel, 5).
				SetAnchor(UIAnchorStyle.All).
				GetPanel();

			m_NameModeCheckBoxes = new UICheckBox[Utils.MaxEnumValue<OutsideConnectionSettings.NameModeType>() + 1];
			m_NameModeCheckBoxes[0] = fw.ui.UIHelper.AddCheckBox(m_NameGenerationPanel).
				SetName("VanillaCheckBox").
				SetText("Vanilla").
				SetTextColor(Color.white).
				SetObjectUserData(OutsideConnectionSettings.NameModeType.Vanilla).
				SetGroup(m_NameGenerationPanel).
				MoveInnerTopRightOf(m_NameGenerationPanel, new Vector3(75, 15)).
				GetCheckBox();

			m_NameModeCheckBoxes[1] = fw.ui.UIHelper.AddCheckBox(m_NameGenerationPanel).
				SetName("CustomSingleCheckBox").
				SetText("Custom Single").
				SetTextColor(Color.white).
				SetObjectUserData(OutsideConnectionSettings.NameModeType.CustomSingle).
				SetGroup(m_NameGenerationPanel).
				MoveBottomOf(m_NameModeCheckBoxes[0], 10).
				SetRelativeX(m_NameModeCheckBoxes[0].relativePosition.x).
				GetCheckBox();

			m_NameModeCheckBoxes[2] = fw.ui.UIHelper.AddCheckBox(m_NameGenerationPanel).
				SetName("CustomRandomCheckBox").
				SetText("Custom Random").
				SetTextColor(Color.white).
				SetObjectUserData(OutsideConnectionSettings.NameModeType.CustomRandom).
				SetGroup(m_NameGenerationPanel).
				MoveBottomOf(m_NameModeCheckBoxes[1], 10).
				SetRelativeX(m_NameModeCheckBoxes[1].relativePosition.x).
				GetCheckBox();

			foreach (var checkBox in m_NameModeCheckBoxes)
			{
				checkBox.eventCheckChanged += OnNameGenerationModeChanged;
			}

			m_RandomNameContainerScrollbar = fw.ui.UIHelper.AddScrollbarWithTrack(m_NameGenerationPanel, UIOrientation.Vertical).
				MoveLeftOf(m_NameModeCheckBoxes[0], 15).
				SetRelativeY(m_NameModeCheckBoxes[0].relativePosition.y).
				SpanInnerBottom(m_NameGenerationPanel, 5).
				SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top | UIAnchorStyle.Bottom).
				GetScrollbar();

			m_RandomNameContainer = fw.ui.UIHelper.AddScrollablePanel(m_NameGenerationPanel, UIOrientation.Vertical).
				SetName("RandomNameContainer").
				SetBackgroundSprite(fw.CommonSprites.EmptySprite).
				SetColor(Color.grey).
				MoveLeftOf(m_RandomNameContainerScrollbar).
				SpanInnerLeft(m_NameGenerationPanel, 5).
				SetRelativeY(m_RandomNameContainerScrollbar.relativePosition.y).
				SetHeight(m_RandomNameContainerScrollbar.height).
				SetAnchor(UIAnchorStyle.All).
				SetVerticalScrollbar(m_RandomNameContainerScrollbar).
				GetScrollabelPanel();

			m_SingleNameTextFrame = fw.ui.UIHelper.AddTextField(m_NameGenerationPanel).
				SetName("SingleNameTextField").
				SetRelativePosition(m_RandomNameContainer.relativePosition).
				SpanInnerRight(m_RandomNameContainerScrollbar).
				SetHeight(45).
				SetNormalBgSprite(fw.CommonSprites.InfoViewPanel).
				SetColor(Color.black).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Top).
				GetTextField(true);
			m_SingleNameTextFrame.eventTextSubmitted += OnSingleNameSubmitted;

			m_RandomNameButtonPanel = fw.ui.UIHelper.AddPanel(m_NameGenerationPanel).
				SetName("RandomNameButtonPanel").
				SetHeight(30).
				MoveInnerBottomRightOf(m_NameGenerationPanel).
				SpanLeft(m_RandomNameContainerScrollbar, 5).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Bottom | UIAnchorStyle.Right).
				GetPanel();

			var pasteButton = fw.ui.UIHelper.AddTextButton(m_RandomNameButtonPanel, "paste").
				MoveInnerBottomRightOf(m_RandomNameButtonPanel, new Vector2(5, 5)).
				GetButton();
			pasteButton.eventClicked += OnRndNamePasteClicked;

			var copyButton = fw.ui.UIHelper.AddTextButton(m_RandomNameButtonPanel, "copy").
				SetRelativeY(pasteButton.relativePosition.y).
				MoveLeftOf(pasteButton, 10).
				GetButton();
			copyButton.eventClicked += OnRndNameCopyClicked;

			var clearButton = fw.ui.UIHelper.AddTextButton(m_RandomNameButtonPanel, "clear").
				SetRelativeY(copyButton.relativePosition.y).
				MoveLeftOf(copyButton, 10).
				GetButton();
			clearButton.eventClicked += OnRndNameClearClicked;

			m_NameGenerationFooterPanel = fw.ui.UIHelper.AddPanel(m_MainPanel).
				SetName("NameGenerationFooterPanel").
				MoveBottomOf(m_NameGenerationPanel).
				MoveInnerLeftOf(m_MainPanel).
				SpanInnerBottomRight(m_MainPanel).
				SwapZOrder(m_BottomResizeHandle).
				SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Bottom | UIAnchorStyle.Right).
				GetPanel();

			m_RandomNameCountLabel = fw.ui.UIHelper.AddLabel(m_NameGenerationFooterPanel, true).
				SetName("NameGenerationRandomCountLabel").
				SetPrefix("Random Names-Count: ").
				SetAutoSize(true).
				MoveInnerBottomLeftOf(m_NameGenerationFooterPanel, new Vector3(10, 7)).
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
			m_NameGenerationFooterPanel.Hide();
			m_RandomNameContainerScrollbar.Hide();
			m_RandomNameButtonPanel.Hide();
			m_RandomNameContainer.Hide();
			m_SingleNameTextFrame.Hide();

			switch (m_CachedSettings.NameMode)
			{
				case OutsideConnectionSettings.NameModeType.CustomRandom:
					m_RandomNameContainerScrollbar.Show();
					m_RandomNameContainer.Show();
					m_NameGenerationFooterPanel.Show();
					m_RandomNameButtonPanel.Show();
					break;
				case OutsideConnectionSettings.NameModeType.CustomSingle:
					m_SingleNameTextFrame.Show();
					break;
				default:
					break;
			}
		}

		private void RefreshResourceComponents()
		{
			if ((m_CachedSettings.CurrentDirectionFlags & Building.Flags.IncomingOutgoing) == 0)
			{
				m_GoodsSubPanel.Hide();
			}
			else
			{
				m_GoodsSubPanel.Show();
				RefreshImportResourceComponents();
				RefreshExportResourceComponents();

				m_ResourceTabstrip.selectedIndex = m_ResourceImportScrollPanel.isEnabled ? 0 : 1;
			}
		}

		private void RefreshImportResourceComponents()
		{
			var oldIsRefreshing = m_IsRefreshing;
			m_IsRefreshing = true;

			if ((m_CachedSettings.CurrentDirectionFlags & Building.Flags.Outgoing) != 0)
			{
				m_ResourceImportScrollPanel.isEnabled = true;
				m_ResourceTabstrip.Find<UIButton>("Button-Import").Show();
				var total = m_CachedSettings.ImportResourceRatio.Sum();
				for (int i = 0; i < m_CachedSettings.ImportResourceRatio.Length; ++i)
				{
					var row = m_ResourceImportScrollPanel.components[i].gameObject.GetComponent<MaterialRowComponent>();
					row.RefreshData(m_CachedSettings.ImportResourceRatio[i], total);
				}
			}
			else
			{
				m_ResourceImportScrollPanel.isEnabled = false;
				m_ResourceTabstrip.Find<UIButton>("Button-Import").Hide();
			}

			m_IsRefreshing = oldIsRefreshing;
		}

		private void RefreshExportResourceComponents()
		{
			var oldIsRefreshing = m_IsRefreshing;
			m_IsRefreshing = true;

			if ((m_CachedSettings.CurrentDirectionFlags & Building.Flags.Incoming) != 0)
			{
				m_ResourceExportScrollPanel.isEnabled = true;
				m_ResourceTabstrip.Find<UIButton>("Button-Export").Show();
				var total = m_CachedSettings.ExportResourceRatio.Sum();
				for (int i = 0; i < m_CachedSettings.ExportResourceRatio.Length; ++i)
				{
					var row = m_ResourceExportScrollPanel.components[i].gameObject.GetComponent<MaterialRowComponent>();
					row.RefreshData(m_CachedSettings.ExportResourceRatio[i], total);
				}
			}
			else
			{
				m_ResourceExportScrollPanel.isEnabled = false;
				m_ResourceTabstrip.Find<UIButton>("Button-Export").Hide();
			}

			m_IsRefreshing = oldIsRefreshing;
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

		private void RefreshCargoCapacityTextfield()
		{
			var oldIsRefreshing = m_IsRefreshing;
			m_IsRefreshing = true;

			var connectionAI = Utils.QueryBuildingAI(buildingID) as OutsideConnectionAI;
			var cargoCapacity = m_CachedSettings.CargoCapacity < 0 ? connectionAI.m_cargoCapacity : m_CachedSettings.CargoCapacity;
			m_CargoCapacityTextfield.text = cargoCapacity.ToString();

			m_IsRefreshing = oldIsRefreshing;
		}


		private void RefreshResidentCapacityTextfield()
		{
			var oldIsRefreshing = m_IsRefreshing;
			m_IsRefreshing = true;

			var connectionAI = Utils.QueryBuildingAI(buildingID) as OutsideConnectionAI;
			var residentCapacity = m_CachedSettings.ResidentCapacity < 0 ? connectionAI.m_residentCapacity : m_CachedSettings.ResidentCapacity;
			m_ResidentCapacityTextfield.text = residentCapacity.ToString();

			m_IsRefreshing = oldIsRefreshing;
		}


		private void RefreshTouristPanel()
		{
			var oldIsRefreshing = m_IsRefreshing;
			m_IsRefreshing = true;

			int total = m_CachedSettings.TouristFactors.Sum();

			for (int i = 0; i < m_CachedSettings.TouristFactors.Length; ++i)
			{
				m_TouristFactorTextFields[i].text = m_CachedSettings.TouristFactors[i].ToString();
			}

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
			RefreshCargoCapacityTextfield();
			RefreshResidentCapacityTextfield();
			RefreshTouristPanel();
			RefreshResourceComponents();

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

		private void OnImportResourceRatioChanged(UIComponent component, float value)
		{
			if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
				return;

			var total = m_CachedSettings.ImportResourceRatio.Sum();
			Utils.ApplyNewImportResourceRatio(ref m_CachedSettings.ImportResourceRatio, (int)value * 100, (TransferManager.TransferReason)component.objectUserData);
			RefreshImportResourceComponents();
		}

		private void OnExportResourceRatioChanged(UIComponent component, float value)
		{
			if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
				return;

			Utils.ApplyNewExportResourceRatio(ref m_CachedSettings.ExportResourceRatio, (int)value * 100, (TransferManager.TransferReason)component.objectUserData);
			RefreshExportResourceComponents();
		}

		private void OnResetImportResourceRatios(UIComponent component, UIMouseEventParameter eventParam)
		{
			if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
				return;

			m_CachedSettings.ImportResourceRatio = Utils.GetDefaultImportResourceRatio();
			RefreshImportResourceComponents();
		}

		private void OnResetExportResourceRatios(UIComponent component, UIMouseEventParameter eventParam)
		{
			if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
				return;

			m_CachedSettings.ExportResourceRatio = Utils.GetDefaultExportResourceRatio();
			RefreshExportResourceComponents();
		}

		private void OnTouristFactorChanged(UIComponent component, string text)
		{
			if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
				return;

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

		private void OnCargoCapacityChanged(UIComponent component, string newText)
		{
			if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
				return;

			var buildingAI = Utils.QueryBuildingAI(m_BuildingID) as OutsideConnectionAI;
			if (string.IsNullOrEmpty(newText))
			{
				m_CachedSettings.CargoCapacity = buildingAI.m_cargoCapacity;
			}
			else
			{
				if (int.TryParse(newText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out int value))
					m_CachedSettings.CargoCapacity = Mathf.Clamp(value, 0, 1000000);
			}

			RefreshCargoCapacityTextfield();
		}

		private void OnResidentCapacityChanged(UIComponent component, string newText)
		{
			if (m_IsRefreshing || m_BuildingID == 0 || m_CachedSettings == null)
				return;

			var buildingAI = Utils.QueryBuildingAI(m_BuildingID) as OutsideConnectionAI;
			if (string.IsNullOrEmpty(newText))
			{
				m_CachedSettings.ResidentCapacity = buildingAI.m_residentCapacity;
			}
			else
			{
				if (int.TryParse(newText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out int value))
					m_CachedSettings.ResidentCapacity = Mathf.Clamp(value, 0, 1000000);
			}

			RefreshResidentCapacityTextfield();
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
			if (isChecked)
				buildingFlags |= flag;
			else
				buildingFlags &= ~flag;

			m_CachedSettings.CurrentDirectionFlags = buildingFlags & Building.Flags.IncomingOutgoing;
			BuildingManager.instance.m_buildings.m_buffer[m_BuildingID].m_flags = buildingFlags;	 // need to push back the copy

			RefreshResourceComponents();

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

		private void OnRndNameCopyClicked(UIComponent comp, UIMouseEventParameter mouseParam)
		{
			string copy = "";
			foreach (var name in m_CachedSettings.RandomGenerationNames)
			{
				copy += name + '\n';
			}
			GUIUtility.systemCopyBuffer = copy;
		}

		private void OnRndNamePasteClicked(UIComponent comp, UIMouseEventParameter mouseParam)
		{
			var text = GUIUtility.systemCopyBuffer;
			foreach (var name in text.Split('\n'))
			{
				if (!string.IsNullOrEmpty(name))
					m_CachedSettings.RandomGenerationNames.Add(name);
			}

			RefreshData();
		}

		private void OnRndNameClearClicked(UIComponent comp, UIMouseEventParameter mouseParam)
		{
			m_CachedSettings.RandomGenerationNames.Clear();

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
