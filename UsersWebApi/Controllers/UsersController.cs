using System.Security.Claims;

namespace UsersWebApi.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UsersController : Controller
    {
        protected APIResponse _response;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Логин авторизовавшегося пользователя
        private readonly string? userLogin;

        public UsersController(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor) 
        {
            _userRepository= userRepository;
            _response = new();
            _httpContextAccessor = httpContextAccessor;
            userLogin = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        [HttpGet(Name ="GetAllUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<APIResponse>> GetUsers()
        {
            try
            {
                _response.Result = await _userRepository.GetUsersAsync();
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("age",Name = "GetAllUserOnAge")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<APIResponse>> GetUsersOnAge(int age)
        {
            try
            {
                _response.Result = await _userRepository.GetUsersOnAgeAsync(age);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("login", Name ="GetUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetUser(string login)
        {
            try
            {
                UserSearchDTO? user = await _userRepository.GetUserAsync(login);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = user;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
                                
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return _response;
            }
            
        }

        [HttpGet("login/password", Name = "GetUserByLoginAndPassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetUser(string login, string password)
        {
            try
            {
                if(login != userLogin)
                {
                    ModelState.AddModelError("CustomError", "Login error!");
                    return BadRequest(ModelState);
                }
                var user = await _userRepository.GetUserAsync(login,password);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = user;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return _response;
            }

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> CreateUser([FromBody] UserCreateDTO createDTO)
        {
            try
            {
                if (createDTO == null)
                {
                    return BadRequest(createDTO);
                }
                if (createDTO.Gender > 2 || createDTO.Gender < 0)
                {
                    ModelState.AddModelError("CustomError", "Gender is not normal!");
                    return BadRequest(ModelState);
                }


                if (await _userRepository.GetUserAsync(createDTO.Login) != null)
                {
                    ModelState.AddModelError("CustomError", "User alredy Exists!");
                    return BadRequest(ModelState);
                }

                User? user = await _userRepository.CreateAsync(createDTO, userLogin);
                if(user == null)
                {
                    return BadRequest(user);
                }
                _response.Result = user;
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetUser", new { login = createDTO.Login }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;

        }

      //  [HttpDelete("login", Name ="DeleteUser")]
      //  [ProducesResponseType(StatusCodes.Status204NoContent)]
      //  [ProducesResponseType(StatusCodes.Status404NotFound)]
      //  public IActionResult DeleteUser(string login)
      //  {
      //      var user = _context.Users.FirstOrDefault(u => u.Login == login);
      //      if (user == null)
      //      {
      //          return NotFound();
      //      }
      //      _context.Users.Remove(user);
      //      _context.SaveChanges();
      //      return NoContent();
      //  }
      //
      //  [HttpPut("login", Name = "UpdateUser")]
      //  [ProducesResponseType(StatusCodes.Status400BadRequest)]
      //  [ProducesResponseType(StatusCodes.Status204NoContent)]
      //  [ProducesResponseType(StatusCodes.Status404NotFound)]
      //  public IActionResult UpdateUser(string login, [FromBody] UserDTO userDTO)
      //  {
      //     if(userDTO == null)
      //     {
      //         return BadRequest();
      //     }
      //    
      //     //if (_context.Users.FirstOrDefault(u => u.Login.ToLower() == userDTO.Login.ToLower()) != null)
      //     //{
      //     //    ModelState.AddModelError("CustomError", "User login alredy Exists!");
      //     //    return BadRequest(ModelState);
      //     //}
      //    
      //     var user = _context.Users.FirstOrDefault(u => u.Login == login);
      //     if(user == null)
      //     {
      //         return NotFound();
      //     }
      //    
      //     User updUser = new()
      //     {
      //         Id = user.Id,
      //         Login = userDTO.Login,
      //         Password = userDTO.Password,
      //         Name = userDTO.Name,
      //         Gender = userDTO.Gender,
      //         Birthday = userDTO.Birthday,
      //         ModifiedOn = DateTime.Now
      //     };
      //    
      //     _context.Users.Update(updUser);
      //     _context.SaveChanges();
      //    
      //     return NoContent();
      //
      //  }

     //   [HttpPut("login", Name = "UpdatePartialUser")]
     //   [ProducesResponseType(StatusCodes.Status400BadRequest)]
     //   [ProducesResponseType(StatusCodes.Status204NoContent)]
     //   [ProducesResponseType(StatusCodes.Status404NotFound)]
     //   public IActionResult UpdatePartialUser(string login, JsonPatchDocument<UserDTO> patchDTO)
     //   {
     //    //  if (patchDTO == null)
     //    //  {
     //    //      return BadRequest();
     //    //  }
     //    //
     //    //  // if (UsersDb.userList.FirstOrDefault(u => u.Login.ToLower() == userDTO.Login.ToLower()) != null)
     //    //  // {
     //    //  //     ModelState.AddModelError("CustomError", "User login alredy Exists!");
     //    //  //     return BadRequest(ModelState);
     //    //  // }
     //    //
     //    //  var user = _context.Users.FirstOrDefault(u => u.Login == login);
     //    //
     //    //  UserDTO userDTO = new()
     //    //  {
     //    //      Login = user.Login,
     //    //      Password = user.Password,
     //    //      Name = user.Name,
     //    //      Gender = user.Gender,
     //    //      Birthday = user.Birthday
     //    //  };
     //    //
     //    //
     //    //
     //    //  if (user == null)
     //    //  {
     //    //      return NotFound();
     //    //  }
     //    //  patchDTO.ApplyTo(userDTO, ModelState);
     //    //
     //    //  User updUser = new()
     //    //  {
     //    //      Id = user.Id,
     //    //      Login = userDTO.Login,
     //    //      Password = userDTO.Password,
     //    //      Name = userDTO.Name,
     //    //      Gender = userDTO.Gender,
     //    //      Birthday = userDTO.Birthday,
     //    //      ModifiedOn = DateTime.Now
     //    //  };
     //    //
     //    //  if (!ModelState.IsValid)
     //    //  {
     //    //      return BadRequest(ModelState);
     //    //  }
     //    //
     //    //  _context.Users.Update(updUser);
     //    //  _context.SaveChanges();
     //    //
     //    //  return NoContent();
     //
     //   }
    }
}
