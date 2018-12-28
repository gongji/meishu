
			float3 interpolation_c2( float3 x ) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }

			// from: https://github.com/BrianSharpe/GPU-Noise-Lib/blob/master/gpu_noise_lib.glsl
			void perlin_hash(float3 gridcell, float s, bool tile, 
								out float4 lowz_hash_0,
								out float4 lowz_hash_1,
								out float4 lowz_hash_2,
								out float4 highz_hash_0,
								out float4 highz_hash_1,
								out float4 highz_hash_2)
			{
				const float2 OFFSET = float2( 50.0, 161.0 );
				const float DOMAIN = 69.0;
				const float3 SOMELARGEFLOATS = float3( 635.298681, 682.357502, 668.926525 );
				const float3 ZINC = float3( 48.500388, 65.294118, 63.934599 );

				gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * ( 1.0 / DOMAIN )) * DOMAIN;
				float d = DOMAIN - 1.5;
				float3 gridcell_inc1 = step( gridcell, float3( d,d,d ) ) * ( gridcell + 1.0 );

				gridcell_inc1 = tile ? gridcell_inc1 % s : gridcell_inc1;

				float4 P = float4( gridcell.xy, gridcell_inc1.xy ) + OFFSET.xyxy;
				P *= P;
				P = P.xzxz * P.yyww;
				float3 lowz_mod = float3( 1.0 / ( SOMELARGEFLOATS.xyz + gridcell.zzz * ZINC.xyz ) );
				float3 highz_mod = float3( 1.0 / ( SOMELARGEFLOATS.xyz + gridcell_inc1.zzz * ZINC.xyz ) );
				lowz_hash_0 = frac( P * lowz_mod.xxxx );
				highz_hash_0 = frac( P * highz_mod.xxxx );
				lowz_hash_1 = frac( P * lowz_mod.yyyy );
				highz_hash_1 = frac( P * highz_mod.yyyy );
				lowz_hash_2 = frac( P * lowz_mod.zzzz );
				highz_hash_2 = frac( P * highz_mod.zzzz );
			}

			// from: https://github.com/BrianSharpe/GPU-Noise-Lib/blob/master/gpu_noise_lib.glsl
			float perlin(float3 P, float s, bool tile) {
				P *= s;

				float3 Pi = floor(P);
				float3 Pi2 = floor(P);
				float3 Pf = P - Pi;
				float3 Pf_min1 = Pf - 1.0;

				float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
				perlin_hash( Pi2, s, tile, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1 );

				float4 grad_x0 = hashx0 - 0.49999;
				float4 grad_y0 = hashy0 - 0.49999;
				float4 grad_z0 = hashz0 - 0.49999;
				float4 grad_x1 = hashx1 - 0.49999;
				float4 grad_y1 = hashy1 - 0.49999;
				float4 grad_z1 = hashz1 - 0.49999;
				float4 grad_results_0 = rsqrt( grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0 ) * ( float2( Pf.x, Pf_min1.x ).xyxy * grad_x0 + float2( Pf.y, Pf_min1.y ).xxyy * grad_y0 + Pf.zzzz * grad_z0 );
				float4 grad_results_1 = rsqrt( grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1 ) * ( float2( Pf.x, Pf_min1.x ).xyxy * grad_x1 + float2( Pf.y, Pf_min1.y ).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1 );

				float3 blend = interpolation_c2( Pf );
				float4 res0 = lerp( grad_results_0, grad_results_1, blend.z );
				float4 blend2 = float4( blend.xy, float2( 1.0 - blend.xy ) );
				float final = dot( res0, blend2.zxzx * blend2.wwyy );
				final *= 1.0/sqrt(0.75);
				return ((final * 1.5) + 1.0) * 0.5;
			}

			float perlin(float3 P) {
				return perlin(P, 1, false);
}

			float get_perlin_5_octaves(float3 p, bool tile) {
				
				float3 xyz = p;
				float amplitude_factor = 0.5;
				float frequency_factor = 2.0;

				float a = 1.0;
				float perlin_value = 0.0;
				perlin_value += a * perlin(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.02);
				perlin_value += a * perlin(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.03);
				perlin_value += a * perlin(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.01);
				perlin_value += a * perlin(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.01);
				perlin_value += a * perlin(xyz).r;

				return perlin_value;
}

			float3 encode_curl(float3 c) {
				return (c + 1.0) * 0.5;
			}

			float3 decode_curl(float3 c) {
				return (c - 0.5) * 2.0;
}

