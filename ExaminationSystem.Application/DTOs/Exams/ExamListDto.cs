﻿namespace ExaminationSystem.Application.DTOs.Exams;

public class ExamListDto
{
    public ExamType ExamType { get; set; }
    public string Title { get; set; } = string.Empty;
    public int MaxDuration { get; set; }
    public int TotalGrade { get; set; }
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public DateTime DeadlineDate { get; set; }

    public int CourseID { get; set; }
    public string Course { get; set; }
}

