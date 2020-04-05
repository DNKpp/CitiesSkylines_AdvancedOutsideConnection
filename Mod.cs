using ICities;

namespace AdvancedOutsideConnection
{
    public class Mod : IUserMod
    {
        private static readonly string version = "0.0.1";

        public static string ModName => "AdvancedOutsideConnection";

        public string Name { get { return ModName; } }

        public string Description => "Advanced options for outside connections.";
    }
}
