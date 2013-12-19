using System.Web;

namespace Common.WebDefinitions.Extensions
{
    public static class StringExtensions
    {
        public static HtmlString ToHtmlString(this string text)
        {
            return new HtmlString(text);
        }
    }
}
