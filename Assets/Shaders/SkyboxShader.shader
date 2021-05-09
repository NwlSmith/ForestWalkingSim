// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SkyboxShader"
{
	Properties
	{
		_SkyColorBottom("Sky Color Bottom", Color) = (0.07471521,0.4779798,0.6886792,0)
		_LandColor("Land Color", Color) = (0.389596,0.8018868,0.442467,0)
		_MidColor("Mid Color", Color) = (0.07471521,0.4779798,0.6886792,0)
		_speed("speed", Float) = 0
		_blend("blend", Float) = 0
		_CloudColor("Cloud Color", Color) = (1,1,1,0)
		_MidColor2("Mid Color 2", Color) = (0.5425863,0.7506646,0.8396226,0)
		_SkyColorTop("SkyColorTop", Color) = (0.6981132,0.4379672,0.4379672,0)
		_topblend("top blend", Range( 0 , 3)) = 1.432217
		_midblend("mid blend", Range( 0 , 10)) = 1.432217
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _blend;
			uniform float4 _SkyColorBottom;
			uniform float4 _SkyColorTop;
			uniform float _topblend;
			uniform float4 _MidColor;
			uniform float4 _MidColor2;
			uniform float _midblend;
			uniform sampler2D _TextureSample1;
			uniform float4 _TextureSample1_ST;
			uniform float4 _LandColor;
			uniform float _speed;
			uniform float4 _CloudColor;
			float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }
			float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }
			float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }
			float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }
			float snoise( float3 v )
			{
				const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
				float3 i = floor( v + dot( v, C.yyy ) );
				float3 x0 = v - i + dot( i, C.xxx );
				float3 g = step( x0.yzx, x0.xyz );
				float3 l = 1.0 - g;
				float3 i1 = min( g.xyz, l.zxy );
				float3 i2 = max( g.xyz, l.zxy );
				float3 x1 = x0 - i1 + C.xxx;
				float3 x2 = x0 - i2 + C.yyy;
				float3 x3 = x0 - 0.5;
				i = mod3D289( i);
				float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
				float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
				float4 x_ = floor( j / 7.0 );
				float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
				float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
				float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
				float4 h = 1.0 - abs( x ) - abs( y );
				float4 b0 = float4( x.xy, y.xy );
				float4 b1 = float4( x.zw, y.zw );
				float4 s0 = floor( b0 ) * 2.0 + 1.0;
				float4 s1 = floor( b1 ) * 2.0 + 1.0;
				float4 sh = -step( h, 0.0 );
				float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
				float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
				float3 g0 = float3( a0.xy, h.x );
				float3 g1 = float3( a0.zw, h.y );
				float3 g2 = float3( a1.xy, h.z );
				float3 g3 = float3( a1.zw, h.w );
				float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
				g0 *= norm.x;
				g1 *= norm.y;
				g2 *= norm.z;
				g3 *= norm.w;
				float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
				m = m* m;
				m = m* m;
				float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
				return 42.0 * dot( m, px);
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1 = v.vertex;
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float3 normalizeResult6 = normalize( i.ase_texcoord1.xyz );
				float temp_output_8_0 = max( normalizeResult6.y , 0.0 );
				float4 lerpResult63 = lerp( _SkyColorBottom , _SkyColorTop , ( _topblend * temp_output_8_0 ));
				float temp_output_10_0 = ( min( normalizeResult6.y , 0.0 ) * -1.0 );
				float temp_output_16_0 = ( 1.0 - ( temp_output_8_0 + temp_output_10_0 ) );
				float4 lerpResult71 = lerp( _MidColor , _MidColor2 , ( _midblend * 1.0 * temp_output_16_0 ));
				float2 uv_TextureSample1 = i.ase_texcoord2.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float3 temp_output_38_0 = ( normalizeResult6 + ( _speed * _Time.y ) );
				float simplePerlin3D55 = snoise( temp_output_38_0*2.0 );
				simplePerlin3D55 = simplePerlin3D55*0.5 + 0.5;
				float4 ifLocalVar59 = 0;
				if( ( simplePerlin3D55 * max( normalizeResult6.y , 0.0 ) ) >= 0.29 )
				ifLocalVar59 = _CloudColor;
				
				
				finalColor = ( ( pow( temp_output_8_0 , _blend ) * lerpResult63 ) + ( pow( temp_output_16_0 , 0.8 ) * lerpResult71 * tex2D( _TextureSample1, uv_TextureSample1 ) ) + ( _LandColor * temp_output_10_0 ) + ifLocalVar59 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18800
0;45;1440;754;5091.749;2642.219;6.320699;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;43;-1192.459,244.4038;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;6;-819.9459,-30.82688;Inherit;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;7;-500.6789,103.3972;Inherit;True;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMinOpNode;9;-259.8729,290.7493;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;58;-405.6621,-1152.254;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;42;-652.0735,-810.098;Inherit;False;Property;_speed;speed;4;0;Create;True;0;0;0;False;0;False;0;0.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-23.64661,295.395;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;8;-293.2007,-179.7217;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-391.61,-960.5872;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;179.9277,166.6257;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-379.0713,-336.9138;Inherit;False;Property;_topblend;top blend;9;0;Create;True;0;0;0;False;0;False;1.432217;1.21;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;60.08032,-881.8852;Inherit;False;Constant;_cloudseed;cloud seed;11;0;Create;True;0;0;0;False;0;False;2;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-276.7187,1080.001;Inherit;False;Property;_midblend;mid blend;10;0;Create;True;0;0;0;False;0;False;1.432217;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-201.5389,-982.9177;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;16;458.2971,160.3278;Inherit;True;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;321.334,747.0739;Inherit;False;Property;_MidColor;Mid Color;2;0;Create;True;0;0;0;False;0;False;0.07471521,0.4779798,0.6886792,0;0.04903874,0.1958796,0.5471698,0.8862745;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;136.6173,1088.146;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;34.26467,-328.7693;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-95.70612,82.23311;Inherit;False;Property;_blend;blend;5;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;55;417.5081,-1032.298;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;64;-135.2488,-525.1364;Inherit;False;Property;_SkyColorTop;SkyColorTop;8;0;Create;True;0;0;0;False;0;False;0.6981132,0.4379672,0.4379672,0;0,0.7093024,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;70;-32.89622,891.7784;Inherit;False;Property;_MidColor2;Mid Color 2;7;0;Create;True;0;0;0;False;0;False;0.5425863,0.7506646,0.8396226,0;1,0.768868,0.9878352,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;51;1072.763,-953.5214;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;43.3619,-694.9392;Inherit;False;Property;_SkyColorBottom;Sky Color Bottom;0;0;Create;True;0;0;0;False;0;False;0.07471521,0.4779798,0.6886792,0;0,0.3113208,0.5471698,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;71;547.3478,871.8499;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;61;867.7735,320.0263;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;60;1428.389,-811.3917;Inherit;False;Property;_CloudColor;Cloud Color;6;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-18.21,610.4677;Inherit;False;Property;_LandColor;Land Color;1;0;Create;True;0;0;0;False;0;False;0.389596,0.8018868,0.442467,0;0.0821021,0.3390019,0.3867924,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;1410.962,-1089.343;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;63;444.9952,-545.0649;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;76;1187.27,506.8381;Inherit;True;Property;_TextureSample1;Texture Sample 1;11;0;Create;True;0;0;0;False;0;False;-1;91f2eef97ddef4826b5944ff4baa8ef5;09227d992df6e4b9ea6df54cd0c0cc92;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;57;311.0225,-176.3995;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;2.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;1423.986,-582.379;Inherit;False;Constant;_cloudheight;cloud height;11;0;Create;True;0;0;0;False;0;False;0.29;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;1086.891,-179.2212;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;59;1826.852,-987.6356;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0.2;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;628.5486,417.5179;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;1152.972,163.305;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldPosInputsNode;5;-1095.888,-22.45374;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;37;715.6234,-1324.444;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;None;12a68da8bef6f4b5f91b604cf32d6ae9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;23;-605.7831,331.0698;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;1535.531,184.8716;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;50;1883.688,126.1019;Float;False;True;-1;2;ASEMaterialInspector;100;1;SkyboxShader;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;6;0;43;0
WireConnection;7;0;6;0
WireConnection;9;0;7;1
WireConnection;10;0;9;0
WireConnection;8;0;7;1
WireConnection;41;0;42;0
WireConnection;41;1;58;2
WireConnection;11;0;8;0
WireConnection;11;1;10;0
WireConnection;38;0;6;0
WireConnection;38;1;41;0
WireConnection;16;1;11;0
WireConnection;73;0;72;0
WireConnection;73;2;16;0
WireConnection;69;0;68;0
WireConnection;69;1;8;0
WireConnection;55;0;38;0
WireConnection;55;1;75;0
WireConnection;51;0;7;1
WireConnection;71;0;18;0
WireConnection;71;1;70;0
WireConnection;71;2;73;0
WireConnection;61;0;16;0
WireConnection;53;0;55;0
WireConnection;53;1;51;0
WireConnection;63;0;14;0
WireConnection;63;1;64;0
WireConnection;63;2;69;0
WireConnection;57;0;8;0
WireConnection;57;1;62;0
WireConnection;12;0;57;0
WireConnection;12;1;63;0
WireConnection;59;0;53;0
WireConnection;59;1;74;0
WireConnection;59;2;60;0
WireConnection;59;3;60;0
WireConnection;13;0;15;0
WireConnection;13;1;10;0
WireConnection;17;0;61;0
WireConnection;17;1;71;0
WireConnection;17;2;76;0
WireConnection;37;1;38;0
WireConnection;23;0;7;1
WireConnection;19;0;12;0
WireConnection;19;1;17;0
WireConnection;19;2;13;0
WireConnection;19;3;59;0
WireConnection;50;0;19;0
ASEEND*/
//CHKSM=FDDA90FE5C4B2E6185D3D78C3E8EB959472D65F5