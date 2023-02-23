
# adventurebot

ToDo: Build an adventure bot in c#

### Help along the way

https://medium.com/analytics-vidhya/a-practical-guide-to-azure-durable-functions-part-3-configurations-6baa1b49f532

https://github.com/Azure/azure-functions-openapi-extension/blob/main/docs/openapi-core.md

https://github.com/fbeltrao/Samples/tree/master/serverless-user-email-confirmation

### Setup Database

Create an Azure Cosmos DB and upload the json file in `adventure-bot-game-entries.zip`. Use the parameters found in src/AdventureBotLib/Models/Strings.cs. When you create the database you can choose serverless which only bills when the app is in use.

### Configuration explorer settings
```
App:BaseUrl=https://yourblazorwasmhostedsite.com
AwsSes:SmtpFromEmail=youremail@gmail.com
AwsSes:SmtpHost=smtp.gmail.com
AwsSes:SmtpPort=587
AwsSes:SmtpPassword=Your Gmail app-specific password
AwsSes:SmtpToEmail=youremail@gmail.com
AwsSes:SmtpUserName=youremail@gmail.com
```

### local.settings.json

`local.settings.json` contains a required field: `AppConfigurationConnectionString` with the value `Endpoint=https://redacted.azconfig.io;Id=redacted;Secret=redacted"`. This value comes from `App Configuration` in azure. You will need to create this resource, populate it with the values below. The medium article from analytics-vidhya above has instructions on how to do this in part-3. Once complete, in `App Configuration` navigate to the `Access Keys` tab, copy the `Connection string`, and paste it into the `AppConfigurationConnectionString` in `local.settings.json`.

`local.settings.json` contains a required field: `CosmosDbConnectionString` with the value `AccountEndpoint=https://redacted.documents.azure.com:443/;AccountKey=redacted`. Go to the `Function App`, `Configuration` tab. Add a new setting with property `CosmosDbConnectionString` and value `@Microsoft.KeyVault(SecretUri=https://redacted.vault.azure.net/secrets/CosmosDbConnectionString)`. Create an entry in the azure vault called `CosmosDbConnectionString`


### Debugging Azure Functions:

You have the following options:

1. For multiple projects loaded at the same time: In VS Code: Open `main.code-workspace` then click Open Workspace. If you use this then set in local.settings.json `AzureWebJobsStorage` to `DefaultEndpointsProtocol=https;AccountName=redacted;AccountKey=redacted;EndpointSuffix=core.windows.net`. For more info see: https://github.com/microsoft/vscode-azurefunctions/issues/1121

2. Otherwise open `Read` or `ReadWrite` as separate projects. If you use separate projects then you can use `Azurite`. If you use Azurite then set in local.settings.json `AzureWebJobsStorage` to `UseDevelopmentStorage=true`

Azurite: Start

### Debugging Blazor App:

navigate to src/AdventureBotUI/Client and run dotnet watch
