# Ansible automation

## Prerequisites

- `ansible`
- `az` CLI authenticated to your Azure subscription
- `docker`
- `kubectl`

## Variables

Update [group_vars/all.yml](./group_vars/all.yml) for:

- `acr_name` / `acr_login_server`
- `aks_resource_group` / `aks_cluster_name`
- `image_tag`

## Build and push images

```bash
ansible-playbook playbooks/build-and-push.yml
```

## Deploy to AKS

```bash
ansible-playbook playbooks/deploy-aks.yml
```
