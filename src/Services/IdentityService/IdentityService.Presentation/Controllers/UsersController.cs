using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using IdentityService.Application.Users.Commands;
using IdentityService.Application.Users.Queries;
using IdentityService.Contracts.Users;
using IdentityService.Contracts.Common;

namespace IdentityService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Get user details including roles, permissions, and organizations
    /// </summary>
    [HttpGet("{id:guid}/details")]
    public async Task<ActionResult<UserDetailsResponse>> GetUserDetails(Guid id)
    {
        var query = new GetUserDetailsQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Search users with filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserListResponse>> SearchUsers([FromQuery] UserSearchRequest request)
    {
        var query = new SearchUsersQuery(
            request.SearchTerm,
            request.Status,
            request.OrganizationId,
            request.Page,
            request.PageSize
        );

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var command = new CreateUserCommand(
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password,
            request.SendWelcomeEmail,
            request.OrganizationId,
            currentUserId
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUser), new { id = result }, result);
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var command = new UpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.ProfilePicture,
            currentUserId
        );

        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Update user status
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var command = new UpdateUserStatusCommand(id, request.Status, request.Reason, currentUserId);

        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var command = new DeleteUserCommand(id, currentUserId);

        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var command = new AssignRoleCommand(
            id,
            request.RoleId,
            request.OrganizationId,
            request.ExpiresAt,
            currentUserId
        );

        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    public async Task<ActionResult> RemoveRole(Guid id, Guid roleId, [FromQuery] Guid? organizationId = null)
    {
        var currentUserId = GetCurrentUserId();
        var command = new RemoveRoleCommand(id, roleId, organizationId, currentUserId);

        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Grant permission to user
    /// </summary>
    [HttpPost("{id:guid}/permissions")]
    public async Task<ActionResult> GrantPermission(Guid id, [FromBody] GrantPermissionRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var command = new GrantPermissionCommand(
            id,
            request.PermissionId,
            request.OrganizationId,
            request.ExpiresAt,
            request.Conditions,
            currentUserId
        );

        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Revoke permission from user
    /// </summary>
    [HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
    public async Task<ActionResult> RevokePermission(Guid id, Guid permissionId, [FromQuery] Guid? organizationId = null)
    {
        var currentUserId = GetCurrentUserId();
        var command = new RevokePermissionCommand(id, permissionId, organizationId, currentUserId);

        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Get user roles
    /// </summary>
    [HttpGet("{id:guid}/roles")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetUserRoles(Guid id, [FromQuery] Guid? organizationId = null)
    {
        var query = new GetUserRolesQuery(id, organizationId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user permissions
    /// </summary>
    [HttpGet("{id:guid}/permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetUserPermissions(Guid id, [FromQuery] Guid? organizationId = null)
    {
        var query = new GetUserPermissionsQuery(id, organizationId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user organizations
    /// </summary>
    [HttpGet("{id:guid}/organizations")]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetUserOrganizations(Guid id)
    {
        var query = new GetUserOrganizationsQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user sessions
    /// </summary>
    [HttpGet("{id:guid}/sessions")]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetUserSessions(Guid id, [FromQuery] bool activeOnly = true)
    {
        var query = new GetUserSessionsQuery(id, activeOnly);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}