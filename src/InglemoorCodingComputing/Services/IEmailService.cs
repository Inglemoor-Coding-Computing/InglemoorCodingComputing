namespace InglemoorCodingComputing.Services;

public interface IEmailService
{
    void Send(string to, string subject, string text);
}
