Shader "Custom/VertexColorShaderSimple"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1) // This property is not used for vertex colors, but is good practice to have
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR; // Reads vertex color data
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // Passes color to the fragment shader
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
                return i.color; // Returns the interpolated vertex color
            }
            ENDCG
        }
    }
}
