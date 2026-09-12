#!/usr/bin/env bash

set -euo pipefail

PROJECT_NAME="${PROJECT_NAME:-oficina-mecanica}"
AWS_REGION="${AWS_REGION:-us-east-1}"

account_id="$(aws sts get-caller-identity --query Account --output text)"
state_bucket="${PROJECT_NAME}-terraform-state-${account_id}"

if aws s3api head-bucket --bucket "${state_bucket}" >/dev/null 2>&1; then
  echo "Bucket de estado encontrado. Reutilizando ${state_bucket}."
else
  echo "Bucket de estado nao encontrado. Criando ${state_bucket}."

  if [[ "${AWS_REGION}" == "us-east-1" ]]; then
    aws s3api create-bucket \
      --bucket "${state_bucket}" \
      --region "${AWS_REGION}" >/dev/null
  else
    aws s3api create-bucket \
      --bucket "${state_bucket}" \
      --region "${AWS_REGION}" \
      --create-bucket-configuration "LocationConstraint=${AWS_REGION}" >/dev/null
  fi

  aws s3api wait bucket-exists --bucket "${state_bucket}"
fi

aws s3api put-public-access-block \
  --bucket "${state_bucket}" \
  --public-access-block-configuration \
  'BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true'

aws s3api put-bucket-versioning \
  --bucket "${state_bucket}" \
  --versioning-configuration 'Status=Enabled'

aws s3api put-bucket-encryption \
  --bucket "${state_bucket}" \
  --server-side-encryption-configuration \
  '{"Rules":[{"ApplyServerSideEncryptionByDefault":{"SSEAlgorithm":"AES256"}}]}'

bucket_policy="$(printf '{"Version":"2012-10-17","Statement":[{"Sid":"DenyInsecureTransport","Effect":"Deny","Principal":"*","Action":"s3:*","Resource":["arn:aws:s3:::%s","arn:aws:s3:::%s/*"],"Condition":{"Bool":{"aws:SecureTransport":"false"}}}]}' "${state_bucket}" "${state_bucket}")"

aws s3api put-bucket-policy \
  --bucket "${state_bucket}" \
  --policy "${bucket_policy}"

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  echo "state_bucket=${state_bucket}" >> "${GITHUB_OUTPUT}"
fi

echo "Bucket de estado pronto: ${state_bucket}"
