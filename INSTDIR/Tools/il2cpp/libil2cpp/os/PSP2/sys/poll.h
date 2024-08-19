#pragma once
// stub code for ps4 and poll

#define POLLIN     0x001
#define POLLPRI    0x002
#define POLLOUT    0x004
#define POLLERR    0x008
#define POLLHUP    0x010
#define POLLNVAL   0x020
#define POLLRDNORM 0x040
#define POLLRDBAND 0x080
#define POLLWRNORM 0x100
#define POLLWRBAND 0x200
#define POLLMSG    0x400
#define POLLRDHUP  0x2000

typedef unsigned long nfds_t;

struct pollfd
{
    int fd;
    short events;
    short revents;
};

int poll(pollfd* handles, int numHandles, int timeout);

// Pipe read,write and pipe stub functions
inline int read(int, char*, int)
{
    printf("psp2 stub read\n");
    return 0;
}

inline int write(int, char*, int)
{
    printf("psp2 stub write\n");
    return 0;
}

inline int pipe(int* pipeHandles)
{
    printf("psp2 stub pipe\n");
    return 0;
}

inline int close(int fildes)
{
    printf("psp2 stub close\n");
    return 0;
}
