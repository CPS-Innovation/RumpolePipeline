#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "rumpole-pipeline"
}

variable "env" {
  type = string 
}

variable "app_service_plan_sku" {
  type = object({
    tier = string
    size = string
  })
}

variable "stub_blob_storage_connection_string" {
  type = string
}

variable "coordinator_user_impersonation_scope_id" {
  type = string
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
    pdfgenerator_valid_audience = string
    pdfgenerator_valid_scopes = string
	pdfgenerator_valid_roles = string
    textextractor_valid_audience = string
    textextractor_valid_scopes = string
	textextractor_valid_roles = string
  })
}