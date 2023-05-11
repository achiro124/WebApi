namespace UsersWebApi.Models.Dto
{
    public class UserUpdatePasswordDTO
    {
        public string? Login { get; set; }
        public string? NewPassword { get; set; }
    }
}
