using Api.Dto;
using Api.Interface;
using Api.Models;
using Api.Models.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Api.Enums;

namespace Api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IStateRepository stateRepository;

        public UserController(IUserRepository userRepository,
            IGroupRepository groupRepository,
            IStateRepository stateRepository)
        {
            this.userRepository = userRepository;
            this.groupRepository = groupRepository;
            this.stateRepository = stateRepository;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] Pagination pagination)
        {
            var users = await userRepository.GetUsersAsync(pagination);
            return Ok(users);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {   
            if(!await userRepository.UserExistsAsync(userId)) 
                return NotFound();

            var users = await userRepository.GetUserAsync(userId);
                
            return Ok(users);
        }

        [HttpPost("userCreate")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreate userCreate)
        {
            if(userCreate == null)
                return BadRequest();

            var group = await groupRepository.GetGroupAsync(userCreate.User_group_id);
            var state = await stateRepository.GetStateAsync(States.active.ToString());

            if(group == null || state == null)
                return NotFound("Group or State is Not Found!");

            var hasAdmin = await userRepository.UsersAlredyHaveAdminAsync();

            if (hasAdmin && group.Code.ToLower() == Groups.admin.ToString())
                return BadRequest("Admin already in database!");

            if (Buffer.logins.Contains(userCreate.Login))
                return BadRequest("User with this login already exists");

            Buffer.logins.Add(userCreate.Login);

            await Task.Delay(5000);
            userRepository.CreateUser(new User
            {
                Login = userCreate.Login,
                Password = userCreate.Password,
                User_group_id = group,
                User_state_id = state
            });
            await userRepository.SaveUserAsync();

            Buffer.logins.Remove(userCreate.Login);

            return Ok("Successfully created");
        }

        [HttpDelete("userDelete")]
        [Authorize]
        public async Task<IActionResult> DeleteUser([FromBody] BaseInfo baseInfo)
        {
            if (baseInfo == null)
                return BadRequest();

            var user = await userRepository.GetUserAsync(baseInfo.Id);
            var state = await stateRepository.GetStateAsync(States.blocked.ToString());

            if (user == null || state == null)
                return NotFound();
            
            user.User_state_id = state;

            userRepository.UpdateUser(user);
            await userRepository.SaveUserAsync();

            return Ok("Successfully deleted");
        }
    }
}
