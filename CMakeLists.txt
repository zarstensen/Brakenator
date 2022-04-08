cmake_minimum_required (VERSION 3.8) # C# support

set(CMAKE_CXX_STANDARD 17)

project("Brakenator" VERSION "0.1.0" LANGUAGES CXX)

# place all predefined target projects into a filter
set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set(PREDEFINED_TARGETS_FOLDER "Targets")

set (BN_SRC
	src/Brakenator.cpp
)

set (BN_INCLUDE
	include/Brakenator.h
)

# Add source to this project's executable.
add_library (${PROJECT_NAME} SHARED
	${BN_INCLUDE} ${BN_SRC}
)

if(MSVC)
  target_compile_options(${PROJECT_NAME}  PUBLIC "/ZI")
  target_link_options(${PROJECT_NAME}  PUBLIC "/INCREMENTAL")
endif()

target_include_directories(${PROJECT_NAME} PUBLIC ${CMAKE_SOURCE_DIR} "include/" "ipp_src/")
target_compile_definitions(${PROJECT_NAME} PRIVATE BN_EXPORTS)

# place files into the correct filters
source_group("src" FILES ${SRC})
source_group("include" FILES ${INCLUDE})

# UI
add_subdirectory("BrakenatorUI")
