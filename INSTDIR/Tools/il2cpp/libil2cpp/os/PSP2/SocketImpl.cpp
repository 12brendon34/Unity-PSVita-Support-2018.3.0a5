#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include <cassert>
#include <string.h>
#include <libnetctl.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <sys/poll.h>
#include "vm/Exception.h"
#include "utils/StringUtils.h"
#include "os/Error.h"
#include "os/Posix/PosixHelpers.h"
#include "os/PSP2/SocketImpl.h"
#include "os/PSP2/SocketImpl_Adapters.h"
#include "os/PSP2/SocketImpl_Helpers.h"

#define SOCKET_NOT_IMPLEMENTED \
    assert(0 && "Not supported on target platform");

namespace il2cpp
{
namespace os
{
    static bool hostent_get_info(struct hostent *he, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        if (he == NULL)
            return false;

        if (he->h_length != 4 || he->h_addrtype != AF_INET)
            return false;

        name.assign(he->h_name);

        for (int32_t i = 0; he->h_aliases[i] != NULL; ++i)
            aliases.push_back(he->h_aliases[i]);

        for (int32_t i = 0; he->h_addr_list[i] != NULL; ++i)
            addr_list.push_back(
                utils::StringUtils::NPrintf("%u.%u.%u.%u", 16,
                    (uint8_t)he->h_addr_list[i][0],
                    (uint8_t)he->h_addr_list[i][1],
                    (uint8_t)he->h_addr_list[i][2],
                    (uint8_t)he->h_addr_list[i][3]));

        return true;
    }

    static bool hostent_get_info_with_local_ips(struct hostent *he, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        int32_t nlocal_in = 0;

        if (he != NULL)
        {
            if (he->h_length != 4 || he->h_addrtype != AF_INET)
                return false;

            name.assign(he->h_name);

            for (int32_t i = 0; he->h_aliases[i] != NULL; ++i)
                aliases.push_back(he->h_aliases[i]);
        }

        struct in_addr *local_in = get_local_ips(AF_INET, &nlocal_in);

        if (nlocal_in)
        {
            for (int32_t i = 0; i < nlocal_in; ++i)
            {
                const uint8_t *ptr = (uint8_t*)&local_in[i];

                addr_list.push_back(
                    utils::StringUtils::NPrintf("%u.%u.%u.%u", 16,
                        (uint8_t)ptr[0],
                        (uint8_t)ptr[1],
                        (uint8_t)ptr[2],
                        (uint8_t)ptr[3]));
            }

            free(local_in);
        }
        else if (he == NULL)
        {
            // If requesting "" and there are no other interfaces up, MS returns 127.0.0.1
            addr_list.push_back("127.0.0.1");
            return true;
        }

        if (nlocal_in == 0 && he != NULL)
        {
            for (int32_t i = 0; he->h_addr_list[i] != NULL; ++i)
            {
                addr_list.push_back(
                    utils::StringUtils::NPrintf("%u.%u.%u.%u", 16,
                        (uint8_t)he->h_addr_list[i][0],
                        (uint8_t)he->h_addr_list[i][1],
                        (uint8_t)he->h_addr_list[i][2],
                        (uint8_t)he->h_addr_list[i][3]));
            }
        }

        return true;
    }

    void SocketImpl::Startup()
    {
    }

    void SocketImpl::Cleanup()
    {
    }

