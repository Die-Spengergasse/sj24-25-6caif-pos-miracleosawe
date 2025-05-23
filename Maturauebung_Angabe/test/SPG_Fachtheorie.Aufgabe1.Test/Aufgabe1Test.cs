using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    //[Collection("Sequential")]
    public class Aufgabe1Test
    {
        private CourseContext GetEmptyDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;

            var db = new CourseContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        [Fact]
        public void CreateDatabaseTest()
        {
            using var db = GetEmptyDbContext();
        }

        /// <summary>
        /// Der Test AddSpeakerSuccessTest beweist, dass Sie einen Referenten (Speaker) in die Datenbank einf�gen k�nnen.
        /// Pr�fen Sie im Assert, ob ein Prim�rschl�ssel generiert wurde.
        /// </summary>
        [Fact]
        public void AddSpeakerSuccessTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();

            var Name = new Name("Miracle", "Osawe");
            var Topic = Model.Topic.SofwareEngeering;

            var Speaker = new Speaker(Name, "email@gmail.com", Topic);
            // Act
            db.Add(Speaker);
            db.SaveChanges();

            // Assert
            db.ChangeTracker.Clear();
            var UserFromDb = db.Users.First();
            Assert.True(UserFromDb.Id == Speaker.Id);
        }

        /// <summary>
        /// Der Test AddSubscriptionSuccessTest beweist, dass Sie einen Kurs samt Anmeldung (Subscription) anlegen k�nnen.
        /// Legen Sie hierf�r einen Speaker, einen Attendee und einen Kurs (Course) an. Stellen Sie im Assert sicher, dass f�r
        /// das gespeicherte Objekt vom Typ CourseSubscription einen Prim�rschlussel generiert wurde.
        /// </summary>
        [Fact]
        public void AddSubscriptionSuccessTest()
        {
            using var db = GetEmptyDbContext();

            // Arrange

            // Speaker
            var Name = new Name("Miracle", "Osawe");
            var Topic = Model.Topic.DatabaseSystems;
            var Speaker = new Speaker(Name, "user@email.com", Topic);

            // Attendee
            var AttendeeName = new Name("Teacher", "Boss");
            var Attendee = new Attendee(AttendeeName, "teacher@gmail.com", new DateTime(2004, 10, 07));

            // Course
            var Topic2 = Model.Topic.DatabaseSystems;
            var Course = new Course("String", "programmieren", "move and dance", Topic2, new DateTime(2025, 05, 23), Speaker);

            var CourseSub = new CourseSubscription(Course, Attendee, new DateTime(2003, 03, 03), "bought");
            // Act

            db.Add(CourseSub);
            db.SaveChanges();

            // Assert
            db.ChangeTracker.Clear();
            var CourseFromDb = db.CourseSubscriptions.First();
            Assert.True(CourseFromDb.Id == CourseSub.Id);
        }

        /// <summary>
        /// Der Test DiscriminatorHasCorrectTypeSuccessTest beweist, dass der OR Mapper das Feld UserType in User korrekt bef�llt.
        /// Legen Sie daf�r einen Datensatz vom Typ Speaker an und pr�fen Sie das Feld.
        /// </summary>
        [Fact]
        public void DiscriminatorHasCorrectTypeSuccessTest()
        {
            using var db = GetEmptyDbContext();
            // Arrange
            var Name = new Name("Miracle", "Osawe");
            var Topic = Model.Topic.DatabaseSystems;
            var Speaker = new Speaker(Name, "user@email.com", Topic);

            // Act
            db.Add(Speaker);
            db.SaveChanges();

            // Assert
            // db.ChangeTracker.Clear();
            // var UserTyeFromDb = db.CourseSubscriptions.First();
            // Assert.True(UserTyeFromDb.UserType = Speaker.UserType);
            
        }

    }
}