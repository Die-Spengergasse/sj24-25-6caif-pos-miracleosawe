using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Course
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Course() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Course(string courseId, string name, string description, Topic topic, DateTime begin, Speaker speaker)
        {
            CourseId = courseId;
            Name = name;
            Description = description;
            Topic = topic;
            Begin = begin;
            Speaker = speaker;
        }
        [Key]
        public String CourseId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public Topic Topic { get; set; }
        public DateTime Begin { get; set; }
        public Speaker Speaker { get; set; }
    }
}