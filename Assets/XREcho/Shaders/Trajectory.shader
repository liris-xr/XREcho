Shader "Unlit/Trajectory"
{
    Properties
    {
        _StartColor ("Start color", Color) = (0, 0, 1, 1)
        _EndColor ("End color", Color) = (1, 0, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _StartColor;
            float4 _EndColor;
            float epsilon = 1e-10;
            
            float3 rgb_to_hsv(in float3 rgb)
            {
                float4 p = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0/3.0) : float4(rgb.gb, 0.0, -1.0/3.0);
                float4 q = (rgb.r < p.x) ? float4(p.xyw, rgb.r) : float4(rgb.r, p.yzx);
                float c = q.x - min(q.w, q.y);
                float h = abs((q.w - q.y) / (6 * c + epsilon) + q.z);
                float3 hcv = float3(h, c, q.x);
                                
                float s = hcv.y / (hcv.z + epsilon);
                return float3(hcv.x, s, hcv.z);
            }
            
            float3 hsv_to_rgb(in float3 hsv)
            {
                float r = abs(hsv.x * 6 - 3) - 1;
                float g = 2 - abs(hsv.x * 6 - 2);
                float b = 2 - abs(hsv.x * 6 - 4);
                const float3 rgb = saturate(float3(r,g,b));
                return ((rgb - 1) * hsv.y + 1) * hsv.z;
            }
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 start_hsv = rgb_to_hsv(_StartColor.rgb);
                float3 stop_hsv = rgb_to_hsv(_EndColor.rgb);

                float lerp_hue = lerp(start_hsv.x, stop_hsv.x, i.color);
                const float3 lerp_hsv = float3(lerp_hue, 1, 1);
                
                return fixed4(hsv_to_rgb(lerp_hsv), 1);
            }
            ENDCG
        }
    }
}
