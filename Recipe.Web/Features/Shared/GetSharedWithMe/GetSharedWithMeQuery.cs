using MediatR;

namespace Recipe.Web.Features.Shared.GetSharedWithMe;

public record GetSharedWithMeQuery(string UserId) : IRequest<SharedWithMeResponse>;
