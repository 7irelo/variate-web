output "resource_group_name" {
  value = azurerm_resource_group.platform.name
}

output "aks_cluster_name" {
  value = azurerm_kubernetes_cluster.platform.name
}

output "aks_kube_config" {
  value     = azurerm_kubernetes_cluster.platform.kube_config_raw
  sensitive = true
}

output "acr_name" {
  value = azurerm_container_registry.platform.name
}

output "acr_login_server" {
  value = azurerm_container_registry.platform.login_server
}