float get_perlin_7_octaves(float3 p, float s) {
				float3 xyz = p;
				float f = 1.0;
				float a = 1.0;

				float perlin_value = 0.0;
				perlin_value += a * perlin(xyz, s * f, true).r; a *= 0.5; f *= 2.0;
				perlin_value += a * perlin(xyz, s * f, true).r; a *= 0.5; f *= 2.0;
				perlin_value += a * perlin(xyz, s * f, true).r; a *= 0.5; f *= 2.0;
				perlin_value += a * perlin(xyz, s * f, true).r; a *= 0.5; f *= 2.0;
				perlin_value += a * perlin(xyz, s * f, true).r; a *= 0.5; f *= 2.0;
				perlin_value += a * perlin(xyz, s * f, true).r; a *= 0.5; f *= 2.0;
				perlin_value += a * perlin(xyz, s * f, true).r;

				return perlin_value;
}


			float3 curl_noise(float3 pos) {
				float e = 0.05;
				float n1, n2, a, b;
				float3 c;

				n1 = get_perlin_5_octaves(pos.xyz + float3( 0, e, 0), true);
				n2 = get_perlin_5_octaves(pos.xyz + float3( 0,-e, 0), true);
				a = (n1-n2)/(2*e);
				n1 = get_perlin_5_octaves(pos.xyz + float3( 0, 0, e), true);
				n2 = get_perlin_5_octaves(pos.xyz + float3( 0, 0,-e), true);
				b = (n1-n2)/(2*e);

				c.x = a - b;

				n1 = get_perlin_5_octaves(pos.xyz + float3( 0, 0, e), true);
				n2 = get_perlin_5_octaves(pos.xyz + float3( 0, 0,-e), true);
				a = (n1-n2)/(2*e);
				n1 = get_perlin_5_octaves(pos.xyz + float3( e, 0, 0), true);
				n2 = get_perlin_5_octaves(pos.xyz + float3(-e, 0, 0), true);
				b = (n1-n2)/(2*e);

				c.y = a - b;

				n1 = get_perlin_5_octaves(pos.xyz + float3( e, 0, 0), false);
				n2 = get_perlin_5_octaves(pos.xyz + float3(-e, 0, 0), false);
				a = (n1-n2)/(2*e);
				n1 = get_perlin_5_octaves(pos.xyz + float3( 0, e, 0), false);
				n2 = get_perlin_5_octaves(pos.xyz + float3( 0,-e, 0), false);
				b = (n1-n2)/(2*e);

				c.z = a - b;

				return c;
}

