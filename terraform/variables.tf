variable "region" {
  default     = "eastus"
  description = "The Azure region to host this infrastructure."
  type        = string
}

// TODO: The following environment secrets should ideally be accessed through
// some Key Vault resource instead of being supplied here.

variable "plaid_client_id" {
  description = "A Plaid client ID used to identify calls to the Plaid API."
  type        = string
  sensitive   = true
}

variable "plaid_client_secret" {
  description = "A Plaid client secret used to authenticate calls to the Plaid API."
  type        = string
  sensitive   = true
}

variable "plaid_client_access_token" {
  description = "A Plaid client access token used to make Plaid API requests related to a specific Plaid Item."
  type        = string
  sensitive   = true
}

variable "twilio_account_sid" {
  description = "A string identifier (SID) used to identify a specific Twilio SMS API account resource."
  type        = string
  sensitive   = true
}

variable "twilio_authentication_token" {
  description = "An authentication token used to authenticate calls to the Twilio SMS API."
  type        = string
  sensitive   = true
}

variable "twilio_sender_phone_number" {
  description = "The phone number of the Twilio SMS sender."
  type        = string
  sensitive   = true
}

variable "twilio_recipient_phone_number" {
  description = "The phone number of the Twilio SMS recipient."
  type        = string
  sensitive   = true
}
