using ExaminationSystemWebAPI.Helpers.Enums;

namespace ExaminationSystemWebAPI.ViewModels.Questions;

public class UpdateQuestionLevelViewModel
{
    public string ID { get; set; } = string.Empty;
    public QuestionLevel QuestionLevel { get; set; }
}
