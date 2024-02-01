cmake_minimum_required(VERSION 3.15)

project(Croupier CXX C)

# Find latest version at https://github.com/OrfeasZ/ZHMModSDK/releases
# Set ZHMMODSDK_DIR variable to a local directory to use a local copy of the ZHMModSDK.
set(CMAKE_MODULE_PATH ${PROJECT_SOURCE_DIR}/cmake)
set(CMAKE_PREFIX_PATH ${CMAKE_PREFIX_PATH};${PROJECT_SOURCE_DIR}/deps)
set(ZHMMODSDK_VER "v2.0.0-rc.3")
include(cmake/setup-zhmmodsdk.cmake)

# Set C++ standard to C++23.
set(CMAKE_CXX_STANDARD 23)

add_compile_definitions(UNAC_VERSION="1.0.7")

# Create the Croupier mod library.
add_library(Croupier SHARED
	src/Croupier.cpp
	src/Croupier.h
	src/json.hpp
	"src/Roulette.cpp" "src/util.h" "src/EventSystem.h" "src/Events.h" "src/Enums.h" "src/RouletteRuleset.h" "src/RouletteMission.h" "src/CroupierClient.h" "src/CroupierClient.cpp" "src/SpinParser.h" "src/SpinParser.cpp" "src/RouletteMission.cpp" "src/Disguise.h" "src/Target.h" "src/Exception.h" "src/unac.h" "src/unac.c" "deps/iconv.h")

add_library(unac STATIC)
target_sources(unac PRIVATE "src/unac.h" "src/unac.c")
find_library(ICONV_LIB_PATH iconv)
find_package(directx-headers CONFIG REQUIRED)

target_link_libraries(Croupier
	ZHMModSDK
	${ICONV_LIB_PATH}
	Microsoft::DirectX-Guids
	Microsoft::DirectX-Headers
)

install(TARGETS Croupier
	RUNTIME DESTINATION bin
)

# Install the mod to the game folder when the `GAME_INSTALL_PATH` variable is set.
zhmmodsdk_install(Croupier)
