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

  # TODO[2]: See TODO[0].
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

  # TODO[5]: Omitting setting specific network rules just yet as allowing
  # specific IPs seems to bypass the uploading restriction fine, but syncing
  # triggers fails. We need to fix this by likely allow Azure services to access
  # the storage account, which presumably should already be allowed 🤔.
  # network_rules {
  #   bypass         = ["AzureServices"]
  #   default_action = "Deny"
  # }
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

// TODO[4]: We should figure out if this resource should be eventually removed
// after the application is working correctly. It might be useful to have it
// forever, but time will tell...
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

  # TODO[1]: See TODO[0].
  # identity {
  #   type = "SystemAssigned"
  # }
  # storage_uses_managed_identity = true
  storage_account_access_key = azurerm_storage_account.storage_account.primary_access_key
}

# TODO[3]: See TODO[0].
# resource "azurerm_role_assignment" "role_assignment" {
#   principal_id = azurerm_linux_function_app.linux_function_app.identity[0].principal_id
#   scope        = azurerm_storage_account.storage_account.id

#   role_definition_name = "Storage Blob Data Owner"
# }
