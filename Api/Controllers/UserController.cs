using Api.Data;
using Api.Dto;
using Api.Models;
using Api.Models.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext dataContext;

        enum Groups
        {
            admin,
            user
        }
        enum States
        {
            active,
            blocked
        }

        public UserController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] Pagination pagination)
        {
            var users = await dataContext.Users
                .Include(x => x.User_group_id)
                .Include(x => x.User_state_id)
                .Where(u => u.User_state_id.Code.ToLower() == States.active.ToString())
                .Skip((pagination.PageNumber-1)*pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {   
            if(!dataContext.Users.Where(u => u.Id == userId).Any()) 
                return NotFound();

            var users = await dataContext.Users
                .Include(x => x.User_group_id)
                .Include(x => x.User_state_id)
                .Where(u => u.Id == userId)
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("userCreate")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreate userCreate)
        {
            if(userCreate == null)
                return BadRequest();

            var group = await dataContext.Groups.FirstOrDefaultAsync(x => x.Id == userCreate.User_group_id);
            var state = await dataContext.Statements.FirstOrDefaultAsync(x => x.Code.ToLower() == States.active.ToString());

            if(group == null || state == null)
                return NotFound("Group or State is Not Found!");

            var hasAdmin = await dataContext.Users.AnyAsync(x => x.User_group_id.Code.ToLower() == Groups.admin.ToString());

            if (hasAdmin && group.Code.ToLower() == Groups.admin.ToString())
                return BadRequest("Admin already in database!");

            if (Buffer.logins.Contains(userCreate.Login))
                return BadRequest("User with this login already exists");

            Buffer.logins.Add(userCreate.Login);

            await Task.Delay(5000);
            dataContext.Add(new User
            {
                Login = userCreate.Login,
                Password = userCreate.Password,
                User_group_id = group,
                User_state_id = state
            });
            await dataContext.SaveChangesAsync();

            Buffer.logins.Remove(userCreate.Login);

            return Ok("Successfully created");
        }

        [HttpDelete("userDelete")]
        [Authorize]
        public async Task<IActionResult> DeleteUser([FromBody] BaseInfo baseInfo)
        {
            if (baseInfo == null)
                return BadRequest();

            var user = await dataContext.Users.Include(x => x.User_state_id).FirstOrDefaultAsync(x => x.Id == baseInfo.Id);
            var state = await dataContext.Statements.FirstOrDefaultAsync(x => x.Code == States.blocked.ToString());

            if (user == null || state == null)
                return NotFound();
            
            user.User_state_id = state;

            dataContext.Update(user);
            await dataContext.SaveChangesAsync();

            return Ok("Successfully deleted");
        }

        [HttpPost("groupCreate")]
        public async Task<IActionResult> GroupUser()
        {
            if (await dataContext.Groups.AnyAsync())
                return Ok("All groups already in database!");

            dataContext.AddRange(
                new User_group
                {
                    Code = Groups.admin.ToString(),
                    Description = "This means that user is admin!"
                },
                new User_group
                {
                    Code = Groups.user.ToString(),
                    Description = "This means that user is... user!"
                }
            );

            await dataContext.SaveChangesAsync();

            return Ok("Successfully created");
        }

        [HttpPost("stateCreate")]
        public async Task<IActionResult> StateUser()
        {
            if (await dataContext.Statements.AnyAsync())
                return Ok("All states already in database!");

            dataContext.AddRange(
                new User_state
                {
                    Code = States.active.ToString(),
                    Description = "This means that user is not blocked!"
                },
                new User_state
                {
                    Code = States.blocked.ToString(),
                    Description = "This means that user is blocked!"
                }
            );

            await dataContext.SaveChangesAsync();

            return Ok("Successfully created");
        }
    }
}
