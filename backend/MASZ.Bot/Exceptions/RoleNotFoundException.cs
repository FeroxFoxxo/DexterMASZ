using MASZ.Bot.Abstractions;
using MASZ.Bot.Enums;

namespace MASZ.Bot.Exceptions;

public class RoleNotFoundException : ApiException
{
	public RoleNotFoundException(ulong roleId) : base($"Role {roleId} not found.", ApiError.RoleNotFound)
	{
		RoleId = roleId;
	}
	
	public ulong RoleId { get; set; }
}