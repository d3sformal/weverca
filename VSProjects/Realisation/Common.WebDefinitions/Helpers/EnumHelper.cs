using System.ComponentModel;
using System.Reflection;

namespace Common.WebDefinitions.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the description from <see cref="DescriptionAttribute"/> used to decorate enum record.
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="value">The enum value.</param>
        /// <returns>The description specified by the <see cref="DescriptionAttribute"/>, of  one is used; result of <c>.ToString()</c> otherwise.</returns>
        public static string GetDescription<T>(T value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if ((attributes != null) && (attributes.Length > 0))
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
