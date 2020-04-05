using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    class OverviewPanelExtension : OverviewPanelBase
    {
        private UIPanel m_MainPanel = null;
        private UISlicedSprite m_Caption = null;
        private UIScrollablePanel m_ScrollablePanel = null;
        private UIPanel m_OutsideConnectionTitle = null;
        private UIScrollbar m_Scrollbar = null;
        private UILabel m_OutsideConnectionCountLabel = null;
        private UIResizeHandle m_BottomResizeHandle = null;
        private static readonly string m_MainPanelName = "AOCMainPanel";

        private int m_LastOutsideConnectionCount = 0;

        private bool m_Initialized = false;

        enum SortCriterion
        {
            DEFAULT,
            TRANSPORT_TYPE,
            NAME
        };
        private SortCriterion m_CurSortCriterion = SortCriterion.DEFAULT;
        private bool m_InverseSort = false;

        private void OnDestroy()
        {
            if (m_MainPanel)
                UnityEngine.Object.Destroy(m_MainPanel);
        }

        private void OnEnable()
        {
            if (m_Initialized)
                return;

#if DEBUG
            m_MainPanel = UIView.Find<UIPanel>(m_MainPanelName);
            if (m_MainPanel)
            {
                UnityEngine.Object.Destroy(m_MainPanel);
                m_MainPanel = null;

                Utils.Log("Cleared " + m_MainPanelName + " from previous sessions.");
            }
#endif
            var objectOfType = UnityEngine.Object.FindObjectOfType<UIView>();
            transform.parent = objectOfType.transform;
            m_MainPanel = gameObject.AddComponent<UIPanel>();

            var infoPanel = UIView.Find<UIPanel>(UIUtils.OutgoingConnectionInfoViewPanelName);
            var outConsInfoViewPanel = infoPanel.GetComponent<OutsideConnectionsInfoViewPanel>();

            m_MainPanel.name = m_MainPanelName;
            m_MainPanel.relativePosition = new Vector3(359f, 0f);
            m_MainPanel.size = new Vector2(500, 700);
            m_MainPanel.backgroundSprite = infoPanel.backgroundSprite;
            m_MainPanel.minimumSize = new Vector2(m_MainPanel.width, 150);

            m_OutsideConnectionCountLabel = m_MainPanel.AddUIComponent<UILabel>();
            m_OutsideConnectionCountLabel.font = WidgetsFactory.instance.textFont;
            m_OutsideConnectionCountLabel.textScale = 1;
            m_OutsideConnectionCountLabel.name = "OutsideConnectionCount";
            m_OutsideConnectionCountLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;

            m_BottomResizeHandle = UIUtils.MakeCopy(WidgetsFactory.instance.verticalResizeHandle, m_MainPanel);
            m_BottomResizeHandle.size = new Vector2(m_MainPanel.width, 35);
            m_BottomResizeHandle.relativePosition = new Vector3(0, m_MainPanel.height - m_BottomResizeHandle.height);
            m_BottomResizeHandle.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Bottom;
            m_BottomResizeHandle.edges = UIResizeHandle.ResizeEdge.Bottom;
            m_OutsideConnectionCountLabel.relativePosition = m_BottomResizeHandle.relativePosition + new Vector3(10, 10);

            m_Caption = WidgetsFactory.AddCaption(m_MainPanel, "Advanced Outside Connections Overview");
            m_Caption.relativePosition = new Vector3(0, 0);

            SetupOutsideConnectionTitle();

            m_ScrollablePanel = m_MainPanel.AddUIComponent<UIScrollablePanel>();
            m_ScrollablePanel.relativePosition = new Vector3(0, 85);
            m_ScrollablePanel.autoLayout = true;
            m_ScrollablePanel.autoLayoutPadding = new RectOffset(5, 5, 1, 0);
            m_ScrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_ScrollablePanel.clipChildren = true;
            m_ScrollablePanel.name = "ScrollablePanel";
            m_ScrollablePanel.scrollWheelDirection = UIOrientation.Vertical;
            m_ScrollablePanel.scrollWheelAmount = 38;
            //m_ScrollablePanel.useTouchMouseScroll = true;
            m_ScrollablePanel.builtinKeyNavigation = true;
            m_ScrollablePanel.height = m_MainPanel.height - m_ScrollablePanel.relativePosition.y - m_BottomResizeHandle.height;
            m_ScrollablePanel.anchor = UIAnchorStyle.All;

            m_Scrollbar = m_MainPanel.AddUIComponent<UIScrollbar>();
            m_Scrollbar.size = new Vector2(21, m_ScrollablePanel.height);
            m_Scrollbar.relativePosition = new Vector3(m_MainPanel.width - m_Scrollbar.width - 5, m_ScrollablePanel.relativePosition.y);
            m_Scrollbar.anchor = UIAnchorStyle.Right | UIAnchorStyle.Top | UIAnchorStyle.Bottom;
            m_Scrollbar.name = "Scrollbar";
            m_Scrollbar.orientation = UIOrientation.Vertical;
            m_Scrollbar.autoHide = false;
            m_Scrollbar.incrementAmount = 38;
            m_Scrollbar.stepSize = 1;
            var scrollbarTrack = m_Scrollbar.AddUIComponent<UISlicedSprite>();
            scrollbarTrack.name = "Track";
            scrollbarTrack.spriteName = "ScrollbarTrack";
            scrollbarTrack.relativePosition = Vector3.zero;
            scrollbarTrack.size = m_Scrollbar.size;
            scrollbarTrack.anchor = UIAnchorStyle.All;
            m_Scrollbar.trackObject = scrollbarTrack;
            var scrollbarThumb = scrollbarTrack.AddUIComponent<UISlicedSprite>();
            scrollbarThumb.name = "Thumb";
            scrollbarThumb.spriteName = "ScrollbarThumb";
            scrollbarThumb.width = m_Scrollbar.width - 6;
            m_Scrollbar.thumbObject = scrollbarThumb;
            m_ScrollablePanel.verticalScrollbar = m_Scrollbar;
            m_ScrollablePanel.width = m_Scrollbar.relativePosition.x;

            m_Initialized = true;
        }

        private void Update()
        {
            if (m_Initialized && m_LastOutsideConnectionCount != BuildingManager.instance.GetOutsideConnections().m_size)
            {
                RefreshOutsideConnections();
                m_LastOutsideConnectionCount = BuildingManager.instance.GetOutsideConnections().m_size;
            }
        }

        private void RefreshOutsideConnections()
        {
            if (!OutsideConnectionSettingsManager.exists || !m_Initialized)
                return;

            OutsideConnectionSettingsManager.instance.SyncWithBuildingManager();

            var outsideConnectionCount = OutsideConnectionSettingsManager.instance.SettingsDict.Count;
            foreach (var element in OutsideConnectionSettingsManager.instance.SettingsDict)
            {
                OutsideConnectionInfo outsideConnectionInfo;
                if (outsideConnectionCount >= m_ScrollablePanel.components.Count)
                {
                    var gameObject = new GameObject("AOCOutsideConnectionInfo", new System.Type[] { typeof(OutsideConnectionInfo) });
                    outsideConnectionInfo = gameObject.GetComponent<OutsideConnectionInfo>();
                    outsideConnectionInfo.component.width = m_ScrollablePanel.width - 5;
                    m_ScrollablePanel.AttachUIComponent(gameObject);
                }
                else
                {
                    outsideConnectionInfo = m_ScrollablePanel.components[outsideConnectionCount].GetComponent<OutsideConnectionInfo>();
                }
                outsideConnectionInfo.buildingID = element.Key;
            }

            m_OutsideConnectionCountLabel.text = "Outside Connections-Count: " + m_ScrollablePanel.components.Count;

            OnTransportTypeSort();
        }

        private void SetupOutsideConnectionTitle()
        {
            m_OutsideConnectionTitle = m_MainPanel.AddUIComponent<UIPanel>();
            m_OutsideConnectionTitle.relativePosition = new Vector3(5, 45);
            m_OutsideConnectionTitle.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;
            m_OutsideConnectionTitle.name = "OutsideConnectionTitle";
            m_OutsideConnectionTitle.clipChildren = false;
            m_OutsideConnectionTitle.autoLayout = true;
            m_OutsideConnectionTitle.autoLayoutDirection = LayoutDirection.Horizontal;
            m_OutsideConnectionTitle.autoLayoutPadding = new RectOffset(10, 15, 5, 5);

            var titleButtons = new UIButton[2];

            titleButtons[0] = m_OutsideConnectionTitle.AddUIComponent<UIButton>();
            titleButtons[0].name = "TransportTypeButton";
            titleButtons[0].verticalAlignment = UIVerticalAlignment.Middle;
            titleButtons[0].horizontalAlignment = UIHorizontalAlignment.Center;
            titleButtons[0].size = new Vector2(32, 32);
            titleButtons[0].disabledBgSprite = "InfoIconPublicTransportDisabled";
            titleButtons[0].pressedBgSprite = "InfoIconPublicTransportPressed";
            titleButtons[0].normalBgSprite = "InfoIconPublicTransport";
            titleButtons[0].focusedBgSprite = "InfoIconPublicTransportFocused";
            titleButtons[0].hoveredBgSprite = "InfoIconPublicTransportHovered";

            titleButtons[0].eventClick += delegate
            {
                OnTransportTypeSort();
            };

            titleButtons[1] = m_OutsideConnectionTitle.AddUIComponent<UIButton>();
            titleButtons[1].name = "NameTitle";
            titleButtons[1].text = "Connection Name";
            titleButtons[1].textHorizontalAlignment = UIHorizontalAlignment.Center;
            titleButtons[1].size = new Vector2(240, 24);
            titleButtons[1].textScale = 1.0625f;

            titleButtons[1].eventClick += delegate
            {
                OnNameSort();
            };
        }

        private void OnTransportTypeSort()
        {
            Sort(SortCriterion.TRANSPORT_TYPE,
                (UIComponent lhs, UIComponent rhs) =>
                {
                    var lhsSettings = lhs.GetComponent<OutsideConnectionInfo>().currentSettings;
                    var rhsSettings = rhs.GetComponent<OutsideConnectionInfo>().currentSettings;
                    var value = lhsSettings.Type - rhsSettings.Type;
                    if (value != 0)
                        return m_InverseSort ? -value : value;
                    return CompareNames(lhs, rhs);
                }
            );
        }

        private void OnNameSort()
        {
            Sort(SortCriterion.NAME, CompareNames);
        }

        private void Sort(SortCriterion criterion, Comparison<UIComponent> compare)
        {
            if (m_CurSortCriterion == criterion)
                m_InverseSort = !m_InverseSort;
            else
                m_InverseSort = false;
            if (m_ScrollablePanel.components.Count > 0)
            {
                OverviewPanelBase.Quicksort(m_ScrollablePanel.components, compare);
                m_CurSortCriterion = criterion;
                m_ScrollablePanel.Invalidate();
            }
        }

        private int CompareNames(UIComponent lhs, UIComponent rhs)
        {
            var lhsSettings = lhs.GetComponent<OutsideConnectionInfo>().currentSettings;
            var rhsSettings = rhs.GetComponent<OutsideConnectionInfo>().currentSettings;
            var value = OverviewPanelBase.NaturalCompare(lhsSettings.Name, rhsSettings.Name);
            return m_InverseSort ? -value : value;
        }
    }
}
