namespace InglemoorCodingComputing.Services;

using Konscious.Security.Cryptography;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Security.Cryptography;

public sealed class UserAuthService : IUserAuthService
{
    private readonly IApprovedEmailsService _approvedEmailsService;
    private readonly Container _container;
    private readonly int _saltSize;
    private readonly int _hashSize;
    private readonly int _parallelism;
    private readonly int _memorySize;
    private readonly int _iterations;
    private readonly string _adminKey;

    public event EventHandler<Guid>? OnAdminRevoked;

    public UserAuthService(IConfiguration configuration, CosmosClient cosmosClient, IApprovedEmailsService approvedEmailsService)
    {
        _approvedEmailsService = approvedEmailsService;
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:AuthContainer"]);
        _saltSize = int.Parse(configuration["Argon2id:SaltLength"]);
        _hashSize = int.Parse(configuration["Argon2id:HashLength"]);
        _parallelism = int.Parse(configuration["Argon2id:Parallelism"]);
        _memorySize = int.Parse(configuration["Argon2id:Memory"]);
        _iterations = int.Parse(configuration["Argon2id:Iterations"]);
        _adminKey = configuration["AdminKey"];
    }

    public async Task<UserAuth?> UserWithEmail(string email)
    {
        var iterator = _container.GetItemLinqQueryable<UserAuth>().Where(x => x.Email == email).ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                return item;
        }
        return null;
    }

    public async Task<UserAuth?> TryReadUserAsync(Guid id)
    {
        try
        {
            return (await _container.ReadItemAsync<UserAuth>(id.ToString(), new(id.ToString()))).Resource;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> TryDeleteUserAsync(Guid id)
    {
        try
        {
            await _container.DeleteItemAsync<UserAuth>(id.ToString(), new(id.ToString()));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Authenticates user.
    /// </summary>
    /// <param name="username">student id</param>
    /// <param name="password"></param>
    /// <param name="ipAddress"></param>
    /// <param name="userAgent"></param>
    /// <returns></returns>
    public async Task<UserAuth?> AuthenticateAsync(string email, string password, string? ipAddress = null, string? userAgent = null)
    {
        if (await UserWithEmail(email) is not UserAuth user || user.Hash is null)
            return null;

        var hash = user.Hash;

        var testHash = GetHash(password, hash.Salt);

        if (!testHash.SequenceEqual(hash.Hash))
        {
            if (ipAddress is not null && userAgent is not null)
                await AddLoginAttempt(user, ipAddress, userAgent, false, "Password");
            
            return null;
        }

        if (ipAddress is not null && userAgent is not null)
            await AddLoginAttempt(user, ipAddress, userAgent, true, "Password");

        if (hash.Hash.Length != _hashSize || hash.Salt.Length != _saltSize || hash.Parallelism != _parallelism || hash.Memory != _memorySize || hash.Iterations != _iterations)
        {
            // Rehash with new parameters
            var newHash = GetHash(password, out var newSalt);
            var newUser = user with { Hash = new(newHash, newSalt, _iterations, _parallelism, _memorySize) };
            await _container.ReplaceItemAsync(newUser, user.Id.ToString(), new(user.Id.ToString()));
        }
        return user;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username">student id</param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<UserAuth?> AddUserAsync(string email, string password)
    {
        if (!(await UserWithEmail(email) is null && await _approvedEmailsService.EmailApprovedAsync(email)))
            return null;

        var hash = GetHash(password, out var salt);
        var id = Guid.NewGuid();
        UserAuth user = new(id, email, false, new(hash, salt, _iterations, _parallelism, _memorySize));
        await _container.CreateItemAsync(user, new(id.ToString()));
        return user;
    }

    public async Task<UserAuth?> AddGoogleUserAsync(string email, string googleId)
    {
        if (!(await UserWithEmail(email) is null && await _approvedEmailsService.EmailApprovedAsync(email)))
            return null;

        var id = Guid.NewGuid();
        UserAuth user = new(id, email, false, null)
        {
            GoogleId = googleId,
        };
        await _container.CreateItemAsync(user, new(id.ToString()));
        return user;
    }

    public async Task<bool> GrantAdminAsync(string email, string key)
    {
        if (string.IsNullOrEmpty(_adminKey) || key != _adminKey)
            return false;

        var iterator = _container.GetItemLinqQueryable<UserAuth>().Where(x => x.Email == email).ToFeedIterator();
        List<UserAuth> userAuths = new();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                userAuths.Add(item);
        }
        if (userAuths.Count == 0)
            return false;

        var userAuth = userAuths.First();

        var newUserAuth = userAuth with { IsAdmin = true };
        await _container.ReplaceItemAsync(newUserAuth, userAuth.Id.ToString(), partitionKey: new(userAuth.Id.ToString()));
        return true;
    }

    public async Task<bool> RevokeAdminAsync(string email, string key)
    {
        if (string.IsNullOrEmpty(_adminKey) || key != _adminKey)
            return false;

        var iterator = _container.GetItemLinqQueryable<UserAuth>().Where(x => x.Email == email).ToFeedIterator();
        List<UserAuth> userAuths = new();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                userAuths.Add(item);
        }
        if (userAuths.Count == 0)
            return false;

        var userAuth = userAuths.First();

        var newUserAuth = userAuth with { IsAdmin = false };
        await _container.ReplaceItemAsync(newUserAuth, userAuth.Id.ToString(), partitionKey: new(userAuth.Id.ToString()));
        OnAdminRevoked?.Invoke(this, userAuth.Id);
        return true;
    }

    private byte[] GetHash(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(_saltSize);
        return GetHash(password, salt);
    }

    private byte[] GetHash(string password, byte[] salt)
    {
        using Argon2id argon2 = new(Encoding.UTF8.GetBytes(password)) { DegreeOfParallelism = _parallelism, MemorySize = _memorySize, Iterations = _iterations, Salt = salt };
        return argon2.GetBytes(_hashSize);
    }

    public async IAsyncEnumerable<UserAuth> GetAdmins()
    {
        var iterator = _container.GetItemLinqQueryable<UserAuth>().Where(x => x.IsAdmin).ToFeedIterator();
        List<UserAuth> userAuths = new();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                yield return item;
        }
    }

    public bool AdminKeyValid(string key) =>
        !string.IsNullOrEmpty(_adminKey) && key == _adminKey;

    public async Task<bool> HasAdminAsync(Guid id)
    {
        var item = await _container.ReadItemAsync<UserAuth>(id.ToString(), new(id.ToString()));
        return item.Resource.IsAdmin;
    }

    public async Task ChangePasswordAsync(UserAuth userAuth, string password)
    {
        var newHash = GetHash(password, out var newSalt);
        var newUser = userAuth with { Hash = new(newHash, newSalt, _iterations, _parallelism, _memorySize) };
        await _container.ReplaceItemAsync(newUser, userAuth.Id.ToString(), new(userAuth.Id.ToString()));
    }

    public async Task<UserAuth?> AuthenticateWithGoogleIdAsync(string id, string? ipAddress = null, string? userAgent = null)
    {
        var iterator = _container.GetItemLinqQueryable<UserAuth>().Where(x => x.GoogleId == id).ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
            {
                if (ipAddress is not null && userAgent is not null)
                    await AddLoginAttempt(item, ipAddress, userAgent, true, "Google");
                return item;
            }
        }
        return null;
    }

    private Task AddLoginAttempt(UserAuth user, string ipAddress, string userAgent, bool success, string method) =>
        _container.ReplaceItemAsync(user with { LoginAttempts = user.LoginAttempts
                                                                   .Where(x => DateTime.UtcNow - x.Time <= TimeSpan.FromDays(7))
                                                                   .Append(new(ipAddress, userAgent, true, DateTime.UtcNow, method))
                                                                   .ToList() }, user.Id.ToString());
    public async Task<bool> TryUpdateSecurityStamp(Guid id)
    {
        try
        {
            var auth = await TryReadUserAsync(id);
            if (auth is null)
                return false;
            await _container.ReplaceItemAsync(auth with { SecurityTimeStamp = DateTime.UtcNow }, auth.Id.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
}