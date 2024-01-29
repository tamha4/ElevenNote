using ElevenNote.Models.Responses;
using ElevenNote.Models.Token;
using ElevenNote.Models.User;
using ElevenNote.Services.Token;
using ElevenNote.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ElevenNote.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userService;
        private readonly ITokenService _tokenService;

        public UserController(IUserServices userServices, ITokenService tokenService)
        {
            _userService = userServices;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegister model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var registerResult = await _userService.RegisterUserAsync(model);
            if (registerResult)
            {
                TextResponses  response= new("User was registered.");
                return Ok(response);
            }
            return BadRequest(new TextResponses("User could not be registered"));
        }

        [Authorize]
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int userId){
            UserDetail? detail = await _userService.GetUserByIdAsync(userId);
            if(detail is null)
                return NotFound();

            return Ok(detail);
        }

        [HttpPost("~/api/Token")]
        public async Task<IActionResult> GetToken([FromBody] TokenRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TokenResponse? response = await _tokenService.GetTokenAsync(request);

            if(response is null)
            return BadRequest(new TextResponses("Invalid Username or password"));

            return Ok(response);
        }
    }
}
