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
