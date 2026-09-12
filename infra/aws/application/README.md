# Deploy da aplicacao no EKS

Esta raiz Terraform implanta somente os recursos pertencentes a aplicacao:

- namespace, ConfigMap e Secrets;
- Deployment e Service interno da API;
- Horizontal Pod Autoscaler (HPA);
- regras, consumidor e plugin JWT da aplicacao no Kong;
- monitor sintetico, alertas e dashboard da aplicacao no New Relic.

VPC e RDS pertencem ao repositorio de banco. EKS, ECR, Metrics Server, Kong e o
agente de infraestrutura do New Relic pertencem ao repositorio de Kubernetes.
Os valores necessarios sao lidos dos respectivos states remotos no S3.
