Shader "UI/FogOfWarCutout"
{
    Properties
    {
        _MainTex       ("Texture", 2D) = "white" {}
        _Color         ("Tint", Color) = (0,0,0,0.55)
        _CenterX       ("Reveal Center X (0-1)", Float) = 0.5
        _CenterY       ("Reveal Center Y (0-1)", Float) = 0.5
        _Radius        ("Reveal Radius (0-1)", Float) = 0.12
        _Soft          ("Edge Softness", Float) = 0.03
        [HideInInspector] _StencilComp     ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil         ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp       ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask       ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"          = "Transparent"
            "IgnoreProjector"= "True"
            "RenderType"     = "Transparent"
            "PreviewType"    = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            float     _CenterX;
            float     _CenterY;
            float     _Radius;
            float     _Soft;
            float4    _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPos = v.vertex;
                o.vertex   = UnityObjectToClipPos(o.worldPos);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color    = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float aspect  = _ScreenParams.x / _ScreenParams.y;
                float2 uv     = float2(i.texcoord.x * aspect, i.texcoord.y);
                float2 center = float2(_CenterX * aspect, _CenterY);
                float  dist   = distance(uv, center);
                float  alpha  = smoothstep(_Radius - _Soft, _Radius + _Soft, dist);

                fixed4 col = i.color;
                col.a *= alpha;
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
