using AuthService.Contracts.Dtos.ReconfirmEmail;
using Core.Abstractions;

namespace AuthService.Application.Features.ReconfirmEmail;

public record ReconfirmEmailCommand(ReconfirmEmailRequest Request) : ICommand;