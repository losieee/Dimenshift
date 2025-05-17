Shader "Custom/FillYShader_Grow"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0, 0, 1)
        _FillAmount ("Fill Amount", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _FillAmount;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ���� Y��ǥ ����
                float3 localPos = mul(unity_WorldToObject, float4(i.worldPos, 1.0)).xyz;

                // �ּ�~�ִ� Y ���� (�ٿ�� �ڽ��� Y: -0.5 ~ +0.5��� ����)
                float fillTop = lerp(-0.5, 0.5, _FillAmount);

                // �Ʒ����� ���� 1, ������ ���� 0
                float alpha = smoothstep(fillTop + 0.01, fillTop - 0.01, localPos.y);

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}