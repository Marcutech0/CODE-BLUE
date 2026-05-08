using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;

namespace CodeBlue
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        const string MIXER_MASTER = "MasterVolume";
        const string MIXER_UI = "UIVolume";
        const string MIXER_MUSIC = "MusicVolume";
        const string MIXER_GAME = "GameVolume";

        [Header("SFX")]
        [field: SerializeField] public List<AudioClip> SFXClips { get; private set; } = new();

        [SerializeField]
        AudioClip[] _bgms;

        [Header("References")]
        [SerializeField] AudioMixer _mixer;
        [SerializeField] AudioSource _uiMaster;
        [SerializeField] AudioSource _musicMaster;
        [SerializeField] AudioSource _gameMaster;

        Coroutine _bgmCoroutine;

        public void PlayGameSfx(string sfxName)
        {
            var clip = GetSFXClip(sfxName);
            _gameMaster.PlayOneShot(clip);
        }
        public void PlayUISfx(string sfxName)
        {
            var clip = GetSFXClip(sfxName);
            _uiMaster.PlayOneShot(clip);
        }
        public void PlayRandomBGM()
        {
            if (_bgmCoroutine != null)
                StopCoroutine(_bgmCoroutine);

            var bgm = _bgms.SelectRandom();
            _bgmCoroutine = StartCoroutine(PlayBGMInternal(bgm));
        }

        IEnumerator PlayBGMInternal(AudioClip bgm)
        {
            print("[Audio] playing " + bgm.name);
            _musicMaster.Stop();
            _musicMaster.clip = bgm;
            _musicMaster.Play();

            yield return new WaitForSeconds(bgm.length);

            PlayRandomBGM();
        }

        AudioClip GetSFXClip(string sfxName)
        {
            var clip = SFXClips.Where(s => s.name == "sfx_" + sfxName || s.name == "sfx-" + sfxName).FirstOrDefault();
            Assert.IsNotNull(clip, "SFX " + sfxName + " not found!");
            return clip;
        }

        public void SetMasterVolume(float val) => SetMixerChannelVol(MIXER_MASTER, val);
        public void SetMusicVolume(float val) => SetMixerChannelVol(MIXER_MUSIC, val);
        public void SetUIVolume(float val) => SetMixerChannelVol(MIXER_UI, val);
        public void SetGameVolume(float val) => SetMixerChannelVol(MIXER_GAME, val);

        void SetMixerChannelVol(string channel, float val) => _mixer.SetFloat(channel, Mathf.Log10(val) * 20);

        #region networked functions

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_PlayGameSfxRpc(string sfxName) => PlayGameSfx(sfxName);

        #endregion
    }
}
