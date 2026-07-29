data "aws_caller_identity" "current" {}

data "aws_iam_roles" "eks_cluster" {
  name_regex = ".*-LabEksClusterRole-.*"
}

data "aws_iam_roles" "eks_node" {
  name_regex = ".*-LabEksNodeRole-.*"
}

locals {
  common_tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  public_subnets   = zipmap(var.availability_zones, var.public_subnet_cidrs)
  database_subnets = zipmap(var.availability_zones, var.database_subnet_cidrs)

  eks_cluster_role_arn = one(data.aws_iam_roles.eks_cluster.arns)
  eks_node_role_arn    = one(data.aws_iam_roles.eks_node.arns)
}

check "academy_eks_cluster_role" {
  assert {
    condition     = length(data.aws_iam_roles.eks_cluster.arns) == 1
    error_message = "A conta deve possuir exatamente uma role LabEksClusterRole."
  }
}

check "academy_eks_node_role" {
  assert {
    condition     = length(data.aws_iam_roles.eks_node.arns) == 1
    error_message = "A conta deve possuir exatamente uma role LabEksNodeRole."
  }
}
