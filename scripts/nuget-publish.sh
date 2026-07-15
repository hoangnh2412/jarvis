#!/usr/bin/env bash
#
# Pack + push các package NuGet Jarvis.
# Token đọc từ file .env (biến NUGET_API_KEY). KHÔNG hard-code / commit token.
#
# Cách dùng:
#   ./scripts/nuget-publish.sh                     # pack + push nhóm mặc định (authentication)
#   ./scripts/nuget-publish.sh --all               # tất cả project có <PackageId>
#   ./scripts/nuget-publish.sh Jarvis.Caching Jarvis.BlobStoring
#   ./scripts/nuget-publish.sh --pack-only          # chỉ pack, không push (kiểm tra .nupkg)
#
# Chuẩn bị: cp .env.example .env  → điền NUGET_API_KEY.
#
set -euo pipefail

# ---- Repo root (script nằm trong scripts/) ----
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$ROOT_DIR"

# ---- Cấu hình (override qua biến môi trường nếu cần) ----
CONFIG="${CONFIG:-Release}"
OUT_DIR="${OUT_DIR:-$ROOT_DIR/artifacts/nuget}"
NUGET_SOURCE_DEFAULT="https://api.nuget.org/v3/index.json"

# Nhóm project mặc định khi không truyền tham số.
DEFAULT_PROJECTS=(
  Jarvis.Authentication
  Jarvis.Authentication.Jwt
  Jarvis.Authentication.ApiKey
  Jarvis.Authentication.Basic
)

# ---- Nạp .env ----
if [[ -f .env ]]; then
  set -a
  # shellcheck disable=SC1091
  source .env
  set +a
fi
NUGET_SOURCE="${NUGET_SOURCE:-$NUGET_SOURCE_DEFAULT}"

# ---- Parse tham số ----
PACK_ONLY=0
PROJECTS=()
for arg in "$@"; do
  case "$arg" in
    --pack-only|--no-push)
      PACK_ONLY=1
      ;;
    --all)
      while IFS= read -r csproj; do
        PROJECTS+=("$(basename "$(dirname "$csproj")")")
      done < <(grep -rl "<PackageId>" --include="*.csproj" . | grep -vE '/(obj|bin)/' | sort)
      ;;
    -*)
      echo "Tham số không hợp lệ: $arg" >&2
      exit 2
      ;;
    *)
      PROJECTS+=("$arg")
      ;;
  esac
done
[[ ${#PROJECTS[@]} -eq 0 ]] && PROJECTS=("${DEFAULT_PROJECTS[@]}")

# ---- Kiểm tra token trước khi push ----
if [[ $PACK_ONLY -eq 0 && -z "${NUGET_API_KEY:-}" ]]; then
  echo "ERROR: NUGET_API_KEY chưa được set." >&2
  echo "       cp .env.example .env  → điền token, hoặc: export NUGET_API_KEY=..." >&2
  exit 1
fi

# ---- Pack (dọn output cũ để chỉ push đúng package vừa tạo) ----
rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR"
echo "==> Config=$CONFIG  Output=$OUT_DIR"
echo "==> Projects: ${PROJECTS[*]}"

for proj in "${PROJECTS[@]}"; do
  csproj="$proj/$proj.csproj"
  [[ -f "$csproj" ]] || { echo "ERROR: không tìm thấy $csproj" >&2; exit 1; }
  echo "==> Pack $proj"
  # -p:GeneratePackageOnBuild=false: các csproj bật cờ này lúc build gây NU5026 khi chạy `dotnet pack`.
  dotnet pack "$csproj" -c "$CONFIG" -o "$OUT_DIR" -p:GeneratePackageOnBuild=false --nologo
done

if [[ $PACK_ONLY -eq 1 ]]; then
  echo "==> pack-only: bỏ qua push. File .nupkg tại: $OUT_DIR"
  ls -1 "$OUT_DIR"/*.nupkg
  exit 0
fi

# ---- Push ----
shopt -s nullglob
pkgs=("$OUT_DIR"/*.nupkg)
if [[ ${#pkgs[@]} -eq 0 ]]; then
  echo "ERROR: không có .nupkg nào để push." >&2
  exit 1
fi

echo "==> Push ${#pkgs[@]} package tới $NUGET_SOURCE (--skip-duplicate)"
for nupkg in "${pkgs[@]}"; do
  echo "==> Push $(basename "$nupkg")"
  dotnet nuget push "$nupkg" \
    --api-key "$NUGET_API_KEY" \
    --source "$NUGET_SOURCE" \
    --skip-duplicate
done

echo "==> Hoàn tất: đã push ${#pkgs[@]} package."
