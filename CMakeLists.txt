cmake_minimum_required(VERSION 3.15)

project(Croupier CXX)

# Find latest version at https://github.com/OrfeasZ/ZHMModSDK/releases
# Set ZHMMODSDK_DIR variable to a local directory to use a local copy of the ZHMModSDK.
set(ZHMMODSDK_VER "v2.0.0-rc.1")
include(cmake/setup-zhmmodsdk.cmake)

# Set C++ standard to C++23.
set(CMAKE_CXX_STANDARD 23)

# Create the Croupier mod library.
add_library(Croupier SHARED
	src/Croupier.cpp
	src/Croupier.h
	src/CroupierWindow.cpp
	src/CroupierWindow.h
	src/json.hpp
	src/DeclareDPIAware.manifest
 "src/Roulette.cpp" "src/util.h" "src/EventSystem.h" "src/Events.h" "src/Enums.h" "src/RouletteRuleset.h" "src/RouletteMission.h" "src/CroupierClient.h" "src/CroupierClient.cpp")

target_link_libraries(Croupier
	ZHMModSDK
	D2D1
	DWrite
	WindowsCodecs
)

install(TARGETS Croupier
	RUNTIME DESTINATION bin
)

# Install the mod to the game folder when the `GAME_INSTALL_PATH` variable is set.
zhmmodsdk_install(Croupier)
