﻿namespace ExaminationSystem.Application.DTOs.Exams;

public class AddExamDto
{
    public ExamType ExamType { get; set; }
    public int MaxDuration { get; set; }
    public int TotalGrade { get; set; }
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public DateTime DeadlineDate { get; set; }

    public int CourseID { get; set; }
}

