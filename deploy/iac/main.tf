# Sample Terraform for Acme API Platform (Azure)

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "acme_rg" {
  name     = "acme-api-rg"
  location = "East US"
}

resource "azurerm_postgresql_flexible_server" "acme_pg" {
  name                = "acme-postgres"
  resource_group_name = azurerm_resource_group.acme_rg.name
  location            = azurerm_resource_group.acme_rg.location
  administrator_login          = "pgadmin"
  administrator_password       = "ChangeMe123!"
  sku_name            = "B_Standard_B1ms"
  storage_mb          = 32768
  version             = "13"
  zone                = "1"
}

resource "azurerm_redis_cache" "acme_redis" {
  name                = "acme-redis"
  location            = azurerm_resource_group.acme_rg.location
  resource_group_name = azurerm_resource_group.acme_rg.name
  capacity            = 1
  family              = "C"
  sku_name            = "Basic"
}

resource "azurerm_container_registry" "acme_acr" {
  name                = "acmeacr"
  resource_group_name = azurerm_resource_group.acme_rg.name
  location            = azurerm_resource_group.acme_rg.location
  sku                 = "Basic"
  admin_enabled       = true
}
