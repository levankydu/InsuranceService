using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace test0000001.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var name = enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name;
            return name ?? enumValue.ToString();
        }
    }
}
