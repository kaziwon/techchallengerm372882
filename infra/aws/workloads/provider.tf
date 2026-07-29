provider "aws" {
  region = var.aws_region
}

data "terraform_remote_state" "platform" {
  backend = "s3"

  config = {
    bucket  = var.state_bucket_name
    key     = "oficina-mecanica/platform/terraform.tfstate"
    region  = var.aws_region
    encrypt = true
  }
}

data "aws_eks_cluster" "main" {
  name = data.terraform_remote_state.platform.outputs.eks_cluster_name
}

data "aws_eks_cluster_auth" "main" {
  name = data.terraform_remote_state.platform.outputs.eks_cluster_name
}

provider "kubernetes" {
  host                   = data.aws_eks_cluster.main.endpoint
  cluster_ca_certificate = base64decode(data.aws_eks_cluster.main.certificate_authority[0].data)
  token                  = data.aws_eks_cluster_auth.main.token
}
