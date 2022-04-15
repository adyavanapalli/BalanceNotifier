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
