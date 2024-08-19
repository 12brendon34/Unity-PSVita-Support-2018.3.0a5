#include <kernel.h>

#define MB_NONE
#define MB_ISB      __builtin_isb ()
#define MB_DMB      __builtin_dmb ()

static inline void atomic_thread_fence(memory_order_relaxed_t)
{
}

static inline void atomic_thread_fence(memory_order_release_t)
{
    MB_DMB;
}

static inline void atomic_thread_fence(memory_order_acquire_t)
{
    MB_DMB;
}

static inline void atomic_thread_fence(memory_order_acq_rel_t)
{
    MB_DMB;
}

static inline void atomic_thread_fence(int /* memory_order_seq_cst_t */)
{
    MB_DMB;
}

static inline atomic_word atomic_load_explicit(const volatile atomic_word* p, memory_order_relaxed_t)
{
    return *p;
}

static inline atomic_word atomic_load_explicit(const volatile atomic_word* p, memory_order_acquire_t)
{
    atomic_word res = *p;
    MB_DMB;
    return res;
}

static inline atomic_word atomic_load_explicit(const volatile atomic_word* p, int /* memory_order_seq_cst_t */)
{
    atomic_word res = *p;
    MB_DMB;
    return res;
}

static inline void atomic_store_explicit(volatile atomic_word* p, atomic_word v, memory_order_relaxed_t)
{
    *p = v;
}

static inline void atomic_store_explicit(volatile atomic_word* p, atomic_word v, memory_order_release_t)
{
    MB_DMB;
    *p = v;
}

static inline void atomic_store_explicit(volatile atomic_word* p, atomic_word v, int /* memory_order_seq_cst_t */)
{
    MB_DMB;
    *p = v;
    MB_DMB;
}

#define ATOMIC_XCHG(PRE, POST) \
    unsigned int old; \
    unsigned int newval; \
    PRE; \
    do \
    { \
        old = __ldrex((volatile int*) p); \
        newval = (unsigned int) v; \
    } while (__strex(newval, (volatile int*) p) != 0); \
    POST; \
    return (atomic_word) old;

static inline atomic_word atomic_exchange_explicit(volatile atomic_word* p, atomic_word v, memory_order_relaxed_t)
{
    ATOMIC_XCHG(MB_NONE, MB_NONE)
}

static inline atomic_word atomic_exchange_explicit(volatile atomic_word* p, atomic_word v, memory_order_release_t)
{
    ATOMIC_XCHG(MB_DMB, MB_NONE)
}

static inline atomic_word atomic_exchange_explicit(volatile atomic_word* p, atomic_word v, memory_order_acquire_t)
{
    ATOMIC_XCHG(MB_NONE, MB_ISB)
}

static inline atomic_word atomic_exchange_explicit(volatile atomic_word* p, atomic_word v, memory_order_acq_rel_t)
{
    ATOMIC_XCHG(MB_DMB, MB_ISB)
}

static inline atomic_word atomic_exchange_explicit(volatile atomic_word* p, atomic_word v, int /* memory_order_seq_cst_t */)
{
    ATOMIC_XCHG(MB_DMB, MB_DMB)
}

// atomic_compare_exchange_weak_explicit: can fail spuriously even if *p == *oldval

#define ATOMIC_CMP_XCHG(PRE, SUCC, POST) \
    unsigned int old; \
    atomic_word tmp = *oldval; \
    bool res = false; \
    PRE; \
    old = __ldrex ((volatile int*) p); \
    if ((atomic_word) old == tmp) \
    { \
        res = __strex ((unsigned int) newval, (volatile int*) p) == 0; \
        if (res) \
        { \
            SUCC; \
        } \
    } \
    POST; \
    *oldval = (atomic_word) old; \
    return res;

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_relaxed_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_NONE, MB_NONE, MB_NONE)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_release_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_NONE, MB_NONE)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acquire_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_NONE, MB_ISB, MB_NONE)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acq_rel_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_ISB, MB_NONE)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, int /* memory_order_seq_cst_t */, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_DMB, MB_NONE)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_release_t, memory_order_release_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_NONE, MB_NONE)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acquire_t, memory_order_acquire_t)
{
    ATOMIC_CMP_XCHG(MB_NONE, MB_NONE, MB_ISB)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acq_rel_t, memory_order_acq_rel_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_NONE, MB_ISB)
}

static inline bool atomic_compare_exchange_weak_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, int /* memory_order_seq_cst_t */, int /* memory_order_seq_cst_t */)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_NONE, MB_DMB)
}

// atomic_compare_exchange_strong_explicit: does loop and only returns false if *p != *oldval

