env = "qa"
env_suffix = "-qa"
stub_blob_storage_connection_string="DefaultEndpointsProtocol=https;AccountName=saqacmsdocumentservices;AccountKey=nmbdArGrAOzr2nHk1srJkzt2lURPPnFEW5pUfx/oGFlT08Ec70RC6uzdNDXOJjM/rKq5X3g/1A70Zk92HR044Q==;EndpointSuffix=core.windows.net"
fa_rumpole_gateway_identity_principal_id="5e58b752-a3f2-4b42-b615-d96ab2377eaf"
app_service_plan_sku = {
    size = "B3"
    tier = "Basic"
}
default_workspace_name = "[FILLIN]"

coordinator_details = {
    application_registration_id = "[FILLIN]"
    user_impersonation_scope_id = "[FILLIN]"
}

pdf_generator_details = {
    application_registration_id = "[FILLIN]"
    user_impersonation_scope_id = "[FILLIN]"
    application_create_role_id  = "[FILLIN]"
}

text_extractor_details = {
    application_registration_id = "[FILLIN]"
    application_text_extraction_role_id  = "[FILLIN]"
}

gateway_details = {
    application_registration_id = "[FILLIN]"
    user_impersonation_scope_id = "[FILLIN]"
}

auth_details = {
    coordinator_valid_audience = "api://fa-rumpole-pipeline-qa-coordinator"
    pdf_generator_valid_audience = "api://fa-rumpole-pipeline-qa-pdf-generator"
    text_extractor_valid_audience = "api://fa-rumpole-pipeline-qa-text-extractor"
}