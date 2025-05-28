using System;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Name
    {
        public Name(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
        public String FirstName { get; set; }
        public String LastName { get; set; }
    }
}