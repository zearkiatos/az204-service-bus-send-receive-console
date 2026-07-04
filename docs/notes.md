# Send and receive messages from Azure Service Bus

## Step 1: Create resource group

```sh
$ az group create --name myResourceGroup --location eastus
```

## Step 2: Declare variables

```sh
# Create variables
resourceGroup=myResourceGroup
location=eastus
namespaceName=svcbusns$RANDOM

# Print namespace
$ echo $namespaceName
```

## Step 3: Create service bus namespace

```sh
$ az servicebus namespace create \
    --resource-group $resourceGroup \
    --name $namespaceName \
    --location $location
```

## Step 4: Create queue

```sh
$ az servicebus queue create --resource-group $resourceGroup \
    --namespace-name $namespaceName \
    --name myqueue
```

## Step 5: Declare variable for user principal account

```sh
userPrincipal=$(az rest --method GET --url https://graph.microsoft.com/v1.0/me \
    --headers 'Content-Type=application/json' \
    --query userPrincipalName --output tsv)
```

## Step 6: Declare resourceID variable

```sh
resourceID=$(az servicebus namespace show --name $namespaceName \
    --resource-group $resourceGroup \
    --query id --output tsv)
```

## Step 7: Create an assign user Azure Service Bus Data Owner

```sh
$ az role assignment create --assignee $userPrincipal \
    --role "Azure Service Bus Data Owner" \
    --scope $resourceID
```