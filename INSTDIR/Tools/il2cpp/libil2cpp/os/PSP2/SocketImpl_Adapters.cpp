#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include <libnetctl.h>

#include "arpa/inet.h"
#include "sys/socket.h"

#include "os/Socket.h"

#include "SocketImpl_Adapters.h"


namespace il2cpp
{
namespace os
{
    int32_t convert_socket_flags(os::SocketFlags flags)
    {
        int32_t c_flags = 0;

        if (flags)
        {
            if (flags & ~(os::kSocketFlagsPeek))
            {
                return -1;
            }

            if (flags & os::kSocketFlagsPeek)
                c_flags |= SCE_NET_MSG_PEEK;
        }

        return c_flags;
    }

    int32_t convert_address_family(AddressFamily family)
    {
        switch (family)
        {
            case kAddressFamilyUnspecified:
                return SCE_NET_EAFNOSUPPORT;

            case kAddressFamilyInterNetwork:
                return SCE_NET_AF_INET;

            case kAddressFamilyUnix:
            case kAddressFamilyIpx:
            case kAddressFamilySna:
            case kAddressFamilyDecNet:
            case kAddressFamilyAppleTalk:
            case kAddressFamilyInterNetworkV6:
            case kAddressFamilyIrda:
            default:
                // Not supported on this platform
                break;
        }

        return kAddressFamilyError;
    }

    int32_t convert_socket_type(SocketType type)
    {
        switch (type)
        {
            case kSocketTypeStream:
                return SCE_NET_SOCK_STREAM;

            case kSocketTypeDgram:
                return SCE_NET_SOCK_DGRAM;

            case kSocketTypeRaw:
                return SCE_NET_SOCK_RAW;

            case kSocketTypeRdm:
            case kSocketTypeSeqpacket:
            default:
                // Not supported on this platform
                break;
        }

        return kSocketTypeError;
    }

    int32_t convert_socket_protocol(ProtocolType protocol)
    {
        // Protocol (only valid for RAW socket on PS Vita)
        switch (protocol)
        {
            case kProtocolTypeIP:
                return SCE_NET_IPPROTO_IP;

            case kProtocolTypeIcmp:
                return SCE_NET_IPPROTO_ICMP;

            case kProtocolTypeIgmp:
                return SCE_NET_IPPROTO_IGMP;

            case kProtocolTypeTcp:
                return SCE_NET_IPPROTO_TCP;

            case kProtocolTypeUdp:
                return SCE_NET_IPPROTO_UDP;

            case kProtocolTypeIPv6:
            case kProtocolTypeGgp:
            case kProtocolTypePup:
            case kProtocolTypeIdp:
            default:
                // Not supported on this platform
                break;
        }

        return kProtocolTypeUnknown;
    }

    int32_t convert_sce_net_errno(int32_t error)
    {
        // WSA error number = SCE net error number + 10000
        // e.g. kWSAeconnrefused = 10061 and SCE_NET_ECONNREFUSED = 61
        // see: https://psvita.scedev.net/docs/vita-en,libnet-Reference-vita,Error_Codes_129_1/
        return error + 10000;
    }

