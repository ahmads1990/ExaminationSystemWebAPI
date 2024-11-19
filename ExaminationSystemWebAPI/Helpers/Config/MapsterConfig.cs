using Mapster;

namespace ExaminationSystemWebAPI.Helpers.Config;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // for nested mapping
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
    }
}
