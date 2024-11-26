namespace ExaminationSystemWebAPI.Helpers;

public static class CustomClaimTypes
{
    public static readonly string ISADMIN = "isAdmin";
    public static readonly string ISINSTRUCTOR = "isInstructor";
    public static readonly string ISSTUDENT = "isStudent";
    public static readonly List<string> ALLOWEDTYPES = new List<string> { ISADMIN, ISINSTRUCTOR, ISSTUDENT };
}
