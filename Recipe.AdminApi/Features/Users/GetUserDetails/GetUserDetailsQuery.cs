using MediatR;

namespace Recipe.AdminApi.Features.Users.GetUserDetails;

public record GetUserDetailsQuery(string UserId) : IRequest<GetUserDetailsResponse>;
