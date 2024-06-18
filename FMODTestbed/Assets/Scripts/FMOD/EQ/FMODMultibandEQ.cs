using FMOD;

namespace Sonosthesia
{
    public class FMODMultibandEQ : FMODProcessor
    {
        protected override bool PerformTrySetup(ChannelGroup channelGroup)
        {
            return true;
        }

        protected override void PerformCleanup(ChannelGroup channelGroup)
        {
            
        }
    }
}