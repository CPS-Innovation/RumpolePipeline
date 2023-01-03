env = "qa"
app_service_plan_sku = {
    size = "B3"
    tier = "Basic"
}
ddei_config = {
    base_url = "https://fa-${local.ddei_resource_name}.azurewebsites.net/api/"
}
queue_config = {
    update_search_index_by_version_queue_name = "update-search-index-by-version"
    update_search_index_by_blob_name_queue_name = "update-search-index-by-blob-name"
}