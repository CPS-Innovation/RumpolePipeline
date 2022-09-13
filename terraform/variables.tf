#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "rumpole-pipeline"
}

variable "env" {
  type = string 
}

variable "env_suffix" {
  type = string
}

variable "app_service_plan_sku" {
  type = object({
    tier = string
    size = string
  })
}

variable "default_workspace_name" {
  type = string	
}

variable "stub_blob_storage_connection_string" {
  type = string
}

variable "gateway_details" {
  type = object({
    application_registration_id = string
    user_impersonation_scope_id = string
  })
}

variable "coordinator_details" {
  type = object({
    application_registration_id = string
    user_impersonation_scope_id = string
  })
}

variable "pdf_generator_details" {
  type = object({
    application_registration_id = string
    user_impersonation_scope_id = string
    application_create_role_id = string
  })
}

variable "text_extractor_details" {
  type = object({
    application_registration_id = string
    application_text_extraction_role_id = string
  })
}

# TODO get rid of this as it will change every time gateway is rebuilt
variable "fa_rumpole_gateway_identity_principal_id" {
  type = string
}

variable "auth_details" {
  type = object({
    coordinator_valid_audience = string
    coordinator_valid_scopes = string
	coordinator_valid_roles = string
    pdf_generator_valid_audience = string
    pdf_generator_valid_scopes = string
	pdf_generator_valid_roles = string
    text_extractor_valid_audience = string
    text_extractor_valid_scopes = string
	text_extractor_valid_roles = string
  })
}