#undef ATOMIC_CMP_XCHG
#define ATOMIC_CMP_XCHG(PRE, POST) \
    unsigned int old; \
    atomic_word tmp = *oldval; \
    bool res = false; \
    PRE; \
    do \
    { \
        old = __ldrex ((volatile int*) p); \
        if ((atomic_word) old != tmp) \
        { \
            goto end; \
        } \
        res = __strex ((unsigned int) newval, (volatile int*) p) == 0; \
    } while (!res); \
    POST; \
end: \
    *oldval = (atomic_word) old; \
    return res;

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_relaxed_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_NONE, MB_NONE)
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_release_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_NONE)
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acquire_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_NONE, MB_ISB)
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acq_rel_t, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_ISB)
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, int /* memory_order_seq_cst_t */, memory_order_relaxed_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_DMB)
}

#undef ATOMIC_CMP_XCHG
#define ATOMIC_CMP_XCHG(PRE, POST) \
    unsigned int old; \
    atomic_word tmp = *oldval; \
    bool res = false; \
    PRE; \
    do \
    { \
        old = __ldrex ((volatile int*) p); \
        if ((atomic_word) old != tmp) \
        { \
            break; \
        } \
        res = __strex ((unsigned int) newval, (volatile int*) p) == 0; \
    } while (!res); \
    POST; \
    *oldval = (atomic_word) old; \
    return res;

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_release_t, memory_order_release_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_NONE)
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acquire_t, memory_order_acquire_t)
{
    ATOMIC_CMP_XCHG(MB_NONE, MB_ISB)
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, memory_order_acq_rel_t, memory_order_acq_rel_t)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_ISB)
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word* p, atomic_word *oldval, atomic_word newval, int /* memory_order_seq_cst_t */, int /* memory_order_seq_cst_t */)
{
    ATOMIC_CMP_XCHG(MB_DMB, MB_DMB)
}

#define ATOMIC_OP(PRE, OP, POST) \
    unsigned int old; \
    unsigned int newval; \
    PRE; \
    do \
    { \
        old = __ldrex((volatile int*) p); \
        newval = old OP (unsigned int) v; \
    } while (__strex(newval, (volatile int*) p) != 0); \
    POST; \
    return (atomic_word) old;

static inline atomic_word atomic_fetch_add_explicit(volatile atomic_word* p, atomic_word v, memory_order_relaxed_t)
{
    ATOMIC_OP(MB_NONE, +, MB_NONE)
}

static inline atomic_word atomic_fetch_add_explicit(volatile atomic_word* p, atomic_word v, memory_order_release_t)
{
    ATOMIC_OP(MB_DMB, +, MB_NONE)
}

static inline atomic_word atomic_fetch_add_explicit(volatile atomic_word* p, atomic_word v, memory_order_acquire_t)
{
    ATOMIC_OP(MB_NONE, +, MB_ISB)
}

static inline atomic_word atomic_fetch_add_explicit(volatile atomic_word* p, atomic_word v, memory_order_acq_rel_t)
{
    ATOMIC_OP(MB_DMB, +, MB_ISB)
}

static inline atomic_word atomic_fetch_add_explicit(volatile atomic_word* p, atomic_word v, int /* memory_order_seq_cst_t */)
{
    ATOMIC_OP(MB_DMB, +, MB_DMB)
}

static inline atomic_word atomic_fetch_sub_explicit(volatile atomic_word* p, atomic_word v, memory_order_relaxed_t)
{
    ATOMIC_OP(MB_NONE, -, MB_NONE)
}

static inline atomic_word atomic_fetch_sub_explicit(volatile atomic_word* p, atomic_word v, memory_order_release_t)
{
    ATOMIC_OP(MB_DMB, -, MB_NONE)
}

static inline atomic_word atomic_fetch_sub_explicit(volatile atomic_word* p, atomic_word v, memory_order_acquire_t)
{
    ATOMIC_OP(MB_NONE, -, MB_ISB)
}

static inline atomic_word atomic_fetch_sub_explicit(volatile atomic_word* p, atomic_word v, memory_order_acq_rel_t)
{
    ATOMIC_OP(MB_DMB, -, MB_ISB)
}

static inline atomic_word atomic_fetch_sub_explicit(volatile atomic_word* p, atomic_word v, int /* memory_order_seq_cst_t */)
{
    ATOMIC_OP(MB_DMB, -, MB_DMB)
}

/*
 *  extensions
 */

static inline void atomic_retain(volatile int* p)
{
    atomic_fetch_add_explicit(p, 1, memory_order_relaxed);
}

