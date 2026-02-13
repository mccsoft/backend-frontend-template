#!/bin/bash

# --- Color definitions ---
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[1;36m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# --- Usage check ---
if [ -z "$1" ]; then
  echo -e "${RED}Error:${NC} Environment name not provided."
  echo "Usage: $0 <environment>"
  echo "Example: $0 dev"
  exit 1
fi

ENVIRONMENT="$1"

# --- Load environment variables ---
ENV_FILE="./stages/${ENVIRONMENT}.env"
if [ ! -f "$ENV_FILE" ]; then
  echo -e "${RED}Error:${NC} Environment file not found: $ENV_FILE"
  exit 1
fi

echo -e "${BLUE}Loading environment variables from:${NC} $ENV_FILE"
# Export all variables defined in the .env file
set -a
source "$ENV_FILE"
set +a

# --- Debug: show loaded environment variables ---
echo -e "${YELLOW}Loaded environment variables:${NC}"
env | grep -E "environment|namespace|hostname"

# --- Define directories ---
SRC_DIR="$PWD"
DST_DIR="$PWD/deploy_${ENVIRONMENT}"

# --- Create destination directory ---
mkdir -p "$DST_DIR"

# --- Process YAML files ---
echo -e "${YELLOW}Processing YAML templates...${NC}"

find "$SRC_DIR" -maxdepth 1 -type f \( -name "*.yaml" -o -name "*.yml" \) | while read -r file; do
  rel_path="${file#$SRC_DIR/}"
  out_file="$DST_DIR/$rel_path"
  mkdir -p "$(dirname "$out_file")"

  echo -e "${GREEN}Processing:${NC} $file → $out_file"
  envsubst < "$file" > "$out_file"
done

echo -e "${BLUE}All done!${NC} Processed templates are in: ${YELLOW}$DST_DIR${NC}"

echo "Setting namespace to: ${namespace}"
echo "##vso[task.setvariable variable=K8S_NAMESPACE]${namespace}"
