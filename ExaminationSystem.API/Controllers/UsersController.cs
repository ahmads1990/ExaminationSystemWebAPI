using ExaminationSystem.API.Common;
using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Requests.Users;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for user profile management.
/// </summary>
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="request">The change password request containing the old and new passwords.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if password stands mutated.</returns>
    [HttpPut("me/change-password")]
    public async Task<ApiResponse<string>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _userService.ChangePassword(CurrentUserId!.Value, request.OldPassword, request.NewPassword, cancellationToken);
        return result == UserOperationResult.Success
            ? new SuccessResponse<string>("", "Your password has been changed successfully.")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }
}
