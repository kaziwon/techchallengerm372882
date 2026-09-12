terraform {
  backend "s3" {
    key          = "oficina-mecanica/application/terraform.tfstate"
    region       = "us-east-1"
    encrypt      = true
    use_lockfile = true
  }
}
