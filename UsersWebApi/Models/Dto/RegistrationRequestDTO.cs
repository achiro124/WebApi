namespace UsersWebApi.Models.Dto
{
    public class RegistrationRequestDTO
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public bool Admin { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
