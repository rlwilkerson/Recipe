using MediatR;

namespace Recipe.AdminApi.Features.Users.SetAdminRole;

public record SetAdminRoleCommand(
    string UserId,
    bool AssignRole) : IRequest<SetAdminRoleResponse>;
