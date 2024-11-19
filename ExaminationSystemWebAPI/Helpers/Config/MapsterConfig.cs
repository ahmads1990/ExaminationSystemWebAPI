using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.ViewModels.Choice;
using Mapster;

namespace ExaminationSystemWebAPI.Helpers.Config;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // for nested mapping
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

        // Choices
        //config
        //    .NewConfig<Choice, ChoiceViewModel>()
        //    .Map(dest => dest.ChoiceOrder, src => src.ChoiceOrder.ToString());
    }
}
