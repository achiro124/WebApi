using Azure.Core.Pipeline;

namespace UsersWebApi.Models.Dto
{
    public class UserUpdateDTO
    {
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
