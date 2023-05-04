using Api.Interface;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using static Api.Enums;

namespace Api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository groupRepository;

        public GroupController(IGroupRepository groupRepository)
        {
            this.groupRepository = groupRepository;
        }

        [HttpPost("groupCreate")]
        public async Task<IActionResult> GroupUser()
        {
            if (await groupRepository.GroupHasSomeAsync())
                return Ok("All groups already in database!");

            groupRepository.CreateGroup(new User_group
            {
                Code = Groups.admin.ToString(),
                Description = "This means that user is admin!"
            });

            groupRepository.CreateGroup(new User_group
            {
                Code = Groups.user.ToString(),
                Description = "This means that user is... user!"
            });

            await groupRepository.SaveGroupAsync();

            return Ok("Successfully created");
        }
    }
}
