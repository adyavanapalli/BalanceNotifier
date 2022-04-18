terraform {
  cloud {
    organization = "adyavanapalli"

    workspaces {
      name = "BalanceNotifier"
    }
  }

  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
    }
  }
}

provider "azurerm" {
  features {}

  # TODO: See TODO[0].
  # storage_use_azuread = true
}

locals {
  common_resource_suffix = "BalanceNotifier-${var.region}"
}

resource "azurerm_resource_group" "resource_group" {
  location = var.region
  name     = "rg-${local.common_resource_suffix}"
}

#tfsec:ignore:azure-storage-queue-services-logging-enabled
resource "azurerm_storage_account" "storage_account" {
  #checkov:skip=CKV_AZURE_33: Queue services are unused.
  #checkov:skip=CKV_AZURE_35: This will be addressed in a later TODO.
  #checkov:skip=CKV_AZURE_43: Storage account name DOES indeed adhere to naming rules.
  #checkov:skip=CKV2_AZURE_1: Customer Managed Keys are unneeded for this storage account.
  #checkov:skip=CKV2_AZURE_18: Customer Managed Keys are unneeded for this storage account.

  account_tier             = "Standard"
  account_replication_type = "LRS"
  location                 = azurerm_resource_group.resource_group.location
  name                     = "st${lower(replace(local.common_resource_suffix, "-", ""))}"
  resource_group_name      = azurerm_resource_group.resource_group.name

  allow_nested_items_to_be_public = false
  enable_https_traffic_only       = true
  min_tls_version                 = "TLS1_2"

  # TODO: Omitting setting specific network rules just yet as allowing specific
  # IPs seems to bypass the uploading restriction fine, but syncing triggers
  # fails. We need to fix this by likely allow Azure services to access the
  # storage account, which presumably should already be allowed 🤔.
  # network_rules {
  #   bypass         = ["AzureServices"]
  #   default_action = "Deny"
  # }
  # Funny, because you would think a default allow would allow Microsoft
  # services too...
  #tfsec:ignore:azure-storage-default-action-deny
  #tfsec:ignore:azure-storage-allow-microsoft-service-bypass
  network_rules {
    default_action = "Allow"
  }

  # TODO[0]: We typically should enable Azure RBAC with storage accounts, but
  # there is currently a bug in Azure Functions Core Tools that prevents
  # function app deployments with managed identities due to
  # `AzureWebJobsDashboard__accountName` not being a valid prefix key for
  # `AzureWebJobsDashboard`. Until that bug is fixed, we'll have to stick to
  # using access keys for blob access from function apps.
  # shared_access_key_enabled = false
}

resource "azurerm_service_plan" "service_plan" {
  location            = var.region
  name                = "plan-${local.common_resource_suffix}"
  os_type             = "Linux"
  resource_group_name = azurerm_resource_group.resource_group.name
  sku_name            = "Y1"
}

# TODO: We should figure out if this resource should be eventually removed after
# the application is working correctly. It might be useful to have it forever,
# but time will tell...
resource "azurerm_application_insights" "application_insights" {
  application_type    = "web"
  location            = azurerm_resource_group.resource_group.location
  name                = "appi-${local.common_resource_suffix}"
  resource_group_name = azurerm_resource_group.resource_group.name
}

resource "azurerm_linux_function_app" "linux_function_app" {
  location            = azurerm_resource_group.resource_group.location
  name                = "func-${local.common_resource_suffix}"
  resource_group_name = azurerm_resource_group.resource_group.name
  service_plan_id     = azurerm_service_plan.service_plan.id
  site_config {
    application_insights_key = azurerm_application_insights.application_insights.instrumentation_key
    application_stack {
      dotnet_version = "6.0"
    }
    use_32_bit_worker = false
  }
  storage_account_name = azurerm_storage_account.storage_account.name

  app_settings = {
    # TODO[1]: According to [Timer trigger for Azure
    # Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp#ncrontab-time-zones):
    # "WEBSITE_TIME_ZONE is not currently supported on the Linux Consumption
    # plan.", so we'll comment this out and its corresponding declaration until
    # it's supported on the Linux Consumption plan.
    # WEBSITE_TIME_ZONE = var.timezone
  }
  identity {
    type = "SystemAssigned"
  }
  # TODO: See TODO[0].
  # storage_uses_managed_identity = true
  storage_account_access_key = azurerm_storage_account.storage_account.primary_access_key
}

# TODO: See TODO[0].
# resource "azurerm_role_assignment" "role_assignment" {
#   principal_id = azurerm_linux_function_app.linux_function_app.identity[0].principal_id
#   scope        = azurerm_storage_account.storage_account.id

#   role_definition_name = "Storage Blob Data Owner"
# }

data "azurerm_subscription" "subscription" {}

resource "azurerm_key_vault" "key_vault" {
  location            = azurerm_resource_group.resource_group.location
  name                = "kv-${lower(replace(local.common_resource_suffix, "-", ""))}"
  resource_group_name = azurerm_resource_group.resource_group.name
  sku_name            = "standard"
  tenant_id           = data.azurerm_subscription.subscription.tenant_id

  enable_rbac_authorization = true
  network_acls {
    bypass         = "AzureServices"
    default_action = "Deny"
  }
}

resource "azurerm_key_vault_secret" "key_vault_secrets" {
  for_each = {
    PLAID_CLIENT_ID           = var.plaid_client_id
    PLAID_CLIENT_SECRET       = var.plaid_client_secret
    PLAID_CLIENT_ACCESS_TOKEN = var.plaid_client_access_token

    TWILIO_ACCOUNT_SID            = var.twilio_account_sid
    TWILIO_AUTHENTICATION_TOKEN   = var.twilio_authentication_token
    TWILIO_SENDER_PHONE_NUMBER    = var.twilio_sender_phone_number
    TWILIO_RECIPIENT_PHONE_NUMBER = var.twilio_recipient_phone_number
  }

  content_type = "text/plain"
  key_vault_id = azurerm_key_vault.key_vault.id
  name         = each.key
  value        = each.value
}

resource "azurerm_role_assignment" "role_assignment" {
  principal_id = azurerm_linux_function_app.linux_function_app.identity[0].principal_id
  scope        = azurerm_key_vault.key_vault.id

  role_definition_name = "Key Vault Secrets User"
}
