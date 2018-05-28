// Displacement with Dispersion 
// by cornusammonis from shadertoy
// 
// https://www.shadertoy.com/view/4ldGDB
//

/*
	A fracturing dynamical system
	see: https://www.shadertoy.com/view/MsyXRW
*/

#define _G0 0.25
#define _G1 0.125
#define _G2 0.0625
#define W0 -3.0
#define W1 0.5
#define TIMESTEP 0.1
#define ADVECT_DIST 2.0
#define DV 0.70710678
#define SQRT_3_OVER_2 0.86602540378
#define SQRT_3_OVER_2_INV 0.13397459621
#define DV 0.70710678

float2 diagH(float2 x, float2 x_v, float2 x_h, float2 x_d) {
	return 0.5 * ((x + x_v) * SQRT_3_OVER_2_INV + (x_h + x_d) * SQRT_3_OVER_2);
}

float2 diagV(float2 x, float2 x_v, float2 x_h, float2 x_d) {
	return 0.5 * ((x + x_h) * SQRT_3_OVER_2_INV + (x_v + x_d) * SQRT_3_OVER_2);
}

// nonlinearity
float nl(float x) {
	return 1.0 / (1.0 + exp(W0 * (W1 * x - 0.5)));
}

float4 gaussian(float4 x, float4 x_nw, float4 x_n, float4 x_ne, float4 x_w, float4 x_e, float4 x_sw, float4 x_s, float4 x_se) {
	return _G0 * x + _G1 * (x_n + x_e + x_w + x_s) + _G2 * (x_nw + x_sw + x_ne + x_se);
}

float2 normz(float2 x) {
	return x == float2(0.0, 0.0) ? float2(0.0, 0.0) : normalize(x);
}

float3 normz(float3 x) {
	return x == float3(0.0, 0.0, 0.0) ? float3(0.0, 0.0, 0.0) : normalize(x);
}

float4 adfloatt(
	in Texture2D<float4> tex, 
	in SamplerState state,  
	float2 vUv, float2 ab, float2 step) {

	float2 aUv = vUv - ab * ADVECT_DIST * step;

	float2 n = float2(0.0, step.y);
	float2 ne = float2(step.x, step.y);
	float2 e = float2(step.x, 0.0);
	float2 se = float2(step.x, -step.y);
	float2 s = float2(0.0, -step.y);
	float2 sw = float2(-step.x, -step.y);
	float2 w = float2(-step.x, 0.0);
	float2 nw = float2(-step.x, step.y);

	float4 u = tex.SampleLevel(state, frac(aUv), 0);
	float4 u_n = tex.SampleLevel(state, frac(aUv + n), 0);
	float4 u_e = tex.SampleLevel(state, frac(aUv + e), 0);
	float4 u_s = tex.SampleLevel(state, frac(aUv + s), 0);
	float4 u_w = tex.SampleLevel(state, frac(aUv + w), 0);
	float4 u_nw = tex.SampleLevel(state, frac(aUv + nw), 0);
	float4 u_sw = tex.SampleLevel(state, frac(aUv + sw), 0);
	float4 u_ne = tex.SampleLevel(state, frac(aUv + ne), 0);
	float4 u_se = tex.SampleLevel(state, frac(aUv + se), 0);

	return gaussian(u, u_nw, u_n, u_ne, u_w, u_e, u_sw, u_s, u_se);
}

