#!/usr/bin/env sh
set -eu
docker compose up --build -d
printf '\nDevOpsHub: http://localhost:3000\nDemo: admin@devopshub.local / Admin123!\n'
