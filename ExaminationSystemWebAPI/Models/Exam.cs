﻿using ExaminationSystemWebAPI.Models.Joins;

namespace ExaminationSystemWebAPI.Models;

public class Exam : BaseModel
{
    public ExamType ExamType { get; set; }
    public int MaxDuration { get; set; }
    public int TotalGrade { get; set; }
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public DateTime DeadlineDate { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public enum ExamType
{
    Quiz = 0,
    Final
};