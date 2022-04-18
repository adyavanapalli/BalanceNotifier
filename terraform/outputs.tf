output "linux_function_app_name" {
  description = "The name of the Azure Function App."
  value       = azurerm_linux_function_app.linux_function_app.name
}

output "resource_group_name" {
  description = "The name of the resource group the Azure Function App resides in."
  value       = azurerm_linux_function_app.linux_function_app.resource_group_name
}
