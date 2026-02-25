locals {
  acr_name = substr(lower(replace("${var.project_name}${var.environment}acr", "-", "")), 0, 50)
}

resource "azurerm_container_registry" "platform" {
  name                = local.acr_name
  resource_group_name = azurerm_resource_group.platform.name
  location            = azurerm_resource_group.platform.location
  sku                 = var.acr_sku
  admin_enabled       = false
  tags                = local.tags
}