static inline bool atomic_release(volatile int* p)
{
    bool res = atomic_fetch_sub_explicit(p, 1, memory_order_release) == 1;
    if (res)
    {
        atomic_thread_fence(memory_order_acquire);
    }
    return res;
}

/*
 *  double word
 */

// Note: the only way to get atomic 64-bit memory accesses on ARM is to use ldrexd/strexd with a loop
// (ldrd and strd instructions are not guaranteed to appear atomic)

static inline atomic_word2 atomic_load_explicit(const volatile atomic_word2* p, int)
{
    atomic_word2 val;
    do
    {
        val.v = __ldrexd((volatile long long*)p);
    }
    while (__strexd(val.v, (volatile long long*)p) != 0);

    return val;
}

static inline void atomic_store_explicit(volatile atomic_word2* p, atomic_word2 v, memory_order_relaxed_t)
{
    do
    {
        __ldrexd((volatile long long*)p);
    }
    while (__strexd(v.v, (volatile long long*)p) != 0);
}

static inline void atomic_store_explicit(volatile atomic_word2* p, atomic_word2 v, memory_order_release_t)
{
    MB_DMB;
    do
    {
        __ldrexd((volatile long long*)p);
    }
    while (__strexd(v.v, (volatile long long*)p) != 0);
}

static inline atomic_word2 atomic_exchange_explicit(volatile atomic_word2* p, atomic_word2 v, memory_order_acq_rel_t)
{
    atomic_word2 old;

    MB_DMB;
    do
    {
        old.v = __ldrexd((volatile long long*)p);
    }
    while (__strexd(v.v, (volatile long long*)p) != 0);
    MB_ISB;

    return old;
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_relaxed_t, memory_order_relaxed_t)
{
    atomic_word2 old;

    bool res = false;
    do
    {
        old.v = __ldrexd((volatile long long*)p);
        if (old.v != oldval->v)
        {
            break;
        }
        res = __strexd(newval.v, (volatile long long*)p) == 0;
    }
    while (!res);

    *oldval = old;

    return res;
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_acquire_t, memory_order_acquire_t)
{
    atomic_word2 old;

    bool res = false;
    do
    {
        old.v = __ldrexd((volatile long long*)p);
        if (old.v != oldval->v)
        {
            break;
        }
        res = __strexd(newval.v, (volatile long long*)p) == 0;
    }
    while (!res);
    MB_ISB;

    *oldval = old;

    return res;
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_release_t, memory_order_release_t)
{
    atomic_word2 old;

    bool res = false;
    MB_DMB;
    do
    {
        old.v = __ldrexd((volatile long long*)p);
        if (old.v != oldval->v)
        {
            break;
        }
        res = __strexd(newval.v, (volatile long long*)p) == 0;
    }
    while (!res);

    *oldval = old;

    return res;
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_acq_rel_t, memory_order_acq_rel_t)
{
    atomic_word2 old;

    bool res = false;
    MB_DMB;
    do
    {
        old.v = __ldrexd((volatile long long*)p);
        if (old.v != oldval->v)
        {
            break;
        }
        res = __strexd(newval.v, (volatile long long*)p) == 0;
    }
    while (!res);
    MB_ISB;

    *oldval = old;

    return res;
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_seq_cst_t, memory_order_seq_cst_t)
{
    atomic_word2 old;

    bool res = false;
    MB_DMB;
    do
    {
        old.v = __ldrexd((volatile long long*)p);
        if (old.v != oldval->v)
        {
            break;
        }
        res = __strexd(newval.v, (volatile long long*)p) == 0;
    }
    while (!res);
    MB_DMB;

    *oldval = old;

    return res;
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_acquire_t, memory_order_relaxed_t)
{
    return atomic_compare_exchange_strong_explicit(p, oldval, newval, memory_order_acquire, memory_order_acquire);
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_release_t, memory_order_relaxed_t)
{
    return atomic_compare_exchange_strong_explicit(p, oldval, newval, memory_order_release, memory_order_release);
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_acq_rel_t, memory_order_relaxed_t)
{
    return atomic_compare_exchange_strong_explicit(p, oldval, newval, memory_order_acq_rel, memory_order_acq_rel);
}

static inline bool atomic_compare_exchange_strong_explicit(volatile atomic_word2* p, atomic_word2* oldval, atomic_word2 newval, memory_order_seq_cst_t, memory_order_relaxed_t)
{
    return atomic_compare_exchange_strong_explicit(p, oldval, newval, memory_order_seq_cst, memory_order_seq_cst);
}
