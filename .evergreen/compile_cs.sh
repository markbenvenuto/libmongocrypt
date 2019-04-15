# Compiles libmongocrypt dependencies and targets.
# 
# Set extra cflags for libmongocrypt variables by setting LIBMONGOCRYPT_EXTRA_CFLAGS.
#

set -o xtrace
set -o errexit

echo "Begin compile process"

evergreen_root="$(pwd)"

. ${evergreen_root}/libmongocrypt/.evergreen/setup-env.sh

. ${evergreen_root}/libmongocrypt/.evergreen/setup-venv.sh

cd $evergreen_root

dotnet_tool=$(which dotnet)

$dotnet_tool build libmongocrypt/cmake-build/lang/cs/cs.sln

$dotnet_tool test libmongocrypt/cmake-build/lang/cs/MongoDB.Crypt.Test/MongoDB.Crypt.Test.csproj
