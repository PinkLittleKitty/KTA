Shader "XFrame/XFrame Default" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

Subshader {	

  Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode Off }
	  
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma fragmentoption ARB_precision_hint_fastest
      #include "XFrame.cginc"
      ENDCG
  }

}
FallBack Off
}
