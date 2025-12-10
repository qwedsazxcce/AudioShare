cmake_minimum_required(VERSION 3.22.1)
project(audioshare_opensl)

add_library(audioshare-opensl SHARED
        opensl_player.cpp)

find_library(log-lib log)
find_library(opensl-lib OpenSLES)

target_link_libraries(
        audioshare-opensl
        ${log-lib}
        ${opensl-lib})
