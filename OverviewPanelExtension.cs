
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.UI;
using System;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    class OverviewPanelExtension : OverviewPanelBase
    {
        private UIPanel m_MainPanel = null;
        private UIPanel m_Caption = null;
        private UIScrollablePanel m_ScrollablePanel = null;
        private UIPanel m_OutsideConnectionTitle = null;
        private UIScrollbar m_Scrollbar = null;
        private UILabel m_OutsideConnectionCountLabel = null;
        private UIResizeHandle m_BottomResizeHandle = null;
        private static readonly string m_MainPanelName = "AOCMainPanel";
        private bool m_IsAttachedToOutConPanel = true;

        OutsideConnectionsInfoViewPanel m_OutsideConnectionsInfoViewPanel = null;
        OutsideConnectionDetailPanel m_OutsideConnectionDetailPanel = null;

        private int m_LastOutsideConnectionCount = 0;

        private bool m_Initialized = false;

        enum SortCriterion
        {
            DEFAULT,
            TRANSPORT_TYPE,
            NAME,
            DIRECTION
        };
        private SortCriterion m_CurSortCriterion = SortCriterion.DEFAULT;
        private bool m_InverseSort = false;

        private void Update()
        {
            if (m_Initialized && m_LastOutsideConnectionCount != BuildingManager.instance.GetOutsideConnections().m_size)
            {
                RefreshOutsideConnections();
                m_LastOutsideConnectionCount = BuildingManager.instance.GetOutsideConnections().m_size;
            }
        }

        private void OnDestroy()
        {
            m_Initialized = false;

            SetupEventsWithOutsideConnectionInfoViewPanel(false);

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
            m_MainPanel.minimumSize = new Vector2(m_MainPanel.width, 150);

            m_OutsideConnectionCountLabel = WidgetsFactory.AddLabel(m_MainPanel, "", true, "NameGenerationRandomCountLabel");
            m_OutsideConnectionCountLabel.name = "OutsideConnectionCount";
            m_OutsideConnectionCountLabel.prefix = "Outside Connections-Count: ";
            m_OutsideConnectionCountLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;

            m_BottomResizeHandle = WidgetsFactory.AddPanelBottomResizeHandle(m_MainPanel);
            m_OutsideConnectionCountLabel.relativePosition = m_BottomResizeHandle.relativePosition + new Vector3(10, 10);

            m_Caption = WidgetsFactory.AddPanelCaption(m_MainPanel, "Advanced Outside Connections Overview", CommonSprites.IconOutsideConnections.normal);
            var closeButton = m_Caption.Find<UIButton>("Close");
            closeButton.eventClick += delegate
            {
                OnClose();
            };

            InitOutsideConnectionTitle();

            m_ScrollablePanel = WidgetsFactory.AddScrollablePanel(m_MainPanel);
            m_ScrollablePanel.relativePosition = new Vector3(0, 85);
            m_ScrollablePanel.width = m_MainPanel.width -WidgetsFactory.DefaultVScrollbarWidth - 1;
            m_ScrollablePanel.height = m_MainPanel.height - m_ScrollablePanel.relativePosition.y - m_BottomResizeHandle.height;

            m_Scrollbar = WidgetsFactory.AddVerticalScrollbar(m_MainPanel, m_ScrollablePanel);

            var detailPanelGO = new GameObject("AOCDetailMainPanelGO", new Type[] { typeof(OutsideConnectionDetailPanel) });
            m_OutsideConnectionDetailPanel = detailPanelGO.GetComponent<OutsideConnectionDetailPanel>();
            m_OutsideConnectionDetailPanel.eventNameChanged += OnConnectionNameChanged;
            m_OutsideConnectionDetailPanel.eventDirectionChanged += delegate (ushort buildingID, Building.Flags flag)
            {
                OnConnectionInfoChanged(buildingID);
            };

            SetupEventsWithOutsideConnectionInfoViewPanel(true);

            m_MainPanel.Hide();
            m_MainPanel.eventPositionChanged += OnPositionChanged;

            m_Initialized = true;
        }

        private void InitOutsideConnectionTitle()
        {
            m_OutsideConnectionTitle = m_MainPanel.AddUIComponent<UIPanel>();
            m_OutsideConnectionTitle.relativePosition = new Vector3(5, 45);
            m_OutsideConnectionTitle.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;
            m_OutsideConnectionTitle.name = "OutsideConnectionTitle";
            m_OutsideConnectionTitle.clipChildren = false;
            m_OutsideConnectionTitle.autoLayout = true;
            m_OutsideConnectionTitle.autoLayoutDirection = LayoutDirection.Horizontal;
            m_OutsideConnectionTitle.autoLayoutPadding = new RectOffset(10, 15, 5, 5);

            var titleButtons = new UIButton[3];

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
            titleButtons[1].size = new Vector2(235, 24);
            titleButtons[1].textScale = 1.0625f;

            titleButtons[1].eventClick += delegate
            {
                OnNameSort();
            };

            titleButtons[2] = WidgetsFactory.AddButton(m_OutsideConnectionTitle, "Direction", "DirectionButton");
            titleButtons[2].size = new Vector2(100, 24);
            titleButtons[2].textScale = 1.0625f;

            titleButtons[2].eventClick += delegate
            {
                OnDirectionSort();
            };
        }

        private void SetupEventsWithOutsideConnectionInfoViewPanel(bool init)
        {
            var outsideConnectionInfoPanel = UIView.Find<UIPanel>(UIUtils.OutgoingConnectionInfoViewPanelName);
            m_OutsideConnectionsInfoViewPanel = outsideConnectionInfoPanel.gameObject.GetComponent<OutsideConnectionsInfoViewPanel>();

            if (init)
                outsideConnectionInfoPanel.eventVisibilityChanged += OnOutsideConnectionInfoViewPanelVisibilityChanged;
            else
                outsideConnectionInfoPanel.eventVisibilityChanged -= OnOutsideConnectionInfoViewPanelVisibilityChanged;

            if (init)
                outsideConnectionInfoPanel.eventPositionChanged += OnOutsideConnectionInfoViewPanelPositionChanged;
            else
                outsideConnectionInfoPanel.eventPositionChanged -= OnOutsideConnectionInfoViewPanelPositionChanged;
        }

        private void MoveNextToUIComponent(UIComponent comp)
        {
            m_MainPanel.absolutePosition = new Vector3(comp.absolutePosition.x + comp.width + 1, comp.absolutePosition.y);
            m_MainPanel.zOrder = comp.zOrder;
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

        private void OnClose()
        {
            m_MainPanel.Hide();
        }

        private void OnShow()
        {
            m_IsAttachedToOutConPanel = true;
            var panel = m_OutsideConnectionsInfoViewPanel.component;
            MoveNextToUIComponent(panel);
            m_MainPanel.Show();
        }

        private void OnOutsideConnectionInfoViewPanelVisibilityChanged(UIComponent comp, bool isVisible)
        {
            if (isVisible)
                OnShow();
            else
            {
                if (!Utils.IsRoutesViewOn())
                    OnClose();
            }
        }

        private void OnOutsideConnectionInfoViewPanelPositionChanged(UIComponent comp, Vector2 newPosition)
        {
            if (m_IsAttachedToOutConPanel)
                MoveNextToUIComponent(m_OutsideConnectionsInfoViewPanel.component);
        }

        private void OnPositionChanged(UIComponent comp, Vector2 newPosition)
        {
            if (!m_IsAttachedToOutConPanel)
                return;

            var panelComp = m_OutsideConnectionsInfoViewPanel.component;
            var diff = m_MainPanel.absolutePosition - panelComp.absolutePosition;
            if (diff.y != 0 || diff.x != panelComp.width + 1)
                m_IsAttachedToOutConPanel = false;
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
                if (!m_OutsideConnectionDetailPanel.component.isVisible)
                {
                    var position = m_MainPanel.absolutePosition;
                    position.x += m_MainPanel.width + 1;
                    m_OutsideConnectionDetailPanel.component.absolutePosition = position;
                    m_OutsideConnectionDetailPanel.component.Show();
                }
            }
        }

        private void OnConnectionNameChanged(ushort buildingID, string name)
        {
            if (buildingID == 0)
                return;

            if (m_OutsideConnectionDetailPanel.buildingID == buildingID)
                m_OutsideConnectionDetailPanel.RefreshData();

            OnConnectionInfoChanged(buildingID);
        }

        private void OnConnectionInfoChanged(ushort buildingID)
        {
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

        private int CompareNames(UIComponent lhs, UIComponent rhs)
        {
            if (lhs == null || lhs == null)
                return 0;

            var lhsComponent = lhs.GetComponent<OutsideConnectionInfo>();
            var rhsComponent = rhs.GetComponent<OutsideConnectionInfo>();
            if (lhsComponent == null || rhsComponent == null)
                return 0;

            var buildingMgr = BuildingManager.instance;
            var value = OverviewPanelBase.NaturalCompare(lhsComponent.currentSettings.Name, rhsComponent.currentSettings.Name);
            return m_InverseSort ? -value : value;
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

        private void OnNameSort()
        {
            if (!m_Initialized)
                return;

            Sort(SortCriterion.NAME, CompareNames);
        }

        private void OnDirectionSort()
        {
            if (!m_Initialized)
                return;

            Sort(SortCriterion.DIRECTION,
                (UIComponent lhs, UIComponent rhs) =>
                {
                    if (lhs == null || lhs == null)
                        return 0;

                    var lhsComponent = lhs.GetComponent<OutsideConnectionInfo>();
                    var rhsComponent = rhs.GetComponent<OutsideConnectionInfo>();
                    if (lhsComponent == null || rhsComponent == null || lhsComponent.buildingID == 0 || rhsComponent.buildingID == 0)
                        return 0;

                    var lhsDir = Utils.QueryBuilding(lhsComponent.buildingID).m_flags & Building.Flags.IncomingOutgoing;
                    var rhsDir = Utils.QueryBuilding(rhsComponent.buildingID).m_flags & Building.Flags.IncomingOutgoing;
                    var value = lhsDir - rhsDir;
                    if (value != 0)
                        return m_InverseSort ? -value : value;
                    return CompareNames(lhs, rhs);
                }
            );
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
    }
}
