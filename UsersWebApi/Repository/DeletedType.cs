using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace UsersWebApi.Repository
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeletedType
    {
        [EnumMember(Value = "soft")]
        soft,
        [EnumMember(Value = "hard")]
        hard
    }
}
