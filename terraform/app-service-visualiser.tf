locals {
  app_name = "as-web-${local.resource_name}-visualiser" 
}

resource "azurerm_app_service" "as_web" {
  name                = local.app_name
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.asp.id
  https_only          = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"  = azurerm_application_insights.ai.instrumentation_key
    "REACT_APP_COORDINATOR"           = "https://${azurerm_function_app.fa_coordinator.name}.azurewebsites.net/api/cases/{caseId}?code=${data.azurerm_function_app_host_keys.ak_coordinator.default_function_key}&force={force}"
    "REACT_APP_SEARCH_INDEX"          = "https://${azurerm_search_service.ss.name}.search.windows.net/indexes/${jsondecode(file("search-index-definition.json")).name}/docs?api-version=2021-04-30-Preview&api-key=${azurerm_search_service.ss.query_keys[0].key}&search="
  }

  site_config {
    app_command_line = "node subsititute-config.js; npx serve -s"
    linux_fx_version = "NODE|14-lts"
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "RedirectToLoginPage"
    default_provider              = "AzureActiveDirectory"
    token_store_enabled = true
    active_directory {
      client_id         = azuread_application.as_read.application_id
      client_secret     = azuread_application_password.as_password.value
      allowed_audiences = ["https://CPSGOVUK.onmicrosoft.com/${local.app_name}"]
    }
  }
}

resource "azuread_application" "as_read" {
  display_name               = local.app_name

  identifier_uris            = ["https://CPSGOVUK.onmicrosoft.com/${local.app_name}"]
  owners                     = ["4acc9fb2-3e32-4109-b3d1-5fcd3a253e4e"] // Stef's admin account todo: get rid of this

  single_page_application {
    redirect_uris = [
      "https://${local.app_name}.azurewebsites.net/",
    ]
  }
  web {
    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled = true
    }

    redirect_uris = [
      "https://${local.app_name}.azurewebsites.net/.auth/login/aad/callback"
    ]

  }
  
  required_resource_access {
    resource_app_id = "00000002-0000-0000-c000-000000000000" # Azure AD Graph (deprecated!?)

    resource_access {
      id   = "311a71cc-e848-46a1-bdf8-97ff7156d8e6" # read user
      type = "Scope"
    }
  }
}

resource "azuread_application_password" "as_password" {
  application_object_id = azuread_application.as_read.id
  end_date_relative     = "17520h"
}
