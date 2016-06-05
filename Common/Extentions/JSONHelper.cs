using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Common.Extentions
{
    public static class JSONHelper
    {
        /// <summary>
        /// Десериализация из жсона в объект
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T DeserializeFromJSON<T>(this string json)
        {
            T obj;// = Activator.CreateInstance<T>();

            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(ms);
            }
            
            return obj;
        }

        /// <summary>
        /// Сереализация объекта в JSON
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeToJSON(this object o)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(o.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, o);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
