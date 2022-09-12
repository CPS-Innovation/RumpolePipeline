env = "dev"
env_suffix = "-dev"
stub_blob_storage_connection_string="DefaultEndpointsProtocol=https;AccountName=sadevcmsdocumentservices;AccountKey=06beksVS54Cw5YqSLpvKrJStK8yYMsSui1cPO3MT4+pnHys6sCBFqBq17ix5ZGXuL5cHxnBIslXzZsL24ZRa7g==;EndpointSuffix=core.windows.net"
fa_rumpole_gateway_identity_principal_id="0cb4da1a-7c17-4b51-9ba9-36b8819485c9"
coordinator_user_impersonation_scope_id="42b54fc3-3b35-4109-95f5-e62d23f739d8"
app_service_plan_sku = {
    size = "B3"
    tier = "Basic"
}

pdf_generator_details = {
	application_registration_id = "6cda2834-224f-4578-9b4f-2792102411c9"
	user_impersonation_scope_id = "cd5af403-f1a8-43fa-8baf-ff64efcbb085"
	application_create_role_id  = "1d9e799a-c0ce-4266-a5c8-a82514774689"
}

text_extractor_details = {
	application_registration_id = "6525d99f-9d0e-4293-bfb2-5e24a1295bf0"
	application_text_extraction_role_id  = "0ee34100-d0d0-4ce0-97d0-a654f1758155"
}

gateway_details = {
	application_registration_id = "514b76b3-c2ce-44d1-add3-f905a1e810be"
	user_impersonation_scope_id = "d8e5cd36-8100-75d1-090e-eebe50d7fff7"
}

auth_details = {
	coordinator_valid_audience = "api://fa-rumpole-pipeline-dev-coordinator"
	coordinator_valid_scopes = "user_impersonation"
	coordinator_valid_roles = ""
	pdf_generator_valid_audience = "api://fa-rumpole-pipeline-dev-pdf-generator"
	pdf_generator_valid_scopes = "user_impersonation"
	pdf_generator_valid_roles = "application.create"
	text_extractor_valid_audience = "api://fa-rumpole-pipeline-dev-text-extractor"
	text_extractor_valid_scopes = ""
	text_extractor_valid_roles = "application.extracttext"
}
