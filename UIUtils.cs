using ColossalFramework.UI;
using UnityEngine;

namespace ImprovedOutsideConnection
{
    public static class UIUtils
    {
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
