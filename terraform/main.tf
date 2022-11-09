terraform {

  required_version = ">=1.0.0"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.1.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.15.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "~> 3.1.0"
    }

    /*restapi = {
      source = "Mastercard/restapi"
      version = "1.16.1"
    }*/
  }

  /*
  backend "azurerm" {
    storage_account_name = "__terraform_storage_account__"
    container_name       = "__terraform_container_name__"
    key                  = "__terraform_key__"
    access_key           = "__storage_key__"
  }
  */

  backend "azurerm" {
    resource_group_name  = "rg-terraform"
    storage_account_name = "cpsdevstorageterraform"
    container_name       = "terraform-rumpole-pipeline"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      recover_soft_deleted_key_vaults = true
      purge_soft_delete_on_destroy    = true
    }    
  }
}

locals {
  resource_name = "${var.env != "prod" ? "${var.resource_name_prefix}-${var.pipeline_name_prefix}-${var.env}" : "${var.resource_name_prefix}-${var.pipeline_name_prefix}"}"
  gateway_resource_name = "${var.env != "prod" ? "${var.resource_name_prefix}-${var.env}" : var.resource_name_prefix}"
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  application_id = "__terraform_service_principal_app_id__"
}

data "azurerm_subscription" "current" {}

resource "random_uuid" "random_id" {
  count = 4
}
