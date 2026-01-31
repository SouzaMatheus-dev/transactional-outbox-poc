#!/usr/bin/env bash
# Script para criar cluster k3d (Kubernetes dentro do Docker)
# Requer: Docker, k3d (https://k3d.io/)

CLUSTER_NAME="outbox-poc"
API_PORT=8080

k3d cluster delete "$CLUSTER_NAME" 2>/dev/null || true
k3d cluster create "$CLUSTER_NAME" \
  --agents 1 \
  --servers 1 \
  --port "${API_PORT}:80@loadbalancer" \
  --port "30443:443@loadbalancer"

echo "Cluster $CLUSTER_NAME criado. API sera exposta em http://localhost:$API_PORT apos o deploy."
