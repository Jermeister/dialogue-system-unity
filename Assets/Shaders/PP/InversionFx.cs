using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class InversionFx : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public RenderTargetIdentifier source;
        private RenderTargetHandle tempRenderTargetHandler;
        private RenderTargetHandle cameraColorTexture;

        public Material material;
        
        public CustomRenderPass(Material material)
        {
            this.material = material;
            
            tempRenderTargetHandler.Init("_TemporaryColorTexture");
            cameraColorTexture.Init("_CameraColorTexture");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get();

            //commandBuffer.GetTemporaryRT(tempRenderTargetHandler.id, renderingData.cameraData.cameraTargetDescriptor);
            //Blit(commandBuffer, source, tempRenderTargetHandler.Identifier(), material);
            //Blit(commandBuffer, tempRenderTargetHandler.Identifier(), source);

            commandBuffer.GetTemporaryRT(cameraColorTexture.id, renderingData.cameraData.cameraTargetDescriptor);
            Blit(commandBuffer, source, cameraColorTexture.Identifier(), material);
            Blit(commandBuffer, cameraColorTexture.Identifier(), source);

            context.ExecuteCommandBuffer(commandBuffer);
            
            CommandBufferPool.Release(commandBuffer);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material material = null;
    }

    public Settings settings = new Settings();
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(settings.material);

        // Configures where the render pass should be injected.
        // m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


