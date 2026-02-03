# Setup

## Gather required credentials
1. Navigate to the Azure Communication Services resource.
2. Select "Keys" from the left-hand menu.
3. Copy the "Endpoint" value.

## Deploy the Azure Function App
1. [![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FLex-Agile-Solutions%2Fteams-token-exchange%2Fmain%2Fdeploy%2Fazuredeploy.json)
2. Fill in the required parameters as desired, "Acs Endpoint" should contain the endpoint from the Azure Communication Services resource.

## Grand the Azure Function App access to the Azure Communication Services resource
1. On Azure navigate to the Azure Communication Services resource.
2. Select "Access control (IAM)".
3. Click "Add role assignment".
4. Select "Contributor" role.
5. On the next screen, select "Managed identity".
6. Select the newly created Azure Function App.
7. Assign the role.

## Gather remaining credentials
1. Navigate to the Azure Function App.
2. Copy the "Default domain".
3. Select "App keys" from the left-hand menu.
4. Copy the "default" key.
5. Locate your Entra App Registration.
6. Copy the "Application (client) ID".

## Generate the QR code
1. Generate a QR code using the details from the previous steps.
2. Scan the QR code with the Settings Application on the hardware.