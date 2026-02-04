using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using shot_reminder_2.Application.Use_Cases.Users;
using shot_reminder_2.Contracts.Users;

namespace shot_reminder_2.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly CreateUserHandler _createUserHandler;

    public UsersController(CreateUserHandler createUserHandler)
    {
        _createUserHandler = createUserHandler;
    }

    //[HttpPost]
    //public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
    //{
    //    var createUserResult = await _createUserHandler.HandleAsync(new CreateUserCommand
    //        (
    //            FirstName:request.FirstName,
    //            LastName:request.LastName,
    //            Email:request.Email
    //        ));

    //    var response = new CreateUserResponse(Id: createUserResult.Id);

    //    return Created(nameof(Create), response);
    //}
}
