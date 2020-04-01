using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImprovedOutsideConnection
{
    public static class Utils
    {
        public static T MaxEnumValue<E, T>()
        {
            return Enum.GetValues(typeof(E)).Cast<T>().Max();
        }

        public static int MaxEnumValue<E>()
        {
            return MaxEnumValue<E, int>();
        }

        public static T MinEnumValue<E, T>()
        {
            return Enum.GetValues(typeof(E)).Cast<T>().Min();
        }

        public static int MinEnumValue<E>()
        {
            return MinEnumValue<E, int>();
        }
    }
}
