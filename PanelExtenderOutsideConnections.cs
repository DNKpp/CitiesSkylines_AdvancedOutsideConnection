using ColossalFramework.UI;
using Harmony;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    class TemplateWidgets
    {
        public UIButton button = null;
        public UIButton closeButton = null;
        public UICheckBox checkbox = null;
        public UILabel label = null;
        public UILabel headlineLabel = null;
        public UITabContainer tabContainer = null;
        public UITabstrip tabstrip = null;
        public UIPanel panel = null;
        public UIPanel mainPanel = null;
        public UISlicedSprite vThumb = null;
        public UISlicedSprite vTrack = null;
        public UISprite icon = null;
    }

    class UISettingsPanel : UIPanel
    {
        private ushort m_BuildingID = 0;
        private OutsideConnectionSettings m_Settings = null;
        private bool m_Initialized = false;

        public OutsideConnectionSettings Settings { get { return m_Settings; } }

        private UILabel m_BuildingIDLabel = null;
        private UILabel m_LocationLabel = null;
        private UIPanel m_NameModePanel = null;
        private UICheckBox[] m_NameModeCheckboxes = null;
        private UITextField m_NameString = null;


        public void Setup(ushort buildingID, OutsideConnectionSettings settings, TemplateWidgets templates)
        {
            m_BuildingID = buildingID;
            m_Settings = settings;

            if (!m_Initialized)
            {
                //autoSize = true;
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoLayoutPadding = new RectOffset(5, 5, 10, 10);
                backgroundSprite = "GenericPanel";
                color = new Color32(45, 52, 61, 255);
                size = new Vector2(430, 260);

                m_BuildingIDLabel = AddUIComponent<UILabel>();
                UIUtils.CopyStyle(m_BuildingIDLabel, templates.label);
                m_BuildingIDLabel.name = "BuildingIDLabel";
                m_BuildingIDLabel.text = "BuildingID:\t" + m_BuildingID;

                m_LocationLabel = AddUIComponent<UILabel>();
                UIUtils.CopyStyle(m_LocationLabel, templates.label);
                m_LocationLabel.name = "LocationLabel";
                m_LocationLabel.text = "Location:\t" + settings.Position.x + " / " + settings.Position.z;

                SetupNameModeCheckboxes(templates);

                m_NameModeCheckboxes[(int)settings.NameMode].isChecked = true;
                m_NameString.text = "";
                foreach (var str in settings.NameText)
                {
                    if (m_NameString.text.Length != 0)
                        m_NameString.text += ";";
                    m_NameString.text += str;
                }

                //autoFitChildrenHorizontally = true;
                //autoFitChildrenVertically = true;

                m_Initialized = true;
            }
        }

        private void SetupNameModeCheckboxes(TemplateWidgets templates)
        {
            int enumMax = Utils.MaxEnumValue<OutsideConnectionSettings.NameModeType>();
            int enumMin = Utils.MinEnumValue<OutsideConnectionSettings.NameModeType>();
            m_NameModeCheckboxes = new UICheckBox[enumMax + 1 - enumMin];

            m_NameModePanel = AddUIComponent<UIPanel>();
            m_NameModePanel.autoSize = true;
            m_NameModePanel.autoLayout = true;
            m_NameModePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_NameModePanel.autoLayoutPadding = new RectOffset(5, 10, 5, 5);
            m_NameModePanel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            m_NameModePanel.name = "NameModePanel";
            m_NameModePanel.backgroundSprite = "GenericPanel";
            m_NameModePanel.padding = new RectOffset(5, 5, 5, 5);

            var nameModeLabel = m_NameModePanel.AddUIComponent<UILabel>();
            UIUtils.CopyStyle(nameModeLabel, templates.label);
            nameModeLabel.autoSize = true;
            nameModeLabel.name = "NameModeLabel";
            nameModeLabel.text = "Name Mode:";
            nameModeLabel.textColor = templates.button.textColor;

            var subPanel = m_NameModePanel.AddUIComponent<UIPanel>();
            UIUtils.CopyStyle(subPanel, m_NameModePanel);
            subPanel.name = "SubNameModePanel";
            subPanel.anchor = UIAnchorStyle.Right;
            subPanel.autoLayoutDirection = LayoutDirection.Vertical;
            subPanel.autoLayoutPadding = new RectOffset(5, 5, 0, 10);
            subPanel.backgroundSprite = null;
            //subPanel.minimumSize = new Vector2(150, 3 * 16 + 2 * 10 + 2 * 5);
            //subPanel.size = subPanel.minimumSize;

            for (int i = enumMin; i <= enumMax; ++i)
            {
                var nameModeStr = ((OutsideConnectionSettings.NameModeType)i).ToString();
                var checkbox = subPanel.AddUIComponent<UICheckBox>();
                UIUtils.CopyStyle(checkbox, templates.checkbox);
                checkbox.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
                checkbox.name = "NameModeCheckbox-" + nameModeStr;
                checkbox.text = nameModeStr;
                checkbox.label.relativePosition = new Vector3(22f, 2f);
                checkbox.label.textColor = templates.button.textColor;
                checkbox.group = subPanel;
                checkbox.objectUserData = (OutsideConnectionSettings.NameModeType)i;
                checkbox.eventCheckChanged += new PropertyChangedEventHandler<bool>(OnNameModeSubmitted);

                var sprite = checkbox.AddUIComponent<UISprite>();
                sprite.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
                sprite.atlas = base.atlas;
                sprite.spriteName = "check-unchecked";
                sprite.size = new Vector2(16f, 16f);
                sprite.relativePosition = Vector3.zero;

                var checkedSprite = checkbox.AddUIComponent<UISprite>();
                checkedSprite.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
                checkedSprite.atlas = base.atlas;
                checkedSprite.spriteName = "check-checked";
                checkedSprite.size = new Vector2(16f, 16f);
                checkedSprite.relativePosition = Vector3.zero;
                checkbox.checkedBoxObject = checkedSprite;

                m_NameModeCheckboxes[i - enumMin] = checkbox;
            }

            m_NameString = m_NameModePanel.AddUIComponent<UITextField>();
            m_NameString.atlas = m_NameModePanel.atlas;
            m_NameString.readOnly = false;
            m_NameString.multiline = false;
            m_NameString.numericalOnly = false;
            m_NameString.isPasswordField = false;
            m_NameString.allowFloats = false;
            m_NameString.allowNegative = false;
            m_NameString.minimumSize = new Vector2(400, 30);
            m_NameString.size = m_NameString.minimumSize;
            m_NameString.normalBgSprite = "GenericPanel";
            m_NameString.selectionSprite = "EmptySprite";
            m_NameString.focusedBgSprite = "TextFieldPanel";
            m_NameString.hoveredBgSprite = "TextFieldPanelHovered";
            m_NameString.builtinKeyNavigation = true;
            m_NameString.verticalAlignment = UIVerticalAlignment.Middle;
            m_NameString.horizontalAlignment = UIHorizontalAlignment.Left;
            m_NameString.padding = new RectOffset(8, 3, 5, 5);
            m_NameString.textScale = 1f;
            m_NameString.color = Color.black;
            m_NameString.eventTextSubmitted += new PropertyChangedEventHandler<string>(OnNameTextSubmitted);

            subPanel.autoFitChildrenHorizontally = true;
            subPanel.autoFitChildrenVertically = true;

            //m_NameModePanel.size = new Vector2(subPanel.width + m_NameModePanel.padding.left + m_NameModePanel.padding.right, 300);
            m_NameModePanel.size = new Vector2(420, 160);
        }

        private void OnNameTextSubmitted(UIComponent component, string text)
        {
            m_Settings.NameText.Clear();
            foreach (var str in text.Split(';'))
            {
                str.Trim();
                if (str.Length != 0)
                    m_Settings.NameText.Add(str);
            }
        }

        private void OnNameModeSubmitted(UIComponent component, bool isChecked)
        {
            if (isChecked)
            {
                m_Settings.NameMode = (OutsideConnectionSettings.NameModeType)component.objectUserData;
            }
        }
    }

    class PanelExtenderOutsideConnections : MonoBehaviour
    {
        static string OutConInfoViewPanelName = "(Library) OutsideConnectionsInfoViewPanel";

        private OutsideConnectionsInfoViewPanel m_OutConsInfoPanel = null;
        private UIPanel m_MainPanel = null;
        private UIButton[] m_TabstripButtons = null;
        private bool m_Initialized = false;

        private void OnDestroy()
        {
            if (m_MainPanel)
                UnityEngine.Object.Destroy(m_MainPanel);
        }


        private void LateUpdate()
        {
            if (!m_Initialized)
            {
                Init();
            }
            else
            {
                //if (m_OutConsInfoPanel.component.isVisible)
                //{
                //    //OutsideConnectionSettingsManager.instance.SyncWithBuildingManager();
                //    m_MainPanel.Show();
                //}
                //m_WasVisible = m_OutConsInfoPanel.component.isVisible;
                //this.UpdateBindings();
            }
        }

        private TemplateWidgets GatherTemplates()
        {
            var templates = new TemplateWidgets();

            //templates.mainPanel = (UIPanel)m_OutConsInfoPanel.component;
            templates.panel = m_OutConsInfoPanel.Find<UIPanel>("ExportLegend");
            templates.mainPanel = (UIPanel)templates.panel.parent.parent.parent;
            templates.button = m_OutConsInfoPanel.Find<UIButton>("Export");
            templates.label = m_OutConsInfoPanel.Find<UILabel>("ExportTotal");
            templates.tabstrip = m_OutConsInfoPanel.Find<UITabstrip>("Tabstrip");
            var caption = m_OutConsInfoPanel.Find<UISlicedSprite>("Caption");
            templates.headlineLabel = caption.Find<UILabel>("Label");
            templates.icon = caption.Find<UISprite>("Icon");
            templates.closeButton = caption.Find<UIButton>("Close");

            var trafficRoutesInfoPanel = UIView.Find<UIPanel>("(Library) TrafficRoutesInfoViewPanel");
            if (trafficRoutesInfoPanel)
            {
                var trafficRoutesPanelComponent = trafficRoutesInfoPanel.GetComponent<TrafficRoutesInfoViewPanel>();
                templates.checkbox = trafficRoutesPanelComponent.Find<UICheckBox>("CheckboxPedestrians");
            }
            else
                Debug.LogError("ImprovedOusideConnection: Start: \"(Library) TrafficRoutesInfoViewPane\" not found!");

            var publicTransportDetailPanel = UIView.library.Get<PublicTransportDetailPanel>("PublicTransportDetailPanel");
            templates.vTrack = publicTransportDetailPanel.Find<UISlicedSprite>("Track");
            templates.vThumb = publicTransportDetailPanel.Find<UISlicedSprite>("Thumb");
            return templates;
        }

        private void Init()
        {
            if (m_Initialized)
                return;

#if DEBUG
            m_MainPanel = UIView.Find<UIPanel>("IOCPanel");
            if (m_MainPanel)
            {
                UnityEngine.Object.Destroy(m_MainPanel);
                m_MainPanel = null;

                Debug.Log("ImprovedOusideConnection: Cleared IOCPanel from previous sessions.");
            }
#endif

            var infoPanel = UIView.Find<UIPanel>(OutConInfoViewPanelName);
            Debug.Log("ImprovedOusideConnection: Start: found related info UIPanel: " + (infoPanel != null));
            if (!infoPanel)
                return;

            m_OutConsInfoPanel = infoPanel.GetComponent<OutsideConnectionsInfoViewPanel>();
            Debug.Log("ImprovedOusideConnection: Start: found related info Component: " + (m_OutConsInfoPanel != null));
            if (!m_OutConsInfoPanel)
                return;

            var templates = GatherTemplates();

            m_MainPanel = m_OutConsInfoPanel.component.AddUIComponent<UIPanel>();
            m_MainPanel.name = "IOCPanel";
            //m_MainPanel.autoSize = true;
            m_MainPanel.size = new Vector2(500, 700);
            m_MainPanel.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;
            m_MainPanel.autoLayout = true;
            m_MainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_MainPanel.autoLayoutStart = LayoutStart.TopLeft;
            m_MainPanel.autoLayoutPadding = new RectOffset(10, 10, 10, 5);
            m_MainPanel.relativePosition = new Vector3(359f, 0f);
            m_MainPanel.backgroundSprite = infoPanel.backgroundSprite;
            //m_MainPanel.eventVisibilityChanged +=


            var headlineLabel = m_MainPanel.AddUIComponent<UILabel>();
            headlineLabel.relativePosition = new Vector3(100f, 7f);
            headlineLabel.text = "Outside Connection Settings";
            UIUtils.CopyStyle(headlineLabel, templates.label);
            headlineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            headlineLabel.textAlignment = UIHorizontalAlignment.Center;
            headlineLabel.textColor = Color.white;
            headlineLabel.textScale = 1f;
            headlineLabel.padding = new RectOffset(40, 40, 2, 2);

            var tabStrip = m_MainPanel.AddUIComponent<UITabstrip>();
            UIUtils.CopyStyle(tabStrip, templates.tabstrip);
            tabStrip.autoSize = true;

            var tabs = m_MainPanel.AddUIComponent<UITabContainer>();
            tabs.width = 600;
            tabs.height = 600;
            tabStrip.tabPages = tabs;

            var transferTypes = new TransportInfo.TransportType[] { TransportInfo.TransportType.Bus, TransportInfo.TransportType.Airplane, TransportInfo.TransportType.Ship, TransportInfo.TransportType.Train };
            m_TabstripButtons = new UIButton[transferTypes.Length];

            OutsideConnectionSettingsManager.instance.SyncWithBuildingManager();
            var connectionSettingsArr = OutsideConnectionSettingsManager.instance.GetSettingsAsArray();

            UISettingsPanel[] settingsPanels = new UISettingsPanel[connectionSettingsArr.Length];
            int i = 0;
            int settingsI = 0;
            foreach (var transType in transferTypes)
            {
                var transTypeStr = transType.ToString();

                var button = tabStrip.AddUIComponent<UIButton>();
                UIUtils.CopyStyle(button, templates.button);
                button.name = "Button" + transTypeStr;
                button.autoSize = true;
                button.textPadding = new RectOffset(30, 30, 7, 5);
                button.text = transTypeStr;
                m_TabstripButtons[i] = button;

                var tab = (UIPanel)tabs.AddTabPage("Tab" + transTypeStr);
                //tab.autoSize = true;
                //tab.width = 200;
                tab.autoLayout = true;
                tab.autoLayoutDirection = LayoutDirection.Horizontal;
                tab.autoLayoutStart = LayoutStart.TopLeft;

                var scrollable = tab.AddUIComponent<UIScrollablePanel>();
                var vScrollbar = tab.AddUIComponent<UIScrollbar>();
                vScrollbar.autoSize = true;
                vScrollbar.autoHide = false;
                vScrollbar.orientation = UIOrientation.Vertical;
                vScrollbar.anchor = UIAnchorStyle.Top | UIAnchorStyle.Bottom | UIAnchorStyle.Right;
                //vScrollbar.height = 500f;
                vScrollbar.width = 21;
                //vScrollbar.stepSize = 1f;
                vScrollbar.incrementAmount = 60f;
                //vScrollbar.scrollEasingType = ColossalFramework.EasingType.ExpoEaseInOut;
                //vScrollbar.scrollEasingTime = 0.15f;
                //vScrollbar.scrollSize = 762.9997f;
                //vScrollbar.position = new Vector3(560, 600);
                var track = vScrollbar.AddUIComponent<UISlicedSprite>();
                UIUtils.CopyStyle(track, templates.vTrack);
                track.height = 600;
                vScrollbar.trackObject = track;
                var thumb = vScrollbar.AddUIComponent<UISlicedSprite>();
                UIUtils.CopyStyle(thumb, templates.vThumb);
                vScrollbar.thumbObject = thumb;


                scrollable.verticalScrollbar = vScrollbar;
                scrollable.autoLayoutDirection = LayoutDirection.Vertical;
                scrollable.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
                scrollable.autoLayoutStart = LayoutStart.TopLeft;
                scrollable.autoLayout = true;
                scrollable.scrollWheelAmount = 109;
                scrollable.scrollWheelDirection = UIOrientation.Vertical;
                scrollable.size = new Vector2(440, 600);
                //scrollable.backgroundSprite = "GenericPanel";
                scrollable.builtinKeyNavigation = true;
                //scrollable.useScrollMomentum = true;
                scrollable.useTouchMouseScroll = true;

                foreach(var settings in connectionSettingsArr)
                {
                    if (settings.Value.Type == transType)
                    {
                        var settingsPanel = scrollable.AddUIComponent<UISettingsPanel>();
                        settingsPanel.Setup(settings.Key, settings.Value, templates);
                        settingsPanels[settingsI++] = settingsPanel;
                    }
                }
                ++i;
            }

            // This is needed, because otherwise the tab panel will appear buggy
            m_TabstripButtons[1].SimulateClick();
            m_TabstripButtons[0].SimulateClick();

            m_MainPanel.eventVisibilityChanged += new PropertyChangedEventHandler<bool>(OnVisibilityChanged);

            m_Initialized = true;
        }

        private void OnVisibilityChanged(UIComponent component, bool visible)
        {
            // This is needed, because otherwise the tab panel will appear buggy
            if (m_Initialized)
            {
                m_TabstripButtons[1].SimulateClick();
                m_TabstripButtons[0].SimulateClick();
            }
        }
    }
}
