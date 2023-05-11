namespace UsersWebApi.Models.Dto
{
    public class UserDTO
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
