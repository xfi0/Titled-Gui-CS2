using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ClickableTransparentOverlay;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Titled_Gui.Classes.VPK.Types;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;
using Titled_Gui.Classes.VPK;

namespace Titled_Gui.Classes.Rendering.ChamsRenderer
{
    public readonly record struct ChamsMeshDraw(IReadOnlyList<Bone> Bones, GpuMesh Mesh, Vector4 VisibleColor, Vector4 OccludedColor);

    public abstract class ChamsRendererBase(string featureName) : IDisposable
    {
        protected const int StyleDisabled = 0;
        protected const int StyleFlat = 1;
        protected const int StyleTextured = 2;
        protected const int StyleMetallic = 3;
        protected const int StyleWireframe = 4;
        protected const int StyleCs2Glow = 5;
        protected const int StyleLsd = 6;
        protected const int StylePlasma = 7;
        public static readonly string VsSource = """
        cbuffer CB : register(b0) {
            float4x4 WorldViewProj;
            float4   Color;
            int      Style;
            float3   CamPos;
            float4   GlowColor;
            float    GlowThickness;
            float    GlowIntensity;
            int      OcclusionMode;
            float    Time;
            float2   _pad;
        };
        cbuffer BoneBuffer : register(b1) {
            float4 BoneRow[384];
        };
        struct VS_INPUT {
            float3 Pos     : POSITION;
            float3 Normal  : NORMAL;
            uint4  Joints  : BLENDINDICES;
            float4 Weights : BLENDWEIGHT;
        };
        struct PS_INPUT {
            float4 Pos       : SV_POSITION;
            float3 WorldPos  : TEXCOORD0;
            float3 Normal    : TEXCOORD1;
            float4 screenPos : TEXCOORD2;
        };
        PS_INPUT VS(VS_INPUT input) {
            float3 skinnedPos    = float3(0,0,0);
            float3 skinnedNormal = float3(0,0,0);
            float  totalWeight   = 0;
            [unroll]
            for (int i = 0; i < 4; i++) {
                float w = input.Weights[i];
                if (w < 0.0001f) continue;
                uint  j    = min(input.Joints[i], 127u);
                uint  base = j * 3u;
                float4 r0  = BoneRow[base];
                float4 r1  = BoneRow[base + 1u];
                float4 r2  = BoneRow[base + 2u];
                float4 p4  = float4(input.Pos, 1.0f);
                skinnedPos.x    += dot(p4, r0) * w;
                skinnedPos.y    += dot(p4, r1) * w;
                skinnedPos.z    += dot(p4, r2) * w;
                skinnedNormal.x += dot(input.Normal, r0.xyz) * w;
                skinnedNormal.y += dot(input.Normal, r1.xyz) * w;
                skinnedNormal.z += dot(input.Normal, r2.xyz) * w;
                totalWeight     += w;
            }
            if (totalWeight > 0.0001f) {
                float invW    = 1.0f / totalWeight;
                skinnedPos   *= invW;
                skinnedNormal *= invW;
            } else {
                skinnedPos    = input.Pos;
                skinnedNormal = input.Normal;
            }

            float3 pos     = skinnedPos;
            float4 clipPos = mul(float4(pos, 1.0f), WorldViewProj);

            if (GlowThickness > 0.0f) {
                float3 n     = normalize(skinnedNormal);
                float4 clipN = mul(float4(pos + n, 1.0f), WorldViewProj);
                float invW0  = 1.0f / max(abs(clipPos.w), 1e-4f);
                float invW1  = 1.0f / max(abs(clipN.w),  1e-4f);
                float2 ndc0  = clipPos.xy * invW0;
                float2 ndc1  = clipN.xy   * invW1;
                float2 ndcDir = ndc1 - ndc0;
                float ndcLen  = length(ndcDir);
                if (ndcLen > 1e-6f) {
                    ndcDir /= ndcLen;
                    float ndcThickness = GlowThickness * 0.0022f;
                    clipPos.xy += ndcDir * ndcThickness * clipPos.w;
                }
            }

            PS_INPUT o;
            o.Pos       = clipPos;
            o.WorldPos  = pos;
            o.Normal    = normalize(skinnedNormal);
            o.screenPos = clipPos;
            return o;
        }
        """;

