#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
BOOTSTRAP_DIR="${SCRIPT_DIR}/../bootstrap"
PROJECT_NAME="${PROJECT_NAME:-oficina-mecanica}"
AWS_REGION="${AWS_REGION:-us-east-1}"

account_id="$(aws sts get-caller-identity --query Account --output text)"
state_bucket="${PROJECT_NAME}-terraform-state-${account_id}"

terraform -chdir="${BOOTSTRAP_DIR}" init -input=false

import_resource() {
  local address="$1"

  if terraform -chdir="${BOOTSTRAP_DIR}" state show "${address}" >/dev/null 2>&1; then
    return
  fi

  terraform -chdir="${BOOTSTRAP_DIR}" import \
    -input=false \
    -var="aws_region=${AWS_REGION}" \
    "${address}" \
    "${state_bucket}" || true
}

if aws s3api head-bucket --bucket "${state_bucket}" >/dev/null 2>&1; then
  echo "Bucket de estado encontrado. Reconstruindo o estado local do bootstrap."
  import_resource aws_s3_bucket.terraform_state
  import_resource aws_s3_bucket_public_access_block.terraform_state
  import_resource aws_s3_bucket_versioning.terraform_state
  import_resource aws_s3_bucket_server_side_encryption_configuration.terraform_state
  import_resource aws_s3_bucket_policy.require_tls
else
  echo "Bucket de estado nao encontrado. O Terraform ira cria-lo."
fi

terraform -chdir="${BOOTSTRAP_DIR}" apply \
  -auto-approve \
  -input=false \
  -var="aws_region=${AWS_REGION}"

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  echo "state_bucket=${state_bucket}" >> "${GITHUB_OUTPUT}"
fi

echo "Bucket de estado pronto: ${state_bucket}"
