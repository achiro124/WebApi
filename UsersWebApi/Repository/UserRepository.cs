

using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace UsersWebApi.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private string secretKey;
        private User authUser;
        public UserRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueUser(string login)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            return user == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login.ToLower() == loginRequestDTO.Login.ToLower() &&
            u.Password == loginRequestDTO.Password);

            if (user == null)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null
                };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Admin.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO
            {
                Token = tokenHandler.WriteToken(token),
                User = user
            };

            authUser = user;
            return loginResponseDTO;
        }

        public async Task<User> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            User user = new()
            {
                Login = registrationRequestDTO.Login,
                Name = registrationRequestDTO.Name,
                Password = registrationRequestDTO.Name,
                Admin = registrationRequestDTO.Admin,
                Gender = registrationRequestDTO.Gender,
                Birthday = registrationRequestDTO.Birthday,
                CreatedOn = DateTime.Now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            user.Password = "";
            return user;
        }

        public async Task<User> GetUserAsync(string login) => await _context.Users.FirstOrDefaultAsync(x => x.Login == login);

        public async Task<User> CreateAsync(UserCreateDTO userCreateDTO)
        {
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Name = userCreateDTO.Name,
                Login = userCreateDTO.Login,
                Password = userCreateDTO.Password,
                Admin = userCreateDTO.Admin,
                Gender = userCreateDTO.Gender,
                Birthday = userCreateDTO.Birthday,
                CreatedOn = DateTime.Now,
                //CreatedBy = authUser.Login
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

    }
}
