using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using UsersWebApi.Models;
using UsersWebApi.Repository;

namespace UsersWebApi.Controllers
{

    /// <summary>
    /// Контроллер для обработки запросов пользователей.
    /// </summary>


    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private APIResponse _response;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Логин авторизовавшегося пользователя
        private readonly string authUserLogin = "";

        /// <summary>
        /// Конструктор
        /// </summary>
        public UsersController(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor) 
        {
            _userRepository= userRepository;
            _response = new();
            _httpContextAccessor = httpContextAccessor;

            Guid id = Guid.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value);
            var user =  _userRepository.GetUserById(id);
            authUserLogin = user.Login;
        }




        //GET
        /// <summary>
        /// Получение списка активных пользователей. Доступно админам.
        /// </summary>
        /// <returns></returns>

        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Получение списка пользователей старше определенного возраста. Доступно админам.
        /// </summary>

        [HttpGet("GetAllUserByAge")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Получение пользователя по логину. Доступно админу.
        /// </summary>

        [HttpGet("GetUser", Name = "GetUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Запрос пользователя по логину и паролю. Доступно всем активным пользователям.
        /// </summary>

        [HttpGet("User")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Создание пользователя. Доступно админу
        /// </summary>
        /// 
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
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

                if(createDTO.Birthday > DateTime.Now)
                {
                    ModelState.AddModelError("CustomError", "Birthday is not normal!");
                    return BadRequest(ModelState);
                }


                if (!_userRepository.IsUniqueLogin(createDTO.Login))
                {
                    ModelState.AddModelError("CustomError", "User with this username already exists!");
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

        /// <summary>
        /// Обновление пользователя. Доступно всем активным пользователям.
        /// </summary>

        [HttpPut("UpdateUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateUser([FromBody] UserUpdateDTO userUpdate)
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


        /// <summary>
        /// Обновление пользователей. Доступно админу.
        /// </summary>

        [HttpPut("UpdateUserByAdmin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdateUserByAdmin(string login, [FromBody] UserUpdateDTO userUpdate)
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

        /// <summary>
        /// Обновление пароля пользователя. Доступно всем активным пользователям.
        /// </summary>

        [HttpPut("UpdatePassword")]
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

        /// <summary>
        /// Обновление пароля пользователей. Доступно админу.
        /// </summary>

        [HttpPut("UpdatePasswordByAdmin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdatePassword([FromBody] UserUpdatePasswordDTO userUpdatePasswordDTO)
        {
            try
            {
                if(userUpdatePasswordDTO == null)
                {
                    return BadRequest(userUpdatePasswordDTO);
                }
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

        /// <summary>
        /// Обновление логина пользователя. Доступно всем активным пользователям.
        /// </summary>

        [HttpPut("UpdateLogin")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateLogin(string newLogin)
        {
            try
            {
                if (!_userRepository.IsUniqueLogin(newLogin))
                {
                    ModelState.AddModelError("CustomError", "User with this username already exists!");
                    return BadRequest(ModelState);
                }

                User? user = await _userRepository.UpdateLoginAsync(authUserLogin,newLogin);
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

        /// <summary>
        /// Обновление логина пользователей. Доступно админу.
        /// </summary>
        /// 
        [HttpPut("UpdateLoginByAdmin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> UpdateLoginByAdmin(string userLogin, string newLogin)
        {
            try
            {
                if (!_userRepository.IsUniqueLogin(newLogin))
                {
                    ModelState.AddModelError("CustomError", "User with this username already exists!");
                    return BadRequest(ModelState);
                }
       
                User? user = await _userRepository.UpdateLoginAsync(userLogin, newLogin, authUserLogin);
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

        /// <summary>
        /// Восстановление пользователя. Доступно админу.
        /// </summary>

        [HttpPut("RecoveryUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> RecoveryUser(string login)
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

        /// <summary>
        /// Мягкое или жесткое удаление пользователя. Доступно админу.
        /// </summary>

        [HttpDelete("DeleteUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> DeleteUser(string login, DeletedType deletedType)
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
