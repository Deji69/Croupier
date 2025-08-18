cmake_minimum_required(VERSION 3.15)

project(Croupier CXX C)

# Find latest version at https://github.com/OrfeasZ/ZHMModSDK/releases
# Set ZHMMODSDK_DIR variable to a local directory to use a local copy of the ZHMModSDK.
set(CMAKE_MODULE_PATH ${PROJECT_SOURCE_DIR}/cmake)
set(CMAKE_PREFIX_PATH ${CMAKE_PREFIX_PATH};${PROJECT_SOURCE_DIR}/deps)
set(ZHMMODSDK_VER "v3.1.1")
include(cmake/setup-zhmmodsdk.cmake)

# Set C++ standard to C++23.
set(CMAKE_CXX_STANDARD 23)

add_compile_definitions(UNAC_VERSION="1.0.7")

# Create the Croupier mod library.
add_library(Croupier SHARED
	src/Croupier.cpp
	src/Croupier.h
	src/json.hpp
	"src/unac.h" "src/unac.c"
	"src/Roulette.cpp" "src/util.h" "src/EventSystem.h" "src/Events.h" "src/Enums.h" "src/RouletteRuleset.h" "src/RouletteMission.h" "src/CroupierClient.h"
	"src/CroupierClient.cpp" "src/SpinParser.h" "src/SpinParser.cpp" "src/RouletteMission.cpp" "src/Disguise.h"
	"src/Target.h" "src/Exception.h"
	"deps/iconv.h" "src/KillConfirmation.h" "src/KillMethod.h"   
	"src/ProcessUtils.cpp" "src/ProcessUtils.h"
	"src/ZHMUtils.h" "src/ZHMUtils.cpp"
	"src/Debug.h"
	"src/UI.h"
	"src/UI.cpp"
	"src/Config.h" "src/Config.cpp" "src/State.h"
	"src/App.h"
	"src/App.cpp"
	"src/State.cpp"
 "src/Bingo.h")

add_library(unac STATIC)
target_sources(unac PRIVATE "src/unac.h" "src/unac.c")
find_library(ICONV_LIB_PATH iconv)
include(FetchContent)
FetchContent_Declare(
	directx-headers
	GIT_REPOSITORY https://github.com/microsoft/DirectX-Headers.git
	GIT_TAG        9be295b3b81ce1d0ff2b44f18d0eb86ea54c5122 # release-1.10.0
)

FetchContent_MakeAvailable(directx-headers)

target_compile_options(Croupier PRIVATE -utf-8)

target_link_libraries(Croupier PRIVATE
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
