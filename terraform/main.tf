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
}

locals {
  common_resource_suffix = "BalanceNotifier-${var.region}"
}

resource "azurerm_resource_group" "resource_group" {
  location = var.region
  name     = "rg-${local.common_resource_suffix}"
}

resource "azurerm_storage_account" "storage_account" {
  access_tier              = "Standard"
  account_replication_type = "LRS"
  location                 = var.region
  name                     = "st${lower(replace(local.common_resource_suffix, "-", ""))}"
  resource_group_name      = azurerm_resource_group.resource_group.name

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
}
