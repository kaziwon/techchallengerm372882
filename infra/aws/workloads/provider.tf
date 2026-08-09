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

provider "kubernetes" {
  host                   = data.aws_eks_cluster.main.endpoint
  cluster_ca_certificate = base64decode(data.aws_eks_cluster.main.certificate_authority[0].data)

  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = "aws"
    args = [
      "eks",
      "get-token",
      "--region",
      var.aws_region,
      "--cluster-name",
      data.terraform_remote_state.platform.outputs.eks_cluster_name,
    ]
  }
}

provider "helm" {
  kubernetes = {
    host                   = data.aws_eks_cluster.main.endpoint
    cluster_ca_certificate = base64decode(data.aws_eks_cluster.main.certificate_authority[0].data)
    exec = {
      api_version = "client.authentication.k8s.io/v1beta1"
      command     = "aws"
      args = [
        "eks",
        "get-token",
        "--region",
        var.aws_region,
        "--cluster-name",
        data.terraform_remote_state.platform.outputs.eks_cluster_name,
      ]
    }
  }
}

provider "newrelic" {}
