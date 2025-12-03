using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Helpers;
using UnityEngine;
using WebSocketSharp;

namespace TTT.Managers
{
    public partial class AudioManager : GenericSingleton<AudioManager>
    {
        public void OnPlayAudioEvent(object args)
        {
            var eventArgs = args as AudioEventArgs;
            if (eventArgs.ToPlay != null)
            {
                Debug.Log($"Playing {eventArgs.Type}");
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

        public void OnSystemStateChange(object args)
        {
            var eventArgs = args as StateSystemChangeEventArgs;
            string track = eventArgs.NewState switch
            {
                SystemState.MAIN_MENU => "main_menu",
                SystemState.PLAYING => "new_game",
                _ => "",
            };

            if (!track.IsNullOrEmpty())
            {
                PlayMusic(track);
            }
        }
    }
}
