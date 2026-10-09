#!/bin/bash
# =============================================================================
# Automated Shared Library Compiler for GCC Engine
# Usage: ./build_shared.sh
# =============================================================================

# Stop execution if any single compilation block fails
set -e

SOURCE_FILE="DimacsParser.cpp"
OUTPUT_DIR="."

echo "--- Initializing Native Hardware Build Optimization Toolchain ---"

if [ ! -f "$SOURCE_FILE" ]; then
    echo "[Build Error]: Source file '$SOURCE_FILE' could not be located in the current directory."
    exit 1
fi

# Detect OS environment and match compiler target profiles
OS_TYPE="$(uname -s 2>/dev/null || echo "Windows")"

case "$OS_TYPE" in
    Darwin*)
        OUTPUT_LIB="$OUTPUT_DIR/libverification.dylib"
        echo "[Target Platform]: macOS Detected -> Compiling .dylib"
        g++ -O3 -shared -fPIC -std=c++11 "$SOURCE_FILE" -o "$OUTPUT_LIB"
        ;;
    Linux*)
        OUTPUT_LIB="$OUTPUT_DIR/libverification.so"
        echo "[Target Platform]: Linux Detected -> Compiling .so"
        g++ -O3 -shared -fPIC -std=std=c++11 "$SOURCE_FILE" -o "$OUTPUT_LIB"
        ;;
    *)
        OUTPUT_LIB="$OUTPUT_DIR/libverification.dll"
        echo "[Target Platform]: Windows Environment -> Compiling .dll"
        # Compile using standard MinGW toolchain
        g++ -O3 -shared -std=c++11 "$SOURCE_FILE" -o "$OUTPUT_LIB"
        ;;
esac

if [ -f "$OUTPUT_LIB" ]; then
    echo "[Success]: Shared library successfully compiled at: $OUTPUT_LIB"
    echo "--- Build Pipeline Disarmed safely ---"
else
    echo "[Build Error]: Library target generation check failed."
    exit 1
fi
