using MediatR;

namespace Recipe.AdminApi.Features.Users.SetUserAccess;

public record SetUserAccessCommand(
    string UserId,
    bool EnableAccess) : IRequest<SetUserAccessResponse>;
