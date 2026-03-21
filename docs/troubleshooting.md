# Troubleshooting

## Azure Functions deploy fails with sync trigger / malformed content

Symptoms:
- GitHub Actions deploy reaches `Azure/functions-action@v1`
- package upload succeeds
- deployment fails during `Sync Trigger Functionapp`

Recommended checks:
1. Publish the Function project with `dotnet publish`, not only `dotnet build`.
2. Deploy the publish output folder only, not the monorepo root or the project root.
3. Ensure the publish folder contains these files at the root:
   - `host.json`
   - `worker.config.json`
   - `functions.metadata`
   - `extensions.json`
   - `bin/`
4. Publish with `/p:UseAppHost=false` for a cleaner Linux consumption package.
5. When `AzureWebJobsStorage` uses managed identity, grant the Function managed identity these storage RBAC roles:
   - `Storage Blob Data Owner`
   - `Storage Queue Data Contributor`
   - `Storage Account Contributor`
   - `Storage Table Data Contributor`
6. Grant the GitHub OIDC service principal blob data access to the same storage account so deployment can publish the package.

Current repository changes:
- workflow publishes directly from `transaction-service/src/TransactionService.Api/TransactionService.Api.csproj`
- publish folder is cleaned before each deploy
- validation now checks `functions.metadata`, `extensions.json`, and `bin/`
- Terraform grants `Storage Table Data Contributor` to the Function managed identity
