namespace JurisControl.Domain.Enums;

/// <summary>
/// Materias jurídicas del manual clásico (1995) más las modernas.
/// Cada despacho activa un subconjunto en <see cref="Entities.Despacho.MateriasAtiende"/>.
/// </summary>
public static class Materia
{
    public const string Civil = "civil";
    public const string Mercantil = "mercantil";
    public const string Familiar = "familiar";
    public const string Laboral = "laboral";
    public const string Penal = "penal";
    public const string Amparo = "amparo";
    public const string Administrativo = "administrativo";
    public const string Cobranza = "cobranza";
    public const string Fiscal = "fiscal";
    public const string Corporativo = "corporativo";

    public static readonly string[] All =
    {
        Civil, Mercantil, Familiar, Laboral, Penal,
        Amparo, Administrativo, Cobranza, Fiscal, Corporativo
    };

    public static string Label(string key) => key switch
    {
        Civil => "Civil",
        Mercantil => "Mercantil",
        Familiar => "Familiar",
        Laboral => "Laboral",
        Penal => "Penal",
        Amparo => "Amparo",
        Administrativo => "Administrativo",
        Cobranza => "Cobranza",
        Fiscal => "Fiscal",
        Corporativo => "Corporativo",
        _ => key
    };
}
