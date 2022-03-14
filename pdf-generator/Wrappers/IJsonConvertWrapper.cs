using Newtonsoft.Json;

namespace pdf_generator.Wrappers
{
    public interface IJsonConvertWrapper
    {
        string SerializeObject(object objectToSerialize, Formatting formatting, JsonSerializerSettings settings);

        T DeserializeObject<T>(string value);
    }
}
