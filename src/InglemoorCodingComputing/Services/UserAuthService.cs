namespace InglemoorCodingComputing.Services;

using Konscious.Security.Cryptography;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Security.Cryptography;

public sealed class UserAuthService : IUserAuthService
{
    private readonly Container _container;
    private readonly int _saltSize;
    private readonly int _hashSize;
    private readonly int _parallelism;
    private readonly int _memorySize;
    private readonly int _iterations;
    private readonly string _adminKey;

    public event EventHandler<Guid>? OnAdminRevoked;

    public UserAuthService(IConfiguration configuration, CosmosClient cosmosClient)
    {
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

    /// <summary>
    /// Authenticates user.
    /// </summary>
    /// <param name="username">student id</param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<UserAuth?> AuthenticateAsync(string email, string password)
    {
        if (await UserWithEmail(email) is not UserAuth user)
            return null;

        var hash = user.Hash;

        var testHash = GetHash(password, hash.Salt);

        if (!testHash.SequenceEqual(hash.Hash))
            return null;
        
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
        if (await UserWithEmail(email) is not null)
            return null;

        var hash = GetHash(password, out var salt);
        var id = Guid.NewGuid();
        UserAuth user = new(id, email, false, new(hash, salt, _iterations, _parallelism, _memorySize), null);
        await _container.CreateItemAsync(user, new(id.ToString()));
        return user;
    }

    public async Task<UserAuth?> AddGoogleUserAsync(string email, string googleId)
    {
        if (await UserWithEmail(email) is not null)
            return null;

        var id = Guid.NewGuid();
        UserAuth user = new(id, email, false, null, null)
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

    public async Task<UserAuth?> UserWithGoogleIdAsync(string id)
    {
        var iterator = _container.GetItemLinqQueryable<UserAuth>().Where(x => x.GoogleId == id).ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                return item;
        }
        return null;
    }
}