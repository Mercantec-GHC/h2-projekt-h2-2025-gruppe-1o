using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace API.Services;

/// <summary>
/// En service til at spore og begrænse login-forsøg for at forhindre brute-force angreb.
/// </summary>
public class LoginAttemptService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<LoginAttemptService> _logger;

    // Konfiguration for rate limiting
    private const int MaxAttempts = 5; // Maksimalt antal forsøg før lockout
    private const int LockoutMinutes = 15; // Lockout periode i minutter
    private const int DelayIncrementSeconds = 2; // Sekunder at tilføje per mislykket forsøg

    public LoginAttemptService(IMemoryCache cache, ILogger<LoginAttemptService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Tjekker om en given email-adresse er midlertidigt låst.
    /// </summary>
    public bool IsLockedOut(string email)
    {
        var attemptInfo = _cache.Get<LoginAttemptInfo>(email);
        return attemptInfo != null && attemptInfo.LockoutUntil > DateTime.UtcNow;
    }

    /// <summary>
    /// Registrerer et mislykket login-forsøg. Hvis max forsøg er nået, låses kontoen.
    /// </summary>
    /// <returns>Antal sekunders forsinkelse, der skal pålægges anmodningen.</returns>
    public int RecordFailedAttempt(string email)
    {
        var attemptInfo = _cache.Get<LoginAttemptInfo>(email) ?? new LoginAttemptInfo();

        attemptInfo.FailedAttempts++;
        attemptInfo.LastAttempt = DateTime.UtcNow;

        if (attemptInfo.FailedAttempts >= MaxAttempts)
        {
            attemptInfo.LockoutUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            _logger.LogWarning("Konto for email {Email} er nu låst i {LockoutMinutes} minutter.", email, LockoutMinutes);
        }

        _cache.Set(email, attemptInfo, TimeSpan.FromMinutes(LockoutMinutes + 1)); // Cache lidt længere end lockout

        var delay = (attemptInfo.FailedAttempts - 1) * DelayIncrementSeconds;
        return delay;
    }

    /// <summary>
    /// Nulstiller tracking af login-forsøg for en email ved et succesfuldt login.
    /// </summary>
    public void RecordSuccessfulLogin(string email)
    {
        _cache.Remove(email);
        _logger.LogInformation("Succesfuldt login for {Email}. Login-forsøg tæller er nulstillet.", email);
    }

    /// <summary>
    /// Henter det resterende antal sekunder, en konto er låst.
    /// </summary>
    public int GetRemainingLockoutSeconds(string email)
    {
        var attemptInfo = _cache.Get<LoginAttemptInfo>(email);
        if (attemptInfo?.LockoutUntil > DateTime.UtcNow)
        {
            return (int)(attemptInfo.LockoutUntil.Value - DateTime.UtcNow).TotalSeconds;
        }
        return 0;
    }

    /// <summary>
    /// Henter detaljeret information om login-forsøg for en given email.
    /// </summary>
    public LoginAttemptInfo? GetLoginAttemptInfo(string email)
    {
        return _cache.Get<LoginAttemptInfo>(email);
    }
}

/// <summary>
/// Holder styr på login-forsøgs-tilstanden for en bruger.
/// </summary>
public class LoginAttemptInfo
{
    public int FailedAttempts { get; set; } = 0;
    public DateTime LastAttempt { get; set; } = DateTime.UtcNow;
    public DateTime? LockoutUntil { get; set; }
}