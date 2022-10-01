namespace InglemoorCodingComputing.Services;

/// <summary>
/// Stores and retrieves approved emails and domains.
/// </summary>
public interface IApprovedEmailsService
{
    /// <summary>
    /// Adds an email or email pattern to the approved list.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task ApproveEmailAsync(EmailApproval email);

    /// <summary>
    /// Removes an email or email pattern to the approved list.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task UnapproveEmailAsync(EmailApproval email);

    /// <summary>
    /// Returns whether an email has been approved.
    /// </summary>
    /// <param name="email">Email to check.</param>
    /// <returns></returns>
    Task<bool> EmailApprovedAsync(string email);

    /// <summary>
    /// Gets all approved emails
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<EmailApproval> ApprovedEmailsAsync();
}
