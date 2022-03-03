terraform {

  required_version = ">=0.12"
  
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "~>2.0"
    }

    restapi = {
      source = "Mastercard/restapi"
      version = "1.16.1"
    }
  }

    backend "azurerm" {
    storage_account_name = "cpsdevstorageterraform"
    container_name       = "terraform-rumpole-pipeline"
    key                  = "terraform.tfstate"
    access_key           = "zcFXNfxGnrupbk7HAimR5ElrrnlkZBcUYzxSd2008cXMlBJRQNx6NW1U2rEJFJ9dYtAbjucQmNOaNd1A9odNOQ=="
  }
}

provider "azurerm" {
  features {}
}

locals {
  resource_name = "${var.env != "prod" ? "${var.resource_name_prefix}-${var.env}" : var.resource_name_prefix}"  
}

data "azurerm_client_config" "current" {}

# data "azuread_service_principal" "terraform_service_principal" {
#   application_id = "__terraform_service_principal_app_id__"
# }

# data "azurerm_subscription" "current" {}
