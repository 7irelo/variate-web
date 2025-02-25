# Terraform infrastructure for AKS

## What it creates

- Resource Group
- Virtual Network + AKS subnet
- Azure Container Registry
- AKS cluster
- Log Analytics workspace
- AKS permission to pull from ACR (`AcrPull`)

## Usage

```bash
cd platform/infra/terraform
cp terraform.tfvars.example terraform.tfvars
terraform init
terraform plan
terraform apply
```

After apply, configure kubectl:

```bash
az aks get-credentials --resource-group <resource_group_name> --name <aks_cluster_name>
```
