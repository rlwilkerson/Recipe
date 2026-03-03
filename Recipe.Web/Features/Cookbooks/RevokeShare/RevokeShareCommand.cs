using MediatR;

namespace Recipe.Web.Features.Cookbooks.RevokeShare;

public record RevokeShareCommand(int ShareId, string RequestingUserId) : IRequest;
