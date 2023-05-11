namespace UsersWebApi.Controllers
{
    /// <summary>
    /// Контроллер для авторизации и регистрации пользователей
    /// </summary>
    /// 
    [Route("api/Users/UsersAuth")]
    [ApiController]
    public class AuthUsersController : Controller
    {
        private readonly IUserRepository _userRepo;
        private APIResponse _response;


        /// <summary>
        /// Конструктор
        /// </summary>
        public AuthUsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            this._response = new();
        }

        /// <summary>
        /// Авторизация пользователя
        /// </summary>


        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse = await _userRepo.Login(model);
            if (loginResponse.User == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }
            if(loginResponse.User.RevokedOn != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User deleted");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponse;
            return Ok(_response);
        }

        /// <summary>
        /// Регистрация пользователя
        /// </summary>

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            bool ifLoginNameUnique = _userRepo.IsUniqueLogin(model.Login);
            if (!ifLoginNameUnique)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            var user = await _userRepo.Register(model);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

    }
}
