
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.UI;
using System;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    using fw = framework;

    class OverviewPanelExtension : OverviewPanelBase
    {
        private UIPanel m_MainPanel = null;
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
            if (m_Initialized && component.isVisible && m_LastOutsideConnectionCount != BuildingManager.instance.GetOutsideConnections().m_size)
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

            m_MainPanel = fw.ui.UIHelper.MakeMainPanel(gameObject).
                SetAbsolutePosition(new Vector3(400, 20)).
                SetSize(new Vector2(500, 700)).
                SetName(m_MainPanelName).
                SetMinimumSize(new Vector2(500, 150)).
                GetPanel();

            var panelIcon = fw.ui.UIHelper.AddSprite(m_MainPanel).
                SetSize(fw.ui.UIHelper.DefaultPanelIconSize).
                SetSpriteName(fw.CommonSprites.IconOutsideConnections.normal).
                MoveInnerTopLeftOf(m_MainPanel, new Vector3(8, 2)).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left).
                GetSprite();

            var panelClose = fw.ui.UIHelper.AddButton(m_MainPanel).
                SetName("Close").
                SetBackgroundSprites(fw.PreparedSpriteSets.IconClose).
                MoveInnerTopRightOf(m_MainPanel, new Vector3(5, 2)).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Right).
                GetButton();
            panelClose.eventClick += delegate { OnClose(); };

            var panelHeadline = fw.ui.UIHelper.AddLabel(m_MainPanel, true).
                SetAutoSize(false).
                SetAutoHeight(true).
                SetTextScale(1f).
                SetRelativePosition(new Vector3(0, 10)).
                ClampHorizontallyBetween(panelIcon, panelClose, 5, 5).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right).
                SetName("PanelLabel").
                SetText("Advanced Outside Connections Overview").
                SetTextAlignment(UIHorizontalAlignment.Center).
                SwapZOrder(panelClose).
                GetLabel();

            var dragHandle = fw.ui.UIHelper.AddDragHandle(m_MainPanel).
                SetRelativePosition(Vector3.zero).
                SetSize(new Vector2(m_MainPanel.width, fw.ui.UIHelper.DefaultDragHandleSize)).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right).
                SetTarget(m_MainPanel).
                SwapZOrder(panelClose).
                GetDragHandle();

            InitTitlePanel();

            m_BottomResizeHandle = fw.ui.UIHelper.AddResizeHandle(m_MainPanel, UIResizeHandle.ResizeEdge.Bottom).GetResizeHandle();

            m_OutsideConnectionCountLabel = fw.ui.UIHelper.AddLabel(m_MainPanel, true).
                SetName("OutsideConnectionCount").
                SetPrefix("Outside Connections-Count: ").
                SetAutoSize(true).
                MoveInnerBottomLeftOf(m_MainPanel, new Vector3(10, 7)).
                SwapZOrder(m_BottomResizeHandle).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Bottom).
                GetLabel();

            const int ScrollablePanelY = 85;
            m_Scrollbar = fw.ui.UIHelper.AddScrollbarWithTrack(m_MainPanel, UIOrientation.Vertical).
                SetHeight(m_MainPanel.height - ScrollablePanelY - m_BottomResizeHandle.height).
                MoveInnerRightOf(m_MainPanel).
                SetRelativeY(ScrollablePanelY).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top | UIAnchorStyle.Bottom).
                GetScrollbar();

            m_ScrollablePanel = fw.ui.UIHelper.AddScrollablePanel(m_MainPanel, UIOrientation.Vertical).
                SetWidth(m_Scrollbar.absolutePosition.x - m_MainPanel.absolutePosition.x - 1).
                SetHeight(m_Scrollbar.height).
                MoveLeftOf(m_Scrollbar).
                SetRelativeY(ScrollablePanelY).
                SetAnchor(UIAnchorStyle.All).
                SetVerticalScrollbar(m_Scrollbar).
                //SetBackgroundSprite(fw.CommonSprites.EmptySprite).
                GetScrollabelPanel();

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

        private void InitTitlePanel()
        {
            m_OutsideConnectionTitle = fw.ui.UIHelper.AddPanel(m_MainPanel).
                SetRelativePosition(new Vector3(0, 40)).
                SetSize(new Vector2(m_MainPanel.width, 45)).
                SetName("OutsideConnectionTitle").
                SetAutoLayout(true).
                SetAutoLayoutDirection(LayoutDirection.Horizontal).
                SetAutoLayoutPadding(new RectOffset(10, 15, 5, 5)).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Top).
                GetPanel();

            var transportTypeButton = fw.ui.UIHelper.AddButton(m_OutsideConnectionTitle).
                SetBackgroundSprites(fw.PreparedSpriteSets.IconPublicTransport).
                SetName("TransportTypeButton").
                SetVerticalAlignment(UIVerticalAlignment.Middle).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Top).
                GetButton();
            transportTypeButton.eventClick += delegate { OnTransportTypeSort(); };

            var nameButton = fw.ui.UIHelper.AddButton(m_OutsideConnectionTitle).
                SetName("NameTitle").
                SetText("Connection Name").
                SetTextHorizontalAlignment(UIHorizontalAlignment.Center).
                SetSize(new Vector2(235, m_OutsideConnectionTitle.height)).
                SetAnchor(UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right).
                SetTextScale(1.0625f).
                GetButton();
            nameButton.eventClick += delegate { OnNameSort(); };

            var directionButton = fw.ui.UIHelper.AddButton(m_OutsideConnectionTitle).
                SetName("DirectionTitle").
                SetText("Direction").
                SetTextHorizontalAlignment(UIHorizontalAlignment.Center).
                SetSize(new Vector2(100, m_OutsideConnectionTitle.height)).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.Top).
                SetTextScale(1.0625f).
                GetButton();
            directionButton.eventClick += delegate { OnDirectionSort(); };
        }

        private void SetupEventsWithOutsideConnectionInfoViewPanel(bool init)
        {
            var outsideConnectionInfoPanel = UIView.Find<UIPanel>(fw.CommonPanelNames.OutgoingConnectionInfoViewPanel);
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
                    outsideConnectionInfo.component.width = m_ScrollablePanel.width - 2;
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
