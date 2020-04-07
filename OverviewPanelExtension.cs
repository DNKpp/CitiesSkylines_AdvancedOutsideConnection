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

        OutsideConnectionDetailPanel m_OutsideConnectionDetailPanel = null;

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
            m_Initialized = false;

            if (m_MainPanel)
                UnityEngine.Object.Destroy(m_MainPanel);

            if (m_OutsideConnectionDetailPanel)
                UnityEngine.Object.Destroy(m_OutsideConnectionDetailPanel.gameObject);
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

            m_MainPanel = WidgetsFactory.MakeMainPanel(gameObject, m_MainPanelName, new Vector2(500, 700));
            m_MainPanel.relativePosition = new Vector3(359f, 0f);
            m_MainPanel.minimumSize = new Vector2(m_MainPanel.width, 150);

            m_OutsideConnectionCountLabel = WidgetsFactory.AddLabel(m_MainPanel, "", true, "NameGenerationRandomCountLabel");
            m_OutsideConnectionCountLabel.name = "OutsideConnectionCount";
            m_OutsideConnectionCountLabel.prefix = "Outside Connections-Count: ";
            m_OutsideConnectionCountLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;

            m_BottomResizeHandle = WidgetsFactory.AddPanelBottomResizeHandle(m_MainPanel);
            m_OutsideConnectionCountLabel.relativePosition = m_BottomResizeHandle.relativePosition + new Vector3(10, 10);

            m_Caption = WidgetsFactory.AddCaption(m_MainPanel, "Advanced Outside Connections Overview");
            m_Caption.relativePosition = new Vector3(0, 0);

            SetupOutsideConnectionTitle();

            m_ScrollablePanel = WidgetsFactory.AddScrollablePanel(m_MainPanel);
            m_ScrollablePanel.relativePosition = new Vector3(0, 85);
            m_ScrollablePanel.width = m_MainPanel.width -WidgetsFactory.DefaultVScrollbarWidth - 1;
            m_ScrollablePanel.height = m_MainPanel.height - m_ScrollablePanel.relativePosition.y - m_BottomResizeHandle.height;

            m_Scrollbar = WidgetsFactory.AddVerticalScrollbar(m_MainPanel, m_ScrollablePanel);

            var detailPanelGO = new GameObject("AOCDetailMainPanelGO", new Type[] { typeof(OutsideConnectionDetailPanel) });
            m_OutsideConnectionDetailPanel = detailPanelGO.GetComponent<OutsideConnectionDetailPanel>();
            m_OutsideConnectionDetailPanel.eventNameChanged += OnConnectionNameChanged;

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

            var outsideConnectionCount = 0;
            foreach (var element in OutsideConnectionSettingsManager.instance.SettingsDict)
            {
                OutsideConnectionInfo outsideConnectionInfo;
                if (outsideConnectionCount >= m_ScrollablePanel.components.Count)
                {
                    var gameObject = new GameObject("AOCOutsideConnectionInfo", new System.Type[] { typeof(OutsideConnectionInfo) });
                    outsideConnectionInfo = gameObject.GetComponent<OutsideConnectionInfo>();
                    outsideConnectionInfo.component.width = m_ScrollablePanel.width - 5;
                    outsideConnectionInfo.eventDetailsOpen += OnDetailsOpened;
                    outsideConnectionInfo.eventNameChanged += OnConnectionNameChanged;
                    m_ScrollablePanel.AttachUIComponent(gameObject);
                }
                else
                {
                    outsideConnectionInfo = m_ScrollablePanel.components[outsideConnectionCount].GetComponent<OutsideConnectionInfo>();
                }
                outsideConnectionInfo.buildingID = element.Key;
                ++outsideConnectionCount;
            }

            m_OutsideConnectionCountLabel.text = m_ScrollablePanel.components.Count.ToString();

            m_CurSortCriterion = SortCriterion.DEFAULT;
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
            if (!m_Initialized)
                return;

            Sort(SortCriterion.TRANSPORT_TYPE,
                (UIComponent lhs, UIComponent rhs) =>
                {
                    if (lhs == null || lhs == null)
                        return 0;

                    var lhsComponent = lhs.GetComponent<OutsideConnectionInfo>();
                    var rhsComponent = rhs.GetComponent<OutsideConnectionInfo>();
                    if (lhsComponent == null || rhsComponent == null || lhsComponent.buildingID == 0 || rhsComponent.buildingID == 0)
                        return 0;

                    var value = Utils.QueryTransportInfo(lhsComponent.buildingID).m_transportType - Utils.QueryTransportInfo(rhsComponent.buildingID).m_transportType;
                    if (value != 0)
                        return m_InverseSort ? -value : value;
                    return CompareNames(lhs, rhs);
                }
            );
        }

        private void OnNameSort()
        {
            if (!m_Initialized)
                return;

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
            if (lhs == null || lhs == null)
                return 0;

            var lhsSettings = lhs.GetComponent<OutsideConnectionInfo>().currentSettings;
            var rhsSettings = rhs.GetComponent<OutsideConnectionInfo>().currentSettings;
            if (lhsSettings == null || rhsSettings == null)
                return 0;

            var value = OverviewPanelBase.NaturalCompare(lhsSettings.Name, rhsSettings.Name);
            return m_InverseSort ? -value : value;
        }

        private void OnDetailsOpened(ushort buildingID)
        {
            if (m_OutsideConnectionDetailPanel.component.isVisible && m_OutsideConnectionDetailPanel.buildingID == buildingID)
            {
                m_OutsideConnectionDetailPanel.component.Hide();
            }
            else
            {
                m_OutsideConnectionDetailPanel.ChangeTarget(buildingID);
                m_OutsideConnectionDetailPanel.component.Show();
            }
        }

        private void OnConnectionNameChanged(ushort buildingID, string newName)
        {
            if (buildingID == 0)
                return;


            if (m_OutsideConnectionDetailPanel.buildingID == buildingID)
                m_OutsideConnectionDetailPanel.RefreshData();

            foreach (var rowPanel in m_ScrollablePanel.components)
            {
                var infoPanel = rowPanel.gameObject.GetComponent<OutsideConnectionInfo>();
                if (infoPanel != null && infoPanel.buildingID == buildingID)
                {
                    infoPanel.RefreshData();
                    return;
                }
            }
        }
    }
}
