# Contributing

This app leverages [ASP.NET Core Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-6.0) and [Tailwind CSS](https://tailwindcss.com/).

The following tools are required for development:
* [dotnet 6 sdk](https://dot.net)
* [node.js](https://nodejs.org/en/)
* [Tailwind CLI](https://tailwindcss.com/docs/installation)

The working directory is `./src/InglemoorCodingComputing/`

## Development Secrets
To add development secrets use the [dotnet secrets manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

### Email:

*Optional*

`SMTP__Username`

`SMTP__Server`

`SMTP_Port`

`SMTP_Password`

`SMTP__From` (usually the same as `SMTP__Username`)

[learn more](https://www.cloudflare.com/learning/email-security/what-is-smtp/)

[for use in development to not actually send emails](https://ethereal.email/)

### Cosmos DB

`Cosmos__Throughput` recommended="400"

`Cosmos__DatabaseName` recommended="InglemoorCodingDB"

`Cosmos__ConnStr`

`Cosmos__UserContainer` recommended="Users"

`Cosmos__AuthContainer` recommended="Authentication"

`Cosmos__MeetingsContainer` recommended="Meetings"

`Cosmos__StaticPagesContainer` recommended="Pages"

use the [Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21) in development instead of [Azure Cosmos DB](https://azure.microsoft.com/en-us/products/cosmos-db/).

### Blob Storage

`BlobStorage__ConnStr`

use [Azureite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio) in development instead of [Azure Blob Storage](https://azure.microsoft.com/en-us/products/storage/blobs/)

### Authentication

#### Password Hashing
see [Argon2Id](https://github.com/P-H-C/phc-winner-argon2)

`Argon2id__SaltLength`

`Argon2id__Parallelism`

`Argon2id__Memory` (in KB)

`Argon2id__Iterations`

`Argon2id__HashLength`


#### Google Auth
*optional*

see [setup instructions](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins)

`Authentication__Google__ClientSecret`

`Authentication__Google__ClientId`

### Admin:
*optional*

`AdminKey`: A string used to grant users the admin role.

## Starting Tailwind
In a separate console window enter the following:

Linux/Mac:
```
./tailwind.sh
```

Windows:
```
tailwind.cmd
```

This starts the Tailwind hot reload loop.

## Running
### With the CLI:
```
dotnet watch run
```

### Using Visual Studio:
Use [VS2022](https://visualstudio.microsoft.com/vs/), the sln file is inside `./src/`

