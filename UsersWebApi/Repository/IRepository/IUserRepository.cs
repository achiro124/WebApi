namespace UsersWebApi.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string login);

        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<User> Register(RegistrationRequestDTO registrationRequestDTO);
        Task<User> GetUserAsync(string login);
        Task<User> CreateAsync(UserCreateDTO userCreateDTO);
        

    }
}
