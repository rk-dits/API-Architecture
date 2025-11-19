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
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all permissions with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PageResult<PermissionDto>>> GetPermissions(
        [FromQuery] string? category = null,
        [FromQuery] Guid? parentPermissionId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPermissionsQuery(category, parentPermissionId, pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            return StatusCode(500, "An error occurred while retrieving permissions");
        }
    }

    /// <summary>
    /// Get a specific permission by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PermissionDto>> GetPermission(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPermissionQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound($"Permission with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while retrieving the permission");
        }
    }

    /// <summary>
    /// Create a new permission
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<ActionResult<Guid>> CreatePermission([FromBody] CreatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CreatePermissionCommand(
                request.Name,
                request.Description,
                request.Category,
                request.ParentPermissionId,
                request.Conditions
            );

            var permissionId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetPermission), new { id = permissionId }, permissionId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission {PermissionName}", request.Name);
            return StatusCode(500, "An error occurred while creating the permission");
        }
    }

    /// <summary>
    /// Update an existing permission
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] UpdatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdatePermissionCommand(
                id,
                request.Name,
                request.Description,
                request.Category,
                request.ParentPermissionId,
                request.Conditions
            );

            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return NotFound($"Permission with ID {id} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while updating the permission");
        }
    }

    /// <summary>
    /// Delete a permission
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> DeletePermission(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeletePermissionCommand(id);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return NotFound($"Permission with ID {id} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
            return StatusCode(500, "An error occurred while deleting the permission");
        }
    }

    /// <summary>
    /// Get permissions assigned to a user
    /// </summary>
    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetUserPermissions(
        Guid userId,
        [FromQuery] Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserPermissionsQuery(userId, organizationId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user permissions");
        }
    }

    /// <summary>
    /// Assign a permission to a user
    /// </summary>
    [HttpPost("users/assign")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new AssignPermissionToUserCommand(
                request.UserId,
                request.PermissionId,
                request.OrganizationId,
                request.ExpiresAt
            );

            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return BadRequest("Failed to assign permission to user");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission {PermissionId} to user {UserId}", request.PermissionId, request.UserId);
            return StatusCode(500, "An error occurred while assigning the permission");
        }
    }

    /// <summary>
    /// Remove a permission from a user
    /// </summary>
    [HttpPost("users/remove")]
    [Authorize(Roles = "SuperAdmin,PlatformAdmin,OrganizationAdmin")]
    public async Task<IActionResult> RemovePermission([FromBody] RemovePermissionFromUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
                return BadRequest("Failed to remove permission from user");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission {PermissionId} from user {UserId}", command.PermissionId, command.UserId);
            return StatusCode(500, "An error occurred while removing the permission");
        }
    }

    /// <summary>
    /// Get permission hierarchy starting from root or specific permission
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<PermissionHierarchyDto>>> GetPermissionHierarchy(
        [FromQuery] Guid? rootPermissionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPermissionHierarchyQuery(rootPermissionId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission hierarchy");
            return StatusCode(500, "An error occurred while retrieving permission hierarchy");
        }
    }

    /// <summary>
    /// Search permissions by name or description
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PageResult<PermissionDto>>> SearchPermissions(
        [FromQuery] string searchTerm,
        [FromQuery] string? category = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var query = new SearchPermissionsQuery(searchTerm, category, pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching permissions with term {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching permissions");
        }
    }

    /// <summary>
    /// Check if a user has a specific permission
    /// </summary>
    [HttpGet("users/{userId:guid}/check/{permission}")]
    public async Task<ActionResult<bool>> CheckUserPermission(
        Guid userId,
        string permission,
        [FromQuery] Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new CheckUserPermissionQuery(userId, permission, organizationId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return StatusCode(500, "An error occurred while checking user permission");
        }
    }
}