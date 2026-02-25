variable "project_name" {
  description = "Project name used for Azure resources."
  type        = string
  default     = "variate"
}

variable "environment" {
  description = "Deployment environment name."
  type        = string
  default     = "prod"
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "eastus2"
}

variable "aks_node_count" {
  description = "Initial AKS node count."
  type        = number
  default     = 3
}

variable "aks_node_vm_size" {
  description = "AKS node VM SKU."
  type        = string
  default     = "Standard_D4s_v5"
}

variable "acr_sku" {
  description = "Azure Container Registry SKU."
  type        = string
  default     = "Standard"
}

variable "vnet_cidr" {
  description = "CIDR block for platform VNet."
  type        = string
  default     = "10.40.0.0/16"
}

variable "aks_subnet_cidr" {
  description = "CIDR block for AKS subnet."
  type        = string
  default     = "10.40.1.0/24"
}

variable "tags" {
  description = "Common resource tags."
  type        = map(string)
  default = {
    managed_by = "terraform"
    workload   = "variate-ecommerce"
  }
}
