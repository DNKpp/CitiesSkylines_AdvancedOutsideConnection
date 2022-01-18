
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedOutsideConnection.framework.ui
{
	public class UIHelper
	{
		private static UIHelper m_Instance = null;
		public static UIHelper instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new UIHelper();
					m_Instance.Init();
				}
				return m_Instance;
			}
		}

		private void Init()
		{
			var outsideConnectionInfoPanel = UIView.Find<UIPanel>(CommonPanelNames.OutgoingConnectionInfoViewPanel);
			var infoPanelComponent = outsideConnectionInfoPanel.GetComponent<OutsideConnectionsInfoViewPanel>();

			m_TextFont = infoPanelComponent.Find<UILabel>("ExportTotal").font;
			var caption = infoPanelComponent.Find<UISlicedSprite>("Caption");
			m_HeadlineFont = caption.Find<UILabel>("Label").font;

			var publicTransportDetailPanel = UIView.Find<UIPanel>(CommonPanelNames.PublicTransportDetailPanel);
			m_VerticalResizeHoverCursor = publicTransportDetailPanel.Find<UIResizeHandle>("Resize Handle").hoverCursor;
		}

		private UIFont m_HeadlineFont = null;
		public UIFont headlineFont => m_HeadlineFont;
		private UIFont m_TextFont = null;
		public UIFont textFont => m_TextFont;
		private Color32 m_HeadlineColor = Color.white;
		public Color32 headlineColor => m_HeadlineColor;
		private Color32 m_TextColor = new Color32(185, 221, 254, 255);
		public Color32 textColor => m_TextColor;

		public static Color32 contenPanelColor => new Color32(206, 206, 206, 255);

		private CursorInfo m_VerticalResizeHoverCursor = null;
		public CursorInfo verticalResizeHoverCursor => m_VerticalResizeHoverCursor;

		public static int DefaultVScrollbarWidth => 21;
		public static int DefaultHSliderHeight => 13;
		public static Vector2 DefaultSpriteButtonSize => new Vector2(32f, 32f);
		public static Vector2 DefaultPanelIconSize => new Vector2(36f, 36f);
		public static int DefaultDragHandleSize => 40;

		public static UIPanelHelper MakeMainPanel(GameObject gameObject)
		{
			var helper = new UIPanelHelper(gameObject.AddComponent<UIPanel>());
			return helper.SetClipChildren(true).
				SetPosition(Vector2.zero).
				SetBackgroundSprite(CommonSprites.MenuPanel2);
		}

		public static UIPanelHelper AddPanel(UIComponent parent)
		{
			var helper = new UIPanelHelper(parent.AddUIComponent<UIPanel>());
			return helper;
		}

		public static UILabelHelper AddLabel(UIComponent parent, bool isHeadline = false)
		{
			var helper = new UILabelHelper(parent.AddUIComponent<UILabel>());
			return helper.SetName("Label").
				SetRelativePosition(Vector3.zero).
				SetTextAlignment(UIHorizontalAlignment.Left).
				SetVerticalAlignment(UIVerticalAlignment.Middle).
				SetAutoHeight(true).
				SetFont(isHeadline ? instance.headlineFont : instance.textFont).
				SetTextColor(isHeadline ? instance.headlineColor : instance.textColor);
		}

		public static UITextFieldHelper AddTextField(UIComponent parent, bool isHeadline = false)
		{
			var helper = new UITextFieldHelper(parent.AddUIComponent<UITextField>());
			return helper.SetName("TextField").
				SetHorizontalAlignment(UIHorizontalAlignment.Center).
				SetVerticalAlignment(UIVerticalAlignment.Middle).
				SetFont(isHeadline ? instance.headlineFont : instance.textFont).
				SetTextColor(isHeadline ? instance.headlineColor : instance.textColor).
				SetTextScale(1f).
				SetMultiline(false).
				SetIsPasswordField(false).
				SetReadOnly(false).
				SetAllowFloats(false).
				SetAllowNegative(false).
				SetNumericalOnly(false).
				SetBuiltinKeyNavigation(true).
				SetPadding(new RectOffset(0, 0, 9, 3)).
				SetSelectionBackgroundColor(new Color32(233, 201, 148, 255)).
				SetCursorBlinkTime(0.45f).
				SetCursorWidth(1).
				SetSelectionSprite(CommonSprites.EmptySprite).
				SetFocusedBgSprite(CommonSprites.TextField.focused).
				SetHoveredBgSprite(CommonSprites.TextField.hovered);
		}

		public static UIDragHandleHelper AddDragHandle(UIComponent parent)
		{
			var helper = new UIDragHandleHelper(parent.AddUIComponent<UIDragHandle>());
			return helper.SetName("DragHandle").
				SetPosition(Vector3.zero);
		}

		public static UIResizeHandleHelper AddResizeHandle(UIComponent parent, UIResizeHandle.ResizeEdge edges)
		{
			const int smallSideSize = 35;

			var size = new Vector2(0, 0);
			var position = new Vector3(0, 0, 0);
			var anchor = UIAnchorStyle.None;
			bool vertical = (edges & (UIResizeHandle.ResizeEdge.Left | UIResizeHandle.ResizeEdge.Right)) != 0;
			bool horizontal = (edges & (UIResizeHandle.ResizeEdge.Top | UIResizeHandle.ResizeEdge.Bottom)) != 0;

			if (vertical && horizontal)
			{
				size = parent.size;
				anchor = UIAnchorStyle.All;
			}
			else if (vertical)
			{
				size.x = smallSideSize;
				size.y = parent.height;
				anchor = UIAnchorStyle.Top | UIAnchorStyle.Bottom;
				if ((edges & UIResizeHandle.ResizeEdge.Left) != 0)
					anchor |= UIAnchorStyle.Left;
				else
				{
					position.x = parent.width - size.x;
					anchor |= UIAnchorStyle.Right;
				}
			}
			else if (horizontal)
			{
				size.x = parent.width;
				size.y = smallSideSize;
				anchor = UIAnchorStyle.Left | UIAnchorStyle.Right;
				if ((edges & UIResizeHandle.ResizeEdge.Top) != 0)
					anchor |= UIAnchorStyle.Top;
				else
				{
					position.y = parent.height - size.y;
					anchor |= UIAnchorStyle.Bottom;
				}
			}

			var helper = new UIResizeHandleHelper(parent.AddUIComponent<UIResizeHandle>());
			return helper.SetName("ResizeHandle").
				SetHoverCursor(instance.verticalResizeHoverCursor).	 // ToDo: change cursor for vertical cases
				SetRelativePosition(position).
				SetSize(size).
				SetAnchor(anchor).
				SetEdges(edges);
		}

		public static UICheckBoxHelper AddCheckBox(UIComponent parent)
		{
			var helper = new UICheckBoxHelper(parent.AddUIComponent<UICheckBox>());
			helper.SetName("CheckBox").
				SetRelativePosition(Vector3.zero).
				SetSize(new Vector2(100, 20));
			var checkbox = helper.GetCheckBox();
			var label = AddLabel(checkbox).
				SetAutoSize(true).
				////SetAutoHeight(false).
				//SetSize(new Vector2(20, 20)).
				SetRelativePosition(new Vector3(22f, 2f)).
				GetLabel(true);

			var sprite = checkbox.AddUIComponent<UISprite>();
			sprite.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
			sprite.spriteName = CommonSprites.CheckBoxUnchecked;
			sprite.size = new Vector2(16f, 16f);
			sprite.disabledColor = Color.grey;
			sprite.relativePosition = Vector3.zero;

			var checkedSprite = checkbox.AddUIComponent<UISprite>();
			checkedSprite.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;
			checkedSprite.spriteName = CommonSprites.CheckBoxChecked;
			checkedSprite.size = new Vector2(16f, 16f);
			checkedSprite.relativePosition = Vector3.zero;
			checkedSprite.disabledColor = Color.grey;

			helper.SetLabel(label).
				SetCheckedBoxObject(checkedSprite);
			return helper;
		}

		public static UISpriteHelper AddSprite(UIComponent parent)
		{
			var helper = new UISpriteHelper(parent.AddUIComponent<UISprite>());
			return helper;
		}

		public static UISlicedSpriteHelper AddSlicedSprite(UIComponent parent)
		{
			var helper = new UISlicedSpriteHelper(parent.AddUIComponent<UISlicedSprite>());
			return helper;
		}

		public static UISliderHelper AddSlider(UIComponent parent, UIOrientation orientation)
		{
			var size = Vector2.zero;
			if (orientation == UIOrientation.Horizontal)
				size = new Vector2(100, DefaultHSliderHeight);
			else
				size = new Vector2(DefaultHSliderHeight, 100);

			var helper = new UISliderHelper(parent.AddUIComponent<UISlider>());
			return helper.SetName("Slider").
				SetRelativePosition(Vector3.zero).
				SetSize(size).
				SetOrientation(orientation).
				AddDefaultTrackAndThumb();
		}

		public static UIScrollbarHelper CreateScrollbarWithTrack(UIOrientation orientation)
		{
			var go = new GameObject("Scrollbar");
			return SetupScrollbarWithTrack(go.AddComponent<UIScrollbar>(), orientation);
		}

		public static UIScrollbarHelper AddScrollbarWithTrack(UIComponent parent, UIOrientation orientation)
		{
			return SetupScrollbarWithTrack(parent.AddUIComponent<UIScrollbar>(), orientation);
		}

		private static UIScrollbarHelper SetupScrollbarWithTrack(UIScrollbar scrollbar, UIOrientation orientation)
		{
			var helper = new UIScrollbarHelper(scrollbar);
			var size = Vector2.zero;
			if (orientation == UIOrientation.Horizontal)
				size = new Vector2(100, DefaultVScrollbarWidth);
			else
				size = new Vector2(DefaultVScrollbarWidth, 100);

			return helper.SetName("Scrollbar").
				SetRelativePosition(Vector3.zero).
				SetSize(size).
				SetOrientation(orientation).
				SetAutoHide(false).
				SetIncrementAmount(38).
				SetStepSize(1).
				AddDefaultTrackAndThumb();
		}

		public static UIScrollablePanelHelper AddScrollablePanel(UIComponent parent, UIOrientation orientation)
		{
			var helper = new UIScrollablePanelHelper(parent.AddUIComponent<UIScrollablePanel>());
			helper.SetName("ScrollabelPanel").
				SetRelativePosition(Vector3.zero).
				SetSize(new Vector2(100, 100)).
				SetClipChildren(true).
				SetBuiltinKeyNavigation(true).
				SetAutoLayout(true).
				SetAutoLayoutStart(LayoutStart.TopLeft).
				SetAutoLayoutPadding(new RectOffset(2, 2, 1, 0)).
				SetAutoLayoutDirection(orientation == UIOrientation.Horizontal ? LayoutDirection.Horizontal : LayoutDirection.Vertical).
				SetScrollWheelDirection(orientation).
				SetScrollWheelAmount(38);
			return helper;
		}

		public static UIButtonHelper AddButton(UIComponent parent)
		{
			var helper = new UIButtonHelper(parent.AddUIComponent<UIButton>());
			helper.SetName("Button").
				SetAutoSize(true).
				SetRelativePosition(Vector3.zero);
			return helper;
		}

		public static UIButtonHelper AddTextButton(UIComponent parent, string text)
		{
			var helper = new UIButtonHelper(parent.AddUIComponent<UIButton>());
			helper.SetName("Button").
				SetText(text).
				SetBackgroundSprites(CommonSprites.ButtonMenu).
				SetTextPadding(new RectOffset(5, 5, 2, 2)).
				SetAutoSize(true).
				SetRelativePosition(Vector3.zero);
			return helper;
		}

		public static UIMultiStateButtonHelper AddMultiStateButton(UIComponent parent)
		{
			var helper = new UIMultiStateButtonHelper(parent.AddUIComponent<UIMultiStateButton>());
			helper.SetName("MultiStateButton").
				SetRelativePosition(Vector3.zero).
				SetTextHorizontalAlignment(UIHorizontalAlignment.Center).
				SetTextVerticalAlignment(UIVerticalAlignment.Middle).
				SetSpritePadding(new RectOffset(0, 0, 0, 0)).
				SetForegroundSpriteMode(UIForegroundSpriteMode.Stretch).
				SetActiveStateIndex(0);
			return helper;
		}

		public static UIDropDownHelper AddDropDown(UIComponent parent)
		{
			var helper = new UIDropDownHelper(parent.AddUIComponent<UIDropDown>());
			helper.SetName("DropDown").
				SetRelativePosition(Vector3.zero).
				SetSize(new Vector2(75, 25)).
				AddDefaultScrollbar().
				SetListBackground(CommonSprites.OptionsDropboxListbox).
				SetItemHighlight("ListItemHighlight").
				SetItemHover("ListItemHover").
				SetVerticalAlignment(UIVerticalAlignment.Middle).
				SetListHeight(200).
				SetBackgroundSprites(PreparedSpriteSets.OptionsDropbox);
			return helper;
		}

		public static UITabstripHelper AddTabstrip(UIComponent parent)
		{
			var tabstrip = parent.AddUIComponent<UITabstrip>();
			var helper = new UITabstripHelper(tabstrip).
				SetName("Tabstriip").
				SetPosition(Vector3.zero).
				SetAutoSize(true).
				SetPadding(new RectOffset(1, 1, 2, 2));
			return helper;
		}

		public static UITabstripHelper AddTabstripWithButtons(UIComponent parent, string[] buttonTexts, float textScale)
		{
			var helper = AddTabstrip(parent);
			var tabstrip = helper.GetTabtrip();

			foreach (var text in buttonTexts)
			{
				AddButton(tabstrip).
					SetName("Button-" + text).
					SetAutoSize(true).
					SetTextScale(textScale).
					SetText(text).
					SetTextPadding(new RectOffset(5, 5, 2, 2)).
					SetBackgroundSprites(CommonSprites.GenericTab);
			}

			return helper;
		}

		public static UITabContainerHelper AddTabContainer(UIComponent parent)
		{
			var helper = new UITabContainerHelper(parent.AddUIComponent<UITabContainer>());
			return helper.SetName("TabContainer").
				SetPosition(Vector3.zero);
		}
	}

	public class UIHelperException : Exception
	{
		public UIHelperException(string message) : base(message)
		{
		}
	}

	public abstract class UIComponentHelper<T>
		where T : UIComponentHelper<T>
	{
		protected abstract UIComponent GetComponent();

		protected bool m_HasPosition = false;
		public bool HasPosition => m_HasPosition;

		public virtual bool IsValid()
		{
			return HasPosition && (GetComponent().size != null || GetComponent().size == Vector2.zero);
		}

		public virtual T SetAbsolutePosition(Vector3 position)
		{
			GetComponent().absolutePosition = position;
			m_HasPosition = true;
			return this as T;
		}

		public virtual T SetRelativePosition(Vector3 position)
		{
			GetComponent().relativePosition = position;
			m_HasPosition = true;
			return this as T;
		}

		public virtual T SetPosition(Vector3 position)
		{
			GetComponent().position = position;
			m_HasPosition = true;
			return this as T;
		}

		public virtual T SetAnchor(UIAnchorStyle anchor)
		{
			GetComponent().anchor = anchor;
			return this as T;
		}

		public virtual T SetArbitraryPivotOffset(Vector2 offset)
		{
			GetComponent().arbitraryPivotOffset = offset;
			return this as T;
		}

		public virtual T SetArea(Vector4 area)
		{
			GetComponent().area = area;
			m_HasPosition = true;
			return this as T;
		}

		public virtual T SetAutoSize(bool enable)
		{
			GetComponent().autoSize = enable;
			return this as T;
		}

		public virtual T SetBringTooltipToFront(bool enable)
		{
			GetComponent().bringTooltipToFront = enable;
			return this as T;
		}

		public virtual T SetBuiltinKeyNavigation(bool enable)
		{
			GetComponent().builtinKeyNavigation = enable;
			return this as T;
		}

		public virtual T SetCanFocus(bool enable)
		{
			GetComponent().canFocus = enable;
			return this as T;
		}

		public virtual T SetClickSound(AudioClip sound)
		{
			GetComponent().clickSound = sound;
			return this as T;
		}

		public virtual T SetClipChildren(bool enable)
		{
			GetComponent().clipChildren = enable;
			return this as T;
		}

		public virtual T SetColor(Color32 color)
		{
			GetComponent().color = color;
			return this as T;
		}

		public virtual T SetDisabledClickSound(AudioClip sound)
		{
			GetComponent().disabledClickSound = sound;
			return this as T;
		}

		public virtual T SetDisabledColor(Color32 color)
		{
			GetComponent().disabledColor = color;
			return this as T;
		}

		public virtual T SetForceZOrder(int zOrder)
		{
			GetComponent().forceZOrder = zOrder;
			return this as T;
		}

		public virtual T SetHeight(float height)
		{
			GetComponent().height = height;
			return this as T;
		}

		public virtual T SetHoverCursor(ColossalFramework.CursorInfo cursor)
		{
			GetComponent().hoverCursor = cursor;
			return this as T;
		}

		public virtual T SetIsEnabled(bool enable)
		{
			GetComponent().isEnabled = enable;
			return this as T;
		}

		public virtual T SetIsInteractive(bool enable)
		{
			GetComponent().isInteractive = enable;
			return this as T;
		}

		public virtual T SetIsLocalized(bool enable)
		{
			GetComponent().isLocalized = enable;
			return this as T;
		}

		public virtual T SetIsTooltipLocalized(bool enable)
		{
			GetComponent().isTooltipLocalized = enable;
			return this as T;
		}

		public virtual T SetIsVisible(bool enable)
		{
			GetComponent().isVisible = enable;
			return this as T;
		}

		public virtual T SetLimits(Vector4 limits)
		{
			GetComponent().limits = limits;
			return this as T;
		}

		public virtual T SetMaximumSize(Vector2 maxSize)
		{
			GetComponent().maximumSize = maxSize;
			return this as T;
		}

		public virtual T SetMinimumSize(Vector2 minSize)
		{
			GetComponent().minimumSize = minSize;
			return this as T;
		}

		public virtual T SetName(string name)
		{
			GetComponent().name = name;
			return this as T;
		}

		public virtual T SetObjectUserData(object userData)
		{
			GetComponent().objectUserData = userData;
			return this as T;
		}

		public virtual T SetOpacity(float opacity)
		{
			GetComponent().opacity = opacity;
			return this as T;
		}

		public virtual T SetPivot(UIPivotPoint pivot)
		{
			GetComponent().pivot = pivot;
			return this as T;
		}

		public virtual T SetPlayAudioEvents(bool enable)
		{
			GetComponent().playAudioEvents = enable;
			return this as T;
		}

		public virtual T SetSize(Vector2 size)
		{
			GetComponent().autoSize = false;
			GetComponent().size = size;
			return this as T;
		}

		public virtual T SetStrinUserData(string userData)
		{
			GetComponent().stringUserData = userData;
			return this as T;
		}

		public virtual T SetTabIndex(int index)
		{
			GetComponent().tabIndex = index;
			return this as T;
		}

		public virtual T SetTooltip(string tooltip)
		{
			GetComponent().tooltip = tooltip;
			return this as T;
		}

		public virtual T SetTooltipAnchor(UITooltipAnchor anchor)
		{
			GetComponent().tooltipAnchor = anchor;
			return this as T;
		}

		public virtual T SetTooltipBox(UIComponent tooltipBox)
		{
			GetComponent().tooltipBox = tooltipBox;
			return this as T;
		}

		public virtual T SetTooltipLocalID(string tooltipLocalID)
		{
			GetComponent().tooltipLocaleID = tooltipLocalID;
			return this as T;
		}

		public virtual T SetTransformPosition(Vector3 transform)
		{
			GetComponent().transformPosition = transform;
			return this as T;
		}

		public virtual T SetWidth(float width)
		{
			GetComponent().width = width;
			return this as T;
		}

		public virtual T SetZOrder(int zOrder)
		{
			GetComponent().zOrder = zOrder;
			return this as T;
		}

		public T SwapZOrder(UIComponent other)
		{
			var thisComponent = GetComponent();
			var oldZ = thisComponent.zOrder;
			thisComponent.zOrder = other.zOrder;
			other.zOrder = oldZ;
			return this as T;
		}

		public T ClampHorizontallyBetween(UIComponent left, UIComponent right, float paddingLeft = 0, float paddingRight = 0)
		{
			var component = GetComponent();
			SetAbsolutePosition(new Vector3(left.absolutePosition.x + left.width + paddingLeft, component.absolutePosition.y));
			SetSize(new Vector2(right.absolutePosition.x - (component.absolutePosition.x + paddingRight), component.height));
			return this as T;
		}

		public T ExpandHorizontallyUntil(UIComponent other, int padding = 0)
		{
			var component = GetComponent();
			if (component.absolutePosition.x + padding < other.absolutePosition.x)
			{
				SetWidth(other.absolutePosition.x - (component.absolutePosition.x + padding));
			}
			else if (other.absolutePosition.x + other.width + padding < component.absolutePosition.x)
			{
				var oldPos = component.absolutePosition.x;
				SetAbsoluteX(other.absolutePosition.x + other.width + padding);
				SetWidth(component.width + oldPos - component.absolutePosition.x);
			}
			return this as T;
		}

		public T ExpandVerticallyUntil(UIComponent other, int padding = 0)
		{
			var component = GetComponent();
			if (component.absolutePosition.y + padding < other.absolutePosition.y)
			{
				SetHeight(other.absolutePosition.y - (component.absolutePosition.y + padding));
			}
			else if (other.absolutePosition.y + other.height + padding < component.absolutePosition.y)
			{
				var oldPos = component.absolutePosition.y;
				SetAbsoluteY(other.absolutePosition.y + other.height + padding);
				SetHeight(component.height + oldPos - component.absolutePosition.y);
			}
			return this as T;
		}

		public T SpanLeft(UIComponent other, float padding = 0)
		{
			var oldX = GetComponent().absolutePosition.x;
			SetAbsoluteX(other.absolutePosition.x + other.width + padding);
			SetWidth(GetComponent().width + (oldX - GetComponent().absolutePosition.x));
			return this as T;
		}

		public T SpanTop(UIComponent other, float padding = 0)
		{
			var oldY = GetComponent().absolutePosition.y;
			SetAbsoluteX(other.absolutePosition.y + other.height + padding);
			SetHeight(GetComponent().height + (oldY - GetComponent().absolutePosition.y));
			return this as T;
		}

		public T SpanRight(UIComponent other, float padding = 0)
		{
			SetWidth(Mathf.Max(0, other.absolutePosition.x - (GetComponent().absolutePosition.x + padding)));
			return this as T;
		}

		public T SpanBottom(UIComponent other, float padding = 0)
		{
			SetHeight(Mathf.Max(0, other.absolutePosition.y - (GetComponent().absolutePosition.y + padding)));
			return this as T;
		}

		public T SpanInnerTopLeft(UIComponent other)
		{
			SpanInnerTop(other);
			SpanInnerLeft(other);
			return this as T;
		}

		public T SpanInnerTopLeft(UIComponent other, Vector2 padding)
		{
			SpanInnerTop(other, padding.y);
			SpanInnerLeft(other, padding.x);
			return this as T;
		}

		public T SpanInnerTopRight(UIComponent other)
		{
			SpanInnerTop(other);
			SpanInnerRight(other);
			return this as T;
		}

		public T SpanInnerTopRight(UIComponent other, Vector2 padding)
		{
			SpanInnerTop(other, padding.y);
			SpanInnerRight(other, padding.x);
			return this as T;
		}

		public T SpanInnerBottomLeft(UIComponent other)
		{
			SpanInnerBottom(other);
			SpanInnerLeft(other);
			return this as T;
		}

		public T SpanInnerBottomLeft(UIComponent other, Vector2 padding)
		{
			SpanInnerBottom(other, padding.y);
			SpanInnerLeft(other, padding.x);
			return this as T;
		}

		public T SpanInnerBottomRight(UIComponent other)
		{
			SpanInnerBottom(other);
			SpanInnerRight(other);
			return this as T;
		}

		public T SpanInnerBottomRight(UIComponent other, Vector2 padding)
		{
			SpanInnerBottom(other, padding.y);
			SpanInnerRight(other, padding.x);
			return this as T;
		}

		public T SpanInnerLeft(UIComponent other, float padding = 0)
		{
			var oldX = GetComponent().absolutePosition.x;
			SetAbsoluteX(other.absolutePosition.x + padding);
			SetWidth(GetComponent().width + (oldX - GetComponent().absolutePosition.x));
			return this as T;
		}

		public T SpanInnerTop(UIComponent other, float padding = 0)
		{
			var oldY = GetComponent().absolutePosition.y;
			SetAbsoluteX(other.absolutePosition.y + padding);
			SetHeight(GetComponent().height + (oldY - GetComponent().absolutePosition.y));
			return this as T;
		}

		public T SpanInnerRight(UIComponent other, float padding = 0)
		{
			SetWidth(Mathf.Max(0, other.absolutePosition.x + other.width - (GetComponent().absolutePosition.x + padding)));
			return this as T;
		}

		public T SpanInnerBottom(UIComponent other, float padding = 0)
		{
			SetHeight(Mathf.Max(0, other.absolutePosition.y + other.height - (GetComponent().absolutePosition.y + padding)));
			return this as T;
		}

		public T MoveXBy(float value)
		{
			SetAbsoluteX(GetComponent().absolutePosition.x + value);
			return this as T;
		}

		public T MoveYBy(float value)
		{
			SetAbsoluteY(GetComponent().absolutePosition.y + value);
			return this as T;
		}

		public T SetX(float value)
		{
			SetPosition(new Vector3(value, GetComponent().position.y));
			return this as T;
		}

		public T SetRelativeX(float value)
		{
			SetRelativePosition(new Vector3(value, GetComponent().relativePosition.y));
			return this as T;
		}

		public T SetAbsoluteX(float value)
		{
			SetAbsolutePosition(new Vector3(value, GetComponent().absolutePosition.y));
			return this as T;
		}

		public T SetY(float value)
		{
			SetPosition(new Vector3(GetComponent().position.x, value));
			return this as T;
		}

		public T SetRelativeY(float value)
		{
			SetRelativePosition(new Vector3(GetComponent().relativePosition.x, value));
			return this as T;
		}

		public T SetAbsoluteY(float value)
		{
			SetAbsolutePosition(new Vector3(GetComponent().absolutePosition.x, value));
			return this as T;
		}

		public T MoveInnerTopLeftOf(UIComponent other)
		{
			return MoveInnerTopLeftOf(other, Vector3.zero);
		}

		public T MoveInnerTopLeftOf(UIComponent other, Vector3 offset)
		{
			SetAbsolutePosition(other.absolutePosition + offset);
			return this as T;
		}

		public T MoveInnerTopRightOf(UIComponent other)
		{
			return MoveInnerTopRightOf(other, Vector3.zero);
		}

		public T MoveInnerTopRightOf(UIComponent other, Vector3 offset)
		{
			offset.x = -offset.x;
			offset += new Vector3(other.width - GetComponent().width, 0);
			SetAbsolutePosition(other.absolutePosition + offset);
			return this as T;
		}

		public T MoveInnerBottomLeftOf(UIComponent other)
		{
			return MoveInnerBottomLeftOf(other, Vector3.zero);
		}

		public T MoveInnerBottomLeftOf(UIComponent other, Vector3 offset)
		{
			offset.y = -offset.y;
			offset += new Vector3(0, other.height - GetComponent().height);
			SetAbsolutePosition(other.absolutePosition + offset);
			return this as T;
		}

		public T MoveInnerBottomRightOf(UIComponent other)
		{
			return MoveInnerBottomRightOf(other, Vector3.zero);
		}

		public T MoveInnerBottomRightOf(UIComponent other, Vector3 offset)
		{
			offset = -offset;
			offset += new Vector3(other.width - GetComponent().width, other.height - GetComponent().height);
			SetAbsolutePosition(other.absolutePosition + offset);
			return this as T;
		}

		public T MoveInnerTopOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteY(other.absolutePosition.y + dist);
			return this as T;
		}

		public T MoveInnerBottomOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteY(other.absolutePosition.y + other.height - (GetComponent().height + dist));
			return this as T;
		}

		public T MoveInnerLeftOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteX(other.absolutePosition.x + dist);
			return this as T;
		}

		public T MoveInnerRightOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteX(other.absolutePosition.x + other.width - (GetComponent().width + dist));
			return this as T;
		}

		public T MoveTopRightOf(UIComponent other)
		{
			return MoveTopRightOf(other, Vector3.zero);
		}

		public T MoveTopRightOf(UIComponent other, Vector3 offset)
		{
			offset.y = -offset.y;
			offset += new Vector3(other.width, GetComponent().height);
			SetAbsolutePosition(other.absolutePosition + offset);
			return this as T;
		}

		public T MoveTopLeftOf(UIComponent other)
		{
			return MoveTopLeftOf(other, Vector3.zero);
		}

		public T MoveTopLeftOf(UIComponent other, Vector3 offset)
		{
			offset += new Vector3(GetComponent().width, GetComponent().height);
			SetAbsolutePosition(other.absolutePosition - offset);
			return this as T;
		}

		public T MoveBottomRightOf(UIComponent other)
		{
			return MoveBottomRightOf(other, Vector3.zero);
		}

		public T MoveBottomRightOf(UIComponent other, Vector3 offset)
		{
			offset += new Vector3(other.width, other.height);
			SetAbsolutePosition(other.absolutePosition + offset);
			return this as T;
		}

		public T MoveBottomLeftOf(UIComponent other)
		{
			return MoveBottomLeftOf(other, Vector3.zero);
		}

		public T MoveBottomLeftOf(UIComponent other, Vector3 offset)
		{
			offset.x = -offset.x;
			offset += new Vector3(-GetComponent().width, other.height);
			SetAbsolutePosition(other.absolutePosition + offset);
			return this as T;
		}

		public T MoveTopOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteY(other.absolutePosition.y - (GetComponent().height + dist));
			return this as T;
		}

		public T MoveBottomOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteY(other.absolutePosition.y + other.height + dist);
			return this as T;
		}

		public T MoveLeftOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteX(other.absolutePosition.x - (GetComponent().width + dist));
			return this as T;
		}

		public T MoveRightOf(UIComponent other, float dist = 0)
		{
			SetAbsoluteX(other.absolutePosition.x + other.width + dist);
			return this as T;
		}
	}

	public abstract class UITextComponentHelper<T> : UIComponentHelper<T>
		where T : UITextComponentHelper<T>
	{
		private UITextComponent GetTextComponent()
		{
			var textComponent = GetComponent() as UITextComponent;
			if (textComponent == null)
				throw new UIHelperException("Returned object by GetComponent is not of type UITextComponent.");
			return textComponent;
		}

		public virtual bool IsValid(bool ignoreEmptyText)
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			return ignoreEmptyText || !string.IsNullOrEmpty(GetTextComponent().text);
		}

		public virtual T SetBottomColoer(Color32 color)
		{
			GetTextComponent().bottomColor = color;
			return this as T;
		}

		public virtual T SetCharacterSpacing(int spacing)
		{
			GetTextComponent().characterSpacing = spacing;
			return this as T;
		}

		public virtual T SetColorizedSprites(bool enable)
		{
			GetTextComponent().colorizeSprites = enable;
			return this as T;
		}

		public virtual T SetDisabledTextColor(Color32 color)
		{
			GetTextComponent().disabledTextColor = color;
			return this as T;
		}

		public virtual T SetDropShadowColor(Color32 color)
		{
			GetTextComponent().dropShadowColor = color;
			return this as T;
		}

		public virtual T SetDropShadowOffset(Vector2 offset)
		{
			GetTextComponent().dropShadowOffset = offset;
			return this as T;
		}

		public virtual T SetFont(UIFont font)
		{
			GetTextComponent().font = font;
			return this as T;
		}

		public virtual T SetLocaleID(string localeID)
		{
			GetTextComponent().localeID = localeID;
			return this as T;
		}

		public virtual T SetOutlineColor(Color32 color)
		{
			GetTextComponent().outlineColor = color;
			return this as T;
		}

		public virtual T SetOutlineSize(int size)
		{
			GetTextComponent().outlineSize = size;
			return this as T;
		}

		public virtual T SetProcessMarkup(bool enable)
		{
			GetTextComponent().processMarkup = enable;
			return this as T;
		}

		public virtual T SetText(string text)
		{
			GetTextComponent().text = text;
			return this as T;
		}

		public virtual T SetTextColor(Color32 color)
		{
			GetTextComponent().textColor = color;
			return this as T;
		}

		public virtual T SetTextScale(float scale)
		{
			GetTextComponent().textScale = scale;
			return this as T;
		}

		public virtual T SetTextScaleMode(UITextScaleMode mode)
		{
			GetTextComponent().textScaleMode = mode;
			return this as T;
		}

		public virtual T SetUseDropShadow(bool enable)
		{
			GetTextComponent().useDropShadow = enable;
			return this as T;
		}

		public virtual T SetUseGradient(bool enable)
		{
			GetTextComponent().useGradient = enable;
			return this as T;
		}

		public virtual T SetUseOutline(bool enable)
		{
			GetTextComponent().useOutline = enable;
			return this as T;
		}
	}

	public class UILabelHelper : UITextComponentHelper<UILabelHelper>
	{
		private UILabel m_Label = null;

		public bool HasText => !string.IsNullOrEmpty(m_Label.text) || !string.IsNullOrEmpty(m_Label.prefix) || !string.IsNullOrEmpty(m_Label.suffix);

		public UILabelHelper(UILabel label)
		{
			m_Label = label;
			if (m_Label == null)
				throw new NullReferenceException();
		}

		public UILabel GetLabel(bool ignoreEmptyText = false)
		{
			base.IsValid(ignoreEmptyText);
			if (!ignoreEmptyText && !HasText)
				throw new UIHelperException("UILabel has no text.");
			return m_Label;
		}

		protected override UIComponent GetComponent()
		{
			return m_Label;
		}

		public UILabelHelper SetAtlas(UITextureAtlas atlas)
		{
			m_Label.atlas = atlas;
			return this;
		}

		public UILabelHelper SetAutoHeight(bool enable)
		{
			m_Label.autoHeight = enable;
			return this;
		}

		public UILabelHelper SetBackgroundSprite(string spriteName)
		{
			m_Label.backgroundSprite = spriteName;
			return this;
		}

		public UILabelHelper SetPadding(RectOffset offset)
		{
			m_Label.padding = offset;
			return this;
		}

		public UILabelHelper SetPrefix(string prefix)
		{
			m_Label.prefix = prefix;
			return this;
		}

		public UILabelHelper SetSuffix(string suffix)
		{
			m_Label.suffix = suffix;
			return this;
		}

		public UILabelHelper SetTabSize(int tabSize)
		{
			m_Label.tabSize = tabSize;
			return this;
		}

		public UILabelHelper SetTextAlignment(UIHorizontalAlignment alignment)
		{
			m_Label.textAlignment = alignment;
			return this;
		}

		public UILabelHelper SetVerticalAlignment(UIVerticalAlignment alignment)
		{
			m_Label.verticalAlignment = alignment;
			return this;
		}

		public UILabelHelper SetWordWrap(bool enable)
		{
			m_Label.wordWrap = enable;
			return this;
		}
	}

	public class UITextFieldHelper : UIInteractiveComponentHelper<UITextFieldHelper>
	{
		private UITextField m_TextField = null;

		public bool HasText => !string.IsNullOrEmpty(m_TextField.text);

		public UITextFieldHelper(UITextField textField)
		{
			m_TextField = textField;
			if (m_TextField == null)
				throw new NullReferenceException();
		}

		public UITextField GetTextField(bool ignoreEmptyText = false)
		{
			base.IsValid(ignoreEmptyText);
			if (!ignoreEmptyText && !HasText)
				throw new UIHelperException("UITextField has no text.");
			return m_TextField;
		}

		protected override UIComponent GetComponent()
		{
			return m_TextField;
		}

		public UITextFieldHelper SetAllowFloats(bool enable)
		{
			m_TextField.allowFloats = enable;
			return this;
		}

		public UITextFieldHelper SetAllowNegative(bool enable)
		{
			m_TextField.allowNegative = enable;
			return this;
		}

		public UITextFieldHelper SetCursorBlinkTime(float time)
		{
			m_TextField.cursorBlinkTime = time;
			return this;
		}

		public UITextFieldHelper SetCursorWidth(int width)
		{
			m_TextField.cursorWidth = width;
			return this;
		}

		public UITextFieldHelper SetIsPasswordField(bool enable)
		{
			m_TextField.isPasswordField = enable;
			return this;
		}

		public UITextFieldHelper SetMaxLength(int maxLength)
		{
			m_TextField.maxLength = maxLength;
			return this;
		}

		public UITextFieldHelper SetMultiline(bool enable)
		{
			m_TextField.multiline = enable;
			return this;
		}

		public UITextFieldHelper SetNumericalOnly(bool enable)
		{
			m_TextField.numericalOnly = enable;
			return this;
		}

		public UITextFieldHelper SetPadding(RectOffset padding)
		{
			m_TextField.padding = padding;
			return this;
		}

		public UITextFieldHelper SetPasswordCharacter(string character)
		{
			m_TextField.passwordCharacter = character;
			return this;
		}

		public UITextFieldHelper SetReadOnly(bool enable)
		{
			m_TextField.readOnly = enable;
			return this;
		}

		public UITextFieldHelper SetSelectionBackgroundColor(Color32 color)
		{
			m_TextField.selectionBackgroundColor = color;
			return this;
		}

		public UITextFieldHelper SetSelectionEnd(int end)
		{
			m_TextField.selectionEnd = end;
			return this;
		}

		public UITextFieldHelper SetSelectionSprite(string spriteName)
		{
			m_TextField.selectionSprite = spriteName;
			return this;
		}

		public UITextFieldHelper SetSelectionStart(int start)
		{
			m_TextField.selectionStart = start;
			return this;
		}

		public UITextFieldHelper SetSelectOnFocus(bool enable)
		{
			m_TextField.selectOnFocus = enable;
			return this;
		}

		public UITextFieldHelper SetSubmitOnFocusLost(bool enable)
		{
			m_TextField.submitOnFocusLost = enable;
			return this;
		}
	}

	public class UIResizeHandleHelper : UIComponentHelper<UIResizeHandleHelper>
	{
		private UIResizeHandle m_ResizeHandle = null;

		public UIResizeHandleHelper(UIResizeHandle handle)
		{
			m_ResizeHandle = handle;
			if (m_ResizeHandle == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_ResizeHandle;
		}

		public UIResizeHandle GetResizeHandle()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");
			if (m_ResizeHandle.hoverCursor == null)
				throw new UIHelperException("UIResizeHandle has no hover cursor.");
			return m_ResizeHandle;
		}

		public UIResizeHandleHelper SetAtlas(UITextureAtlas atlas)
		{
			m_ResizeHandle.atlas = atlas;
			return this;
		}

		public UIResizeHandleHelper SetBackgroundSprite(string spriteName)
		{
			m_ResizeHandle.backgroundSprite = spriteName;
			return this;
		}

		public UIResizeHandleHelper SetEdges(UIResizeHandle.ResizeEdge edges)
		{
			m_ResizeHandle.edges = edges;
			return this;
		}
	}

	public class UIDragHandleHelper : UIComponentHelper<UIDragHandleHelper>
	{
		private UIDragHandle m_DragHandle = null;

		public UIDragHandleHelper(UIDragHandle handle)
		{
			m_DragHandle = handle;
			if (m_DragHandle == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_DragHandle;
		}

		public UIDragHandle GetDragHandle(bool allowNullTarget = false)
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");
			if (!allowNullTarget && m_DragHandle.target == null)
				throw new UIHelperException("UIDragHandle has no valid target.");
			return m_DragHandle;
		}

		public UIDragHandleHelper SetConstraintToScreen(bool enable)
		{
			m_DragHandle.constrainToScreen = enable;
			return this;
		}

		public UIDragHandleHelper SetTarget(UIComponent target)
		{
			m_DragHandle.target = target;
			return this;
		}
	}

	public class UIScrollablePanelHelper : UIComponentHelper<UIScrollablePanelHelper>
	{
		private UIScrollablePanel m_ScrollablePanel = null;

		public bool HasScrollbar => m_ScrollablePanel.verticalScrollbar != null || m_ScrollablePanel.horizontalScrollbar != null;

		public UIScrollablePanelHelper(UIScrollablePanel scrollabelPanel)
		{
			m_ScrollablePanel = scrollabelPanel;
			if (m_ScrollablePanel == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_ScrollablePanel;
		}

		public UIScrollablePanel GetScrollabelPanel(bool ignoreEmptyScrollbar = false)
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");
			if (!ignoreEmptyScrollbar && !HasScrollbar)
				throw new UIHelperException("UIScrollablePanel doesn't have a UIScrollbar attached.");
			return m_ScrollablePanel;
		}

		public UIScrollablePanelHelper SetAtlas(UITextureAtlas atlas)
		{
			m_ScrollablePanel.atlas = atlas;
			return this;
		}

		public UIScrollablePanelHelper SetAutoLayout(bool enable)
		{
			m_ScrollablePanel.autoLayout = enable;
			return this;
		}

		public UIScrollablePanelHelper SetAutoLayoutDirection(LayoutDirection layoutDirection)
		{
			m_ScrollablePanel.autoLayoutDirection = layoutDirection;
			return this;
		}

		public UIScrollablePanelHelper SetAutoLayoutPadding(RectOffset offset)
		{
			m_ScrollablePanel.autoLayoutPadding = offset;
			return this;
		}

		public UIScrollablePanelHelper SetAutoLayoutStart(LayoutStart start)
		{
			m_ScrollablePanel.autoLayoutStart = start;
			return this;
		}

		public UIScrollablePanelHelper SetAutoReset(bool enable)
		{
			m_ScrollablePanel.autoReset = enable;
			return this;
		}

		public UIScrollablePanelHelper SetBackgroundSprite(string spriteName)
		{
			m_ScrollablePanel.backgroundSprite = spriteName;
			return this;
		}

		public UIScrollablePanelHelper SetCustomScrollBounds(bool enable)
		{
			m_ScrollablePanel.customScrollBounds = enable;
			return this;
		}

		public UIScrollablePanelHelper SetFreeScroll(bool enable)
		{
			m_ScrollablePanel.freeScroll = enable;
			return this;
		}

		public UIScrollablePanelHelper SetHorizontalScrollbar(UIScrollbar scrollbar)
		{
			m_ScrollablePanel.horizontalScrollbar = scrollbar;
			return this;
		}

		public UIScrollablePanelHelper SetScrollPadding(RectOffset offset)
		{
			m_ScrollablePanel.scrollPadding = offset;
			return this;
		}

		public UIScrollablePanelHelper SetScrollPosition(Vector2 position)
		{
			m_ScrollablePanel.scrollPosition = position;
			return this;
		}

		public UIScrollablePanelHelper SetScrollWheelAmount(int amount)
		{
			m_ScrollablePanel.scrollWheelAmount = amount;
			return this;
		}

		public UIScrollablePanelHelper SetScrollWheelDirection(UIOrientation wheelDirection)
		{
			m_ScrollablePanel.scrollWheelDirection = wheelDirection;
			return this;
		}

		public UIScrollablePanelHelper SetScrollWithArrowKeys(bool enable)
		{
			m_ScrollablePanel.scrollWithArrowKeys = enable;
			return this;
		}

		public UIScrollablePanelHelper SetUseCenter(bool enable)
		{
			m_ScrollablePanel.useCenter = enable;
			return this;
		}

		public UIScrollablePanelHelper SetUseScrollMomentum(bool enable)
		{
			m_ScrollablePanel.useScrollMomentum = enable;
			return this;
		}

		public UIScrollablePanelHelper SetUseTouchMouseScroll(bool enable)
		{
			m_ScrollablePanel.useTouchMouseScroll = enable;
			return this;
		}

		public UIScrollablePanelHelper SetVerticalScrollbar(UIScrollbar scrollbar)
		{
			m_ScrollablePanel.verticalScrollbar = scrollbar;
			return this;
		}

		public UIScrollablePanelHelper SetWrapLayout(bool enable)
		{
			m_ScrollablePanel.wrapLayout = enable;
			return this;
		}
	}

	public abstract class UISpriteHelper<T> : UIComponentHelper<T>
		where T : UISpriteHelper<T>
	{
		public bool HasSpriteName => !string.IsNullOrEmpty(GetSpriteComponent().spriteName);

		private UISprite GetSpriteComponent()
		{
			var sprite = GetComponent() as UISprite;
			if (sprite == null)
				throw new UIHelperException("Returned object by GetComponent is not of type UISprite.");
			return sprite;
		}

		public T SetAtlas(UITextureAtlas atlas)
		{
			GetSpriteComponent().atlas = atlas;
			return this as T;
		}

		public T SetFillAmount(float fillAmount)
		{
			GetSpriteComponent().fillAmount = fillAmount;
			return this as T;
		}

		public T SetFillDirection(UIFillDirection direction)
		{
			GetSpriteComponent().fillDirection = direction;
			return this as T;
		}

		public T SetFlip(UISpriteFlip flip)
		{
			GetSpriteComponent().flip = flip;
			return this as T;
		}

		public T SetInvertFill(bool invertFill)
		{
			GetSpriteComponent().invertFill = invertFill;
			return this as T;
		}

		public T SetSpriteName(string spriteName)
		{
			GetSpriteComponent().spriteName = spriteName;
			return this as T;
		}
	}

	public class UISpriteHelper : UISpriteHelper<UISpriteHelper>
	{
		private UISprite m_Sprite = null;

		public UISpriteHelper(UISprite sprite)
		{
			m_Sprite = sprite;
			if (m_Sprite == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_Sprite;
		}

		public UISprite GetSprite(bool ignoreEmptySprite = false)
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");
			if (!ignoreEmptySprite && !HasSpriteName)
				throw new UIHelperException("UISprite doesn't have a SpriteName set.");
			return m_Sprite;
		}
	}

	public class UISlicedSpriteHelper : UISpriteHelper<UISlicedSpriteHelper>
	{
		private UISlicedSprite m_SlicedSprite = null;

		public UISlicedSpriteHelper(UISlicedSprite sprite)
		{
			m_SlicedSprite = sprite;
			if (m_SlicedSprite == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_SlicedSprite;
		}

		public UISlicedSprite GetSlicedSprite(bool ignoreEmptySprite = false)
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");
			if (!ignoreEmptySprite && !HasSpriteName)
				throw new UIHelperException("UISlicedSprite doesn't have a SpriteName set.");
			return m_SlicedSprite;
		}
	}

	public class UIScrollbarHelper : UIComponentHelper<UIScrollbarHelper>
	{
		private UIScrollbar m_Scrollbar = null;

		public bool HasValidComponents => (m_Scrollbar.thumbObject && m_Scrollbar.trackObject) || (m_Scrollbar.decrementButton && m_Scrollbar.incrementButton);

		public UIScrollbarHelper(UIScrollbar scrollbar)
		{
			m_Scrollbar = scrollbar;
			if (m_Scrollbar == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_Scrollbar;
		}

		public UIScrollbarHelper SetAutoDisableButtons(bool enable)
		{
			m_Scrollbar.autoDisableButtons = enable;
			return this;
		}

		public UIScrollbarHelper SetAutoHide(bool enable)
		{
			m_Scrollbar.autoHide = enable;
			return this;
		}

		public UIScrollbarHelper SetDecrementButton(UIComponent button)
		{
			m_Scrollbar.decrementButton = button;
			return this;
		}

		public UIScrollbarHelper SetIncrementAmount(float amount)
		{
			m_Scrollbar.incrementAmount = amount;
			return this;
		}

		public UIScrollbarHelper SetIncrementButton(UIComponent button)
		{
			m_Scrollbar.incrementButton = button;
			return this;
		}

		public UIScrollbarHelper SetMaxValue(float amount)
		{
			m_Scrollbar.maxValue = amount;
			return this;
		}

		public UIScrollbarHelper SetMinValue(float amount)
		{
			m_Scrollbar.minValue = amount;
			return this;
		}

		public UIScrollbarHelper SetOrientation(UIOrientation orientation)
		{
			m_Scrollbar.orientation = orientation;
			return this;
		}

		public UIScrollbarHelper SetScrollEasingTime(float time)
		{
			m_Scrollbar.scrollEasingTime = time;
			return this;
		}

		public UIScrollbarHelper SetScrollEasingType(EasingType type)
		{
			m_Scrollbar.scrollEasingType = type;
			return this;
		}

		public UIScrollbarHelper SetScrollSize(float scrollSize)
		{
			m_Scrollbar.scrollSize = scrollSize;
			return this;
		}

		public UIScrollbarHelper SetStepSize(float stepSize)
		{
			m_Scrollbar.stepSize = stepSize;
			return this;
		}

		public UIScrollbarHelper SetThumbObject(UIComponent thumb)
		{
			m_Scrollbar.thumbObject = thumb;
			return this;
		}

		public UIScrollbarHelper SetThumbPadding(RectOffset offset)
		{
			m_Scrollbar.thumbPadding = offset;
			return this;
		}

		public UIScrollbarHelper SetTrackObject(UIComponent track)
		{
			m_Scrollbar.trackObject = track;
			return this;
		}

		public UIScrollbarHelper SetValue(float value)
		{
			m_Scrollbar.value = value;
			return this;
		}

		public UIScrollbarHelper AddDefaultTrackAndThumb()
		{
			var scrollbarTrack = m_Scrollbar.AddUIComponent<UISlicedSprite>();
			scrollbarTrack.name = "Track";
			scrollbarTrack.spriteName = CommonSprites.ScrollbarTrack;
			scrollbarTrack.relativePosition = Vector3.zero;
			scrollbarTrack.size = m_Scrollbar.size;
			scrollbarTrack.anchor = UIAnchorStyle.All;
			m_Scrollbar.trackObject = scrollbarTrack;

			var scrollbarThumb = scrollbarTrack.AddUIComponent<UISlicedSprite>();
			scrollbarThumb.name = "Thumb";
			scrollbarThumb.spriteName = CommonSprites.ScrollbarThumb;
			scrollbarThumb.width = m_Scrollbar.width - 6;
			m_Scrollbar.thumbObject = scrollbarThumb;

			return this;
		}

		public UIScrollbar GetScrollbar()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");
			if (!HasValidComponents)
				throw new UIHelperException("UIScrollbar requires at least a track and thumb component or a increment and decrement button.");
			return m_Scrollbar;
		}
	}

	public class UIPanelHelper : UIComponentHelper<UIPanelHelper>
	{
		private UIPanel m_Panel = null;

		public UIPanelHelper(UIPanel panel)
		{
			m_Panel = panel;
			if (m_Panel == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_Panel;
		}

		public UIPanel GetPanel()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");
			return m_Panel;
		}

		public UIPanelHelper SetAtlas(UITextureAtlas atlas)
		{
			m_Panel.atlas = atlas;
			return this;
		}

		public UIPanelHelper SetAutoFitChildrenHorizontally(bool enable)
		{
			m_Panel.autoFitChildrenHorizontally = enable;
			return this;
		}

		public UIPanelHelper SetAutoFitChildrenVertically(bool enable)
		{
			m_Panel.autoFitChildrenVertically = enable;
			return this;
		}

		public UIPanelHelper SetAutoLayout(bool enable)
		{
			m_Panel.autoLayout = enable;
			return this;
		}

		public UIPanelHelper SetAutoLayoutDirection(LayoutDirection direction)
		{
			m_Panel.autoLayoutDirection = direction;
			return this;
		}

		public UIPanelHelper SetAutoLayoutPadding(RectOffset offset)
		{
			m_Panel.autoLayoutPadding = offset;
			return this;
		}

		public UIPanelHelper SetAutoLayoutStart(LayoutStart start)
		{
			m_Panel.autoLayoutStart = start;
			return this;
		}

		public UIPanelHelper SetBackgroundSprite(string spriteName)
		{
			m_Panel.backgroundSprite = spriteName;
			return this;
		}

		public UIPanelHelper SetFlip(UISpriteFlip flip)
		{
			m_Panel.flip = flip;
			return this;
		}

		public UIPanelHelper SetUseCenter(bool enable)
		{
			m_Panel.useCenter = enable;
			return this;
		}

		public UIPanelHelper SetVerticalSpacing(int space)
		{
			m_Panel.verticalSpacing = space;
			return this;
		}

		public UIPanelHelper SetWrapLayout(bool enable)
		{
			m_Panel.wrapLayout = enable;
			return this;
		}
	}

	public abstract class UIInteractiveComponentHelper<T> : UITextComponentHelper<T>
		where T : UIInteractiveComponentHelper<T>
	{
		private UIInteractiveComponent GetInteractiveComponent()
		{
			var component = GetComponent() as UIInteractiveComponent;
			if (component == null)
				throw new UIHelperException("Returned object by GetComponent is not of type UIInteractiveComponent.");
			return component;
		}

		public virtual T SetAtlas(UITextureAtlas atlas)
		{
			GetInteractiveComponent().atlas = atlas;
			return this as T;
		}

		public virtual T SetDisabledBgSprite(string spriteName)
		{
			GetInteractiveComponent().disabledBgSprite = spriteName;
			return this as T;
		}

		public virtual T SetDisabledFgSprite(string spriteName)
		{
			GetInteractiveComponent().disabledFgSprite = spriteName;
			return this as T;
		}

		public virtual T SetFocusedBgSprite(string spriteName)
		{
			GetInteractiveComponent().focusedBgSprite = spriteName;
			return this as T;
		}

		public virtual T SetFocusedFgSprite(string spriteName)
		{
			GetInteractiveComponent().focusedFgSprite = spriteName;
			return this as T;
		}

		public virtual T SetForegroundSpriteMode(UIForegroundSpriteMode mode)
		{
			GetInteractiveComponent().foregroundSpriteMode = mode;
			return this as T;
		}

		public virtual T SetHorizontalAlignment(UIHorizontalAlignment alignment)
		{
			GetInteractiveComponent().horizontalAlignment = alignment;
			return this as T;
		}

		public virtual T SetHoveredBgSprite(string spriteName)
		{
			GetInteractiveComponent().hoveredBgSprite = spriteName;
			return this as T;
		}

		public virtual T SetHoveredFgSprite(string spriteName)
		{
			GetInteractiveComponent().hoveredFgSprite = spriteName;
			return this as T;
		}

		public virtual T SetNormalBgSprite(string spriteName)
		{
			GetInteractiveComponent().normalBgSprite = spriteName;
			return this as T;
		}

		public virtual T SetNormalFgSprite(string spriteName)
		{
			GetInteractiveComponent().normalFgSprite = spriteName;
			return this as T;
		}

		public virtual T SetScaleFactor(float scale)
		{
			GetInteractiveComponent().scaleFactor = scale;
			return this as T;
		}

		public virtual T SetSpritePadding(RectOffset padding)
		{
			GetInteractiveComponent().spritePadding = padding;
			return this as T;
		}

		public virtual T SetVerticalAlignment(UIVerticalAlignment alignment)
		{
			GetInteractiveComponent().verticalAlignment = alignment;
			return this as T;
		}

		public virtual T SetBackgroundSprites(SpriteSet set)
		{
			var component = GetInteractiveComponent();
			component.normalBgSprite = set.normal;
			component.disabledBgSprite = set.disabled;
			component.hoveredBgSprite = set.hovered;
			component.focusedBgSprite = set.focused;
			return this as T;
		}

		public virtual T SetForegroundSprites(SpriteSet set)
		{
			var component = GetInteractiveComponent();
			component.normalFgSprite = set.normal;
			component.disabledFgSprite = set.disabled;
			component.hoveredFgSprite = set.hovered;
			component.focusedFgSprite = set.focused;
			return this as T;
		}
	}

	public class UIButtonHelper : UIInteractiveComponentHelper<UIButtonHelper>
	{
		private UIButton m_Button = null;

		public UIButtonHelper(UIButton button)
		{
			m_Button = button;
			if (m_Button == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_Button;
		}

		public UIButton GetButton()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			if (string.IsNullOrEmpty(m_Button.text) && string.IsNullOrEmpty(m_Button.normalBgSprite))
				throw new UIHelperException("UIButton has neither text nor normalBgSprite.");
			return m_Button;
		}

		public UIButtonHelper SetButtonsMask(UIMouseButton buttons)
		{
			m_Button.buttonsMask = buttons;
			return this;
		}

		public UIButtonHelper SetDisabledBottomColor(Color32 color)
		{
			m_Button.disabledBottomColor = color;
			return this;
		}

		public UIButtonHelper SetFocusedColor(Color32 color)
		{
			m_Button.focusedColor = color;
			return this;
		}

		public UIButtonHelper SetFocusedTextColor(Color32 color)
		{
			m_Button.focusedTextColor = color;
			return this;
		}

		public UIButtonHelper SetGroup(UIComponent group)
		{
			m_Button.group = group;
			return this;
		}

		public UIButtonHelper SetHoveredColor(Color32 color)
		{
			m_Button.hoveredColor = color;
			return this;
		}

		public UIButtonHelper SetHoveredTextColor(Color32 color)
		{
			m_Button.hoveredTextColor = color;
			return this;
		}

		public UIButtonHelper SetPressedBgSprite(string spriteName)
		{
			m_Button.pressedBgSprite = spriteName;
			return this;
		}

		public UIButtonHelper SetPressedColor(Color32 color)
		{
			m_Button.pressedColor = color;
			return this;
		}

		public UIButtonHelper SetPressedFgSprite(string spriteName)
		{
			m_Button.pressedFgSprite = spriteName;
			return this;
		}

		public UIButtonHelper SetPressedTextColor(Color32 color)
		{
			m_Button.pressedTextColor = color;
			return this;
		}

		public UIButtonHelper SetButtonState(UIButton.ButtonState state)
		{
			m_Button.state = state;
			return this;
		}

		public UIButtonHelper SetTabStrip(bool enable)
		{
			m_Button.tabStrip = enable;
			return this;
		}

		public UIButtonHelper SetTextHorizontalAlignment(UIHorizontalAlignment alignment)
		{
			m_Button.textHorizontalAlignment = alignment;
			return this;
		}

		public UIButtonHelper SetTextPadding(RectOffset padding)
		{
			m_Button.textPadding = padding;
			return this;
		}

		public UIButtonHelper SetTextVerticalAlignment(UIVerticalAlignment alignment)
		{
			m_Button.textVerticalAlignment = alignment;
			return this;
		}

		public UIButtonHelper SetWordWrap(bool enable)
		{
			m_Button.wordWrap = enable;
			return this;
		}

		public override UIButtonHelper SetBackgroundSprites(SpriteSet set)
		{
			base.SetBackgroundSprites(set);
			m_Button.pressedBgSprite = set.pressed;
			if (m_Button.size == null)
				SetSize(UIHelper.DefaultSpriteButtonSize);
			return this;
		}

		public override UIButtonHelper SetForegroundSprites(SpriteSet set)
		{
			base.SetForegroundSprites(set);
			m_Button.pressedFgSprite = set.pressed;
			if (m_Button.size == null)
				SetSize(UIHelper.DefaultSpriteButtonSize);
			return this;
		}
	}

	public class UIMultiStateButtonHelper : UIComponentHelper<UIMultiStateButtonHelper>
	{
		private UIMultiStateButton m_Button = null;

		public UIMultiStateButtonHelper(UIMultiStateButton button)
		{
			m_Button = button;
			if (m_Button == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_Button;
		}

		public UIMultiStateButton GetMultiStateButton()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			if (string.IsNullOrEmpty(m_Button.text) && string.IsNullOrEmpty(m_Button.normalBgSprite))
				throw new UIHelperException("UIButton has neither text nor normalBgSprite.");
			return m_Button;
		}

		public UIMultiStateButtonHelper SetActiveStateIndex(int index)
		{
			m_Button.activeStateIndex = index;
			return this;
		}

		public UIMultiStateButtonHelper SetAtlas(UITextureAtlas atlas)
		{
			m_Button.atlas = atlas;
			return this;
		}

		public UIMultiStateButtonHelper SetBottomColor(Color32 color)
		{
			m_Button.bottomColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetDisabledTextColor(Color32 color)
		{
			m_Button.disabledTextColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetFocusedColor(Color32 color)
		{
			m_Button.focusedColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetFocusedTextColor(Color32 color)
		{
			m_Button.focusedTextColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetFont(UIFont font)
		{
			m_Button.font = font;
			return this;
		}

		public UIMultiStateButtonHelper SetForegroundSpriteMode(UIForegroundSpriteMode mode)
		{
			m_Button.foregroundSpriteMode = mode;
			return this;
		}

		public UIMultiStateButtonHelper SetGroup(UIComponent group)
		{
			m_Button.group = group;
			return this;
		}

		public UIMultiStateButtonHelper SetHorizontalAlignment(UIHorizontalAlignment alignment)
		{
			m_Button.horizontalAlignment = alignment;
			return this;
		}

		public UIMultiStateButtonHelper SetHoveredColor(Color32 color)
		{
			m_Button.hoveredColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetHoveredTextColor(Color32 color)
		{
			m_Button.hoveredTextColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetOutlineColor(Color32 color)
		{
			m_Button.outlineColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetOutlineSize(int size)
		{
			m_Button.outlineSize = size;
			return this;
		}

		public UIMultiStateButtonHelper SetPressedColor(Color32 color)
		{
			m_Button.pressedColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetPressedTextColor(Color32 color)
		{
			m_Button.pressedTextColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetScaleFactor(float scale)
		{
			m_Button.scaleFactor = scale;
			return this;
		}

		public UIMultiStateButtonHelper SetShadowColor(Color32 color)
		{
			m_Button.shadowColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetShadowOffset(Vector2 offset)
		{
			m_Button.shadowOffset = offset;
			return this;
		}

		public UIMultiStateButtonHelper SetSpritePadding(RectOffset padding)
		{
			m_Button.spritePadding = padding;
			return this;
		}

		public UIMultiStateButtonHelper SetButtonState(UIMultiStateButton.ButtonState state)
		{
			m_Button.state = state;
			return this;
		}

		public UIMultiStateButtonHelper SetText(string text)
		{
			m_Button.text = text;
			return this;
		}

		public UIMultiStateButtonHelper SetTextColor(Color32 color)
		{
			m_Button.textColor = color;
			return this;
		}

		public UIMultiStateButtonHelper SetTextHorizontalAlignment(UIHorizontalAlignment alignment)
		{
			m_Button.textHorizontalAlignment = alignment;
			return this;
		}

		public UIMultiStateButtonHelper SetTextPadding(RectOffset padding)
		{
			m_Button.textPadding = padding;
			return this;
		}

		public UIMultiStateButtonHelper SetTextScale(float scale)
		{
			m_Button.textScale = scale;
			return this;
		}

		public UIMultiStateButtonHelper SetTextScaleMode(UITextScaleMode mode)
		{
			m_Button.textScaleMode = mode;
			return this;
		}

		public UIMultiStateButtonHelper SetTextVerticalAlignment(UIVerticalAlignment alignment)
		{
			m_Button.textVerticalAlignment = alignment;
			return this;
		}

		public UIMultiStateButtonHelper SetUseGradient(bool enable)
		{
			m_Button.useGradient = enable;
			return this;
		}

		public UIMultiStateButtonHelper SetUseOutline(bool enable)
		{
			m_Button.useOutline = enable;
			return this;
		}

		public UIMultiStateButtonHelper SetUseShadow(bool enable)
		{
			m_Button.useShadow = enable;
			return this;
		}

		public UIMultiStateButtonHelper SetSpriteSets(SpriteSet[] backgroundSpriteSets = null, SpriteSet[] foregroundSpriteSets = null)
		{
			int setCount = Math.Max(Math.Max(1, foregroundSpriteSets == null ? 0 : foregroundSpriteSets.Length), backgroundSpriteSets == null ? 0 : backgroundSpriteSets.Length);

			// background sprite state
			var spriteSetList = new List<UIMultiStateButton.SpriteSet>();
			for (int i = 0; i < setCount; ++i)
			{
				if (backgroundSpriteSets != null && i < backgroundSpriteSets.Length)
				{
					spriteSetList.Add(backgroundSpriteSets[i].ToMultiStateButtonSpriteSet(m_Button));
				}
				else
					spriteSetList.Add(new UIMultiStateButton.SpriteSet());
			}
			var spriteSetState = new UIMultiStateButton.SpriteSetState();
			Traverse.Create(spriteSetState).Field<List<UIMultiStateButton.SpriteSet>>("m_SpriteSetStates").Value = spriteSetList;
			Traverse.Create(m_Button).Field<UIMultiStateButton.SpriteSetState>("m_BackgroundSprites").Value = spriteSetState;

			// foreground sprite state
			spriteSetList = new List<UIMultiStateButton.SpriteSet>();
			for (int i = 0; i < setCount; ++i)
			{
				if (foregroundSpriteSets != null && i < foregroundSpriteSets.Length)
					spriteSetList.Add(foregroundSpriteSets[i].ToMultiStateButtonSpriteSet(m_Button));
				else
					spriteSetList.Add(new UIMultiStateButton.SpriteSet());
			}
			spriteSetState = new UIMultiStateButton.SpriteSetState();
			Traverse.Create(spriteSetState).Field<List<UIMultiStateButton.SpriteSet>>("m_SpriteSetStates").Value = spriteSetList;
			Traverse.Create(m_Button).Field<UIMultiStateButton.SpriteSetState>("m_ForegroundSprites").Value = spriteSetState;

			if (m_Button.size == null)
				SetSize(UIHelper.DefaultSpriteButtonSize);
			return this;
		}
	}

	public class UICheckBoxHelper : UIComponentHelper<UICheckBoxHelper>
	{
		private UICheckBox m_CheckBox = null;

		public UICheckBoxHelper(UICheckBox checkBox)
		{
			m_CheckBox = checkBox;
			if (m_CheckBox == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_CheckBox;
		}

		public UICheckBox GetCheckBox()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			//if (string.IsNullOrEmpty(m_CheckBox.text) && string.IsNullOrEmpty(m_CheckBox.normalBgSprite))
			//	throw new UIHelperException("UIButton has neither text nor normalBgSprite.");
			return m_CheckBox;
		}

		public UICheckBoxHelper SetCheckedBoxObject(UIComponent component)
		{
			m_CheckBox.checkedBoxObject = component;
			return this;
		}

		public UICheckBoxHelper SetGroup(UIComponent component)
		{
			m_CheckBox.group = component;
			return this;
		}

		public UICheckBoxHelper SetIsChecked(bool enable)
		{
			m_CheckBox.isChecked = enable;
			return this;
		}

		public UICheckBoxHelper SetLabel(UILabel label)
		{
			m_CheckBox.label = label;
			return this;
		}

		public UICheckBoxHelper SetReadOnly(bool enable)
		{
			m_CheckBox.readOnly = enable;
			return this;
		}

		public UICheckBoxHelper SetText(string text)
		{
			m_CheckBox.text = text;
			return this;
		}

		public UICheckBoxHelper SetTextScale(float scale)
		{
			m_CheckBox.label.textScale = scale;
			return this;
		}

		public UICheckBoxHelper SetTextColor(Color32 color)
		{
			m_CheckBox.label.textColor = color;
			return this;
		}
	}

	public class UISliderHelper : UIComponentHelper<UISliderHelper>
	{
		private UISlider m_Slider = null;

		public UISliderHelper(UISlider slider)
		{
			m_Slider = slider;
			if (m_Slider == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_Slider;
		}

		public UISlider GetSlider()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			//if (string.IsNullOrEmpty(m_CheckBox.text) && string.IsNullOrEmpty(m_CheckBox.normalBgSprite))
			//	throw new UIHelperException("UIButton has neither text nor normalBgSprite.");
			return m_Slider;
		}

		public UISliderHelper SetAtlas(UITextureAtlas atlas)
		{
			m_Slider.atlas = atlas;
			return this;
		}

		public UISliderHelper SetBackgroundSprite(string spriteName)
		{
			m_Slider.backgroundSprite = spriteName;
			return this;
		}

		public UISliderHelper SetFillIndicatorObject(UIComponent indicator)
		{
			m_Slider.fillIndicatorObject = indicator;
			return this;
		}

		public UISliderHelper SetFillMode(UIFillMode mode)
		{
			m_Slider.fillMode = mode;
			return this;
		}

		public UISliderHelper SetFillPadding(RectOffset padding)
		{
			m_Slider.fillPadding = padding;
			return this;
		}

		public UISliderHelper SetMaxValue(float max)
		{
			m_Slider.maxValue = max;
			return this;
		}

		public UISliderHelper SetMinValue(float min)
		{
			m_Slider.minValue = min;
			return this;
		}

		public UISliderHelper SetOrientation(UIOrientation orientation)
		{
			m_Slider.orientation = orientation;
			return this;
		}

		public UISliderHelper SetScrollWheelAmount(float amount)
		{
			m_Slider.scrollWheelAmount = amount;
			return this;
		}

		public UISliderHelper SetStepSize(float amount)
		{
			m_Slider.stepSize = amount;
			return this;
		}

		public UISliderHelper SetThumbObject(UIComponent thumb)
		{
			m_Slider.thumbObject = thumb;
			return this;
		}

		public UISliderHelper SetThumbOffset(Vector2 offset)
		{
			m_Slider.thumbOffset = offset;
			return this;
		}

		public UISliderHelper SetValue(float value)
		{
			m_Slider.value = value;
			return this;
		}

		public UISliderHelper AddDefaultTrackAndThumb()
		{
			var track = m_Slider.AddUIComponent<UISlicedSprite>();
			track.name = "Track";
			track.spriteName = CommonSprites.BudgetSlider;
			track.relativePosition = Vector3.zero;
			track.width = m_Slider.width;
			track.height = 9;
			track.anchor = UIAnchorStyle.All;

			var thumb = track.AddUIComponent<UISlicedSprite>();
			thumb.name = "Thumb";
			thumb.spriteName = CommonSprites.SliderBudget;
			thumb.size = new Vector2(16, 16);
			//scrollbarThumb.width = m_Scrollbar.width - 6;
			m_Slider.thumbObject = thumb;

			return this;
		}
	}

	public class UIDropDownHelper : UIInteractiveComponentHelper<UIDropDownHelper>
	{
		private UIDropDown m_DropDown = null;

		public UIDropDownHelper(UIDropDown dropDown)
		{
			m_DropDown = dropDown;
			if (m_DropDown == null)
				throw new NullReferenceException();

			m_DropDown.triggerButton = m_DropDown;
		}

		protected override UIComponent GetComponent()
		{
			return m_DropDown;
		}

		public UIDropDown GetDropDown()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			//if (string.IsNullOrEmpty(m_CheckBox.text) && string.IsNullOrEmpty(m_CheckBox.normalBgSprite))
			//	throw new UIHelperException("UIButton has neither text nor normalBgSprite.");
			return m_DropDown;
		}

		public UIDropDownHelper SetAutoListWidth(bool enable)
		{
			m_DropDown.autoListWidth = enable;
			return this;
		}

		public UIDropDownHelper SetFilteredItems(int[] items)
		{
			m_DropDown.filteredItems = items;
			return this;
		}

		public UIDropDownHelper SetItemHeight(int height)
		{
			m_DropDown.itemHeight = height;
			return this;
		}

		public UIDropDownHelper SetItemHighlight(string highlight)
		{
			m_DropDown.itemHighlight = highlight;
			return this;
		}

		public UIDropDownHelper SetItemHover(string hover)
		{
			m_DropDown.itemHover = hover;
			return this;
		}

		public UIDropDownHelper SetItemPadding(RectOffset padding)
		{
			m_DropDown.itemPadding = padding;
			return this;
		}

		public UIDropDownHelper SetItems(string[] items)
		{
			m_DropDown.items = items;
			return this;
		}

		public UIDropDownHelper SetListBackground(string spriteName)
		{
			m_DropDown.listBackground = spriteName;
			return this;
		}

		public UIDropDownHelper SetListHeight(int height)
		{
			m_DropDown.listHeight = height;
			return this;
		}

		public UIDropDownHelper SetListOffset(Vector2 offset)
		{
			m_DropDown.listOffset = offset;
			return this;
		}

		public UIDropDownHelper SetListPadding(RectOffset padding)
		{
			m_DropDown.listPadding = padding;
			return this;
		}

		public UIDropDownHelper SetListPosition(UIDropDown.PopupListPosition position)
		{
			m_DropDown.listPosition = position;
			return this;
		}

		public UIDropDownHelper SetListScrollbar(UIScrollbar scrollbar)
		{
			m_DropDown.listScrollbar = scrollbar;
			return this;
		}

		public UIDropDownHelper SetListWidth(int width)
		{
			m_DropDown.listWidth = width;
			return this;
		}

		public UIDropDownHelper SetLocalizedItems(string[] items)
		{
			m_DropDown.localizedItems = items;
			return this;
		}

		public UIDropDownHelper SetPopupColor(Color32 color)
		{
			m_DropDown.popupColor = color;
			return this;
		}

		public UIDropDownHelper SetPopupTextColor(Color32 color)
		{
			m_DropDown.popupTextColor = color;
			return this;
		}

		public UIDropDownHelper SetSelectedIndex(int index)
		{
			m_DropDown.selectedIndex = index;
			return this;
		}

		public UIDropDownHelper SetSelectedValue(string value)
		{
			m_DropDown.selectedValue = value;
			return this;
		}

		public UIDropDownHelper SetTextFieldPadding(RectOffset padding)
		{
			m_DropDown.textFieldPadding = padding;
			return this;
		}

		public UIDropDownHelper SetTriggerButton(UIComponent button)
		{
			m_DropDown.triggerButton = button;
			return this;
		}

		public UIDropDownHelper AddDefaultScrollbar()
		{
			var scrollbarHelper = UIHelper.CreateScrollbarWithTrack(UIOrientation.Vertical);
			SetListScrollbar(scrollbarHelper.GetScrollbar());
			return this;
		}
	}

	public class UITabstripHelper : UIComponentHelper<UITabstripHelper>
	{
		private UITabstrip m_Tabstrip = null;

		public UITabstripHelper(UITabstrip tabstrip)
		{
			m_Tabstrip = tabstrip;
			if (m_Tabstrip == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_Tabstrip;
		}

		public UITabstrip GetTabtrip()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			//if (string.IsNullOrEmpty(m_CheckBox.text) && string.IsNullOrEmpty(m_CheckBox.normalBgSprite))
			//	throw new UIHelperException("UIButton has neither text nor normalBgSprite.");
			return m_Tabstrip;
		}

		public UITabstripHelper SetAtlas(UITextureAtlas atlas)
		{
			m_Tabstrip.atlas = atlas;
			return this;
		}

		public UITabstripHelper SetBackgroundSprite(string spriteName)
		{
			m_Tabstrip.backgroundSprite = spriteName;
			return this;
		}

		public UITabstripHelper SetCloseButton(UIComponent button)
		{
			m_Tabstrip.closeButton = button;
			return this;
		}

		public UITabstripHelper SetCloseOnReclick(bool enable)
		{
			m_Tabstrip.closeOnReclick = enable;
			return this;
		}

		public UITabstripHelper SetNavigateWithArrowTabKeys(bool enable)
		{
			m_Tabstrip.navigateWithArrowTabKeys = enable;
			return this;
		}

		public UITabstripHelper SetPadding(RectOffset padding)
		{
			m_Tabstrip.padding = padding;
			return this;
		}

		public UITabstripHelper SetSelectedIndex(int index)
		{
			m_Tabstrip.selectedIndex = index;
			return this;
		}

		public UITabstripHelper SetStartSelectedIndex(int index)
		{
			m_Tabstrip.startSelectedIndex = index;
			return this;
		}

		public UITabstripHelper SetTabPages(UITabContainer tabContainer)
		{
			m_Tabstrip.tabPages = tabContainer;
			return this;
		}
	}

	public class UITabContainerHelper : UIComponentHelper<UITabContainerHelper>
	{
		private UITabContainer m_TabContainer = null;

		public UITabContainerHelper(UITabContainer tabContainer)
		{
			m_TabContainer = tabContainer;
			if (m_TabContainer == null)
				throw new NullReferenceException();
		}

		protected override UIComponent GetComponent()
		{
			return m_TabContainer;
		}

		public UITabContainer GetTabContainer()
		{
			if (!base.IsValid())
				throw new UIHelperException("UIComponentHelper has invalid state.");

			//if (string.IsNullOrEmpty(m_CheckBox.text) && string.IsNullOrEmpty(m_CheckBox.normalBgSprite))
			//	throw new UIHelperException("UIButton has neither text nor normalBgSprite.");
			return m_TabContainer;
		}

		public UITabContainerHelper SetAtlas(UITextureAtlas atlas)
		{
			m_TabContainer.atlas = atlas;
			return this;
		}

		public UITabContainerHelper SetBackgroundSprite(string spriteName)
		{
			m_TabContainer.backgroundSprite = spriteName;
			return this;
		}

		//public UITabContainerHelper SetOwner(UITabstrip owner)
		//{
		//	m_TabContainer.owner = owner;
		//	return this;
		//}

		public UITabContainerHelper SetPadding(RectOffset padding)
		{
			m_TabContainer.padding = padding;
			return this;
		}

		public UITabContainerHelper SetSelectedIndex(int index)
		{
			m_TabContainer.selectedIndex = index;
			return this;
		}
	}
}