    WaitStatus SocketImpl::GetHostByAddr(const std::string &address, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        struct in_addr inaddr;

        if (inet_pton(AF_INET, address.c_str(), &inaddr) <= 0)
            return kWaitStatusFailure;

        struct hostent *he = gethostbyaddr((char*)&inaddr, sizeof(inaddr), AF_INET);

        if (he == NULL)
        {
            name = address;
            addr_list.push_back(name);

            return kWaitStatusSuccess;
        }

        return hostent_get_info(he, name, aliases, addr_list)
            ? kWaitStatusSuccess
            : kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetHostByName(const std::string &host, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        char this_hostname[256] = {0};

        const char *hostname = host.c_str();
        bool add_local_ips = (*hostname == '\0');

        if (!add_local_ips && gethostname(this_hostname, sizeof(this_hostname)) != -1)
        {
            if (!strcmp(hostname, this_hostname))
                add_local_ips = true;
        }

        struct hostent *he = NULL;
        if (*hostname)
            he = gethostbyname(hostname);

        if (*hostname && he == NULL)
            return kWaitStatusFailure;

        return (add_local_ips
                ? hostent_get_info_with_local_ips(he, name, aliases, addr_list)
                : hostent_get_info(he, name, aliases, addr_list))
            ? kWaitStatusSuccess
            : kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetHostByName(const std::string &host, std::string &name, int32_t &family, std::vector<std::string> &aliases, std::vector<void*> &addr_list, int32_t &addr_size)
    {
        SOCKET_NOT_IMPLEMENTED
        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetHostName(std::string &name)
    {
        char hostname[256];
        int32_t ret = gethostname(hostname, sizeof(hostname));

        if (ret == -1)
            return kWaitStatusFailure;

        name.assign(hostname);

        return kWaitStatusSuccess;
    }

    SocketImpl::SocketImpl(ThreadStatusCallback thread_status_callback)
        :   _is_valid(false)
        ,   _fd(-1)
        ,   _domain(-1)
        ,   _type(-1)
        ,   _protocol(-1)
        ,   _saved_error(kErrorCodeSuccess)
        ,   _still_readable(0)
        ,   _thread_status_callback(thread_status_callback)
    {
    }

    SocketImpl::~SocketImpl()
    {
    }

    WaitStatus SocketImpl::Create(AddressFamily family, SocketType type, ProtocolType protocol)
    {
        _fd = -1;
        _is_valid = false;
        _still_readable = 1;
        _domain = convert_address_family(family);
        _type = convert_socket_type(type);
        _protocol = convert_socket_protocol(protocol);

        _fd = sceNetSocket(__FILE__, _domain, _type, _protocol);
        if (_fd < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        /* .net seems to set this by default for SOCK_STREAM, not for
         * SOCK_DGRAM (see bug #36322)
         */
        int32_t v = 1;
        const int32_t ret = sceNetSetsockopt(_fd, SOL_SOCKET, SO_REUSEADDR, &v, sizeof(v));

        if (ret < 0)
        {
            if (sceNetSocketClose(_fd) < 0)
                StoreLastError();

            return kWaitStatusFailure;
        }

        _is_valid = true;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Create(SocketDescriptor fd, int32_t family, int32_t type, int32_t protocol)
    {
        _fd = fd;
        _is_valid = (fd != -1);
        _still_readable = 1;
        _domain = family;
        _type = type;
        _protocol = protocol;

        assert(_type != -1 && "Unsupported socket type");
        assert(_domain != -1 && "Unsupported address family");
        assert(_protocol != -1 && "Unsupported protocol type");

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Close()
    {
        _saved_error = kErrorCodeSuccess;

        if (_is_valid && _fd != -1)
        {
            if (sceNetSocketClose(_fd) < 0)
                StoreLastError();
        }

        _fd = -1;
        _is_valid = false;
        _still_readable = 0;
        _domain = -1;
        _type = -1;
        _protocol = -1;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SetBlocking(bool blocking)
    {
        /*
        * block == TRUE/FALSE means we will block/not block.
        * But the ioctlsocket call takes TRUE/FALSE for non-block/block
        */
        blocking = !blocking;

        const int result = ioctl(_fd, FIONBIO, &blocking);
        if (result == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    ErrorCode SocketImpl::GetLastError() const
    {
        return _saved_error;
    }

    void SocketImpl::StoreLastError()
    {
        const ErrorCode error = (ErrorCode)convert_sce_net_errno(sce_net_errno);

        Error::SetLastError(error);

        _saved_error = error;
    }

    void SocketImpl::StoreLastError(int32_t error_no)
    {
        const ErrorCode error = (ErrorCode)error_no;

        Error::SetLastError(error);

        _saved_error = error;
    }

    static void sockaddr_from_path(const char *path, struct sockaddr *sa, socklen_t *sa_size)
    {
        struct sockaddr sa_un = {0};
        const size_t len = strlen(path);

        //memcpy (sa_un.sun_path, path, len);

        *sa_size = (socklen_t)len;
        *sa = *((struct sockaddr*)&sa_un);
    }

    static void sockaddr_from_address(uint32_t address, uint16_t port, struct sockaddr *sa, socklen_t *sa_size)
    {
        struct sockaddr_in sa_in = {0};

        sa_in.sin_family = AF_INET;
        sa_in.sin_port = port;
        sa_in.sin_addr.s_addr = address;

        *sa_size = sizeof(struct sockaddr_in);
        *sa = *((struct sockaddr*)&sa_in);
    }

    static bool socketaddr_to_endpoint_info(const struct sockaddr *address, socklen_t address_len, EndPointInfo &info)
    {
        info.family = (os::AddressFamily)address->sa_family;

        if (info.family == os::kAddressFamilyInterNetwork)
        {
            const struct sockaddr_in *address_in = (const struct sockaddr_in *)address;

            info.data.inet.port = ntohs(address_in->sin_port);
            info.data.inet.address = ntohl(address_in->sin_addr.s_addr);

            return true;
        }

        if (info.family == os::kAddressFamilyUnix)
        {
            for (int32_t i = 0; i < address_len; i++)
                info.data.path[i] = address->sa_data[i];

            return true;
        }

        return false;
    }

    WaitStatus SocketImpl::Bind(const char *path)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_path(path, &sa, &sa_size);

        if (sceNetBind(_fd, &sa, sa_size) < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Bind(const char *address, uint16_t port)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(char_to_inet_addr(address), htons(port), &sa, &sa_size);

        if (sceNetBind(_fd, &sa, sa_size) < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Bind(uint32_t address, uint16_t port)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        if (sceNetBind(_fd, &sa, sa_size) < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Bind(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
        NOT_SUPPORTED_IL2CPP(SocketImpl::Bind, "PSVita does not support IPv6.");
        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::ConnectInternal(struct sockaddr *sa, int32_t sa_size)
    {
        int result = sceNetConnect(_fd, sa, (socklen_t)sa_size);
        if (result == 0)
            return kWaitStatusSuccess;

        StoreLastError();
        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Connect(const char *path)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_path(path, &sa, &sa_size);

        return ConnectInternal((struct sockaddr *)&sa, sa_size);
    }

    WaitStatus SocketImpl::Shutdown(int32_t how)
    {
        if (sceNetShutdown(_fd, how) < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        if (how == SHUT_RD || how == SHUT_RDWR)
            _still_readable = 0;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Disconnect(bool reuse)
    {
        NOT_SUPPORTED_IL2CPP(SocketImpl::Disconnect, "This call is not supported for PS Vita.");
        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Connect(uint32_t address, uint16_t port)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        return ConnectInternal((struct sockaddr *)&sa, sa_size);
    }

    WaitStatus SocketImpl::Connect(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
        NOT_SUPPORTED_IL2CPP(SocketImpl::Connect, "PSVita does not support IPv6.");
        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetLocalEndPointInfo(EndPointInfo &info)
    {
        // Note: the size here could probably be smaller
        uint8_t buffer[END_POINT_MAX_PATH_LEN + 3] = {0};
        socklen_t address_len = sizeof(buffer);

        if (sceNetGetsockname(_fd, (struct sockaddr *)buffer, (SceNetSocklen_t*)&address_len) < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        if (!socketaddr_to_endpoint_info((struct sockaddr *)buffer, address_len, info))
        {
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::GetRemoteEndPointInfo(EndPointInfo &info)
    {
        // Note: the size here could probably be smaller
        uint8_t buffer[END_POINT_MAX_PATH_LEN + 3] = {0};
        socklen_t address_len = sizeof(buffer);

        if (sceNetGetpeername(_fd, (struct sockaddr *)buffer, (SceNetSocklen_t*)&address_len) < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        if (!socketaddr_to_endpoint_info((struct sockaddr *)buffer, address_len, info))
        {
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Listen(int32_t backlog)
    {
        if (sceNetListen(_fd, backlog) < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Accept(os::Socket **socket)
    {
        int32_t new_fd = 0;

        *socket = NULL;

        new_fd = sceNetAccept(_fd, (SceNetSockaddr*)NULL, 0);
        if (new_fd < 0)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        *socket = new os::Socket(_thread_status_callback);

        const WaitStatus status = (*socket)->Create(new_fd, _domain, _type, _protocol);

        if (status != kWaitStatusSuccess)
        {
            delete *socket;
            *socket = NULL;
            return status;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Receive(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        *len = 0;

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        return ReceiveFromInternal(data, count, c_flags, len, NULL, 0);
    }

    WaitStatus SocketImpl::ReceiveFromInternal(const uint8_t *data, size_t count, int32_t flags, int32_t *len, struct sockaddr *from, int32_t *fromlen)
    {
        int result = sceNetRecvfrom(_fd, (void*)data, count, flags, from, (SceNetSocklen_t*)fromlen);
        if (result < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        *len = result;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Send(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        *len = 0;

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            StoreLastError(kWSAeopnotsupp);
            return kWaitStatusFailure;
        }

        int result = sceNetSend(_fd, (void*)data, count, flags);
        if (result < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        *len = result;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SendArray(WSABuf *wsabufs, int32_t count, int32_t *sent, SocketFlags flags)
    {
        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        const int32_t ret = sceNetSend(_fd, (void*)wsabufs, count, c_flags);
        if (ret < 0)
        {
            *sent = 0;

            StoreLastError();

            return kWaitStatusFailure;
        }

        *sent = ret;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::ReceiveArray(WSABuf *wsabufs, int32_t count, int32_t *len, SocketFlags flags)
    {
        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        const int32_t ret = sceNetRecv(_fd, (void*)wsabufs, count, c_flags);
        if (ret < 0)
        {
            *len = 0;

            StoreLastError();

            return kWaitStatusFailure;
        }

        *len = ret;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SendTo(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        *len = 0;

        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        int32_t ret = (int32_t)sceNetSendto(_fd, (void*)data, count, c_flags, &sa, sa_size);
        if (ret < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        *len = ret;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SendTo(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        *len = 0;

        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_path(path, &sa, &sa_size);

        const int32_t c_flags = convert_socket_flags(flags);
        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        int32_t ret = (int32_t)sceNetSendto(_fd, (void*)data, count, c_flags, &sa, sa_size);
        if (ret < 0)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        *len = ret;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SendTo(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        NOT_SUPPORTED_IL2CPP(SocketImpl::SendTo, "PSVita does not support IPv6.");
        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::RecvFrom(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        *len = 0;

        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        const WaitStatus status = ReceiveFromInternal(data, count, c_flags, len, &sa, (int32_t*)&sa_size);

        if (status != kWaitStatusSuccess)
        {
            ep.family = os::kAddressFamilyError;
            return status;
        }

        if (sa_size == 0)
            return kWaitStatusSuccess;

        if (!socketaddr_to_endpoint_info(&sa, sa_size, ep))
        {
            ep.family = os::kAddressFamilyError;
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::RecvFrom(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        *len = 0;

        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_path(path, &sa, &sa_size);

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        const WaitStatus status = ReceiveFromInternal(data, count, c_flags, len, &sa, (int32_t*)&sa_size);

        if (status != kWaitStatusSuccess)
        {
            ep.family = os::kAddressFamilyError;
            return kWaitStatusFailure;
        }

        if (sa_size == 0)
            return kWaitStatusSuccess;

        if (!socketaddr_to_endpoint_info(&sa, sa_size, ep))
        {
            ep.family = os::kAddressFamilyError;
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::RecvFrom(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        NOT_SUPPORTED_IL2CPP(SocketImpl::RecvFrom, "PSVita does not support IPv6.");
        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Available(int32_t *amount)
    {
        *amount = 0;
        int result = ioctl(_fd, FIONREAD, amount);
        if (result == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Ioctl(int32_t command, const uint8_t *in_data, int32_t in_len, uint8_t *out_data, int32_t out_len, int32_t *written)
    {
        assert(command != 0xC8000006 /* SIO_GET_EXTENSION_FUNCTION_POINTER */ && "SIO_GET_EXTENSION_FUNCTION_POINTER ioctl command not supported");

        uint8_t *buffer = NULL;

        if (in_len > 0)
        {
            buffer = (uint8_t*)malloc(in_len);
            memcpy(buffer, in_data, in_len);
        }

        const int32_t ret = ioctl(_fd, command, buffer);
        if (ret == -1)
        {
            StoreLastError();

            free(buffer);

            return kWaitStatusFailure;
        }

        if (buffer == NULL)
        {
            *written = 0;
            return kWaitStatusSuccess;
        }

        const int32_t len = (in_len > out_len) ? out_len : in_len;

        if (len > 0 && out_data != NULL)
            memcpy(out_data, buffer, len);

        free(buffer);

        *written = len;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::GetSocketOption(SocketOptionLevel level, SocketOptionName name, uint8_t *buffer, int32_t *length)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            *((int32_t*)buffer) = 0;
            *length = sizeof(int32_t);

            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        uint8_t *tmp_val = buffer;

        const int32_t ret = sceNetGetsockopt(_fd, system_level, system_name, (void*)tmp_val, (SceNetSocklen_t*)length);
        if (ret < 0)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        if (system_name == SO_ERROR)
        {
            if (*((int32_t*)buffer) != 0)
            {
                StoreLastError(*((int32_t*)buffer));
            }
            else
            {
                *((int32_t*)buffer) = _saved_error;
            }
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::GetSocketOptionFull(SocketOptionLevel level, SocketOptionName name, int32_t *first, int32_t *second)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        if (level == kSocketOptionLevelSocket && name == kSocketOptionNameExclusiveAddressUse)
        {
            system_level = SOL_SOCKET;
            system_name = SO_REUSEADDR;
        }
        else
        {
            const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

            if (o_res == SKIP_OPTION)
            {
                *first = 0;
                *second = 0;

                return kWaitStatusSuccess;
            }

            if (o_res == INVALID_OPTION_NAME)
            {
                _saved_error = kWSAenoprotoopt;

                return kWaitStatusFailure;
            }
        }

        int32_t ret = -1;

        switch (name)
        {
            case kSocketOptionNameLinger:
            {
                struct SceNetLinger linger;
                socklen_t lingersize = sizeof(linger);

                ret = sceNetGetsockopt(_fd, system_level, system_name, (char*)&linger, (SceNetSocklen_t*)&lingersize);

                *first = linger.l_onoff;
                *second = linger.l_linger;
            }
            break;

            case kSocketOptionNameDontLinger:
            {
                struct SceNetLinger linger;
                socklen_t lingersize = sizeof(linger);

                ret = sceNetGetsockopt(_fd, system_level, system_name, (char*)&linger, (SceNetSocklen_t*)&lingersize);

                *first = !linger.l_onoff;
            }
            break;

            case kSocketOptionNameSendTimeout:
            case kSocketOptionNameReceiveTimeout:
            {
                socklen_t time_ms_size = sizeof(*first);
                ret = sceNetGetsockopt(_fd, system_level, system_name, (char*)first, (SceNetSocklen_t*)&time_ms_size);
            }
            break;

            default:
            {
                socklen_t valsize = sizeof(*first);
                ret = sceNetGetsockopt(_fd, system_level, system_name, (char*)first, (SceNetSocklen_t*)&valsize);
            }
            break;
        }

        if (ret < 0)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        if (level == kSocketOptionLevelSocket && name == kSocketOptionNameExclusiveAddressUse)
            *first = *first ? 0 : 1;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Poll(std::vector<PollRequest> &requests, int32_t count, int32_t timeout, int32_t *result, int32_t *error)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Poll(std::vector<PollRequest> &requests, int32_t timeout, int32_t *result, int32_t *error)
    {
        const int32_t n_fd = (int32_t)requests.size();
        pollfd *p_fd = (pollfd*)calloc(n_fd, sizeof(pollfd));

        for (int32_t i = 0; i < n_fd; ++i)
        {
            if (requests[i].fd == -1)
            {
                p_fd[i].fd = -1;
                p_fd[i].events = kPollFlagsNone;
                p_fd[i].revents = kPollFlagsNone;
            }
            else
            {
                p_fd[i].fd = requests[i].fd;
                p_fd[i].events = posix::PollFlagsToPollEvents(requests[i].events);
                p_fd[i].revents = kPollFlagsNone;
            }
        }

        int32_t ret = os::posix::Poll(p_fd, n_fd, timeout);
        *result = ret;

        if (ret == -1)
        {
            free(p_fd);

            //*error = SocketErrnoToErrorCode (errno);

            return kWaitStatusFailure;
        }

        if (ret == 0)
        {
            free(p_fd);

            return kWaitStatusSuccess;
        }

        for (int32_t i = 0; i < n_fd; ++i)
        {
            requests[i].revents = posix::PollEventsToPollFlags(p_fd[i].revents);
        }

        free(p_fd);

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Poll(PollRequest& request, int32_t timeout, int32_t *result, int32_t *error)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SetSocketOption(SocketOptionLevel level, SocketOptionName name, int32_t value)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct SceNetLinger linger;

        WaitStatus ret = kWaitStatusFailure;

        switch (name)
        {
            case kSocketOptionNameDontLinger:
                linger.l_onoff = !value;
                linger.l_linger = 0;
                ret = SetSocketOptionInternal(system_level, system_name, &linger, sizeof(linger));
                break;

            default:
                ret = SetSocketOptionInternal(system_level, system_name, (char*)&value, sizeof(value));
                break;
        }

        return ret;
    }

    WaitStatus SocketImpl::SetSocketOptionLinger(SocketOptionLevel level, SocketOptionName name, bool enabled, int32_t seconds)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct SceNetLinger linger;

        linger.l_onoff = enabled;
        linger.l_linger = seconds;

        return SetSocketOptionInternal(system_level, system_name, &linger, sizeof(linger));
    }

    WaitStatus SocketImpl::SetSocketOptionArray(SocketOptionLevel level, SocketOptionName name, const uint8_t *buffer, int32_t length)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct SceNetLinger linger;

        WaitStatus ret = kWaitStatusFailure;

        switch (name)
        {
            case kSocketOptionNameDontLinger:
                if (length == 1)
                {
                    linger.l_linger = 0;
                    linger.l_onoff = (*((char*)buffer)) ? 0 : 1;

                    ret = SetSocketOptionInternal(system_level, system_name, &linger, sizeof(linger));
                }
                else
                {
                    _saved_error = kWSAeinval;

                    return kWaitStatusFailure;
                }
                break;

            default:
                ret = SetSocketOptionInternal(system_level, system_name, buffer, length);
                break;
        }

        return ret;
    }

    WaitStatus SocketImpl::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, uint32_t group_address, uint32_t local_address)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct SceNetIpMreq mreq = {{0}};

        if (group_address)
            mreq.imr_multiaddr.s_addr = htonl(group_address);

        if (local_address)
            mreq.imr_interface.s_addr = htonl(local_address);

        return SetSocketOptionInternal(system_level, system_name, &mreq, sizeof(mreq));
    }

#if IL2CPP_SUPPORT_IPV6
    WaitStatus SocketImpl::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, IPv6Address ipv6, uint64_t interfaceOffset)
    {
        NOT_SUPPORTED_IL2CPP(SocketImpl::Bind, "PSVita does not support IPv6.");
        return kWaitStatusFailure;
    }

#endif

    WaitStatus SocketImpl::SetSocketOptionInternal(int32_t level, int32_t name, const void *value, int32_t len)
    {
        const void *real_val = value;

        if (level == SOL_SOCKET && (name == SO_RCVTIMEO || name == SO_SNDTIMEO))
        {
            struct timeval tv;

            const int32_t ms = *((int32_t*)value);

            tv.tv_sec = ms / 1000;
            tv.tv_usec = (ms % 1000) * 1000;
            real_val = &tv;

            len = sizeof(tv);
        }

        const int32_t ret = sceNetSetsockopt(_fd, level, name, real_val, (socklen_t)len);

        if (ret < 0)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SendFile(const char *filename, TransmitFileBuffers *buffers, TransmitFileOptions options)
    {
        NOT_SUPPORTED_IL2CPP(SocketImpl::SendFile, "This call is not supported for PS Vita.");
        return kWaitStatusFailure;
    }
}
}
#endif // IL2CPP_TARGET_PSP2