    int32_t level_and_name_to_system(SocketOptionLevel level, SocketOptionName name, int32_t *system_level, int32_t *system_name)
    {
        switch (level)
        {
            case kSocketOptionLevelSocket:
                *system_level = SCE_NET_SOL_SOCKET;

                switch (name)
                {
                    case kSocketOptionNameDontLinger:
                        *system_name = SCE_NET_SO_LINGER;
                        break;

                    case kSocketOptionNameReuseAddress:
                        *system_name = SCE_NET_SO_REUSEADDR;
                        break;

                    case kSocketOptionNameKeepAlive:
                        *system_name = SCE_NET_SO_KEEPALIVE;
                        break;

                    case kSocketOptionNameBroadcast:
                        *system_name = SCE_NET_SO_BROADCAST;
                        break;

                    case kSocketOptionNameLinger:
                        *system_name = SCE_NET_SO_LINGER;
                        break;

                    case kSocketOptionNameOutOfBandInline:
                        *system_name = SCE_NET_SO_OOBINLINE;
                        break;

                    case kSocketOptionNameSendBuffer:
                        *system_name = SCE_NET_SO_SNDBUF;
                        break;

                    case kSocketOptionNameReceiveBuffer:
                        *system_name = SCE_NET_SO_RCVBUF;
                        break;

                    case kSocketOptionNameSendLowWater:
                        *system_name = SCE_NET_SO_SNDLOWAT;
                        break;

                    case kSocketOptionNameReceiveLowWater:
                        *system_name = SCE_NET_SO_RCVLOWAT;
                        break;

                    case kSocketOptionNameSendTimeout:
                        *system_name = SCE_NET_SO_SNDTIMEO;
                        break;

                    case kSocketOptionNameReceiveTimeout:
                        *system_name = SCE_NET_SO_RCVTIMEO;
                        break;

                    case kSocketOptionNameError:
                        *system_name = SCE_NET_SO_ERROR;
                        break;

                    case kSocketOptionNameType:
                        *system_name = SCE_NET_SO_TYPE;
                        break;

                    case kSocketOptionNameExclusiveAddressUse:
                        *system_name = SCE_NET_SO_REUSEADDR;
                        break;

                    case kSocketOptionNameDebug:
                    case kSocketOptionNameAcceptConnection:
                    case kSocketOptionNameDontRoute:
                    case kSocketOptionNameUseLoopback:
                    case kSocketOptionNameMaxConnections:
                    // Can't figure out how to map these (or not supported), so fall through
                    default:
                        return INVALID_OPTION_NAME;
                }
                break;

            case kSocketOptionLevelIP:
                *system_level = SCE_NET_IPPROTO_IP;

                switch (name)
                {
                    case kSocketOptionNameHeaderIncluded:
                        *system_name = SCE_NET_IP_HDRINCL;
                        break;

                    case kSocketOptionNameTypeOfService:
                        *system_name = SCE_NET_IP_TOS;
                        break;

                    case kSocketOptionNameIpTimeToLive:
                        *system_name = SCE_NET_IP_TTL;
                        break;

                    case kSocketOptionNameMulticastInterface:
                        *system_name = SCE_NET_IP_MULTICAST_IF;
                        break;

                    case kSocketOptionNameMulticastTimeToLive:
                        *system_name = SCE_NET_IP_MULTICAST_TTL;
                        break;

                    case kSocketOptionNameMulticastLoopback:
                        *system_name = SCE_NET_IP_MULTICAST_LOOP;
                        break;

                    case kSocketOptionNameAddMembership:
                        *system_name = SCE_NET_IP_ADD_MEMBERSHIP;
                        break;

                    case kSocketOptionNameDropMembership:
                        *system_name = SCE_NET_IP_DROP_MEMBERSHIP;
                        break;

                    case kSocketOptionNameDontFragment:
                        *system_name = SCE_NET_IP_DONTFRAG;
                        break;

                    case kSocketOptionNamePacketInformation:
                    case kSocketOptionNameIPOptions:
                    case kSocketOptionNameAddSourceMembership:
                    case kSocketOptionNameDropSourceMembership:
                    case kSocketOptionNameBlockSource:
                    case kSocketOptionNameUnblockSource:
                    // Can't figure out how to map these (or not supported), so fall through
                    default:
                        return INVALID_OPTION_NAME;
                }
                break;

            case kSocketOptionLevelTcp:
                *system_level = SCE_NET_IPPROTO_TCP;

                switch (name)
                {
                    case kSocketOptionNameNoDelay:
                        *system_name = SCE_NET_TCP_NODELAY;
                        break;
                    default:
                        return INVALID_OPTION_NAME;
                }
                break;

            case kSocketOptionLevelUdp:
            default:
                return INVALID_OPTION_NAME;
        }

        return 0;
    }
} // os
} // il2cpp

#endif //IL2CPP_TARGET_PSP2