float2 Ma(
	in Texture2D<float4> tex, 
	in SamplerState state, 
	in float2 uv, float2 step) 
{
	float2 n = float2(0.0, step.y);
	float2 ne = float2(step.x, step.y);
	float2 e = float2(step.x, 0.0);
	float2 se = float2(step.x, -step.y);
	float2 s = float2(0.0, -step.y);
	float2 sw = float2(-step.x, -step.y);
	float2 w = float2(-step.x, 0.0);
	float2 nw = float2(-step.x, step.y);

	float4 u = tex.SampleLevel(state, frac(uv), 0);
	float4 u_n = tex.SampleLevel(state, frac(uv + n), 0);
	float4 u_e = tex.SampleLevel(state, frac(uv + e), 0);
	float4 u_s = tex.SampleLevel(state, frac(uv + s), 0);
	float4 u_w = tex.SampleLevel(state, frac(uv + w), 0);
	float4 u_nw = tex.SampleLevel(state, frac(uv + nw), 0);
	float4 u_sw = tex.SampleLevel(state, frac(uv + sw), 0);
	float4 u_ne = tex.SampleLevel(state, frac(uv + ne), 0);
	float4 u_se = tex.SampleLevel(state, frac(uv + se), 0);

	const float vx = 0.5;
	const float vy = SQRT_3_OVER_2;
	const float hx = SQRT_3_OVER_2;
	const float hy = 0.5;

	float di_n = nl(distance(u_n.xy + n, u.xy));
	float di_w = nl(distance(u_w.xy + w, u.xy));
	float di_e = nl(distance(u_e.xy + e, u.xy));
	float di_s = nl(distance(u_s.xy + s, u.xy));

	float di_nne = nl(distance((diagV(u.xy, u_n.xy, u_e.xy, u_ne.xy) + float2(+vx, +vy)), u.xy));
	float di_ene = nl(distance((diagH(u.xy, u_n.xy, u_e.xy, u_ne.xy) + float2(+hx, +hy)), u.xy));
	float di_ese = nl(distance((diagH(u.xy, u_s.xy, u_e.xy, u_se.xy) + float2(+hx, -hy)), u.xy));
	float di_sse = nl(distance((diagV(u.xy, u_s.xy, u_e.xy, u_se.xy) + float2(+vx, -vy)), u.xy));
	float di_ssw = nl(distance((diagV(u.xy, u_s.xy, u_w.xy, u_sw.xy) + float2(-vx, -vy)), u.xy));
	float di_wsw = nl(distance((diagH(u.xy, u_s.xy, u_w.xy, u_sw.xy) + float2(-hx, -hy)), u.xy));
	float di_wnw = nl(distance((diagH(u.xy, u_n.xy, u_w.xy, u_nw.xy) + float2(-hx, +hy)), u.xy));
	float di_nnw = nl(distance((diagV(u.xy, u_n.xy, u_w.xy, u_nw.xy) + float2(-vx, +vy)), u.xy));

	float2 xy_n = u_n.xy + n - u.xy;
	float2 xy_w = u_w.xy + w - u.xy;
	float2 xy_e = u_e.xy + e - u.xy;
	float2 xy_s = u_s.xy + s - u.xy;

	float2 xy_nne = (diagV(u.xy, u_n.xy, u_e.xy, u_ne.xy) + float2(+vx, +vy)) - u.xy;
	float2 xy_ene = (diagH(u.xy, u_n.xy, u_e.xy, u_ne.xy) + float2(+hx, +hy)) - u.xy;
	float2 xy_ese = (diagH(u.xy, u_s.xy, u_e.xy, u_se.xy) + float2(+hx, -hy)) - u.xy;
	float2 xy_sse = (diagV(u.xy, u_s.xy, u_e.xy, u_se.xy) + float2(+vx, -vy)) - u.xy;
	float2 xy_ssw = (diagV(u.xy, u_s.xy, u_w.xy, u_sw.xy) + float2(-vx, -vy)) - u.xy;
	float2 xy_wsw = (diagH(u.xy, u_s.xy, u_w.xy, u_sw.xy) + float2(-hx, -hy)) - u.xy;
	float2 xy_wnw = (diagH(u.xy, u_n.xy, u_w.xy, u_nw.xy) + float2(-hx, +hy)) - u.xy;
	float2 xy_nnw = (diagV(u.xy, u_n.xy, u_w.xy, u_nw.xy) + float2(-vx, +vy)) - u.xy;

	float2 ma = di_nne * xy_nne + di_ene * xy_ene + di_ese * xy_ese + di_sse * xy_sse + di_ssw * xy_ssw + di_wsw * xy_wsw + di_wnw * xy_wnw + di_nnw * xy_nnw + di_n * xy_n + di_w * xy_w + di_e * xy_e + di_s * xy_s;

	return ma;
}