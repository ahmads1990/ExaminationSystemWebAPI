﻿namespace ExaminationSystem.Domain.Entities;

public class Instructor : BaseModel
{
    public int AppUserId { get; set; }
    public required AppUser AppUser { get; set; }
}
