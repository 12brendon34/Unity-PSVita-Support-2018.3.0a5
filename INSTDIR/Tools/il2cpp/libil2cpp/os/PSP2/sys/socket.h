#include <scetypes.h>
#include <net.h>
#include <net/socket.h>


/* protocol */
#define IPPROTO_IP          SCE_NET_IPPROTO_IP            //0
#define IPPROTO_ICMP        SCE_NET_IPPROTO_ICMP          //1
#define IPPROTO_IGMP        SCE_NET_IPPROTO_IGMP          //2
#define IPPROTO_TCP         SCE_NET_IPPROTO_TCP           //6
#define IPPROTO_UDP         SCE_NET_IPPROTO_UDP           //17
#define SOL_SOCKET          SCE_NET_SOL_SOCKET            //0xffff


/* address_family */
#define AF_UNSPEC           SCE_NET_EAFNOSUPPORT
#define AF_INET             SCE_NET_AF_INET

/* socket types */
#define SOCK_STREAM         SCE_NET_SOCK_STREAM             //1
#define SOCK_DGRAM          SCE_NET_SOCK_DGRAM              //2
#define SOCK_RAW            SCE_NET_SOCK_RAW                //3
#define SOCK_DGRAM_P2P      SCE_NET_SOCK_DGRAM_P2P          //6
#define SOCK_STREAM_P2P     SCE_NET_SOCK_STREAM_P2P         //10


#define TCP_NODELAY         SCE_NET_TCP_NODELAY

#define SHUT_RD             SCE_NET_SHUT_RD                 //0
#define SHUT_WR             SCE_NET_SHUT_WR                 //1
#define SHUT_RDWR           SCE_NET_SHUT_RDWR               //2

/* socket options */
#define SOL_SOCKET          SCE_NET_SOL_SOCKET
#define SO_LINGER           SCE_NET_SO_LINGER
#define SO_REUSEADDR        SCE_NET_SO_REUSEADDR
#define SO_KEEPALIVE        SCE_NET_SO_KEEPALIVE
#define SO_BROADCAST        SCE_NET_SO_BROADCAST
#define SO_OOBINLINE        SCE_NET_SO_OOBINLINE
#define SO_SNDBUF           SCE_NET_SO_SNDBUF
#define SO_RCVBUF           SCE_NET_SO_RCVBUF
#define SO_SNDLOWAT         SCE_NET_SO_SNDLOWAT
#define SO_RCVLOWAT         SCE_NET_SO_RCVLOWAT
#define SO_SNDTIMEO         SCE_NET_SO_SNDTIMEO
#define SO_RCVTIMEO         SCE_NET_SO_RCVTIMEO
#define SO_ERROR            SCE_NET_SO_ERROR
#define SO_TYPE             SCE_NET_SO_TYPE

/* socket option (IP) */
#define IP_HDRINCL          SCE_NET_IP_HDRINCL            //2
#define IP_TOS              SCE_NET_IP_TOS                //3
#define IP_TTL              SCE_NET_IP_TTL                //4
#define IP_MULTICAST_IF     SCE_NET_IP_MULTICAST_IF       //9
#define IP_MULTICAST_TTL    SCE_NET_IP_MULTICAST_TTL      //10
#define IP_MULTICAST_LOOP   SCE_NET_IP_MULTICAST_LOOP     //11
#define IP_ADD_MEMBERSHIP   SCE_NET_IP_ADD_MEMBERSHIP     //12
#define IP_DROP_MEMBERSHIP  SCE_NET_IP_DROP_MEMBERSHIP    //13
#define IP_TTLCHK           SCE_NET_IP_TTLCHK             //23
#define IP_MAXTTL           SCE_NET_IP_MAXTTL             //24
#define IP_DONTFRAG         SCE_NET_IP_DONTFRAG           //26

/* socket option (TCP) */
#define TCP_NODELAY         SCE_NET_TCP_NODELAY           //1
#define TCP_MAXSEG          SCE_NET_TCP_MAXSEG            //2
#define TCP_MSS_TO_ADVERTISE SCE_NET_TCP_MSS_TO_ADVERTISE  //3

#define sockaddr_in         SceNetSockaddrIn
#define sockaddr            SceNetSockaddr
#define in_addr             SceNetInAddr
#define socklen_t           SceNetSocklen_t
#define inet_addr           SceNetInAddr

struct hostent
{
    char     *h_name;
    char    **h_aliases;
    short     h_addrtype;
    short     h_length;
    char    **h_addr_list;
};

#define gethostbyaddr(addr, len, type)  NULL

struct timeval { int tv_sec; int tv_usec; };
