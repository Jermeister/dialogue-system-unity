using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GradientFog : ScriptableRendererFeature
{
    [System.Serializable]
    public class GradientFogSettings
    {
        public Material baseMat = null;
    }
    
    class GradientFogPass : ScriptableRenderPass
    {
        public GradientFogSettings settings;

        private Material fogMaterial;

        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle m_TempTex;

        public GradientFogPass()
        {
            m_TempTex.Init("_TempTex");
            //fogMaterial = new Material(Shader.Find("Shader Graphs/GradientFogGraph"));
            //fogMaterial.SetOverrideTag("RenderType", "Transparent");

            //fogMaterial.renderQueue = 2501;
        }

        public void Setup(RenderTargetIdentifier cameraColorTarget)
        {
            fogMaterial = settings.baseMat;
            source = cameraColorTarget;
        }
        
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("NewInversionFx");

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            
            opaqueDesc.depthBufferBits = 0;
            
            cmd.GetTemporaryRT(m_TempTex.id, opaqueDesc);
            
            
            Blit(cmd, source, m_TempTex.Identifier(), fogMaterial);
            Blit(cmd, m_TempTex.Identifier(), source);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TempTex.id);
        }
    }

    GradientFogPass m_GradientFogPass;
    public GradientFogSettings settings = new GradientFogSettings();

    public override void Create()
    {
        m_GradientFogPass = new GradientFogPass();
        m_GradientFogPass.settings = settings;

        // Configures where the render pass should be injected.
        m_GradientFogPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        m_GradientFogPass.Setup(src);
        renderer.EnqueuePass(m_GradientFogPass);
    }
}


