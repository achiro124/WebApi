namespace UsersWebApi.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueLogin(string login);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<User> Register(RegistrationRequestDTO registrationRequestDTO);

        //GET
        Task<UserSearchDTO?> GetUserAsync(string login);
        Task<User?> GetUserAsync(UserDTO userDTO);
        Task<List<User>> GetUsersAsync();
        Task<List<User>> GetUsersOnAgeAsync(int age);

        //POST
        Task<User> CreateAsync(UserCreateDTO userCreateDTO,string authUserLogin);

        //PUT
        Task<User> UpdateUserAsync(string login, UserUpdateDTO userUpdateDTO, string? loginAdmin = null);
        Task<User> UpdatePasswordAsync(string login, string password, string? loginAdmin = null);
        Task<User> UpdateLoginAsync(string login, string? loginAdmin = null);
        Task<User> RecoveryUserAsync(string login);

        //DELETE
        Task<User> DeleteUserSoftAsync(string login, string loginAdmin);
        Task<User> DeleteUserHardAsync(string login);
        

    }
}
