﻿namespace UsersWebApi.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<UserDTO>> GetUsers()
        {
            return Ok(UsersDb.userList);
        }

        [HttpGet("login", Name ="GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDTO> GetUser(string login)
        {
            var user = UsersDb.userList.FirstOrDefault(u => u.Login == login);
            if(user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDTO> CreateUser([FromBody] UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest(userDTO);
            }

            if (UsersDb.userList.FirstOrDefault(u => u.Login.ToLower() == userDTO.Login.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError","User alredy Exists!");
                return BadRequest(ModelState);
            }

            //userDTO.Id = Guid.NewGuid();
            UsersDb.userList.Add(userDTO);

            return CreatedAtRoute("GetUser", new {login = userDTO.Login}, userDTO);
        }

        [HttpDelete("login", Name ="DeleteUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteUser(string login)
        {
            var user = UsersDb.userList.FirstOrDefault(u => u.Login == login);
            if (user == null)
            {
                return NotFound();
            }
            UsersDb.userList.Remove(user);
            return NoContent();
        }


        [HttpPut("login", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateUser(string login, [FromBody] UserDTO userDTO)
        {
            if(userDTO == null)
            {
                return BadRequest();
            }

           // if (UsersDb.userList.FirstOrDefault(u => u.Login.ToLower() == userDTO.Login.ToLower()) != null)
           // {
           //     ModelState.AddModelError("CustomError", "User login alredy Exists!");
           //     return BadRequest(ModelState);
           // }

            var user = UsersDb.userList.FirstOrDefault(u => u.Login == login);
            if(user == null)
            {
                return NotFound();
            }
            user.Login = userDTO.Login;
            user.Password = userDTO.Password;
            return NoContent();

        }

        [HttpPut("login", Name = "UpdatePartialUser")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartialUser(string login, JsonPatchDocument<UserDTO> patchDTO)
        {
            if (patchDTO == null)
            {
                return BadRequest();
            }

            // if (UsersDb.userList.FirstOrDefault(u => u.Login.ToLower() == userDTO.Login.ToLower()) != null)
            // {
            //     ModelState.AddModelError("CustomError", "User login alredy Exists!");
            //     return BadRequest(ModelState);
            // }

            var user = UsersDb.userList.FirstOrDefault(u => u.Login == login);
            if (user == null)
            {
                return NotFound();
            }
            patchDTO.ApplyTo(user, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();

        }
    }
}