namespace Ninth.Mass.Extinction.Web.ViewModels.Discord;

public class AdminMessage
{
    public long Id { get; set; }

    public string Sender { get; set; }

    public string Body { get; set; }

    public long DateSent { get; set; }

    public bool IsAnonymous { get; set; }
}
