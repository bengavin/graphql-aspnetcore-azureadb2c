using System;
using System.ComponentModel;
using System.Linq;

namespace StarWars.UI.Blazor.Extensions
{
    public static class EnumExtensions
    {
        public static string AsDisplayString<T>(this T value)
        {
            var valueType = typeof(T);
            if (!valueType.IsEnum) { return value.ToString(); }

            var descriptors = (DescriptionAttribute[])valueType.GetMember(value.ToString())?.FirstOrDefault()?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return descriptors.FirstOrDefault()?.Description
                   ?? value.ToString();
        }
    }
}

