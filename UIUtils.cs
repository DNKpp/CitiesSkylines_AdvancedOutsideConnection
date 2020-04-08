using ColossalFramework;
using ColossalFramework.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    public static class CommonSprites
    {
        public static readonly string[] SubBarPublicTransport = new string[]
        {
            "SubBarPublicTransportBus",
            "SubBarPublicTransportMetro",
            "SubBarPublicTransportTrain",
            "SubBarPublicTransportShip",
            "SubBarPublicTransportPlane",
            "SubBarPublicTransportTaxi",
            "SubBarPublicTransportTram",
            "",//"SubBarPublicTransportEvacuationBus",
            "SubBarPublicTransportMonorail",
            "SubBarPublicTransportCableCar",
            "",//"SubBarPublicTransportPedestrian",
            "SubBarPublicTransportTours",
            "",//"SubBarPublicTransportHotAirBalloon",
            "SubBarPublicTransportPost",
            "SubBarPublicTransportTrolleybus",
            "",//"SubBarPublicTransportFishing",
            "",//"SubBarPublicTransportHelicopter",
        };

        public static readonly string MenuPanel2 = "MenuPanel2";
        public static readonly string GenericPanel = "GenericPanel";
        public static readonly string InfoViewPanel = "InfoviewPanel";

        public static readonly string CheckBoxUnchecked = "check-unchecked";
        public static readonly string CheckBoxChecked = "check-checked";

        public class SpriteSet
        {
            public string normal;
            public string disabled;
            public string focused;
            public string hovered;
            public string pressed;
            

            public SpriteSet(string normal, string disabled, string focused, string hovered, string pressed)
            {
                this.normal = normal;
                this.disabled = disabled;
                this.focused = focused;
                this.hovered = hovered;
                this.pressed = pressed;
            }

            public SpriteSet(SpriteSet other)
            {
                normal = other.normal;
                disabled = other.normal;
                focused = other.normal;
                hovered = other.hovered;
                pressed = other.pressed;
            }

            public UIMultiStateButton.SpriteSet ToMultiStateButtonSpriteSet(UIMultiStateButton button)
            {
                var spriteSet = new UIMultiStateButton.SpriteSet();
                spriteSet.Setter(button);
                spriteSet.normal = normal;
                spriteSet.disabled = disabled;
                spriteSet.focused = focused;
                spriteSet.hovered = hovered;
                spriteSet.pressed = pressed;
                return spriteSet;
            }
        };

        public static readonly SpriteSet IconOutsideConnections = new SpriteSet(
            "InfoIconOutsideConnections",
            "InfoIconOutsideConnectionsDisabled",
            "InfoIconOutsideConnectionsFocused",
            "InfoIconOutsideConnectionsHovered",
            "InfoIconOutsideConnectionsPressed"
        );

        public static readonly SpriteSet LocationMarkerActive = new SpriteSet(
            "LocationMarkerActiveNormal",
            "LocationMarkerActiveDisabled",
            "LocationMarkerActiveFocused",
            "LocationMarkerActiveHovered",
            "LocationMarkerActivePressed"
        );

        public static readonly SpriteSet LocationMarker = new SpriteSet(
            "LocationMarkerNormal",
            "LocationMarkerDisabled",
            "LocationMarkerFocused",
            "LocationMarkerHovered",
            "LocationMarkerPressed"
        );

        public static readonly SpriteSet InfoIconBase = new SpriteSet(
            "InfoIconBaseNormal",
            "InfoIconBaseDisabled",
            "InfoIconBaseFocused",
            "InfoIconBaseHovered",
            "InfoIconBasePressed"
        );

        public static readonly SpriteSet InfoIconTrafficRoutes = new SpriteSet(
            "InfoIconTrafficRoutes",
            "InfoIconTrafficRoutesDisabled",
            "InfoIconTrafficRoutesFocused",
            "InfoIconTrafficRoutesHovered",
            "InfoIconTrafficRoutesPressed"
        );

        public static readonly SpriteSet IconClose = new SpriteSet(
            "buttonclose",
            "",
            "",
            "buttonclosehover",
            "buttonclosepressed"
        );

        public static readonly SpriteSet IconArrowUp = new SpriteSet(
            "IconUpArrow",
            "IconUpArrowDisabled",
            "IconUpArrowFocused",
            "IconUpArrowHovered",
            "IconUpArrowPressed"
        );

        public static readonly string EmptySprite = "EmptySprite";

        public static readonly SpriteSet TextField = new SpriteSet(
            "",
            "",
            "TextFieldPanel",
            "TextFieldPanelHovered",
            ""
        );

        public static readonly string ScrollbarThumb = "ScrollbarThumb";
        public static readonly string ScrollbarTrack = "ScrollbarTrack";
    }

    public class WidgetsFactory
    {
        private static WidgetsFactory m_Instance = null;
        public static WidgetsFactory instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new WidgetsFactory();
                    m_Instance.Init();
                }
                return m_Instance;
            }
        }

        private UIFont m_HeadlineFont = null;
        public UIFont headlineFont => m_HeadlineFont;
        private UIFont m_TextFont = null;
        public UIFont textFont => m_TextFont;

        private CursorInfo m_VerticalResizeHoverCursor = null;
        public CursorInfo verticalResizeHoverCursor => m_VerticalResizeHoverCursor;

        private UIResizeHandle m_VerticalResizeHandle = null;
        public UIResizeHandle verticalResizeHandle => m_VerticalResizeHandle;

        private void Init()
        {
            try
            {
                var outsideConnectionInfoPanel = UIView.Find<UIPanel>(UIUtils.OutgoingConnectionInfoViewPanelName);
                var infoPanelComponent = outsideConnectionInfoPanel.GetComponent<OutsideConnectionsInfoViewPanel>();

                m_TextFont = infoPanelComponent.Find<UILabel>("ExportTotal").font;
                var caption = infoPanelComponent.Find<UISlicedSprite>("Caption");
                m_HeadlineFont = caption.Find<UILabel>("Label").font;

                var publicTransportDetailPanel = UIView.Find<UIPanel>(UIUtils.PublicTransportDetailPanel);
                m_VerticalResizeHoverCursor = publicTransportDetailPanel.Find<UIResizeHandle>("Resize Handle").hoverCursor;
            }
            catch (Exception ex)
            {
                Utils.LogError("Error while gathering templates! " + ex.Message + " " + (object)ex.InnerException);
            }
        }

        public static UIPanel AddPanelCaption(UIPanel parent, string text, string iconSpriteName, string name = "Caption")
        {
            var caption = parent.AddUIComponent<UIPanel>();
            caption.name = name;
            caption.relativePosition = Vector3.zero;
            caption.size = new Vector2(parent.width, 40);
            caption.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;

            var icon = AddPanelIcon(caption, iconSpriteName);
            var label = AddLabel(caption, text, true);

            var dragHandle = AddPanelDragHandle(caption);
            dragHandle.target = parent;
            var panelClose = AddPanelCloseButton(caption);

            label.autoSize = false;
            label.relativePosition = new Vector3(icon.position.x + icon.width + 2, 6);
            label.size = new Vector2(panelClose.relativePosition.x - label.relativePosition.x -2, (caption.height - 2 * label.relativePosition.y));
            label.textAlignment = UIHorizontalAlignment.Center;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.anchor = UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Left;

            return caption;
        }

        public static UIButton AddButton(UIComponent parent, CommonSprites.SpriteSet sprites, string name = "Button")
        {
            var button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.normalBgSprite = sprites.normal;
            button.focusedBgSprite = sprites.focused;
            button.disabledBgSprite = sprites.disabled;
            button.pressedBgSprite = sprites.pressed;
            button.hoveredBgSprite = sprites.hovered;
            button.size = new Vector2(32, 32);
            return button;
        }

        public static UIButton AddButton(UIComponent parent, string text, string name = "Button")
        {
            var button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.text = text;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.autoSize = true;
            return button;
        }

        public static UIPanel MakeMainPanel(GameObject gameObject, string name, Vector2 size)
        {
            var newPanel = gameObject.AddComponent<UIPanel>();
            newPanel.name = name;
            newPanel.size = size;
            newPanel.clipChildren = true;
            newPanel.backgroundSprite = CommonSprites.MenuPanel2;
            return newPanel;
        }

        public static UIResizeHandle AddPanelBottomResizeHandle(UIPanel parent, string name = "BottomResizeHandle")
        {
            var resizeHandle = parent.AddUIComponent<UIResizeHandle>();
            resizeHandle.name = name;
            resizeHandle.size = new Vector2(parent.width, 35);
            resizeHandle.relativePosition = new Vector3(0, parent.height - resizeHandle.height);
            resizeHandle.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Bottom;
            resizeHandle.edges = UIResizeHandle.ResizeEdge.Bottom;
            resizeHandle.hoverCursor = WidgetsFactory.instance.verticalResizeHoverCursor;
            return resizeHandle;
        }

        public static UILabel AddLabel(UIComponent parent, string text, bool isHeadline = false, string name = "Label")
        {
            var label = parent.AddUIComponent<UILabel>();
            label.name = name;
            label.text = text;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.textAlignment = UIHorizontalAlignment.Left;
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;

            if (isHeadline)
            {
                label.font = WidgetsFactory.instance.headlineFont;
                label.textColor = Color.white;
            }
            else
            {
                label.font = WidgetsFactory.instance.textFont;
                label.textColor = new Color32(185, 221, 254, 255);
            }
            return label;
        }

        public static UICheckBox AddCheckBox<T>(UIComponent parent, string text, T userData, PropertyChangedEventHandler<bool> OnCheckChanged = null, string name = "CheckBox")
        {
            var checkbox = parent.AddUIComponent<UICheckBox>();
            checkbox.name = name;
            checkbox.height = 20;
            checkbox.width = 100;
            checkbox.objectUserData = userData;
            if (OnCheckChanged != null)
                checkbox.eventCheckChanged += OnCheckChanged;

            var label = AddLabel(checkbox, text, false);
            checkbox.label = label;
            label.name = "Label";
            label.size = new Vector2(75, 20);
            label.relativePosition = new Vector3(22f, 2f);
            checkbox.label = label;

            var sprite = checkbox.AddUIComponent<UISprite>();
            sprite.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
            sprite.spriteName = CommonSprites.CheckBoxUnchecked;
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            var checkedSprite = checkbox.AddUIComponent<UISprite>();
            checkedSprite.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
            checkedSprite.spriteName = CommonSprites.CheckBoxChecked;
            checkedSprite.size = new Vector2(16f, 16f);
            checkedSprite.relativePosition = Vector3.zero;
            checkbox.checkedBoxObject = checkedSprite;

            return checkbox;
        }

        public static UIPanel AddGroupedCheckBoxes<T>(UIComponent parent, KeyValuePair<string, T>[] textUserData, int padding = 5, bool alignVertically = true, PropertyChangedEventHandler<bool> OnCheckChanged = null, string namePrefix = "CheckBox")
        {
            UIPanel subPanel = parent.AddUIComponent<UIPanel>();
            subPanel.name = "CheckBoxPanel";
            var pos = new Vector3(0, 0);
            foreach (var data in textUserData)
            {
                var checkBox = AddCheckBox(subPanel, data.Key, data.Value, OnCheckChanged, namePrefix + "-" + data.Key);
                checkBox.group = subPanel;
                checkBox.relativePosition = pos;
                if (alignVertically)
                {
                    pos.y += checkBox.height + padding;
                }
                else
                {
                    pos.x += checkBox.width + padding;
                }
            }
            return subPanel;
        }

        public static UISprite AddPanelIcon(UIPanel parent, string spriteName)
        {
            var icon = parent.AddUIComponent<UISprite>();
            icon.name = "Icon";
            icon.spriteName = spriteName;
            icon.size = new Vector2(36, 36);
            icon.relativePosition = new Vector3(13, 2);
            icon.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
            return icon;
        }

        public static UIButton AddPanelCloseButton(UIPanel parent)
        {
            var close = AddButton(parent, CommonSprites.IconClose, "Close");
            close.relativePosition = new Vector3(parent.width - close.width - 13, 2);
            close.anchor = UIAnchorStyle.Top | UIAnchorStyle.Right;
            return close;
        }

        public static UIMultiStateButton AddMultistateButton(UIPanel parent, CommonSprites.SpriteSet[] foregroundSprites = null, CommonSprites.SpriteSet[] backgroundSprites = null, string name = "MultistateButton")
        {
            var button = parent.AddUIComponent<UIMultiStateButton>();
            button.name = name;
            button.size = new Vector2(32, 32);
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            int setCount = Math.Max(Math.Max(1, foregroundSprites == null ? 0 : foregroundSprites.Length), backgroundSprites == null ? 0 : backgroundSprites.Length);
            button.spritePadding = new RectOffset(0, 0, 0, 0);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;

            // background sprite states
            var spriteSetList = new List<UIMultiStateButton.SpriteSet>();
            for (int i = 0; i < setCount; ++i)
            {
                if (backgroundSprites != null && i < backgroundSprites.Length)
                {
                    spriteSetList.Add(backgroundSprites[i].ToMultiStateButtonSpriteSet(button));
                }
                else
                    spriteSetList.Add(new UIMultiStateButton.SpriteSet());
            }
            var spriteSetState = new UIMultiStateButton.SpriteSetState();
            Traverse.Create(spriteSetState).Field<List<UIMultiStateButton.SpriteSet>>("m_SpriteSetStates").Value = spriteSetList;
            Traverse.Create(button).Field<UIMultiStateButton.SpriteSetState>("m_BackgroundSprites").Value = spriteSetState;

            // foreground sprite states
            spriteSetList = new List<UIMultiStateButton.SpriteSet>();
            for (int i = 0; i < setCount; ++i)
            {
                if (foregroundSprites != null && i < foregroundSprites.Length)
                    spriteSetList.Add(foregroundSprites[i].ToMultiStateButtonSpriteSet(button));
                else
                    spriteSetList.Add(new UIMultiStateButton.SpriteSet());
            }
            spriteSetState = new UIMultiStateButton.SpriteSetState();
            Traverse.Create(spriteSetState).Field<List<UIMultiStateButton.SpriteSet>>("m_SpriteSetStates").Value = spriteSetList;
            Traverse.Create(button).Field<UIMultiStateButton.SpriteSetState>("m_ForegroundSprites").Value = spriteSetState;

            button.activeStateIndex = 0;

            return button;
        }

        public static UIDragHandle AddPanelDragHandle(UIPanel parent)
        {
            var drag = parent.AddUIComponent<UIDragHandle>();
            drag.name = "DragHandle";
            drag.size = new Vector2(parent.width, 40);
            drag.relativePosition = Vector3.zero;
            drag.anchor = UIAnchorStyle.Top | UIAnchorStyle.Right | UIAnchorStyle.Left;
            return drag;
        }

        public static UITextField AddTextField(UIComponent parent, string text = "", bool headline = false)
        {
            var textfield = parent.AddUIComponent<UITextField>();
            textfield.name = "TextField";
            textfield.text = text;
            textfield.horizontalAlignment = UIHorizontalAlignment.Center;
            textfield.verticalAlignment = UIVerticalAlignment.Middle;
            textfield.textScale = 1f;
            if (headline)
            {
                textfield.font = WidgetsFactory.instance.headlineFont;
                textfield.textColor = Color.white;
            }
            else
            {
                textfield.font = WidgetsFactory.instance.textFont;
                textfield.textColor = new Color32(185, 221, 254, 255);
            }
            textfield.multiline = false;
            textfield.readOnly = false;
            textfield.cursorBlinkTime = 0.45f;
            textfield.cursorWidth = 1;
            textfield.selectionSprite = CommonSprites.EmptySprite;
            textfield.focusedBgSprite = CommonSprites.TextField.focused;
            textfield.hoveredBgSprite = CommonSprites.TextField.hovered;
            textfield.selectionBackgroundColor = new Color32(233, 201, 148, 255);
            textfield.allowFloats = false;
            textfield.numericalOnly = false;
            textfield.allowNegative = false;
            textfield.isPasswordField = false;
            textfield.builtinKeyNavigation = true;
            textfield.padding = new RectOffset(0, 0, 9, 3);
            return textfield;
        }

        public static UIScrollablePanel AddScrollablePanel(UIComponent parent, bool vertical = true)
        {
            var scrollabel = parent.AddUIComponent<UIScrollablePanel>();
            scrollabel.autoLayout = true;
            scrollabel.autoLayoutPadding = new RectOffset(5, 5, 1, 0);
            if (vertical)
            {
                scrollabel.autoLayoutDirection = LayoutDirection.Vertical;
                scrollabel.scrollWheelDirection = UIOrientation.Vertical;
            }
            else
            {
                scrollabel.autoLayoutDirection = LayoutDirection.Horizontal;
                scrollabel.scrollWheelDirection = UIOrientation.Horizontal;
            }
            scrollabel.clipChildren = true;
            //scrollabel.backgroundSprite = "MenuPanel2";
            scrollabel.name = "ScrollablePanel";
            scrollabel.scrollWheelAmount = 38;
            scrollabel.builtinKeyNavigation = true;
            scrollabel.anchor = UIAnchorStyle.All;
            return scrollabel;
        }

        public static int DefaultVScrollbarWidth => 21;

        public static UIScrollbar AddVerticalScrollbar(UIComponent parent, UIScrollablePanel scrollabelPanel)
        {
            var scrollbar = parent.AddUIComponent<UIScrollbar>();
            scrollbar.size = new Vector2(DefaultVScrollbarWidth, scrollabelPanel.height);
            scrollbar.relativePosition = new Vector3(scrollabelPanel.relativePosition.x + scrollabelPanel.width + 1, scrollabelPanel.relativePosition.y);
            scrollbar.anchor = UIAnchorStyle.Right | UIAnchorStyle.Top | UIAnchorStyle.Bottom;
            scrollbar.name = "Scrollbar";
            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.autoHide = false;
            scrollbar.incrementAmount = 38;
            scrollbar.stepSize = 1;
            var scrollbarTrack = scrollbar.AddUIComponent<UISlicedSprite>();
            scrollbarTrack.name = "Track";
            scrollbarTrack.spriteName = CommonSprites.ScrollbarTrack;
            scrollbarTrack.relativePosition = Vector3.zero;
            scrollbarTrack.size = scrollbar.size;
            scrollbarTrack.anchor = UIAnchorStyle.All;
            scrollbar.trackObject = scrollbarTrack;
            var scrollbarThumb = scrollbarTrack.AddUIComponent<UISlicedSprite>();
            scrollbarThumb.name = "Thumb";
            scrollbarThumb.spriteName = CommonSprites.ScrollbarThumb;
            scrollbarThumb.width = scrollbar.width - 6;
            scrollbar.thumbObject = scrollbarThumb;
            scrollabelPanel.verticalScrollbar = scrollbar;
            return scrollbar;
        }
    }

    public static class UIUtils
    {
        public static readonly string OutgoingConnectionInfoViewPanelName = "(Library) OutsideConnectionsInfoViewPanel";
        public static readonly string TrafficRoutesInfoViewPanelName = "(Library) TrafficRoutesInfoViewPanel";
        public static readonly string PublicTransportDetailPanel = "PublicTransportDetailPanel(Clone)";//"(Library) PublicTransportDetailPanel";

        public static T MakeCopy<T>(T source, UIComponent parent = null, bool releaseEvents = false) where T : UIComponent
        {
            T newObject = null;
            if (parent)
            {
                newObject = UnityEngine.Object.Instantiate(source, parent.transform);
                parent.AttachUIComponent(newObject.gameObject);
            }
            else
                newObject = UnityEngine.Object.Instantiate(source);

            parent.AttachUIComponent(newObject.gameObject);

            if (releaseEvents)
                ReleaseEvents(newObject);
            return newObject;
            //var newObject = parent.AddUIComponent<T>();
            //CopyStyle(newObject, source);
            //return newObject;
        }

        public static void ReleaseEvents<T>(T component) where T : UIComponent
        {
            FieldInfo[] array = (from f in component.GetType().GetAllFields()
                                 where typeof(Delegate).IsAssignableFrom(f.FieldType)
                                 select f).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetValue(component, null);
            }
        }

        public static void CopyStyle(UIPanel target, UIPanel source)
        {
            CopyStyle((UIComponent)target, source);

            target.atlas = source.atlas;
            target.autoFitChildrenHorizontally = source.autoFitChildrenHorizontally;
            target.autoFitChildrenVertically = source.autoFitChildrenVertically;
            target.autoLayout = source.autoLayout;
            target.autoLayoutDirection = source.autoLayoutDirection;
            target.autoLayoutPadding = source.autoLayoutPadding;
            target.autoLayoutStart = source.autoLayoutStart;
            target.backgroundSprite = source.backgroundSprite;
            target.flip = source.flip;
            target.padding = source.padding;
            target.useCenter = source.useCenter;
            target.verticalSpacing = source.verticalSpacing;
            target.wrapLayout = source.wrapLayout;
        }

        public static void CopyStyle(UISprite target, UISprite source)
        {
            CopyStyle((UIComponent)target, source);

            target.atlas = source.atlas;
            target.fillAmount = source.fillAmount;
            target.fillDirection = source.fillDirection;
            target.flip = source.flip;
            target.invertFill = source.invertFill;
            target.spriteName = source.spriteName;
        }

        public static void CopyStyle(UISlicedSprite target, UISlicedSprite source)
        {
            CopyStyle((UISprite)target, source);
        }

        public static void CopyStyle(UILabel target, UILabel source)
        {
            CopyStyle((UITextComponent)target, source);

            target.atlas = source.atlas;
            target.autoHeight = source.autoHeight;
            target.backgroundSprite = source.backgroundSprite;
            target.padding = source.padding;
            target.prefix = source.prefix;
            target.suffix = source.suffix;
            target.textAlignment = source.textAlignment;
            target.verticalAlignment = source.verticalAlignment;
            target.wordWrap = source.wordWrap;
        }

        public static void CopyStyle(UICheckBox target, UICheckBox source)
        {
            CopyStyle((UIComponent)target, source);

            var label = target.AddUIComponent<UILabel>();
            CopyStyle(label, source.label);
            target.label = label;
        }

        public static void CopyStyle(UITabstrip target, UITabstrip source)
        {
            CopyStyle((UIComponent)target, source);

            target.atlas = source.atlas;
            target.color = source.color;
            target.backgroundSprite = source.backgroundSprite;
            target.padding = source.padding;
        }

        public static void CopyStyle(UITabContainer target, UITabContainer source)
        {
            CopyStyle((UIComponent)target, source);

            target.atlas = source.atlas;
            target.backgroundSprite = source.backgroundSprite;
            target.padding = source.padding;
        }

        public static void CopyStyle(UIButton target, UIButton source)
        {
            CopyStyle((UIInteractiveComponent)target, source);

            target.disabledBottomColor = source.disabledBottomColor;
            target.pressedBgSprite = source.pressedBgSprite;
            target.pressedColor = source.pressedColor;
            target.pressedFgSprite = source.pressedFgSprite;
            target.pressedTextColor = source.pressedTextColor;
            target.textHorizontalAlignment = source.textHorizontalAlignment;
            target.textPadding = source.textPadding;
            target.textVerticalAlignment = source.textVerticalAlignment;
            target.wordWrap = source.wordWrap;
        }

        public static void CopyStyle(UITextField target, UITextField source)
        {
            CopyStyle((UIInteractiveComponent)target, source);

            target.allowFloats = source.allowFloats;
            target.allowNegative = source.allowNegative;
            target.cursorBlinkTime = source.cursorBlinkTime;
            target.cursorWidth = source.cursorWidth;
            target.isPasswordField = source.isPasswordField;
            target.maxLength = source.maxLength;
            target.multiline = source.multiline;
            target.numericalOnly = source.numericalOnly;
            target.padding = source.padding;
            target.passwordCharacter = source.passwordCharacter;
            target.readOnly = source.readOnly;
            target.selectionBackgroundColor = source.selectionBackgroundColor;
        }

        private static void CopyStyle(UIInteractiveComponent target, UIInteractiveComponent source)
        {
            CopyStyle((UITextComponent)target, source);

            target.atlas = source.atlas;
            target.canFocus = source.canFocus;
            target.disabledBgSprite = source.disabledBgSprite;
            target.disabledFgSprite = source.disabledFgSprite;
            target.focusedBgSprite = source.focusedBgSprite;
            target.focusedFgSprite = source.focusedFgSprite;
            target.foregroundSpriteMode = source.foregroundSpriteMode;
            target.horizontalAlignment = source.horizontalAlignment;
            target.hoveredBgSprite = source.hoveredBgSprite;
            target.hoveredFgSprite = source.hoveredFgSprite;
            target.normalBgSprite = source.normalBgSprite;
            target.normalFgSprite = source.normalFgSprite;
            target.scaleFactor = source.scaleFactor;
            target.spritePadding = source.spritePadding;
            target.verticalAlignment = source.verticalAlignment;
        }

        private static void CopyStyle(UITextComponent target, UITextComponent source)
        {
            CopyStyle((UIComponent)target, source);

            target.bottomColor = source.bottomColor;
            target.characterSpacing = source.characterSpacing;
            target.colorizeSprites = source.colorizeSprites;
            target.disabledTextColor = source.disabledTextColor;
            target.dropShadowColor = source.dropShadowColor;
            target.dropShadowOffset = source.dropShadowOffset;
            target.font = source.font;
            target.outlineColor = source.outlineColor;
            target.outlineSize = source.outlineSize;
            target.textColor = source.textColor;
            target.textScale = source.textScale;
            target.textScaleMode = source.textScaleMode;
            target.useDropShadow = source.useDropShadow;
            target.useGradient = source.useGradient;
            target.useOutline = source.useOutline;
        }

        private static void CopyStyle(UIComponent target, UIComponent source)
        {
            target.anchor = source.anchor;
            target.arbitraryPivotOffset = source.arbitraryPivotOffset;
            target.autoSize = source.autoSize;
            target.bringTooltipToFront = source.bringTooltipToFront;
            target.canFocus = source.canFocus;
            target.clickSound = source.clickSound;
            target.disabledClickSound = source.disabledClickSound;
            target.disabledColor = source.disabledColor;
            target.hoverCursor = source.hoverCursor;
            target.opacity = source.opacity;
            target.pivot = source.pivot;
            target.playAudioEvents = source.playAudioEvents;
            target.size = source.size;
            target.useGUILayout = source.useGUILayout;
        }
    }
}
