locals {
  name_prefix = "${var.project_name}-${var.environment}"
  tags = merge(var.tags, {
    environment = var.environment
    project     = var.project_name
  })
}

resource "azurerm_resource_group" "platform" {
  name     = "${local.name_prefix}-rg"
  location = var.location
  tags     = local.tags
}
