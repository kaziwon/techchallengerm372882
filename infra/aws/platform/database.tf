resource "random_password" "database" {
  length           = 24
  special          = true
  override_special = "!#$%&*()-_=+[]{}:?"
}

resource "aws_db_subnet_group" "database" {
  name       = "${var.project_name}-database-subnets"
  subnet_ids = values(aws_subnet.database)[*].id

  tags = {
    Name = "${var.project_name}-database-subnets"
  }
}

resource "aws_security_group" "database" {
  name        = "${var.project_name}-database-sg"
  description = "Permite MySQL somente a partir do cluster EKS"
  vpc_id      = aws_vpc.main.id

  tags = {
    Name = "${var.project_name}-database-sg"
  }
}

resource "aws_vpc_security_group_ingress_rule" "database_from_eks" {
  security_group_id            = aws_security_group.database.id
  referenced_security_group_id = aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
  description                  = "MySQL a partir dos nodes EKS"
  from_port                    = 3306
  to_port                      = 3306
  ip_protocol                  = "tcp"
}

resource "aws_vpc_security_group_egress_rule" "database" {
  security_group_id = aws_security_group.database.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1"
}

resource "aws_db_instance" "database" {
  identifier = var.database_identifier

  engine         = "mysql"
  engine_version = var.database_engine_version
  instance_class = var.database_instance_class

  db_name  = var.database_name
  username = var.database_username
  password = random_password.database.result
  port     = 3306

  allocated_storage = var.database_allocated_storage
  storage_type      = "gp2"
  storage_encrypted = true

  db_subnet_group_name   = aws_db_subnet_group.database.name
  vpc_security_group_ids = [aws_security_group.database.id]
  publicly_accessible    = false
  multi_az               = false

  backup_retention_period      = 0
  delete_automated_backups     = true
  skip_final_snapshot          = true
  deletion_protection          = false
  monitoring_interval          = 0
  performance_insights_enabled = false
  auto_minor_version_upgrade   = true
  apply_immediately            = true

  depends_on = [aws_vpc_security_group_ingress_rule.database_from_eks]
}
