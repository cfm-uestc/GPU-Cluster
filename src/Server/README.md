## Secret Management

* In development, we store secret in `user-secrets`.

```bash
cd GPUCluster.WebService
dotnet user-secrets init
dotnet user-secrets set "PrivateDockerRepoToken:Username" "user-name"
dotnet user-secrets set "PrivateDockerRepoToken:Password" "password"
dotnet user-secrets set "PrivateDockerRepoToken:Email" "email"
dotnet user-secrets set "PrivateDockerRepoToken:ServerAddress" "url"
```

Then, we use following code to restore string into the `Docker.DotNet.Models.AuthConfig` instance:

```csharp
public class Startup
{
    ...
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    ...

    AuthConfig config = Configuration.GetSection("PrivateDockerRepoToken").Get<AuthConfig>();
```

* In production, we use the secret files stored in system:, you should define a env variable to determine where file lies:

```bash
export GPUCLUSTER_DOCKER_TOKEN=/home/$USER/.secret/token.json
```