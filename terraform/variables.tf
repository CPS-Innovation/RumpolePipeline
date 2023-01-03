#################### Variables ####################

variable "resource_name_prefix" {
  type = string
  default = "polaris-pipeline"
}

variable "ddei_resource_name_prefix" {
  type = string
  default = "polaris-ddei"
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

variable "ddei_config" {
  type = object({
    base_url = string
  })
}

variable "queue_config" {
  type = object({
    update_search_index_by_version_queue_name = string
    update_search_index_by_blob_name_queue_name = string
  })
}
