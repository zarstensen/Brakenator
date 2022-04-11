PROJECT("BNTEST" LANGUAGES CXX)

add_executable(${PROJECT_NAME} "${CMAKE_CURRENT_SRC_DIR}src/main.cpp")

target_link_libraries(${PROJECT_NAME} Brakenator)
target_include_directories(${PROJECT_NAME} PRIVATE "${CMAKE_SRC_DIR}/include")
