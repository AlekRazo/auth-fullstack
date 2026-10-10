namespace AuthApi.DTOs
{
    public record RegisterRequest(string name, string email, string password, int idRole);
    public record LoginRequest(string email, string password);

    public record GetMenuResponse(string name, string link);
    public record GetUserResponse(string name, string email, string role, IEnumerable<GetMenuResponse> menus);
}