        public static readonly string PsSource = """
        Texture2D<float> PrepassDepth  : register(t0);
        SamplerState     PrepassSampler: register(s0);

        cbuffer CB : register(b0) {
            float4x4 WorldViewProj;
            float4   Color;
            int      Style;
            float3   CamPos;
            float4   GlowColor;
            float    GlowThickness;
            float    GlowIntensity;
            int      OcclusionMode;
            float    Time;
            float2   _pad;
        };

        struct PS_INPUT {
            float4 Pos       : SV_POSITION;
            float3 WorldPos  : TEXCOORD0;
            float3 Normal    : TEXCOORD1;
            float4 screenPos : TEXCOORD2;
        };

        float3 hsv2rgb(float h, float s, float v) {
            float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
            float3 p = abs(frac(h.xxx + K.xyz) * 6.0 - K.www);
            return v * lerp(K.xxx, saturate(p - K.xxx), s);
        }

        float hash21(float2 p) {
            float2 q = frac(p * float2(234.56, 123.45));
            q += dot(q, q + 73.21);
            return frac(q.x * q.y);
        }

        float noise2D(float2 p) {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float na = hash21(i);
            float nb = hash21(i + float2(1,0));
            float nc = hash21(i + float2(0,1));
            float nd = hash21(i + float2(1,1));
            return lerp(lerp(na,nb,f.x), lerp(nc,nd,f.x), f.y);
        }

        float fbm(float2 p) {
            float v  = noise2D(p)                              * 0.500;
            v       += noise2D(p * 2.1 + float2(5.2,  1.3))  * 0.250;
            v       += noise2D(p * 4.2 + float2(2.1,  7.8))  * 0.125;
            return v;
        }

        float particles(float2 uv, float t) {
            float2 cell = floor(uv);
            float2 f    = frac(uv);
            float  result = 0.0;
            [unroll] for (int x = -1; x <= 1; x++) {
            [unroll] for (int y = -1; y <= 1; y++) {
                float2 n  = cell + float2(x, y);
                float  h1 = hash21(n);
                float  h2 = hash21(n + 41.37);
                float  spd = 0.55 + h1 * 0.9;
                float2 pos = float2(
                    0.5 + 0.43 * sin(h1 * 6.28318 + t * spd),
                    0.5 + 0.43 * cos(h2 * 6.28318 + t * spd * 1.17)
                );
                float2 diff   = f - float2(x,y) - pos;
                float  twinkle = smoothstep(0.0, 1.0,
                                   sin(t * (3.0+h1*4.0) + h2*12.56) * 0.5 + 0.5);
                result = max(result, smoothstep(0.065, 0.0, length(diff)) * twinkle);
            }}
            return result;
        }

        float4 PS(PS_INPUT input) : SV_Target {

            if (OcclusionMode > 0) {
                float2 screenUV   = input.screenPos.xy / input.screenPos.w;
                screenUV          = screenUV * 0.5 + 0.5;
                screenUV.y        = 1.0 - screenUV.y;
                float prepassDepth = PrepassDepth.Sample(PrepassSampler, screenUV);
                float currentDepth = input.screenPos.z / input.screenPos.w;
                bool  isOccluded   = (currentDepth > prepassDepth + 0.001);
                if (OcclusionMode == 1 && !isOccluded) discard;
                if (OcclusionMode == 2 &&  isOccluded) discard;
            }

            float3 rawN = input.Normal;
            float  nLen      = length(rawN);
            float3 N         = nLen > 0.0001 ? rawN / nLen : float3(0,1,0);
            float3 viewDir   = normalize(CamPos - input.WorldPos);
            float  viewDot   = dot(N, viewDir);
            float  absVD     = saturate(abs(viewDot));
            float3 finalColor = Color.rgb;
            float  finalAlpha = Color.a;
            float  rim       = pow(1.0 - absVD, 2.0);

            if (Style == 1) {
                return float4(finalColor, finalAlpha);
            }

            else if (Style == 2) {
                float3 lightMain  = normalize(float3( 0.3, 0.8, 0.6));
                float3 lightFill  = normalize(float3(-0.2,-0.5, 0.4));
                float  diffMain   = max(0, dot(N, lightMain));
                float  diffFill   = max(0, dot(N, lightFill)) * 0.35;
                float  ambient    = 0.35;
                float3 halfVec    = normalize(lightMain + viewDir);
                float  spec       = pow(saturate(dot(N, halfVec)), 24.0) * 0.4;
                float  lighting   = ambient + diffMain + diffFill + spec;
                float3 rimColor   = finalColor * 1.4;
                float3 litColor   = finalColor * lighting;
                return float4(saturate(lerp(litColor, rimColor, rim * 0.4)), finalAlpha);
            }

            else if (Style == 3) {
                float3 darkColor   = finalColor * 0.3;
                float  reflection  = 0.5 * N.y + 0.5;
                float  fresnel     = rim;
                float  specular    = pow(absVD, 15.0);
                float3 metalColor  = lerp(darkColor, finalColor, reflection);
                metalColor        += float3(1,1,1) * specular * 1.5;
                metalColor        += finalColor * fresnel * 0.8;
                return float4(saturate(metalColor), finalAlpha);
            }

            else if (Style == 4) {
                return float4(finalColor, finalAlpha);
            }

            else if (Style == 5) {
                float rimGlow  = pow(1.0 - absVD, 2.5);
                float rimSharp = pow(1.0 - absVD, 6.0);
                float rimSoft  = pow(1.0 - absVD, 1.2);
                float core  = saturate(rimSharp * 4.0);
                float mid   = saturate(rimGlow  * 2.2);
                float outer = saturate(rimSoft  * 0.55);
                float bloom = core * 0.85 + mid * 0.55 + outer * 0.25;
                float alpha = saturate(bloom) * GlowColor.a;
                float3 glowRGB = GlowColor.rgb * (1.0 + core * GlowIntensity * 2.5
                                                       + mid  * GlowIntensity);
                return float4(glowRGB, alpha);
            }

            else if (Style == 6) {
                float3 p = input.WorldPos * 0.10;

                float twist = sin(length(p.xz) * 1.4 - Time * 0.6) * 0.35;
                p.xy = float2(p.x * cos(twist) - p.y * sin(twist),  p.x * sin(twist) + p.y * cos(twist));

                float pl = sin(p.x * 4.2 + Time * 1.4) + sin(p.y * 2.8 + Time * 2.1) + sin(p.z * 0.4 + Time * 0.8) + sin(length(p.xy) * 2.8 - Time * 2.1) + sin(length(p.yz) * 3.2 + Time * 1.6) + sin(length(p)    * 1.9 - Time * 3.0) + abs(sin(p.x * 7.1 - p.y * 5.3 + Time * 2.7)) * 1.2 + abs(sin(length(p.xz) * 6.4 - Time * 1.8))     * 0.8;

                float t = saturate(pl * 0.0625 + 0.5);
                float ts = pow(t, 1.0);

                float3 c1 = float3(1.00, 0.05, 0.65);
                float3 c2 = float3(0.11, 0.32, 1.00);
                float3 c3 = float3(1.00, 0.65, 0.00);
                float3 c4 = float3(0.50, 0.00, 1.00);
                float3 c5 = float3(0.90, 0.00, 0.20);

                float  seg = ts * 4.0;
                float3 plasmaColor;
                if (seg < 1.0)
                plasmaColor = lerp(c1, c2, seg);
                else if (seg < 2.0)
                plasmaColor = lerp(c2, c3, seg - 1.0);
                else if (seg < 3.0)
                plasmaColor = lerp(c3, c4, seg - 2.0);
                else
                plasmaColor = lerp(c4, c1, seg - 3.0);

                float pulse   = 0.55 + 0.45 * sin(t * 6.28  + Time * 0.9);
                float shimmer = 0.85 + 0.15 * sin(t * 31.4  + Time * 5.3);
                float bright  = pulse * shimmer;

                float vein = pow(saturate(sin(pl * 1.1) * 0.5 + 0.5), 6.0);
                plasmaColor = lerp(plasmaColor, float3(1.0, 0.95, 0.85), vein * 0.6);

                float rim = pow(1.0 - saturate(dot(normalize(input.Normal), normalize(CamPos - input.WorldPos))), 3.0);
                plasmaColor += float3(0.3, 0.0, 0.5) * rim * bright;

                float alpha = finalAlpha * (0.60 + 0.40 * bright);
                return float4(plasmaColor * bright * 1.5, alpha);
            }

            else if (Style == 7) {
                float3 p = input.WorldPos * 0.24;
                float  pl = sin(p.x*2.2 + Time*1.4)
                          + sin(p.y*1.8 + Time*1.1)
                          + sin(p.z*2.4 + Time*0.8)
                          + sin(length(p.xy)*2.8 - Time*2.1)
                          + sin(length(p.yz)*2.2 + Time*1.6)
                          + sin(length(p)   *1.9 - Time*1.0);
                float  t  = saturate(pl * 0.0833 + 0.5);

                float3 c1 = float3(1.0, 0.05, 0.65);
                float3 c2 = float3(0.0, 0.90, 1.00);
                float3 c3 = float3(0.9, 1.00, 0.00);
                float3 c4 = float3(0.5, 0.00, 1.00);

                float  seg = t * 4.0;
                float3 plasmaColor;
                if      (seg < 1.0) plasmaColor = lerp(c1, c2, seg);
                else if (seg < 2.0) plasmaColor = lerp(c2, c3, seg-1.0);
                else if (seg < 3.0) plasmaColor = lerp(c3, c4, seg-2.0);
                else                plasmaColor = lerp(c4, c1, seg-3.0);

                float bright = 0.70 + 0.30 * sin(t * 12.56 + Time);
                return float4(plasmaColor * bright * 1.35,
                              finalAlpha * (0.65 + 0.35 * bright));
            }

            return float4(finalColor, finalAlpha);
        }
        """;
        private ID3D11Device? _device;
        private ID3D11DeviceContext? _deviceContext;
        private ID3D11Texture2D? _target;
        private ID3D11RenderTargetView? _renderTargetView;
        private ID3D11Texture2D? _depthTexture;
        private ID3D11DepthStencilView? _depthStencilView;
        private ID3D11ShaderResourceView? _shaderResourceView;
        private int _width;
        private int _height;
        private ID3D11Texture2D? _prepassTex;
        private ID3D11DepthStencilView? _prepassDepthStencilView;
        private ID3D11ShaderResourceView? _prepassShaderResourceView;
        private ID3D11SamplerState? _prepassSamplerState;
        private ID3D11RasterizerState? _rasterizerStateSolid;
        private ID3D11RasterizerState? _rasterizerStateWire;
        private ID3D11DepthStencilState? _depthStencilStateChams;
        private ID3D11DepthStencilState? _depthStencilStatePrepass;
        private ID3D11BlendState? _blendStateAlpha;
        private ID3D11BlendState? _blendStateNoColor;
        private SkinnedMeshRenderer? _meshRenderer;
        private nint _texId;
        protected ModelCache? _modelCache;
        private bool _initialized = false;
        private DateTime _dateTime = DateTime.UtcNow;
        private readonly string _featureName = featureName;

