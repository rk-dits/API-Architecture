using MediatR;

namespace IdentityService.Application.SuperAdmin.Commands;

public record SuspendUserAccountCommand(Guid UserId, string Reason) : IRequest<bool>;

public record UnsuspendUserAccountCommand(Guid UserId) : IRequest<bool>;

public record ForceUserLogoutCommand(Guid UserId) : IRequest<bool>;

public record ResetUserPasswordCommand(Guid UserId, string NewPassword) : IRequest<bool>;

public record DeleteUserAccountCommand(Guid UserId, bool HardDelete = false) : IRequest<bool>;