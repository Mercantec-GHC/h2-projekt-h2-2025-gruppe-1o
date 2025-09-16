namespace DomainModels.Enums
{
    public enum BillingType
    {
        PerBooking,      // Engangsbeløb per booking
        PerNight,        // Pris per nat
        PerPerson,       // Pris per person (engangsbeløb)
        PerPersonPerNight // Pris per person per nat
    }
}