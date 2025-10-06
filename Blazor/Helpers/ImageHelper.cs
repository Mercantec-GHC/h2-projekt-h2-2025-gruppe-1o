namespace Blazor.Helpers
{
    /// <summary>
    /// Centraliserer logik for at hente stier til billeder i applikationen.
    /// </summary>
    public static class ImageHelper
    {
        private const string DefaultRoomImage = "/images/room-default.png";
        private const string DefaultMeetingRoomImage = "/images/default-image.png";

        /// <summary>
        /// Returnerer den korrekte billedsti for en given hotelværelsestype.
        /// </summary>
        /// <param name="roomTypeName">Navnet på værelsestypen.</param>
        /// <returns>En URL-sti til billedet.</returns>
        public static string GetRoomImagePath(string? roomTypeName) => roomTypeName switch
        {
            "Standard Værelse" => "/images/room-standard.png",
            "Deluxe Suite" => "/images/room-deluxe.png",
            "Presidential Suite" => "/images/room-presidential.png",
            _ => DefaultRoomImage
        };

        /// <summary>
        /// Returnerer den korrekte billedsti for et givent mødelokale.
        /// </summary>
        /// <param name="meetingRoomName">Navnet på mødelokalet.</param>
        /// <returns>En URL-sti til billedet.</returns>
        public static string GetMeetingRoomImagePath(string? meetingRoomName) => meetingRoomName switch
        {
            "Bestyrelseslokalet" => "/images/Bestyrelseslokalet.png",
            "Konferencesalen" => "/images/Konferencesalen.png",
            "Grupperum Alfa" => "/images/GrupperumAlfa.png",
            "Grupperum Beta" => "/images/GrupperumBeta.png",
            "Auditoriet" => "/images/Auditoriet.png",
            _ => DefaultMeetingRoomImage
        };
    }
}