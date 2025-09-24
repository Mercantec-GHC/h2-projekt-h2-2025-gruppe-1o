namespace DomainModels.Enums
{
    public enum TicketStatus
    {
        Open,                 // Sagen er ny og ulæst.
        PendingSupportReply,  // Sagen afventer svar fra en medarbejder.
        PendingCustomerReply, // Sagen afventer svar fra kunden.
        PendingClosure,       // En medarbejder har anmodet om at lukke sagen, afventer kundens accept.
        Closed                // Sagen er lukket af begge parter.
    }
}