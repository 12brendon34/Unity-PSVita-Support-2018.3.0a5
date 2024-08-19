#include "il2cpp-config.h"

#include "MultiplexIO.h"
#include "os\Posix\PosixHelpers.h"

#include <net\epoll.h>
#include <net\errno.h>

#define MAX_EVENTS (16)

namespace il2cpp
{
namespace Sockets
{
    static struct { short unixval; uint32_t sceval; }
    vals[] = { { POLLIN, SCE_NET_EPOLLIN }, { POLLOUT, SCE_NET_EPOLLOUT }, { POLLERR, SCE_NET_EPOLLERR }, { POLLHUP, SCE_NET_EPOLLHUP }, { POLLNVAL, SCE_NET_EPOLLERR } };

    static uint32_t RemapUnixToSCEEvents(short unixEvents)
    {
        uint32_t sceEvents = 0;
        for (int bit = 0; bit < sizeof(vals) / sizeof(vals[0]); bit++)
        {
            if (unixEvents & vals[bit].unixval)
                sceEvents |= vals[bit].sceval;
        }
        return sceEvents;
    }

    static short RemapSCEToUnixEvents(uint32_t sceEvents)
    {
        uint32_t unixEvents = 0;
        for (int bit = 0; bit < sizeof(vals) / sizeof(vals[0]); bit++)
        {
            if (sceEvents & vals[bit].sceval)
                unixEvents |= vals[bit].unixval;
        }
        return unixEvents;
    }

    MultiplexIO::MultiplexIO()
    {
        m_epollId = sceNetEpollCreate("AsyncSocketPoll", 0);
    }

    MultiplexIO::~MultiplexIO()
    {
        sceNetEpollDestroy(m_epollId);
    }

    void MultiplexIO::InterruptPoll()
    {
        sceNetEpollAbort(m_epollId, SCE_NET_EPOLL_ABORT_FLAG_PRESERVATION);
    }

    os::WaitStatus MultiplexIO::Poll(std::vector<il2cpp::os::PollRequest> &requests, int32_t timeout, int32_t *result, int32_t *error)
    {
        const int32_t nfds = (int32_t)requests.size();

        SceNetEpollEvent epoll_events[MAX_EVENTS];
        int intfds_idx, nevents, ret;

        int events_idx = 0;

        // Since control does not like the same FD added twice we have to merge
        //  sets applying to the same FD
        memset(&epoll_events, 0, sizeof(SceNetEpollEvent) * MAX_EVENTS);

        for (int fds_idx = 0; fds_idx < nfds; fds_idx++)
        {
            if (events_idx >= MAX_EVENTS)
                break;

            if (requests[fds_idx].fd != -1)
            {
                epoll_events[events_idx].data.fd = requests[fds_idx].fd;
                epoll_events[events_idx].events = RemapUnixToSCEEvents(os::posix::PollFlagsToPollEvents(requests[fds_idx].events));
                if (epoll_events[events_idx].events)
                {
                    ret = sceNetEpollControl(m_epollId, SCE_NET_EPOLL_CTL_ADD, epoll_events[events_idx].data.fd, &epoll_events[events_idx]);
                    if (ret < 0)
                    {
                        printf("error during sceNetEpollControl 0x%x\n", ret);
                    }
                    events_idx++;
                }
            }
        }

        // calculate and handle timeout (milliseconds -> microseconds)
        timeout = (timeout >= 0) ? timeout * 10000 : -1;

        nevents = 0;
        ret = sceNetEpollWait(m_epollId, epoll_events, MAX_EVENTS, timeout);

        if (ret < 0)
        {
            if (ret != SCE_NET_ERROR_EINTR)     // Some error other than wait canceled...
            {
                printf("sceNetEpollWait with eid 0x%x returned error 0x%08x\n", m_epollId, ret);
            }
        }
        else
        {
            nevents = ret;
        }

        // revoke the add commands
        for (int fds_idx = 0; fds_idx < nfds; fds_idx++)
        {
            SceNetId netId = requests[fds_idx].fd;

            // any pending ones will be re-added next time Poll is called
            ret = sceNetEpollControl(m_epollId, SCE_NET_EPOLL_CTL_DEL, netId, 0);
            if (ret < 0)
            {
                printf("error during sceNetEpollControl del 0x%x\n", ret);
            }

            for (events_idx = 0; events_idx < nevents; events_idx++)
            {
                if (epoll_events[events_idx].data.fd == netId)
                {
                    requests[fds_idx].revents = os::posix::PollEventsToPollFlags(RemapSCEToUnixEvents(epoll_events[events_idx].events));
                    break;
                }
            }
        }
        return kWaitStatusSuccess;
    }

    int MultiplexIO::poll(pollfd *ufds, unsigned int nfds, int timeout)
    {
        SceNetEpollEvent epoll_events[MAX_EVENTS];
        int intfds_idx, nevents, ret;

        int events_idx = 0;

        // Since control does not like the same FD added twice we have to merge
        //  sets applying to the same FD
        memset(&epoll_events, 0, sizeof(SceNetEpollEvent) * MAX_EVENTS);

        for (int fds_idx = 0; fds_idx < nfds; fds_idx++)
        {
            if (events_idx >= MAX_EVENTS)
                break;

            epoll_events[events_idx].data.fd = ufds[fds_idx].fd;
            epoll_events[events_idx].events = RemapUnixToSCEEvents(ufds[fds_idx].events);

            if (epoll_events[events_idx].events)
            {
                ret = sceNetEpollControl(m_epollId, SCE_NET_EPOLL_CTL_ADD, epoll_events[events_idx].data.fd, &epoll_events[events_idx]);
                if (ret < 0)
                {
                    printf("error during sceNetEpollControl 0x%x\n", ret);
                }
                events_idx++;
            }
        }

        // calculate and handle timeout (milliseconds -> microseconds)
        timeout = (timeout >= 0) ? timeout * 10000 : -1;

        nevents = 0;
        ret = sceNetEpollWait(m_epollId, epoll_events, MAX_EVENTS, timeout);

        if ((ret == SCE_NET_ERROR_EINTR) || (ret < 0))
        {
            printf("sceNetEpollWait aborted:0x%x", ret);
        }
        else
        {
            nevents = ret;
        }

        // revoke the add commands
        for (int fds_idx = 0; fds_idx < nfds; fds_idx++)
        {
            ret = sceNetEpollControl(m_epollId, SCE_NET_EPOLL_CTL_DEL, ufds[fds_idx].fd, 0);
            if (ret < 0)
            {
                printf("error during sceNetEpollControl del 0x%x\n", ret);
            }

            for (events_idx = 0; events_idx < nevents; events_idx++)
            {
                if (epoll_events[events_idx].data.fd == ufds[fds_idx].fd)
                {
                    ufds[fds_idx].revents = RemapSCEToUnixEvents(epoll_events[events_idx].events);
                    break;
                }
            }
        }

        return ret;
    }
}
}
