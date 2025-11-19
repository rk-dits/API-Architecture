using BuildingBlocks.Common.Pagination;
using IdentityService.Application.RBAC.Commands;
using IdentityService.Application.RBAC.Queries;
using IdentityService.Contracts.RBAC;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IMediator mediator, ILogger<RolesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PageResult<RoleDto>>> GetRoles(
        [FromQuery] Guid? organizationId = null,
        [FromQuery] RoleType? type = null,
        [FromQuery] RoleLevel? level = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetRolesQuery(organizationId,
                type.HasValue ? (IdentityService.Domain.Entities.RoleType)(int)type.Value : null,
                level.HasValue ? (IdentityService.Domain.Entities.RoleLevel)(int)level.Value : null,
                pageNumber, pageSize);

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, "An error occurred while retrieving roles");
        }
    }

    /// <summary>
    /// Get a specific role by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleDto>> GetRole(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetRoleQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound($"Role with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            return StatusCode(500, "An error occurred while retrieving the role");
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<ActionResult<Guid>> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreateRoleCommand(
                request.Name,
                request.Description,
                request.Type,
                request.Level,
                request.OrganizationId,
                request.ParentRoleId,
                request.PermissionIds
            );

            var roleId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetRole), new { id = roleId }, roleId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role {RoleName}", request.Name);
            return StatusCode(500, "An error occurred while creating the role");
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateRoleCommand(
                id,
                request.Name,
                request.Description,
                request.Type,
                request.Level,
                request.ParentRoleId,
                request.PermissionIds
            );

            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return NotFound($"Role with ID {id} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, "An error occurred while updating the role");
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteRoleCommand(id);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return NotFound($"Role with ID {id} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, "An error occurred while deleting the role");
        }
    }

    /// <summary>
    /// Get roles assigned to a user
    /// </summary>
    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetUserRoles(
        Guid userId,
        [FromQuery] Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserRolesQuery(userId, organizationId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user roles");
        }
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HttpPost("users/assign")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new AssignRoleToUserCommand(
                request.UserId,
                request.RoleId,
                request.OrganizationId,
                request.ExpiresAt
            );

            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return BadRequest("Failed to assign role to user");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", request.RoleId, request.UserId);
            return StatusCode(500, "An error occurred while assigning the role");
        }
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpPost("users/remove")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleFromUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return BadRequest("Failed to remove role from user");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", command.RoleId, command.UserId);
            return StatusCode(500, "An error occurred while removing the role");
        }
    }

    /// <summary>
    /// Get role hierarchy starting from root or specific role
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<RoleHierarchyDto>>> GetRoleHierarchy(
        [FromQuery] Guid? rootRoleId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetRoleHierarchyQuery(rootRoleId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role hierarchy");
            return StatusCode(500, "An error occurred while retrieving role hierarchy");
        }
    }

    /// <summary>
    /// Get permissions assigned to a role
    /// </summary>
    [HttpGet("{id:guid}/permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetRolePermissions(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetRolePermissionsQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for role {RoleId}", id);
            return StatusCode(500, "An error occurred while retrieving role permissions");
        }
    }

    /// <summary>
    /// Assign a permission to a role
    /// </summary>
    [HttpPost("{roleId:guid}/permissions/{permissionId:guid}")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> AssignPermissionToRole(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new AssignPermissionToRoleCommand(roleId, permissionId);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return BadRequest("Failed to assign permission to role");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission {PermissionId} to role {RoleId}", permissionId, roleId);
            return StatusCode(500, "An error occurred while assigning the permission");
        }
    }

    /// <summary>
    /// Remove a permission from a role
    /// </summary>
    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RemovePermissionFromRoleCommand(roleId, permissionId);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return BadRequest("Failed to remove permission from role");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
            return StatusCode(500, "An error occurred while removing the permission");
        }
    }
}