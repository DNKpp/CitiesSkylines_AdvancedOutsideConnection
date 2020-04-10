
namespace AdvancedOutsideConnection.framework
{
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

        public ColossalFramework.UI.UIMultiStateButton.SpriteSet ToMultiStateButtonSpriteSet(ColossalFramework.UI.UIMultiStateButton button)
        {
            var spriteSet = new ColossalFramework.UI.UIMultiStateButton.SpriteSet();
            spriteSet.Setter(button);
            spriteSet.normal = normal;
            spriteSet.disabled = disabled;
            spriteSet.focused = focused;
            spriteSet.hovered = hovered;
            spriteSet.pressed = pressed;
            return spriteSet;
        }

        public SpriteSet Copy()
        {
            return new SpriteSet(this);
        }
    };
}
