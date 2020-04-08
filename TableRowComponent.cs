
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.UI;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    class TableRowComponent : UICustomControl
    {
        private bool m_IsMouseOver = false;
        public bool isMouseOver => m_IsMouseOver;

        private UIPanel m_Background = null;
        private Color32 m_BackgroundColor = new Color32(49, 52, 58, 0);

        protected void Awake()
        {
            if (!component)
                throw new System.Exception("TableRowComponent: You must setup component first before calling base.Awake()");

            m_Background = component.AddUIComponent<UIPanel>();
            m_Background.name = "BackgroundPanel";
            m_Background.backgroundSprite = CommonSprites.InfoViewPanel;
            m_Background.position = Vector3.zero;
            m_Background.size = component.size;
            m_Background.anchor = UIAnchorStyle.All;
            SetBackgroundColor();

            component.eventMouseEnter += OnMouseEnter;
            component.eventMouseLeave += OnMouseLeave;

            component.eventZOrderChanged += delegate
            {
                SetBackgroundColor();
            };
        }

        public void SetBackgroundColor()
        {
            Color32 backgroundColor = m_BackgroundColor;
            backgroundColor.a = (byte)((component.zOrder % 2 != 0) ? 127 : 255);
            if (m_IsMouseOver)
            {
                backgroundColor.r = (byte)Mathf.Min(255, backgroundColor.r * 3 >> 1);
                backgroundColor.g = (byte)Mathf.Min(255, backgroundColor.g * 3 >> 1);
                backgroundColor.b = (byte)Mathf.Min(255, backgroundColor.b * 3 >> 1);
            }
            m_Background.color = backgroundColor;
        }

        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!m_IsMouseOver)
            {
                m_IsMouseOver = true;
                SetBackgroundColor();
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (m_IsMouseOver)
            {
                m_IsMouseOver = false;
                SetBackgroundColor();
            }
        }
    }
}
