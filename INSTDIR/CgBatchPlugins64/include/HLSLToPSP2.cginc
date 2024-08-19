// This file defines an assortment of types and functions to aid compatibility when compiling cross-platform shaders on the
// PSVita platform and is automatically included when compiling any shader.
//
// If required this file can be overridden by copying it to the root of your project folder and then modifying it as required.


float4 mad(float4 m, float4 a, float4 d) { return m * a + d; }
float3 mad(float3 m, float3 a, float3 d) { return m * a + d; }
float2 mad(float2 m, float2 a, float2 d) { return m * a + d; }
float mad(float m, float a, float d) { return m * a + d; }

float clamp(int a, int b, int c) { return clamp((float)a, (float)b, (float)c); }

half  lerp(half  a, half  b, float t) { return lerp(a,b,(half)t); }
half2 lerp(half2 a, half2 b, float t) { return lerp(a,b,(half)t); }
half3 lerp(half3 a, half3 b, float t) { return lerp(a,b,(half)t); }
half4 lerp(half4 a, half4 b, float t) { return lerp(a,b,(half)t); }
half2 lerp(half3 a, half3 b, half2 t) { return lerp((half2)a,(half2)b,t); }
half2 lerp(half4 a, half4 b, half2 t) { return lerp((half2)a,(half2)b,t); }
half3 lerp(half4 a, half4 b, half3 t) { return lerp((half3)a,(half3)b,t); }
half3 lerp(half3 a, half3 b, bool t) { return lerp(a,b,(half)t); }
float2 lerp(float3 a, float3 b, float2 t) { return lerp((float2)a,(float2)b,t); }
float2 lerp(float4 a, float4 b, float2 t) { return lerp((float2)a,(float2)b,t); }
float3 lerp(float4 a, float4 b, float3 t) { return lerp((float3)a,(float3)b,t); }
half lerp(int a, int b, half t) { return lerp((half)a,(half)b,t); }
float lerp(int a, int b, float t) { return lerp((float)a,(float)b,t); }

float abs(int a) { return abs((float)a); }
void clip(int v) { clip((fixed)v); }
float exp2(int v) { return exp2((float)v); }
float min(int a, int b) { return min((float)a, (float)b); }

#if !defined(SV_POSITION)
#    define SV_POSITION POSITION
#endif

#if !defined(SV_Target)
#    define SV_Target COLOR
#endif
#if !defined(SV_Target0)
#    define SV_Target0 COLOR0
#endif
#if !defined(SV_Target1)
#    define SV_Target1 COLOR1
#endif
#if !defined(SV_Target2)
#    define SV_Target2 COLOR2
#endif
#if !defined(SV_Target3)
#    define SV_Target3 COLOR3
#endif
#if !defined(SV_Depth)
#    define SV_Depth DEPTH
#endif
