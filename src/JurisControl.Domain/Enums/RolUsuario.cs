namespace JurisControl.Domain.Enums;

public static class RolUsuario
{
    public const string PlatformAdmin = "platform_admin";
    public const string FirmAdmin     = "firm_admin";
    public const string Lawyer        = "lawyer";
    public const string Clerk         = "clerk";
    public const string ClientPortal  = "client_portal";

    public static readonly string[] All =
    {
        PlatformAdmin, FirmAdmin, Lawyer, Clerk, ClientPortal
    };
}
