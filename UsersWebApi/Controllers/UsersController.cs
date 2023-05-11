using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using UsersWebApi.Models;
using UsersWebApi.Repository;

namespace UsersWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        protected APIResponse _response;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Логин авторизовавшегося пользователя
        private readonly string authUserLogin = "";

        public UsersController(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor) 
        {
            _userRepository= userRepository;
            _response = new();
            _httpContextAccessor = httpContextAccessor;
            authUserLogin = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        }




        //GET

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

        [HttpGet("admin/login", Name ="GetUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetUser([FromBody] string login)
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

        [HttpGet("login", Name = "GetUserByLoginAndPassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetUser([FromBody] UserDTO userDTO)
        {
            try
            {
                if(userDTO.Login != authUserLogin)
                {
                    ModelState.AddModelError("CustomError", "Login error!");
                    return BadRequest(ModelState);
                }
                var user = await _userRepository.GetUserAsync(userDTO);
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


        //POST

        [HttpPost(Name = "CreateUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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


                if (!_userRepository.IsUniqueLogin(createDTO.Login))
                {
                    ModelState.AddModelError("CustomError", "User alredy Exists!");
                    return BadRequest(ModelState);
                }

                User? user = await _userRepository.CreateAsync(createDTO, authUserLogin);
                if (user == null)
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


        //PUT

        [HttpPut(Name = "UpdateUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateUser(UserUpdateDTO userUpdate)
        {
            try
            {
                User? user = await _userRepository.UpdateUserAsync(authUserLogin, userUpdate);
                if (user == null)
                {
                    return NotFound();
                }
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                return _response;
            }
        }

        [HttpPut("admin/{login}",Name = "UpdateUserByAdmin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateUserByAdmin(string login,UserUpdateDTO userUpdate)
        {
            try
            {
                User? user = await _userRepository.UpdateUserAsync(login, userUpdate, authUserLogin);
                if (user == null)
                {
                    return NotFound();
                }
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                return _response;
            }
        }

        [HttpPut("password",Name = "UpdatePassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdatePassword([FromBody]string newPassword)
        {
            try
            {
                User? user = await _userRepository.UpdatePasswordAsync(authUserLogin, newPassword);
                if (user == null)
                {
                    return NotFound();
                }
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                return _response;
            }
        }

        [HttpPut("admin/password", Name = "UpdatePasswordByAdmin")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdatePassword(UserUpdatePasswordDTO userUpdatePasswordDTO)
        {
            try
            {
                User? user = await _userRepository.UpdatePasswordAsync(userUpdatePasswordDTO.Login, userUpdatePasswordDTO.NewPassword, authUserLogin);
                if (user == null)
                {
                    return NotFound();
                }
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                return _response;
            }
        }


        [HttpPut("login", Name = "UpdateLogin")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateLogin([FromBody] string newLogin)
        {
            try
            {
                if (_userRepository.IsUniqueLogin(newLogin))
                {
                    ModelState.AddModelError("CustomError", "User alredy Exists!");
                    return BadRequest(ModelState);
                }

                User? user = await _userRepository.UpdateLoginAsync(newLogin);
                if (user == null)
                {
                    return NotFound();
                }
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                return _response;
            }
        }

      //  [HttpPut("login", Name = "UpdateLogin")]
      //  [Authorize]
      //  [ProducesResponseType(StatusCodes.Status404NotFound)]
      //  [ProducesResponseType(StatusCodes.Status400BadRequest)]
      //  public async Task<ActionResult<APIResponse>> UpdateLoginByAdmin(string login,[FromBody] string newLogin)
      //  {
      //      try
      //      {
      //          if (_userRepository.IsUniqueLogin(newLogin))
      //          {
      //              ModelState.AddModelError("CustomError", "User alredy Exists!");
      //              return BadRequest(ModelState);
      //          }
      //
      //          User? user = await _userRepository.UpdateLoginAsync(newLogin);
      //          if (user == null)
      //          {
      //              return NotFound();
      //          }
      //          _response.StatusCode = HttpStatusCode.NoContent;
      //          _response.IsSuccess = true;
      //          return Ok(_response);
      //      }
      //      catch (Exception ex)
      //      {
      //          _response.IsSuccess = false;
      //          _response.ErrorMessages
      //               = new List<string>() { ex.ToString() };
      //          return _response;
      //      }
      //  }


        [HttpPut("recovery", Name = "RecoveryUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> RecoveryUser([FromBody] string login)
        {
            try
            {
                User? user = await _userRepository.RecoveryUserAsync(login);
                if (user == null)
                {
                    return NotFound();
                }
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                return _response;
            }
        }
        

        //DELETE

        [HttpDelete("admin/login", Name ="DeleteUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteUser([FromBody] string login, DeletedType deletedType)
        {
            try
            {

                User? user = null;
                if (deletedType == DeletedType.soft)
                {
                    user = await _userRepository.DeleteUserSoftAsync(login, authUserLogin);
                }
                if (deletedType == DeletedType.hard)
                {
                    user = await _userRepository.DeleteUserHardAsync(login);
                }

                if (user == null)
                {
                    return NotFound();
                }
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                return _response;
            }
   
        }   
      
    }
}
