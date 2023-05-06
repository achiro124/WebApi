namespace UsersWebApi.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string login);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<User> Register(RegistrationRequestDTO registrationRequestDTO);
        Task<UserSearchDTO?> GetUserAsync(string login);
        Task<User?> GetUserAsync(string login, string password);
        Task<User> CreateAsync(UserCreateDTO userCreateDTO,string authUserLogin);
        Task<List<User>> GetUsersAsync();
        Task<List<User>> GetUsersOnAgeAsync(int age);

    }
}
