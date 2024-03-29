
# adventurebot

A workflow that sends messages for interactive choose-your-own-adventure stories. 

### Install Azure Cli
1. See link [here](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
```
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```
2. run `az login`

### Install Durable Functions Tools

1. See link [here](https://github.com/Azure/azure-functions-core-tools)
```
wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install azure-functions-core-tools-4
```

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

GraphApiApp:TenantId=your_tenant_id 
GraphApiApp:ClientId=your_webhook_client_id
GraphApiApp:ClientSecret=your_webhook_client_secret
```

### local.settings.json

`local.settings.json` contains a required field: `AppConfigurationConnectionString` with the value `Endpoint=https://redacted.azconfig.io;Id=redacted;Secret=redacted"`. This value comes from `App Configuration` in azure. You will need to create this resource, populate it with the values below. The medium article from analytics-vidhya above has instructions on how to do this in part-3. Once complete, in `App Configuration` navigate to the `Access Keys` tab, copy the `Connection string`, and paste it into the `AppConfigurationConnectionString` in `local.settings.json`.

`local.settings.json` contains a required field: `CosmosDbConnectionString` with the value `AccountEndpoint=https://redacted.documents.azure.com:443/;AccountKey=redacted`. Go to the `Function App`, `Configuration` tab. Add a new setting with property `CosmosDbConnectionString` and value `@Microsoft.KeyVault(SecretUri=https://redacted.vault.azure.net/secrets/CosmosDbConnectionString)`. Create an entry in the azure vault called `CosmosDbConnectionString`

### Register an app for the Azure Function webhook

0. Doc found here: https://github.com/microsoftgraph/msgraph-sample-azurefunction-csharp/blob/main/README.md#register-an-app-for-the-azure-function-webhook

1. Return to **App Registrations**, and select **New registration**. On the **Register an application** page, set the values as follows.

    - Set **Name** to `Graph Azure Function Webhook`.
    - Set **Supported account types** to **Accounts in this organizational directory only**.
    - Leave **Redirect URI** blank.

1. Select **Register**. On the **Graph Azure Function webhook** page, copy the value of the **Application (client) ID** and save it, you will need it in the next step.

1. Select **Certificates & secrets** under **Manage**. Select the **New client secret** button. Enter a value in **Description** and select one of the options for **Expires** and select **Add**.

1. Copy the client secret value before you leave this page. You will need it in the next step.

1. Select **API Permissions** under **Manage**. Choose **Add a permission**.

1. Select **Microsoft Graph**, then **Application Permissions**. Add **User.ReadWrite.All** then select **Add permissions**.

1. In the **Configured permissions**, remove the delegated **User.Read** permission under **Microsoft Graph** by selecting the **...** to the right of the permission and selecting **Remove permission**. Select **Yes, remove** to confirm.

1. Select the **Grant admin consent for...** button, then select **Yes** to grant admin consent for the configured application permissions. The **Status** column in the **Configured permissions** table changes to **Granted for ...**.


### Debugging Azure Functions:

You have the following options:

1. For multiple projects loaded at the same time: In VS Code: Open `main.code-workspace` then click Open Workspace. If you use this then set in local.settings.json `AzureWebJobsStorage` to `DefaultEndpointsProtocol=https;AccountName=redacted;AccountKey=redacted;EndpointSuffix=core.windows.net`. For more info see: https://github.com/microsoft/vscode-azurefunctions/issues/1121

2. Otherwise open `Read` or `ReadWrite` as separate projects. If you use separate projects then you can use `Azurite`. If you use Azurite then set in local.settings.json `AzureWebJobsStorage` to `UseDevelopmentStorage=true`. You can then start Azurite with `Azurite: Start`, however, with AzureWebJobsStorage configured with the DefaultEndpointsProtocol then you don't need Azurite at all.

3. When you build the project it should prompt you to build `Read` or `ReadWrite`. Choose whichever workspace you are in.

### Debugging Blazor App:

1. If you are using local storage: Azurite: Start

1. navigate to src/AdventureBotUI/Client and run `dotnet watch` to start the client.

### Using a Discord bot:

1. There is a discord bot over [here](https://github.com/maddadder/microk8s-discord-bot/) that can run the client side operations to advance the quest, and allows all the interaction to stay within Discord without having to navigate outside of Discord. 