float3 voronoi_hash( float3 x, float s) {
				x = x % s;
				x = float3( dot(x, float3(127.1,311.7, 74.7)),
							dot(x, float3(269.5,183.3,246.1)),
							dot(x, float3(113.5,271.9,124.6)));
				
				return frac(sin(x) * 43758.5453123);
			}

			float3 voronoi( in float3 x, float s, bool inverted) {
				x *= s;
				x += 0.5;
				float3 p = floor(x);
				float3 f = frac(x);

				float id = 0.0;
				float2 res = float2( 1.0 , 1.0);
				for(int k=-1; k<=1; k++){
					for(int j=-1; j<=1; j++) {
						for(int i=-1; i<=1; i++) {
							float3 b = float3(i, j, k);
							float3 r = float3(b) - f + voronoi_hash(p + b, s);
							float d = dot(r, r);

							if(d < res.x) {
								id = dot(p + b, float3(1.0, 57.0, 113.0));
								res = float2(d, res.x);			
							} else if(d < res.y) {
								res.y = d;
							}
						}
					}
				}

				float2 result = res;//sqrt(res);
				id = abs(id);

				if(inverted)
					return float3(1.0 - result, id);
				else
					return float3(result, id);
			}

			float get_worley_2_octaves(float3 p, float3 offset) {
				float3 xyz = p + offset;

				float worley_value1 = voronoi(xyz, 1.0, true).r;
				float worley_value2 = voronoi(xyz, 2.0, false).r;

				worley_value1 = saturate(worley_value1);
				worley_value2 = saturate(worley_value2);

				float worley_value = worley_value1;
				worley_value = worley_value - worley_value2 * 0.25;

				return worley_value;;
			}

			float get_worley_3_octaves(float3 p, float s) {
				float3 xyz = p;

				float worley_value1 = voronoi(xyz, 1.0 * s, true).r;
				float worley_value2 = voronoi(xyz, 2.0 * s, false).r;
				float worley_value3 = voronoi(xyz, 4.0 * s, false).r;

				worley_value1 = saturate(worley_value1);
				worley_value2 = saturate(worley_value2);
				worley_value3 = saturate(worley_value3);

				float worley_value = worley_value1;
				worley_value = worley_value - worley_value2 * 0.3;
				worley_value = worley_value - worley_value3 * 0.3;

				return worley_value;;
}

			float3 mod(float3 x, float3 y)
			{
				return x - y * floor(x / y);
			}

			float3 mod289(float3 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float4 mod289(float4 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float4 permute(float4 x)
			{
				return mod289(((x*34.0) + 1.0)*x);
			}

			float4 taylorInvSqrt(float4 r)
			{
				return (float4)1.79284291400159 - r * 0.85373472095314;
			}

			float3 fade(float3 t) {
				return t*t*t*(t*(t*6.0 - 15.0) + 10.0);
			}

			// Classic Perlin noise
			float cnoise(float3 P)
			{
				float3 Pi0 = floor(P); // Integer part for indexing
				float3 Pi1 = Pi0 + (float3)1.0; // Integer part + 1
				Pi0 = mod289(Pi0);
				Pi1 = mod289(Pi1);
				float3 Pf0 = frac(P); // Fractional part for interpolation
				float3 Pf1 = Pf0 - (float3)1.0; // Fractional part - 1.0
				float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
				float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
				float4 iz0 = (float4)Pi0.z;
				float4 iz1 = (float4)Pi1.z;

				float4 ixy = permute(permute(ix) + iy);
				float4 ixy0 = permute(ixy + iz0);
				float4 ixy1 = permute(ixy + iz1);

				float4 gx0 = ixy0 / 7.0;
				float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
				gx0 = frac(gx0);
				float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
				float4 sz0 = step(gz0, (float4)0.0);
				gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
				gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

				float4 gx1 = ixy1 / 7.0;
				float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
				gx1 = frac(gx1);
				float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
				float4 sz1 = step(gz1, (float4)0.0);
				gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
				gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

				float3 g000 = float3(gx0.x, gy0.x, gz0.x);
				float3 g100 = float3(gx0.y, gy0.y, gz0.y);
				float3 g010 = float3(gx0.z, gy0.z, gz0.z);
				float3 g110 = float3(gx0.w, gy0.w, gz0.w);
				float3 g001 = float3(gx1.x, gy1.x, gz1.x);
				float3 g101 = float3(gx1.y, gy1.y, gz1.y);
				float3 g011 = float3(gx1.z, gy1.z, gz1.z);
				float3 g111 = float3(gx1.w, gy1.w, gz1.w);

				float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
				g000 *= norm0.x;
				g010 *= norm0.y;
				g100 *= norm0.z;
				g110 *= norm0.w;

				float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
				g001 *= norm1.x;
				g011 *= norm1.y;
				g101 *= norm1.z;
				g111 *= norm1.w;

				float n000 = dot(g000, Pf0);
				float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
				float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
				float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
				float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
				float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
				float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
				float n111 = dot(g111, Pf1);

				float3 fade_xyz = fade(Pf0);
				float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
				float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
				float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
				return 2.2 * n_xyz;
			}

			// Classic Perlin noise, periodic variant
			float pnoise(float3 P, float3 rep)
			{
				float3 Pi0 = mod(floor(P), rep); // Integer part, modulo period
				float3 Pi1 = mod(Pi0 + (float3)1.0, rep); // Integer part + 1, mod period
				Pi0 = mod289(Pi0);
				Pi1 = mod289(Pi1);
				float3 Pf0 = frac(P); // Fractional part for interpolation
				float3 Pf1 = Pf0 - (float3)1.0; // Fractional part - 1.0
				float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
				float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
				float4 iz0 = (float4)Pi0.z;
				float4 iz1 = (float4)Pi1.z;

				float4 ixy = permute(permute(ix) + iy);
				float4 ixy0 = permute(ixy + iz0);
				float4 ixy1 = permute(ixy + iz1);

				float4 gx0 = ixy0 / 7.0;
				float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
				gx0 = frac(gx0);
				float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
				float4 sz0 = step(gz0, (float4)0.0);
				gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
				gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

				float4 gx1 = ixy1 / 7.0;
				float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
				gx1 = frac(gx1);
				float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
				float4 sz1 = step(gz1, (float4)0.0);
				gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
				gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

				float3 g000 = float3(gx0.x, gy0.x, gz0.x);
				float3 g100 = float3(gx0.y, gy0.y, gz0.y);
				float3 g010 = float3(gx0.z, gy0.z, gz0.z);
				float3 g110 = float3(gx0.w, gy0.w, gz0.w);
				float3 g001 = float3(gx1.x, gy1.x, gz1.x);
				float3 g101 = float3(gx1.y, gy1.y, gz1.y);
				float3 g011 = float3(gx1.z, gy1.z, gz1.z);
				float3 g111 = float3(gx1.w, gy1.w, gz1.w);

				float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
				g000 *= norm0.x;
				g010 *= norm0.y;
				g100 *= norm0.z;
				g110 *= norm0.w;
				float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
				g001 *= norm1.x;
				g011 *= norm1.y;
				g101 *= norm1.z;
				g111 *= norm1.w;

				float n000 = dot(g000, Pf0);
				float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
				float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
				float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
				float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
				float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
				float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
				float n111 = dot(g111, Pf1);

				float3 fade_xyz = fade(Pf0);
				float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
				float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
				float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
				return 2.2 * n_xyz;
			}

			float perlin5oct(float3 p) {

				float3 xyz = p;
				float amplitude_factor = 0.5;
				float frequency_factor = 2.0;

				float a = 1.0;
				float perlin_value = 0.0;
				perlin_value += a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.02);
				perlin_value += a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.03);
				perlin_value += a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.01);
				perlin_value += a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.01);
				perlin_value += a * cnoise(xyz).r;

				return perlin_value;
			}