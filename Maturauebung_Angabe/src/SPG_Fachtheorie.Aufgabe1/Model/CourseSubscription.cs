using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class CourseSubscription
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected CourseSubscription() {}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public int Id { get; set; }
        public Course Course { get; set; } 
        public Attendee Attendee { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public String? Note { get; set; }
        public CourseSubscription(Course course, Attendee attendee, DateTime subscriptionDate, string? note)
        {
            Course = course;
            Attendee = attendee;
            SubscriptionDate = subscriptionDate;
            Note = note;
        }

    }
}