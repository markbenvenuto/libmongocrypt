#!/bin/sh
# Compiles libmongocrypt dependencies and targets.
# 
# Assumes the current working directory contains libmongocrypt.
# So script should be called like: ./libmongocrypt/.evergreen/compile.sh
# The current working directory should be empty aside from 'libmongocrypt'
# since this script creates new directories/files (e.g. mongo-c-driver, venv).
#

set -o xtrace
set -o errexit

echo "Begin compile process"

evergreen_root="$(pwd)"

. ${evergreen_root}/libmongocrypt/.evergreen/setup-venv.sh

cd $evergreen_root
mkdir -p ${evergreen_root}/install

# Build and install libbson.
git clone git@github.com:mongodb/mongo-c-driver.git --config core.eol=lf,core.autocrlf=input
cd mongo-c-driver

find . -name "*.sh" -exec dos2unix {} \;

# Use C driver helper script to find cmake binary, stored in $CMAKE.
if [ "$OS" == "Windows_NT" ]; then
CMAKE=/cygdrive/c/cmake/bin/cmake
else
chmod u+x ./.evergreen/find-cmake.sh
. ./.evergreen/find-cmake.sh
fi
#$CMAKE --version
python ./build/calc_release_version.py > VERSION_CURRENT
python ./build/calc_release_version.py -p > VERSION_RELEASED
mkdir cmake-build && cd cmake-build
# To statically link when using a shared library, compile shared library with -fPIC: https://stackoverflow.com/a/8810996/774658
$CMAKE -DENABLE_MONGOC=OFF -DCMAKE_BUILD_TYPE=Debug -DENABLE_EXTRA_ALIGNMENT=OFF -DCMAKE_C_FLAGS="-fPIC" -DCMAKE_INSTALL_PREFIX=${evergreen_root}/install/mongo-c-driver ../
echo "Installing libbson"
#TODO - set j to a reasonable level
$CMAKE --build . --target install
cd $evergreen_root

# Build and install kms-message.
git clone --depth=1 git@github.com:10gen/kms-message.git && cd kms-message
mkdir cmake-build && cd cmake-build
$CMAKE -DCMAKE_BUILD_TYPE=Debug -DCMAKE_C_FLAGS="-fPIC" -DCMAKE_INSTALL_PREFIX=${evergreen_root}/install/kms-message ../
echo "Installing kms-message"
$CMAKE --build . --target install
cd $evergreen_root

# Build and install libmongocrypt.
cd libmongocrypt
mkdir cmake-build && cd cmake-build
$CMAKE -DCMAKE_BUILD_TYPE=Debug -DCMAKE_PREFIX_PATH="${evergreen_root}/install/mongo-c-driver;${evergreen_root}/install/kms-message" -DCMAKE_INSTALL_PREFIX=${evergreen_root}/install/libmongocrypt ../
echo "Installing libmongocrypt"
$CMAKE --build .
$CMAKE --build . --target test-mongocrypt
cd $evergreen_root
