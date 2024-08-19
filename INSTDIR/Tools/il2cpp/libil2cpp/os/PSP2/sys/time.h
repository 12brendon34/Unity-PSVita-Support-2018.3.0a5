#pragma once

extern "C"
{
typedef long suseconds_t;

// Winsock2.h
// Time.h on BSD
typedef struct timeval
{
    time_t      tv_sec;     /* seconds */
    suseconds_t tv_usec;    /* and microseconds */
} timeval;

int gettimeofday(struct timeval *tv, void *tzp);
}
