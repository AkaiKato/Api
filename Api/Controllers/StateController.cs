using Api.Interface;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using static Api.Enums;

namespace Api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly IStateRepository stateRepository;

        public StateController(IStateRepository stateRepository)
        {
            this.stateRepository = stateRepository;
        }

        [HttpPost("stateCreate")]
        public async Task<IActionResult> StateUser()
        {
            if (await stateRepository.StateHasSomeAsync())
                return Ok("All states already in database!");

            stateRepository.CreateState(new User_state
            {
                Code = States.active.ToString(),
                Description = "This means that user is not blocked!"
            });

            stateRepository.CreateState(new User_state
            {
                Code = States.blocked.ToString(),
                Description = "This means that user is blocked!"
            });

            await stateRepository.SaveStateAsync();

            return Ok("Successfully created");
        }
    }
}
