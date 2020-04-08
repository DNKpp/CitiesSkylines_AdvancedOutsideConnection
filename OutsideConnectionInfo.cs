using ColossalFramework.UI;
using UnityEngine;

namespace AdvancedOutsideConnection
{
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

            m_TransportTypeSprite = m_MainPanel.AddUIComponent<UISprite>();
            m_TransportTypeSprite.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            m_TransportTypeSprite.size = new Vector2(28, 28);
            m_TransportTypeSprite.relativePosition = new Vector3(12, 8);

            m_ConnectionNameTextfield = WidgetsFactory.AddTextField(m_MainPanel);
            m_ConnectionNameTextfield.size = new Vector2(175, 35);
            m_ConnectionNameTextfield.relativePosition = new Vector3(75, 10);
            m_ConnectionNameTextfield.anchor = UIAnchorStyle.Right | UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            m_ConnectionNameTextfield.eventTextSubmitted += delegate (UIComponent component, string newName)
            {
                if (!m_IsRefreshing && m_BuildingID != 0)
                    eventNameChanged?.Invoke(m_BuildingID, newName);
            };

            m_DetailViewButton = m_MainPanel.AddUIComponent<UIButton>();
            m_DetailViewButton.size = new Vector2(28, 28);
            m_DetailViewButton.relativePosition = new Vector3(m_MainPanel.width - 8 - m_DetailViewButton.width, 8);
            m_DetailViewButton.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            
            m_DetailViewButton.normalBgSprite = "LineDetailButton";
            m_DetailViewButton.hoveredBgSprite = "LineDetailButtonHovered";
            //m_DetailViewButton.focusedBgSprite = "";
            m_DetailViewButton.pressedBgSprite = "LineDetailButtonPressed";
            //m_DetailViewButton.disabledBgSprite = "";

            m_DirectionLabel = WidgetsFactory.AddLabel(m_MainPanel, "", false, "DirectionLabel");
            m_DirectionLabel.autoSize = false;
            m_DirectionLabel.textAlignment = UIHorizontalAlignment.Center;
            m_DirectionLabel.width = 60;
            m_DirectionLabel.height = m_ConnectionNameTextfield.height;
            m_DirectionLabel.relativePosition = m_ConnectionNameTextfield.relativePosition + new Vector3(m_ConnectionNameTextfield.width + 15, 0);
            m_DirectionLabel.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;


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

            m_IsRefreshing = true;

            var transportInfo = Utils.QueryTransportInfo(m_BuildingID);
            m_TransportTypeSprite.spriteName = CommonSprites.SubBarPublicTransport[(int)transportInfo.m_transportType];
            m_ConnectionNameTextfield.text = BuildingManager.instance.GetBuildingName(m_BuildingID, InstanceID.Empty);

            m_DirectionLabel.text = Utils.GetStringForDirectionFlag(m_BuildingID);

            m_IsRefreshing = false;
        }

        private void SetBuildingID(ushort id)
        {
            m_BuildingID = id;

            RefreshData();
        }
    }
}
