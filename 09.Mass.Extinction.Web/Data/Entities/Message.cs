namespace _09.Mass.Extinction.Web.Data.Entities
{
    using System;

    public class Message
    {
        public long Id { get; set; }
        public string Sender { get; set; }
        public string Body { get; set; }
        public DateTime DateSent { get; set; }
        public bool IsAnonymous { get; set; }
    }
}
