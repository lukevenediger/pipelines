using System.Web.Mvc;

namespace Pipelines.Web.Helpers
{
    public static class Extensions
    {
        public static string GetVersion(this HtmlHelper helper)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static int ToInt(this string str)
        {
            return int.Parse(str);
        }
    }
}