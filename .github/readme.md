# GitHub Repo Setup
## Steps
  1. Generate a new SSH Key Pair
  1. Add a Deploy Key called `COMMIT_KEY_PUB` containing the public part of the SSH key (do give write access)
  1. Add a Secret called `COMMIT_KEY` containing the private part of the SSH key
  1. Add a Secret called `READ_REPO_PACKAGES` containing the *packages:read* central PAT Token
  1. Change every reference to DemoLibrary! **NB:** *Especially the "repo url" in the csproj file(s) :')*

## Justification
### VersionPrefix
Currently the csproj must have `<VersionPrefix>` set. Initially, this can be 0.0.1, or 1.0.0, etc. It serves as the working pre-release. 
Pushing a tag of a stable release version (e.g. v1.2.3) causes the package to be pushed with the tag version. The `<VersionPrefix>` is then incremented in the patch - in this case 1.2.4.  Pushing pre-release version tags (e.g. v1.2.3-pre.02) is done in CI. A package is pushed, but no changes are made to the source csproj. Manually pushing pre-release tags (or indeed creating in GitHub via Releases) is not well-supported. 

### Deploy Keys
In order for the `tag-ci` pipeline's "tag push" to trigger the `publish` pipeline, the default GITHUB_TOKEN cannot be used. (Workflows are prevented from triggering other ones using this default token).

Using a PAT (personal access token) to achieve the above is not appropriate in organisational settings, as it requires a dedicated user and its scope is not limited to just the repository thus presenting an unnecessary security concern...

... Say hello to Deploy Keys! Using these, one can easily trigger other workflows from a workflow. To use them, follow these steps:
1. Generate an ed25519 ssh key (anywhere you like!):
    ```bash
    ssh-keygen -t ed25519 -f id_ed25519 -N "" -q -C ""
    cat id_ed25519.pub id_ed25519
    ```
1. In GitHub repo, add a new Deploy Key (in Settings), providing just the **public** key. Name it `COMMIT_KEY_PUB`, for example
1. In GitHub repo, add a new repository secret (Settings), providing the **private** key. Name it `COMMIT_KEY`, for example
1. In your workflow, just make sure you check out using the ssh-key (this overrides the default GITHUB_TOKEN):
    ```yml    
    - uses: actions/checkout@v3
      with:
        ssh-key: "${{ secrets.COMMIT_KEY }}"
    ```
1. And now, any branch/tag pushes, etc will not be ignored in otherwise-passing workflow filters

### Read Repo Packages
In order to pull packages from other private repos (in this account), a limited PAT token is used.
This PAT token is defined centrally and has just package-read access (to all repos). This has already been defined, but is:

`Account > Settings > Developer Settings > Personal Access Tokens`
- add one with read:packages

Any repo that references packages from github-based nuget feeds will need auth to read packages. The PAT can be added as a secret in each repo requiring such access. Add a secret called `READ_REPO_PACKAGES` and pop the PAT in it :)

Additionally, to re-use the PAT for local workstation development (to pull in the packages in Visual Studio NuGet Package Manager, for example) add the following nuget config:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    ...
    <add key="ne1410s" value="https://nuget.pkg.github.com/ne1410s/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <ne1410s>
      <add key="Username" value="ne1410s" />
      <add key="ClearTextPassword" value="YOUR_PAT_HERE" />
    </ne1410s>
  </packageSourceCredentials>
</configuration>
```