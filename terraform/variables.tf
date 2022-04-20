variable "region" {
  default     = "eastus"
  description = "The Azure region to host this infrastructure."
  type        = string
}

# TODO: See TODO[1] in main.tf.
# variable "timezone" {
#   default     = "America/New_York"
#   description = "The tz database timezone that the Azure Function App should use."
#   type        = string
# }

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
  description = "A string identifier (SID) used to identify a Twilio API account."
  type        = string
  sensitive   = true
}

variable "twilio_api_key_sid" {
  description = "A string identifier (SID) used to identify a Twilio API key."
  type        = string
  sensitive   = true
}

variable "twilio_api_key_secret" {
  description = "A secret associated a Twilio API key used to authenticate calls to its API."
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
