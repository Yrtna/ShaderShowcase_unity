﻿Shader "Ludiq/Editor/ProbeHighlightMask"
{
    Properties 
	{

	}

    SubShader
    {
        Pass
        {
			Blend One One
			BlendOp Max
			ZWrite Off
			ZTest Always
			Offset 0, -1

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

			fixed4 _Color;
			fixed _ProbeHighlightRoot;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return fixed4(_ProbeHighlightRoot,1,1,1);
            }

            ENDCG
        }
    }
}
