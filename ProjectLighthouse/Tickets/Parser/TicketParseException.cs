using System;

namespace LBPUnion.ProjectLighthouse.Tickets.Parser;

public class TicketParseException : Exception
{
    public TicketParseException(string message)
    {
        this.Message = message;
    }

    public override string Message { get; }
}