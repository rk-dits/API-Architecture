using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using IdentityService.Application.Organizations.Commands;
using IdentityService.Application.Organizations.Queries;
using IdentityService.Contracts.Organizations;

namespace IdentityService.API.Controllers;

/// <summary>
/// Organizations management controller for multi-tenant operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrganizationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(IMediator mediator, ILogger<OrganizationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new organization
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CanCreateOrganizations")]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
    {
        try
        {
            var command = new CreateOrganizationCommand(
                request.Name,
                request.Description,
                request.Website,
                request.ContactEmail,
                request.ContactPhone,
                request.ParentOrganizationId,
                request.Type
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetOrganization), new { id = result.Value!.Id }, result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewOrganizations")]
    public async Task<IActionResult> GetOrganization(Guid id, [FromQuery] bool includeMembers = false, [FromQuery] bool includeSettings = false)
    {
        try
        {
            var query = new GetOrganizationByIdQuery(id, includeMembers, includeSettings);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Get detailed organization information
    /// </summary>
    [HttpGet("{id:guid}/details")]
    [Authorize(Policy = "CanViewOrganizationDetails")]
    public async Task<IActionResult> GetOrganizationDetails(Guid id)
    {
        try
        {
            var query = new GetOrganizationDetailsQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization details {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Search and list organizations
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanListOrganizations")]
    public async Task<IActionResult> GetOrganizations([FromQuery] OrganizationSearchRequest request)
    {
        try
        {
            var query = new GetOrganizationsQuery(
                request.SearchTerm,
                request.Status,
                request.Type,
                null, // ParentOrganizationId
                request.Page,
                request.PageSize
            );

            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations");
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Update organization
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanUpdateOrganizations")]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] UpdateOrganizationRequest request)
    {
        try
        {
            var command = new UpdateOrganizationCommand(
                id,
                request.Name,
                request.Description,
                request.Logo,
                request.Website,
                request.ContactEmail,
                request.ContactPhone
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Delete organization
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDeleteOrganizations")]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        try
        {
            var command = new DeleteOrganizationCommand(id);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok("Organization deleted successfully");
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Get organization members
    /// </summary>
    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = "CanViewOrganizationMembers")]
    public async Task<IActionResult> GetOrganizationMembers(
        Guid id,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetOrganizationMembersQuery(id, searchTerm, role, isActive, page, pageSize);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for organization {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Invite user to organization
    /// </summary>
    [HttpPost("{id:guid}/invitations")]
    [Authorize(Policy = "CanInviteUsers")]
    public async Task<IActionResult> InviteUser(Guid id, [FromBody] OrganizationInviteRequest request)
    {
        try
        {
            var command = new InviteUserToOrganizationCommand(id, request.Email, request.Role, request.Message);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok("User invitation sent successfully");
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting user to organization {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Remove user from organization
    /// </summary>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = "CanManageOrganizationMembers")]
    public async Task<IActionResult> RemoveUserFromOrganization(Guid id, Guid userId)
    {
        try
        {
            var command = new RemoveUserFromOrganizationCommand(id, userId);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok("User removed from organization successfully");
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from organization {OrganizationId}", userId, id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Update user's role in organization
    /// </summary>
    [HttpPut("{id:guid}/members/{userId:guid}/role")]
    [Authorize(Policy = "CanManageOrganizationRoles")]
    public async Task<IActionResult> UpdateUserRole(Guid id, Guid userId, [FromBody] UpdateUserOrganizationRoleRequest request)
    {
        try
        {
            var command = new UpdateUserOrganizationRoleCommand(id, userId, request.Role);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok("User role updated successfully");
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for user {UserId} in organization {OrganizationId}", userId, id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Get organization settings
    /// </summary>
    [HttpGet("{id:guid}/settings")]
    [Authorize(Policy = "CanViewOrganizationSettings")]
    public async Task<IActionResult> GetOrganizationSettings(Guid id)
    {
        try
        {
            var query = new GetOrganizationSettingsQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings for organization {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Update organization settings
    /// </summary>
    [HttpPut("{id:guid}/settings")]
    [Authorize(Policy = "CanUpdateOrganizationSettings")]
    public async Task<IActionResult> UpdateOrganizationSettings(Guid id, [FromBody] UpdateOrganizationSettingsRequest request)
    {
        try
        {
            var command = new UpdateOrganizationSettingsCommand(
                id,
                request.RequireMfa,
                request.AllowSso,
                request.AllowGuestUsers,
                request.MaxUsers,
                request.SessionTimeoutMinutes,
                request.AllowedDomains,
                request.CustomSettings
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok("Organization settings updated successfully");
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings for organization {OrganizationId}", id);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Check organization name availability
    /// </summary>
    [HttpGet("check-name")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckNameAvailability([FromQuery] string name, [FromQuery] Guid? excludeId = null)
    {
        try
        {
            var query = new CheckOrganizationNameAvailabilityQuery(name, excludeId);
            var result = await _mediator.Send(query);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking name availability for: {Name}", name);
            return StatusCode(500, "An internal error occurred");
        }
    }

    /// <summary>
    /// Accept organization invitation
    /// </summary>
    [HttpPost("invitations/accept")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInviteRequest request)
    {
        try
        {
            var command = new AcceptOrganizationInviteCommand(
                request.InviteToken,
                request.FirstName,
                request.LastName,
                request.Password
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok("Invitation accepted successfully");
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation with token: {Token}", request.InviteToken);
            return StatusCode(500, "An internal error occurred");
        }
    }
}