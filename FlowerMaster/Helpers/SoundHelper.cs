using System;
using Vannatech.CoreAudio.Constants;
using Vannatech.CoreAudio.Enumerations;
using Vannatech.CoreAudio.Externals;
using Vannatech.CoreAudio.Interfaces;

namespace FlowerMaster.Helpers
{
    class SoundHelper
    {
        /// <summary>
        /// 是否在静音状态
        /// </summary>
        public static bool isMute = false;
        /// <summary>
        /// 是否用户手动静音状态
        /// </summary>
        public static bool userMute = false;
        /// <summary>
        /// 音频管理变量
        /// </summary>
        private static ISimpleAudioVolume simpleAudioVolume;

        /// <summary>
        /// 初始化音频管理接口
        /// </summary>
        public static void InitSoundAPI()
        {
            var deviceEnumeratorType = Type.GetTypeFromCLSID(new Guid(ComCLSIDs.MMDeviceEnumeratorCLSID));
            var devenum = (IMMDeviceEnumerator)Activator.CreateInstance(deviceEnumeratorType);

            IMMDevice device;
            devenum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);

            object objSessionManager;
            device.Activate(new Guid(ComIIDs.IAudioSessionManager2IID), (uint)CLSCTX.CLSCTX_INPROC_SERVER, IntPtr.Zero, out objSessionManager);
            var sessionManager = objSessionManager as IAudioSessionManager2;
            if (sessionManager == null) throw new Exception("Session没有找到。");

            IAudioSessionEnumerator sessions;
            sessionManager.GetSessionEnumerator(out sessions);

            ISimpleAudioVolume ts;
            sessionManager.GetSimpleAudioVolume(Guid.Empty, 0, out ts);
            simpleAudioVolume = ts;

            ts.GetMute(out isMute);
        }

        /// <summary>
        /// 软件静音
        /// </summary>
        /// <param name="user">是否为用户操作</param>
        public static void Mute(bool user = false)
        {
            if (user) userMute = !userMute;

            bool newValue = !isMute;
            simpleAudioVolume.SetMute(newValue, Guid.NewGuid());

            bool resultValue;
            simpleAudioVolume.GetMute(out resultValue);

            isMute = resultValue;
        }

    }
}
