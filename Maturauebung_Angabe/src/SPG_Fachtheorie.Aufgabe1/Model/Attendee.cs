using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Attendee : User
    {
        public Attendee() {}
        public Attendee(Name name, string email, DateTime dateOfBirth)
        {
            Name = name;
            Email = email;
            DateOfBirth = dateOfBirth;
        }
        
        public DateTime DateOfBirth { get; set; }
    }
}