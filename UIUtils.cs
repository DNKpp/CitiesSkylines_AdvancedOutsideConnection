using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Linq;
using System.Reflection;

namespace AdvancedOutsideConnection
{
    public static class CommonSpriteNames
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

        private UIComponent component = null;
        private UIButton button = null;
        private UIButton closeButton = null;
        private UICheckBox checkbox = null;
        private UILabel label = null;
        private UILabel headlineLabel = null;
        private UITabContainer tabContainer = null;
        private UITabstrip tabstrip = null;
        private UIPanel panel = null;
        private UIPanel mainPanel = null;
        private UISlicedSprite vThumb = null;
        private UISlicedSprite vTrack = null;
        private UISprite icon = null;
        private UIButton tabButton = null;
        private UISlicedSprite caption = null;

        private UIFont m_HeadlineFont = null;
        public UIFont headlineFont => m_HeadlineFont;
        private UIFont m_TextFont = null;
        public UIFont textFont => m_TextFont;

        //private CursorInfo m_VerticalResizeHoverCursor = null;
        //public CursorInfo verticalResizeHoverCursor => m_VerticalResizeHoverCursor;

        private UIResizeHandle m_VerticalResizeHandle = null;
        public UIResizeHandle verticalResizeHandle => m_VerticalResizeHandle;

        private void Init()
        {
            try
            {
                component = UITemplateManager.Get("EmptyContainer");

                var outsideConnectionInfoPanel = UIView.Find<UIPanel>(UIUtils.OutgoingConnectionInfoViewPanelName);
                var infoPanelComponent = outsideConnectionInfoPanel.GetComponent<OutsideConnectionsInfoViewPanel>();

                panel = infoPanelComponent.Find<UIPanel>("ExportLegend");
                mainPanel = outsideConnectionInfoPanel;
                button = infoPanelComponent.Find<UIButton>("Export");
                label = infoPanelComponent.Find<UILabel>("ExportTotal");
                m_TextFont = label.font;
                tabstrip = infoPanelComponent.Find<UITabstrip>("Tabstrip");
                caption = UIUtils.MakeCopy(infoPanelComponent.Find<UISlicedSprite>("Caption"), component);
                headlineLabel = UIUtils.MakeCopy(caption.Find<UILabel>("Label"), component);
                m_HeadlineFont = headlineLabel.font;
                icon = UIUtils.MakeCopy(caption.Find<UISprite>("Icon"), component);
                var captionClose = caption.Find<UIButton>("Close");
                UIUtils.ReleaseEvents(captionClose);
                closeButton = UIUtils.MakeCopy(captionClose, component);
                //ReleaseEvents(ref templates.closeButton);

                //var trafficRoutesInfoPanel = UIView.Find<UIPanel>(UIUtils.TrafficRoutesInfoViewPanelName);
                //var trafficRoutesPanelComponent = trafficRoutesInfoPanel.GetComponent<TrafficRoutesInfoViewPanel>();
                //templates.checkbox = trafficRoutesPanelComponent.Find<UICheckBox>("CheckboxPedestrians");

                var publicTransportDetailPanel = UIView.Find<UIPanel>(UIUtils.PublicTransportDetailPanel);
                //vTrack = publicTransportDetailPanel.Find<UISlicedSprite>("Track");
                //vThumb = publicTransportDetailPanel.Find<UISlicedSprite>("Thumb");
                m_VerticalResizeHandle = UIUtils.MakeCopy(publicTransportDetailPanel.Find<UIResizeHandle>("Resize Handle"), component);
            }
            catch (Exception ex)
            {
                Utils.LogError("Error while gathering templates! " + ex.Message + " " + (object)ex.InnerException);
            }
        }

        public static UISlicedSprite AddCaption(UIComponent parent, string text, string spriteName = null)
        {
            var factory = instance;

            var newCaption = UIUtils.MakeCopy(factory.caption, parent);
            newCaption.width = parent.width;

            var captionClose = newCaption.Find<UIButton>("Close");
            UIUtils.ReleaseEvents(captionClose);
            captionClose.eventClick += delegate (UIComponent component, UIMouseEventParameter mouseParam)
            {
                parent.Hide();
            };

            var dragHandle = newCaption.Find<UIDragHandle>("Drag Handle");
            dragHandle.target = parent;

            var captionLabel = newCaption.Find<UILabel>("Label");
            captionLabel.text = text;

            var captionIcon = newCaption.Find<UISprite>("Icon");
            if (spriteName != null)
                captionIcon.spriteName = spriteName;

            return newCaption;
        }

        //public static UIPanel CopyOverviewPanel(string sourceName)
        //{
        //    var sourcePanel = UIView.library.Get<PublicTransportDetailPanel>("PublicTransportDetailPanel");
        //    var sourcUIComponent = (UIPanel)sourcePanel.component;
        //    var newPanel = UIUtils.MakeCopy(sourcUIComponent);

        //    //var caption = newPanel.Find<UISlicedSprite>("Caption");
        //    //var captionCloseButton = caption.Find<UIButton>("Close");
        //    //UIUtils.ReleaseEvents(captionCloseButton);
        //    //captionCloseButton.eventClick += new MouseEventHandler(
        //    //    (component, mouseParam) =>
        //    //    {
        //    //        if (newPanel && newPanel.isVisible)
        //    //            newPanel.Hide();
        //    //    }
        //    //);

        //    //var captionLabel = caption.Find<UIButton>("Label");

        //    return newPanel;
        //}
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
