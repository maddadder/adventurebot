
# adventurebot

ToDo: Build an adventure bot in c#

### Help along the way

https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-isolated-create-first-csharp?pivots=code-editor-vscode

https://medium.com/analytics-vidhya/a-practical-guide-to-azure-durable-functions-part-3-configurations-6baa1b49f532


### Configuration explorer settings
```
App:BaseUrl=https://redacted.azurewebsites.net
AwsSes:SmtpFromEmail=youremail@gmail.com
AwsSes:SmtpHost=smtp.gmail.com
AwsSes:SmtpPort=587
AwsSes:SmtpPassword=Your Gmail app-specific password
AwsSes:SmtpToEmail=youremail@gmail.com
AwsSes:SmtpUserName=youremail@gmail.com
GitHub:BaseUrl=https://api.github.com
GitHub:Password=YourPersonalAccessToken
GitHub:Username=YourUsername
```

### local.settings.json

`local.settings.json` contains a required field: `AppConfigurationConnectionString` with the value `Endpoint=https://redacted.azconfig.io;Id=redacted;Secret=redacted"`. This value comes from `App Configuration` in azure. You will need to create this resource, populate it with the values below. The medium article from analytics-vidhya above has instructions on how to do this in part-3. Once complete, in `App Configuration` navigate to the `Access Keys` tab, copy the `Connection string`, and paste it into the `AppConfigurationConnectionString` in `local.settings.json`.

`local.settings.json` contains a required field: `CosmosDbConnectionString` with the value `AccountEndpoint=https://redacted.documents.azure.com:443/;AccountKey=redacted`. Go to the `Function App`, `Configuration` tab. Add a new setting with property `CosmosDbConnectionString` and value `@Microsoft.KeyVault(SecretUri=https://redacted.vault.azure.net/secrets/CosmosDbConnectionString)`. Create an entry in the azure vault called `CosmosDbConnectionString`


### Debugging:

Azurite: Start