        protected abstract bool FeatureEnabled { get; }

        protected abstract List<ChamsMeshDraw> CollectDraws();

        protected virtual int GetStyleValue() => StyleFlat;

        protected virtual bool UsePixelPerfect => false;

        protected virtual Vector3 GetCameraPosition() => GameState.renderer != null ? GameState.renderer.LocalPlayer.EyePosition : Vector3.Zero;

        protected static void Diag(string msg) => Console.WriteLine(msg);

        public void RenderFrame()
        {
            if (!FeatureEnabled || GameState.renderer == null || GameState.memory == null || GameState.client == IntPtr.Zero)
                return;

            try // try catch cause this loves to throw
            {
                Initialize();
            }
            catch (Exception e)
            {
                Console.WriteLine(_featureName + " init failed: " + e);
                return;
            }

            if (!_initialized || _device == null || _deviceContext == null || _renderTargetView == null)
                return;

            Vector2 screenSize = GameState.renderer.ScreenSize;
            int width = (int)screenSize.X;
            int height = (int)screenSize.Y;
            if (width <= 0 || height <= 0)
                return;

            EnsureTargets(width, height);

            float[] viewMaterix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            _modelCache?.FlushCompleted();

            float time = (float)(DateTime.UtcNow - _dateTime).TotalSeconds;
            Vector3 cameraPosition = GetCameraPosition();
            int style = System.Math.Clamp(GetStyleValue(), 0, StylePlasma);

            List<ChamsMeshDraw> draws = CollectDraws();
            if (draws.Count <= 0)
                return;

            _deviceContext.OMSetRenderTargets(_renderTargetView, _depthStencilView);
            _deviceContext.RSSetViewport(new Viewport(0, 0, width, height, 0f, 1f));
            _deviceContext.ClearRenderTargetView(_renderTargetView, new Color4(0f, 0f, 0f, 0f));
            _deviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 0);

