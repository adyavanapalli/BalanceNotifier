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
  storage_use_azuread = true
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
  location                 = var.region
  name                     = "st${lower(replace(local.common_resource_suffix, "-", ""))}"
  resource_group_name      = azurerm_resource_group.resource_group.name

  enable_https_traffic_only = true
  min_tls_version           = "TLS1_2"
  network_rules {
    bypass         = ["AzureServices"]
    default_action = "Deny"
  }
  shared_access_key_enabled = false
}

resource "azurerm_service_plan" "service_plan" {
  location            = var.region
  name                = "plan-${local.common_resource_suffix}"
  os_type             = "Linux"
  resource_group_name = azurerm_resource_group.resource_group.name
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "linux_function_app" {
  location            = var.region
  name                = "func-${local.common_resource_suffix}"
  resource_group_name = azurerm_resource_group.resource_group.name
  service_plan_id     = azurerm_service_plan.service_plan.id
  site_config {}
  storage_account_name = azurerm_storage_account.storage_account.name

  identity {
    type = "SystemAssigned"
  }
}

data "azurerm_role_definition" "role_definition" {
  name = "Storage Blob Data Owner"
}

resource "azurerm_role_assignment" "role_assignment" {
  principal_id = azurerm_linux_function_app.linux_function_app.identity.0.principal_id
  scope        = azurerm_storage_account.storage_account.id

  role_definition_id = data.azurerm_role_definition.role_definition.id
}
