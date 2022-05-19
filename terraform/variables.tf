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

variable "fa_rumpole_gateway_identity_principal_id" {
  type = string
}