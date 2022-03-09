locals {
  visualiser_name = "as-web-${local.resource_name}-visualiser" 
}

resource "azurerm_app_service" "as_web" {
  name                = "as-web-${local.resource_name}-visualiser"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.asp.id
  https_only          = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.ai.instrumentation_key
  }

  site_config {
    app_command_line = "npx serve -s"
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
      allowed_audiences = ["https://CPSGOVUK.onmicrosoft.com/as-web-${local.resource_name}-visualiser"]
    }
  }
}

resource "azuread_application" "as_read" {
  display_name               = "as-web-${local.resource_name}-visualiser"
  //oauth2_allow_implicit_flow = false
  identifier_uris            = ["https://CPSGOVUK.onmicrosoft.com/as-web-${local.resource_name}-visualiser"]
  owners                     = ["4acc9fb2-3e32-4109-b3d1-5fcd3a253e4e"] // Stef's admin account todo: get rid of this
  # reply_urls = [
  #   "https://as-web-${local.resource_name}-visualiser.azurewebsites.net/.auth/login/aad/callback",
  # ]
  # homepage = "https://as-web-${local.resource_name}-visualiser.azurewebsites.net"

  required_resource_access {
    resource_app_id = "00000002-0000-0000-c000-000000000000" # Azure AD Graph (deprecated!?)

    resource_access {
      id   = "311a71cc-e848-46a1-bdf8-97ff7156d8e6" # read user
      type = "Scope"
    }
  }

  # required_resource_access {
  #   resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

  #   resource_access {
  #     id   = "5f8c59db-677d-491f-a6b8-5f174b11ec1d" # read all groups (requires admin consent?)
  #     type = "Scope"
  #   }
  # }

  # required_resource_access {
  #   resource_app_id = azuread_application.fa_rumpole.application_id

  #   resource_access {
  #     id   = tolist(azuread_application.fa_rumpole.oauth2_permissions)[0].id
  #     type = "Scope"
  #   }
  # }

  # depends_on = [
  #   azuread_application.fa_rumpole
  # ]
}

resource "azuread_application_password" "as_password" {
  application_object_id = azuread_application.as_read.id
  //description           = "Default app service app password"
  end_date_relative     = "17520h"
  //value                 = "__asap_web_rumpole_app_service_password__"
}
