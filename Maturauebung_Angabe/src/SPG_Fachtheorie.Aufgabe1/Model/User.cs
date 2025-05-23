using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public abstract class User
    {
        public int Id { get; set; }

        public Name Name { get; set; } = null!;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string UserType { get; set; } = null!;

        protected User() { }

        public User(Name name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}