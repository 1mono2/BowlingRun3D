using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum PostprocessTiming
{
    AfterOpaque,
    BeforePostprocess,
    AfterPostprocess
}

public class BagPostProcessRenderPass : ScriptableRenderPass
{
    private const string RenderPassName = nameof(BagPostProcessRenderPass);
    private const string ProfilingSamplerName = "SrcToDest";

    private readonly bool _applyToSceneView;
    private readonly int _mainTexPropertyId = Shader.PropertyToID("_MainTex");
    private readonly Material _material;
    private readonly ProfilingSampler _profilingSampler;
    private readonly int _splitXPropertyId = Shader.PropertyToID("_SplitX");
    private readonly int _splitYPropertyId = Shader.PropertyToID("_SplitY");
    private readonly int _shiftPropertyId = Shader.PropertyToID("_Shift");
    private readonly int _frecPropertyId = Shader.PropertyToID("_Frec");
    private readonly int _colorGapPropertyId = Shader.PropertyToID("_ColorGap");
    private readonly int _ratioPropertyId = Shader.PropertyToID("_Ratio");
    private readonly int _strengthPropertyId = Shader.PropertyToID("_Strength");
    private readonly int _blurPropertyId = Shader.PropertyToID("_Blur");

    private RenderTargetIdentifier _cameraColorTarget;
    private RenderTargetHandle _tempRenderTargetHandle;
    private BagPostProcessVolume _volume;

    public BagPostProcessRenderPass(bool applyToSceneView, Shader shader)
    {
        if (shader == null)
        {
            return;
        }

        _applyToSceneView = applyToSceneView;
        _profilingSampler = new ProfilingSampler(ProfilingSamplerName);
        _tempRenderTargetHandle.Init("_TempRT");

        // マテリアルを作成
        _material = CoreUtils.CreateEngineMaterial(shader);
    }

    public void Setup(RenderTargetIdentifier cameraColorTarget, PostprocessTiming timing)
    {
        _cameraColorTarget = cameraColorTarget;

        renderPassEvent = GetRenderPassEvent(timing);

        // Volumeコンポーネントを取得
        var volumeStack = VolumeManager.instance.stack;
        _volume = volumeStack.GetComponent<BagPostProcessVolume>();
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_material == null)
        {
            return;
        }

        if (!_volume.isActive.GetValue<bool>())
        {
            return;
        }

        // カメラのポストプロセス設定が無効になっていたら何もしない
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        // カメラがシーンビューカメラかつシーンビューに適用しない場合には何もしない
        if (!_applyToSceneView && renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            return;
        }


        var source = renderPassEvent == RenderPassEvent.AfterRendering && renderingData.cameraData.resolveFinalTarget
            ? renderingData.cameraData.renderer.cameraColorTarget
            : _cameraColorTarget;
        // コマンドバッファを作成
        var cmd = CommandBufferPool.Get(RenderPassName);
        cmd.Clear();

        // Cameraのターゲットと同じDescription（Depthは無し）のRenderTextureを取得する
        var tempTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        tempTargetDescriptor.depthBufferBits = 0;
        cmd.GetTemporaryRT(_tempRenderTargetHandle.id, tempTargetDescriptor);

        using (new ProfilingScope(cmd, _profilingSampler))
        {
            // Volumeからプロパティを反映
            _material.SetFloat(_splitXPropertyId, _volume.splitX.value);
            _material.SetFloat(_splitYPropertyId, _volume.splitY.value);
            _material.SetFloat(_shiftPropertyId, _volume.shift.value);
            _material.SetFloat(_frecPropertyId, _volume.frec.value);
            _material.SetFloat(_colorGapPropertyId, _volume.colorGap.value);
            _material.SetFloat(_ratioPropertyId, _volume.ratio.value);
            _material.SetFloat(_strengthPropertyId, _volume.strength.value);
            _material.SetFloat(_blurPropertyId, _volume.blur.value);


            cmd.SetGlobalTexture(_mainTexPropertyId, source);

            // 元のテクスチャから一時的なテクスチャにエフェクトを適用しつつ描画
            Blit(cmd, source, _tempRenderTargetHandle.Identifier(), _material);
        }

        // 一時的なテクスチャから元のテクスチャに結果を書き戻す
        Blit(cmd, _tempRenderTargetHandle.Identifier(), source);

        // 一時的なRenderTextureを解放する
        cmd.ReleaseTemporaryRT(_tempRenderTargetHandle.id);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private static RenderPassEvent GetRenderPassEvent(PostprocessTiming postprocessTiming)
    {
        switch (postprocessTiming)
        {
            case PostprocessTiming.AfterOpaque:
                return RenderPassEvent.AfterRenderingSkybox;
            case PostprocessTiming.BeforePostprocess:
                return RenderPassEvent.BeforeRenderingPostProcessing;
            case PostprocessTiming.AfterPostprocess:
                return RenderPassEvent.AfterRendering;
            default:
                throw new ArgumentOutOfRangeException(nameof(postprocessTiming), postprocessTiming, null);
        }
    }
}