            if (_meshRenderer == null)
                return;

            _meshRenderer.Begin();

            if (UsePixelPerfect)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _deviceContext.OMSetRenderTargets((ID3D11RenderTargetView?)null, _prepassDepthStencilView);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                _deviceContext.ClearDepthStencilView(_prepassDepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);
                _deviceContext.OMSetDepthStencilState(_depthStencilStateChams, 0);
                _deviceContext.OMSetBlendState(_blendStateNoColor, new Color4(0), 0xFFFFFFFF);
                _deviceContext.RSSetState(_rasterizerStateSolid);

                foreach (ChamsMeshDraw draw in draws)
                {
                    if (draw.Bones == null)
                        continue;

                    if (!_meshRenderer.UploadBones(draw.Bones, draw.Mesh.Data))
                        continue;

                    _meshRenderer.UploadConstant(viewMaterix, Vector4.Zero, 0, cameraPosition, 0f, 0f, 0, time);
                    _meshRenderer.DrawMesh(draw.Mesh);
                }
            }

            _deviceContext.OMSetRenderTargets(_renderTargetView, _depthStencilView);
            _deviceContext.OMSetDepthStencilState(_depthStencilStateChams, 0);
            _deviceContext.OMSetBlendState(_blendStateAlpha, new Color4(0), 0xFFFFFFFF);
            _deviceContext.RSSetState(style == StyleWireframe ? _rasterizerStateWire : _rasterizerStateSolid);

