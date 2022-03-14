using Newtonsoft.Json;

namespace pdf_generator.Wrappers
{
    public class JsonConvertWrapper : IJsonConvertWrapper
    {
        public string SerializeObject(object objectToSerialize, Formatting formatting, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(objectToSerialize, formatting, settings);
        }

        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
