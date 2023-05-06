namespace UsersWebApi.Models.Dto
{
    public class UserSearchDTO
    {
        public string Name { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool IsActive { get; set; }
    }
}
