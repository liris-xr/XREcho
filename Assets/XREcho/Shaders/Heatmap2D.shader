Shader "Unlit/Heatmap2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Transparency ("Transparency", Float) = 1.0
        _ScaleLowerBound ("ScaleLowerBound", Float) = 0.0
        _ScaleUpperBound ("ScaleUpperBound", Float) = 1.0
        _FirstColor("FirstColor", Color) = (0, 0, 1, 1)
        _SecondColor("SecondColor", Color) = (0, 1, 0, 1)
        _ThirdColor("ThirdColor", Color) = (1, 0, 0, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Transparency;
            float _ScaleLowerBound;
            float _ScaleUpperBound;
            float4 _FirstColor;
            float4 _SecondColor;
            float4 _ThirdColor;

            float epsilon = 1e-10;

            float map(float value, float min1, float max1, float min2, float max2)
            {
                // Convert the current value to a percentage
                // 0% - min1, 100% - max1
                float perc = (value - min1) / (max1 - min1);

                // Do the same operation backwards with min2 and max2
                return perc * (max2 - min2) + min2;
            }
            
            float3 rgb_to_hsv(in float3 rgb)
            {
                float4 p = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0 / 3.0) : float4(rgb.gb, 0.0, -1.0 / 3.0);
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
                const float3 rgb = saturate(float3(r, g, b));
                return ((rgb - 1) * hsv.y + 1) * hsv.z;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                const float value = tex2D(_MainTex, i.uv).r;
                const float scaled_value = clamp((value - _ScaleLowerBound) / (_ScaleUpperBound - _ScaleLowerBound), 0, 1);

                const float3 first_color_hsv = rgb_to_hsv(_FirstColor.rgb);
                const float3 second_color_hsv = rgb_to_hsv(_SecondColor.rgb);
                const float3 third_color_hsv = rgb_to_hsv(_ThirdColor.rgb);
                
                float3 color_hsv;
                float alpha;
                
                if(scaled_value < 0.5)
                {
                    color_hsv = lerp(first_color_hsv, second_color_hsv, scaled_value / 0.5);
                    alpha = lerp(_FirstColor.a, _SecondColor.a, scaled_value / 0.5);
                } else
                {
                    color_hsv = lerp(second_color_hsv, third_color_hsv, (scaled_value - 0.5) / 0.5);
                    alpha = lerp(_SecondColor.a, _ThirdColor.a, (scaled_value - 0.5) / 0.5);
                }
                
                fixed4 color = float4(hsv_to_rgb(color_hsv), alpha * _Transparency);
                return color;
            }
            ENDCG
        }
    }
}