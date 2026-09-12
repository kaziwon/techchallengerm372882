terraform {
  required_version = ">= 1.10.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }

    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.0"
    }

    helm = {
      source  = "hashicorp/helm"
      version = "~> 3.2"
    }

    newrelic = {
      source  = "newrelic/newrelic"
      version = "~> 3.95"
    }

    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}
