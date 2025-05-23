using System.Dynamic;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Speaker : User
    {
        protected Speaker() {}
        public Speaker(Name name, string email, Topic preferredTopic)
        {
            Name = name;
            Email = email;
            PreferredTopic = preferredTopic;
        }
        public Topic PreferredTopic{ get; set; }
    }
}