namespace InglemoorCodingComputing.Models;

public record LoginAttempt(string IPAddress, string UserAgent, bool Successful, DateTime Time, string Method);