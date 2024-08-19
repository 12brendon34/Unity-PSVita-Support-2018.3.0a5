// PS Vita platform specific implementation.

#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include "socket.h"
#include "poll.h"

#define MAX_EVENTS  3

int poll(pollfd* handles, int numHandles, int timeout)
{
    SceNetId eid;
    SceNetEpollEvent epoll_events[MAX_EVENTS];
    int events_idx, fds_idx, nevents, s, ret;
    bool fds_modified;

    // create multiplexed i/o
    eid = sceNetEpollCreate("il2cpp_poll", 0);
    if (eid < 0)
        return -1;

    // Since control does not like the same FD added twice we have to merge
    //  sets applying to the same FD
    memset(&epoll_events, 0, sizeof(SceNetEpollEvent) * MAX_EVENTS);

    for (fds_idx = 0; fds_idx < numHandles; fds_idx++)
    {
        handles[fds_idx].revents = 0;

        s = handles[fds_idx].fd;
        if (s < SCE_NET_ID_SOCKET_MIN)
            // Based on what other branches do.
            continue;

        if (s > SCE_NET_ID_SOCKET_MAX)
        {
            handles[fds_idx].revents = POLLNVAL;
            ret = 1;
            goto DestroyEpollAndReturn;
        }

        // work out which event we are going to set this on
        for (events_idx = 0; events_idx < MAX_EVENTS; events_idx++)
        {
            if (epoll_events[events_idx].data.ext.id == 0 || epoll_events[events_idx].data.ext.id == s)
                break;
        }

        if (events_idx == MAX_EVENTS)
        {
            // not sure what to do here, perhaps we can just up MAX_EVENTS?
            ret = -1;
            goto DestroyEpollAndReturn;
        }

        if ((handles[fds_idx].events & POLLIN) != 0)
        {
            epoll_events[events_idx].data.ext.id = s;
            epoll_events[events_idx].events |= SCE_NET_EPOLLIN;
        }

        if ((handles[fds_idx].events & POLLOUT) != 0)
        {
            epoll_events[events_idx].data.ext.id = s;
            epoll_events[events_idx].events |= SCE_NET_EPOLLOUT;
        }

        if (((handles[fds_idx].events & (POLLERR | POLLHUP | POLLNVAL)) != 0))
        {
            epoll_events[events_idx].data.ext.id = s;
            epoll_events[events_idx].events |= (SCE_NET_EPOLLERR | SCE_NET_EPOLLHUP);
        }
    }

    // issue the control commands
    for (events_idx = 0; events_idx < MAX_EVENTS; events_idx++)
    {
        if (epoll_events[events_idx].data.ext.id != 0)
        {
            ret = sceNetEpollControl(eid, SCE_NET_EPOLL_CTL_ADD, s, &epoll_events[events_idx]);

            if (ret < 0)
            {
                //ret = sce_net_errno;
                ret = -1;
                goto DestroyEpollAndReturn;
            }
        }
    }

    // calculate and handle timeout (milliseconds -> microseconds)
    if (timeout < 0)
    {
        timeout = -1; // infinate timeout
    }
    else
    {
        timeout = timeout * 1000;
    }

    nevents = sceNetEpollWait(eid, epoll_events, MAX_EVENTS, timeout);
    if (nevents < 0)
    {
        ret = -1;
        goto DestroyEpollAndReturn;
    }

    // Work out what to return (somehow got to match this up with what
    // came in and return in the .revents)
    ret = 0;
    for (events_idx = 0; events_idx < nevents; events_idx++)
    {
        for (fds_idx = 0; fds_idx < numHandles; fds_idx++)
        {
            if (epoll_events[events_idx].data.ext.id == handles[fds_idx].fd)
            {
                if ((epoll_events[events_idx].events & SCE_NET_EPOLLIN) &&
                    ((handles[fds_idx].events & POLLIN) != 0))
                {
                    handles[fds_idx].revents |= POLLIN;
                    ret++;
                }
                if ((epoll_events[events_idx].events & SCE_NET_EPOLLOUT) &&
                    ((handles[fds_idx].events & POLLOUT) != 0))
                {
                    handles[fds_idx].revents |= POLLOUT;
                    ret++;
                }
                if ((epoll_events[events_idx].events & (SCE_NET_EPOLLERR | SCE_NET_EPOLLHUP)) &&
                    ((handles[fds_idx].events & (POLLERR | POLLHUP | POLLNVAL)) != 0))
                {
                    handles[fds_idx].revents |= POLLERR;
                    ret++;
                }
            }
        }
    }

DestroyEpollAndReturn:
    sceNetEpollDestroy(eid);
    return ret;
}

#endif //IL2CPP_TARGET_PSP2
