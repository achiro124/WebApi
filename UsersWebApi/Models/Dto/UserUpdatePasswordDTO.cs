namespace UsersWebApi.Models.Dto
{
    public class UserUpdatePasswordDTO
    {
        public string? Login { get; set; }

        [Required]
        public string? NewPassword { get; set; }
    }
}
