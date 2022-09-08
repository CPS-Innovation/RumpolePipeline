env = "dev"
stub_blob_storage_connection_string="DefaultEndpointsProtocol=https;AccountName=sadevcmsdocumentservices;AccountKey=06beksVS54Cw5YqSLpvKrJStK8yYMsSui1cPO3MT4+pnHys6sCBFqBq17ix5ZGXuL5cHxnBIslXzZsL24ZRa7g==;EndpointSuffix=core.windows.net"
fa_rumpole_gateway_identity_principal_id="0cb4da1a-7c17-4b51-9ba9-36b8819485c9"
coordinator_user_impersonation_scope_id="42b54fc3-3b35-4109-95f5-e62d23f739d8"
app_service_plan_sku = {
    size = "B3"
    tier = "Basic"
}

auth_details = {
	coordinator_valid_audience = "api://fa-rumpole-pipeline-dev-coordinator"
	coordinator_valid_scopes = "user_impersonation"
	coordinator_valid_roles = ""
	pdfgenerator_valid_audience = "api://fa-rumpole-pipeline-dev-pdf-generator"
	pdfgenerator_valid_scopes = "user_impersonation"
	pdfgenerator_valid_roles = ""
	textextractor_valid_audience = "api://fa-rumpole-pipeline-dev-text-extractor"
	textextractor_valid_scopes = "user_impersonation"
	textextractor_valid_roles = ""
}