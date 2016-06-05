using System;
using System.Reflection;

namespace Common.Alias
{
    /// <summary>
    /// Алиасы для объектов
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class AliasAttribute : Attribute
    {
        public string Alias { get; set; }

        public AliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

    public static class EnumExtention
    {
        /// <summary>
        /// Алиас enum
        /// Берется из аттрибута AliasAttribute
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetAlias(this Enum o)
        {
            var type = o.GetType();
            MemberInfo[] memInfo = type.GetMember(o.ToString());

            if (memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(AliasAttribute),false);

                if (attrs.Length > 0)
                    return ((AliasAttribute)attrs[0]).Alias;
            }

            return o.ToString();
        }
    }
}
