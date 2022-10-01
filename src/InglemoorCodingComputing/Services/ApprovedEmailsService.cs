namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public class ApprovedEmailsService : IApprovedEmailsService
{
    private readonly Container _container;

    public ApprovedEmailsService(CosmosClient cosmosClient, IConfiguration configuration)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:ApprovedEmailsContainer"]);
    }

    public Task ApproveEmailAsync(EmailApproval email) =>
        _container.CreateItemAsync(email);

    public async IAsyncEnumerable<EmailApproval> ApprovedEmailsAsync()
    {
        var iterator = _container.GetItemLinqQueryable<EmailApproval>().ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                yield return item;
        }
    }

    public Task<bool> EmailApprovedAsync(string email)
    {
        var domain = email.Split('@')[1];
        return ApprovedEmailsAsync().AnyAsync(x => x.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) || x.Email.Equals(domain)).AsTask();
    }

    public Task UnapproveEmailAsync(EmailApproval email) =>
        _container.DeleteItemAsync<EmailApproval>(email.Id.ToString(), new(email.Id.ToString()));
}
