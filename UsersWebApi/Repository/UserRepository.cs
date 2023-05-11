

using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace UsersWebApi.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private string secretKey;
        public UserRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueLogin(string login)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            return user == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login.ToLower() == loginRequestDTO.Login.ToLower() &&
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
            string role = user.Admin ? "Admin" : "User";

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Login.ToString()),
                    new Claim(ClaimTypes.Role, role)
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



        //GET
        public async Task<UserSearchDTO?> GetUserAsync(string login)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null)
            {
                return null;
            }
            UserSearchDTO userSearchDTO = new()
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                IsActive = user.RevokedOn == null
            };
            return userSearchDTO;
        }

        public async Task<User?> GetUserAsync(UserDTO userDTO) => await _context.Users.FirstOrDefaultAsync(u => u.Login == userDTO.Login &&
                                                                                                                              u.Password == userDTO.Password);

        public async Task<List<User>> GetUsersAsync() => await _context.Users.Where(u => u.RevokedOn == null)
                                                                     .OrderBy(u => u.CreatedOn).ToListAsync();
        public async Task<List<User>> GetUsersOnAgeAsync(int age)
        {
            List<User> users = new List<User>();
            foreach (var user in _context.Users)
            {
                DateTime birthday = (DateTime)user.Birthday;
                if (birthday != DateTime.MinValue)
                {
                    int userAge = DateTime.Now.Year - birthday.Year;
                    if (birthday.AddYears(userAge) > DateTime.Now)
                    {
                        userAge--;
                    }

                    if (userAge > age)
                    {
                        users.Add(user);
                    }
                }

            }
            return users;
        }


        //POST
        public async Task<User> CreateAsync(UserCreateDTO userCreateDTO, string authUserLogin)
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
                CreatedBy = authUserLogin
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        //PUT
        public async Task<User> UpdateUserAsync(string login, UserUpdateDTO userUpdateDTO, string? loginAdmin = null)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Login == login);
            if (user == null) return null;
            if (userUpdateDTO.Name != null)
            {
                user.Name = userUpdateDTO.Name;
            }
            if (userUpdateDTO.Birthday != null)
            {
                user.Birthday = userUpdateDTO.Birthday;
            }
            if (userUpdateDTO.Gender != null)
            {
                user.Gender = (int)userUpdateDTO.Gender;
            }

            user.ModifiedBy = loginAdmin == null ? login : loginAdmin;
            user.ModifiedOn = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdatePasswordAsync(string login,string password, string? loginAdmin = null)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null) return null;

            user.Password = password;
            user.ModifiedBy = loginAdmin == null ? login : loginAdmin;
            user.ModifiedOn = DateTime.Now;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateLoginAsync(string login, string? loginAdmin = null)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null) return null;

            user.Login = login;
            user.ModifiedBy = loginAdmin == null ? login : loginAdmin;
            user.ModifiedOn = DateTime.Now;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> RecoveryUserAsync(string login)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null) return null;
            user.RevokedBy = "";
            user.RevokedOn = null;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        //DELETE

        public async Task<User> DeleteUserSoftAsync(string login, string adminLogin)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if(user == null) return null;
            user.RevokedBy = adminLogin;
            user.RevokedOn = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> DeleteUserHardAsync(string login)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if(user == null) return null;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }


    }
}
