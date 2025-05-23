using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPG_Fachtheorie.Aufgabe1.Infrastructure
{

    public class CourseContext : DbContext
    {
        // TODO: Füge deine DbSets hinzu
        public DbSet<Attendee> Attendees => Set<Attendee>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseSubscription> CourseSubscriptions => Set<CourseSubscription>();
        public DbSet<Speaker> Speakers => Set<Speaker>();
        public DbSet<User> Users => Set<User>();
        
        
        public CourseContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasDiscriminator(u => u.UserType);
        }

    }
}