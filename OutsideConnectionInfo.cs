
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.UI;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    using fw = framework;

    class OutsideConnectionInfo : TableRowComponent
    {
        private ushort m_BuildingID = 0;
        private UIPanel m_MainPanel = null;
        private UISprite m_TransportTypeSprite = null;
        private UITextField m_ConnectionNameTextfield = null;
        private UIButton m_DetailViewButton = null;
        private UILabel m_DirectionLabel = null;
        private OutsideConnectionSettings m_CachedSettings = null;
        public OutsideConnectionSettings currentSettings => m_CachedSettings;

        private bool m_IsRefreshing = true;

        private new void Awake()
        {
            m_MainPanel = gameObject.AddComponent<UIPanel>();
            m_MainPanel.name = "MainPanel";
            m_MainPanel.size = new Vector2(400, 45);

            base.Awake();

            var iconSize = new Vector2(28, 28);
            m_TransportTypeSprite = fw.ui.UIHelper.AddSprite(m_MainPanel).
                SetSize(iconSize).
                SetRelativePosition(new Vector3(12, 8)).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.CenterVertical).
                GetSprite(true);

            m_DetailViewButton = fw.ui.UIHelper.AddButton(m_MainPanel).
                SetName("DetailViewButton").
                SetSize(iconSize).
                MoveInnerRightOf(m_MainPanel, 4).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.CenterVertical).
                SetBackgroundSprites(fw.PreparedSpriteSets.IconLineDetail).
                GetButton();

            m_DirectionLabel = fw.ui.UIHelper.AddLabel(m_MainPanel).
                SetName("DirectionLabel").
                SetAutoSize(false).
                SetTextAlignment(UIHorizontalAlignment.Center).
                SetSize(new Vector2(60, 35)).
                MoveLeftOf(m_DetailViewButton, 30).
                SetAnchor(UIAnchorStyle.Right | UIAnchorStyle.CenterVertical).
                GetLabel(true);

            m_ConnectionNameTextfield = fw.ui.UIHelper.AddTextField(m_MainPanel).
                SetHeight(35).
                SetRelativeY(10).
                ClampHorizontallyBetween(m_TransportTypeSprite, m_DirectionLabel, 15, 25).
                SetAnchor(UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.CenterVertical).
                GetTextField(true);
            m_ConnectionNameTextfield.eventTextSubmitted += delegate (UIComponent component, string newName)
            {
                if (!m_IsRefreshing && m_BuildingID != 0)
                {
                    m_CachedSettings.Name = newName;
                    eventNameChanged?.Invoke(m_BuildingID, newName);
                }
            };

            m_DetailViewButton.eventClick += delegate
            {
                if (m_BuildingID != 0 && eventDetailsOpen != null)
                    eventDetailsOpen(m_BuildingID);
            };

            component.eventVisibilityChanged += delegate (UIComponent comp, bool isVisible)
            {
                if (isVisible)
                {
                    RefreshData();
                }
            };
        }

        public event DetailsOpenEventHandler eventDetailsOpen;
        public event OutsideConnectionPropertyChanged<string> eventNameChanged;

        public ushort buildingID
        {
            get { return m_BuildingID; }
            set { SetBuildingID(value); }
        }

        public void RefreshData()
        {
            if (m_BuildingID == 0 || !OutsideConnectionSettingsManager.instance.SettingsDict.TryGetValue(m_BuildingID, out m_CachedSettings))
                return;

            var connectionAI = Utils.QueryBuildingAI(buildingID) as OutsideConnectionAI;
            if (connectionAI == null)
            {
                m_BuildingID = 0;
                m_CachedSettings = null;
                return;
            }

            m_IsRefreshing = true;

            m_TransportTypeSprite.spriteName = Utils.GetSpriteNameForTransferReason(connectionAI.m_dummyTrafficReason);
            m_ConnectionNameTextfield.text = m_CachedSettings.Name;

            m_DirectionLabel.text = Utils.GetStringForDirectionFlag(Utils.QueryBuilding(m_BuildingID).m_flags);

            m_IsRefreshing = false;
        }

        private void SetBuildingID(ushort id)
        {
            m_BuildingID = id;

            RefreshData();
        }
    }
}
