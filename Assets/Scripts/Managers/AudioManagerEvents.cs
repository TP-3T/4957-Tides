using System;
using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Helpers;

namespace TTT.Managers
{
    public partial class AudioManager : GenericSingleton<AudioManager>
    {
        public void OnPlayAudioEvent(object args)
        {
            var eventArgs = args as AudioEventArgs;
            if (eventArgs.ToPlay != null)
            {
                switch (eventArgs.Type)
                {
                    case AudioTypes.AMBIENCE:
                    {
                        PlayAmbience(eventArgs.ToPlay);
                        break;
                    }
                    case AudioTypes.ONESHOT:
                    {
                        PlayOneShotSound(eventArgs.ToPlay);
                        break;
                    }
                    case AudioTypes.MUSIC:
                    {
                        PlayMusic(eventArgs.ToPlay);
                        break;
                    }
                }
            }
        }
    }
}