            if (UsePixelPerfect)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                _deviceContext.PSSetShaderResource(0, _prepassShaderResourceView);
#pragma warning restore CS8604 // Possible null reference argument.
                _deviceContext.PSSetSampler(0, _prepassSamplerState);

                foreach (ChamsMeshDraw draw in draws)
                {
                    if (draw.Bones == null)
                        continue;

                    if (!_meshRenderer.UploadBones(draw.Bones, draw.Mesh.Data) || draw.Bones == null)
                        continue;

                    _meshRenderer.UploadConstant(viewMaterix, draw.OccludedColor, style, cameraPosition, 0f, 1f, 1, time);
                    _meshRenderer.DrawMesh(draw.Mesh);
                    _meshRenderer.UploadConstant(viewMaterix, draw.VisibleColor, style, cameraPosition, 0f, 1f, 2, time);
                    _meshRenderer.DrawMesh(draw.Mesh);
                }
            }
            else
            {
                foreach (ChamsMeshDraw draw in draws)
                {
                    if (draw.Bones == null)
                        continue;

                    if (!_meshRenderer.UploadBones(draw.Bones, draw.Mesh.Data) || draw.Bones == null)
                        continue;

                    bool visible = draw.Bones.Any(b => b.IsVisible);
                    var color = visible ? draw.VisibleColor : draw.OccludedColor;

                    _meshRenderer.UploadConstant(viewMaterix, color, style, cameraPosition, 0f, 1f, 0, time);
                    _meshRenderer.DrawMesh(draw.Mesh);
                }
            }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _deviceContext.PSSetShaderResource(0, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            if (_texId != IntPtr.Zero)
                GameState.renderer.DrawList.AddImage(_texId, Vector2.Zero, screenSize);
        }

        protected GpuMesh? GetCachedModel(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return _modelCache?.Get(name);
        }

        private void Initialize()
        {
            if (_initialized || GameState.renderer == null)
                return;

            _device = Renderer.GetDevice();
            _deviceContext = Renderer.GetDeviceContext();

            if (_device == null || _deviceContext == null)
                return;

            _modelCache = new(_device);

            EnsureTargets((int)GameState.renderer.ScreenSize.X, (int)GameState.renderer.ScreenSize.Y);
            CreatePipeline();
            RegisterWithImGui();

            _initialized = true;
        }

        private void EnsureTargets(int w, int h)
        {
            if (_device == null || _target != null && _width == w && _height == h)
                return;

            DisposeTargets();
            _width = w;
            _height = h;

            Texture2DDescription colorDesc = new Texture2DDescription(Format.R8G8B8A8_UNorm, w, h, 1, 1, BindFlags.RenderTarget | BindFlags.ShaderResource, ResourceUsage.Default, CpuAccessFlags.None, 1, 0, ResourceOptionFlags.None);
            _target = _device.CreateTexture2D(colorDesc);
            _renderTargetView = _device.CreateRenderTargetView(_target, null);
            _shaderResourceView = _device.CreateShaderResourceView(_target, null);

            Texture2DDescription depthDesc = new Texture2DDescription(Format.D24_UNorm_S8_UInt, w, h, 1, 1, BindFlags.DepthStencil, ResourceUsage.Default, CpuAccessFlags.None, 1, 0, ResourceOptionFlags.None);
            _depthTexture = _device.CreateTexture2D(depthDesc);
            _depthStencilView = _device.CreateDepthStencilView(_depthTexture, null);

            Texture2DDescription prepassDesc = new Texture2DDescription(Format.R32_Typeless, w, h, 1, 1, BindFlags.DepthStencil | BindFlags.ShaderResource, ResourceUsage.Default, CpuAccessFlags.None, 1, 0, ResourceOptionFlags.None);
            _prepassTex = _device.CreateTexture2D(prepassDesc);
            _prepassDepthStencilView = _device.CreateDepthStencilView(_prepassTex, new DepthStencilViewDescription(DepthStencilViewDimension.Texture2D, Format.D32_Float, 0));
            _prepassShaderResourceView = _device.CreateShaderResourceView(_prepassTex, new ShaderResourceViewDescription(ShaderResourceViewDimension.Texture2D, Format.R32_Float, 0, 1, 0, 0));
        }

        private void CreatePipeline()
        {
            if (_device == null || _deviceContext == null)
                return;

            _meshRenderer = new SkinnedMeshRenderer(_device, _deviceContext);
            if (!_meshRenderer.CreatePipeline(VsSource, PsSource))
                return;

            _rasterizerStateSolid = _device.CreateRasterizerState(new RasterizerDescription(CullMode.None, FillMode.Solid));
            _rasterizerStateWire = _device.CreateRasterizerState(new RasterizerDescription(CullMode.None, FillMode.Wireframe));

            _depthStencilStateChams = _device.CreateDepthStencilState(new DepthStencilDescription(true, DepthWriteMask.Zero, ComparisonFunction.LessEqual));
            _depthStencilStatePrepass = _device.CreateDepthStencilState(new DepthStencilDescription(true, DepthWriteMask.All, ComparisonFunction.Less));

            _blendStateAlpha = _device.CreateBlendState(new BlendDescription(Blend.SourceAlpha, Blend.InverseSourceAlpha));

            var noColorDesc = new BlendDescription(Blend.SourceAlpha, Blend.InverseSourceAlpha);
            noColorDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteEnable.None;
            _blendStateNoColor = _device.CreateBlendState(noColorDesc);

            var samplerDesc = new SamplerDescription(Filter.MinMagMipPoint, TextureAddressMode.Clamp, TextureAddressMode.Clamp, TextureAddressMode.Clamp, 0f, 0, ComparisonFunction.Never, 0f, float.MaxValue);
            _prepassSamplerState = _device.CreateSamplerState(samplerDesc);
        }

        private void RegisterWithImGui()
        {
            if (_shaderResourceView == null)
                return;

            System.Collections.IDictionary? dict = Renderer.GetTextureResources();
            if (dict == null)
                return;

            ((ComObject)_shaderResourceView).AddRef();
            _texId = _shaderResourceView.NativePointer;
            dict[_texId] = _shaderResourceView;
        }

        private void DisposeTargets()
        {
            _target?.Dispose();
            _renderTargetView?.Dispose();
            _depthTexture?.Dispose();
            _depthStencilView?.Dispose();
            _prepassTex?.Dispose();
            _prepassDepthStencilView?.Dispose();
            _prepassShaderResourceView?.Dispose();

            if (_shaderResourceView != null)
            {
                System.Collections.IDictionary? dict = Renderer.GetTextureResources();
                dict?.Remove(_texId);
            }

            _target = null;
            _renderTargetView = null;
            _depthTexture = null;
            _depthStencilView = null;
            _prepassTex = null;
            _prepassDepthStencilView = null;
            _prepassShaderResourceView = null;
        }

        public virtual void Dispose()
        {
            _modelCache?.Dispose();
            _modelCache = null;

            DisposeTargets();
            _meshRenderer?.Dispose();
            _meshRenderer = null;
            _rasterizerStateSolid?.Dispose();
            _rasterizerStateWire?.Dispose();
            _depthStencilStateChams?.Dispose();
            _depthStencilStatePrepass?.Dispose();
            _blendStateAlpha?.Dispose();
            _blendStateNoColor?.Dispose();
            _prepassSamplerState?.Dispose();
            _shaderResourceView?.Dispose();
            _shaderResourceView = null;
        }
    }
}