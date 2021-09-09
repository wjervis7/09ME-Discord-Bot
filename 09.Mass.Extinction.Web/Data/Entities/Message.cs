namespace _09.Mass.Extinction.Web.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Message
    {
        public long Id { get; set; }

        public string Sender { get; set; }

        [Display(Name = "Message")]
        public string Body { get; set; }

        [Display(Name = "Date and Time Sent")]
        [UIHint("_FriendlyDate")]
        public DateTime DateSent { get; set; }

        [Display(Name = "Anonymous Message?")]
        [UIHint("_BooleanYesNo")]
        public bool IsAnonymous { get; set; }
    }
}
