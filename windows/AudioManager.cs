using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AudioShare
{
    public class AudioManager
    {
        public static event EventHandler<AudioFrameEventArgs> AudioAvailable;
        public static event EventHandler Stoped;
        public static event EventHandler<int> OnVolumeNotification;

        private static WasapiLoopbackCapture _capture;
        private static MMDevice _device;
        private static int _sampleRate;
        private static WaveFormat _currentFormat;
        private static ChannelRole[] _sourceRoles = ChannelRoleHelper.GetDefaultRoles(2);

        private AudioManager() { }

        public static int SampleRate => _sampleRate;
        public static WaveFormat CurrentFormat => _currentFormat;
        public static ChannelRole[] SourceRoles => _sourceRoles;

        public static void SetDevice(MMDevice device, int sampleRate)
        {
            Logger.Info("set device start");
            if (_capture != null)
            {
                _capture.DataAvailable -= SendAudioData;
            }
            _capture?.Dispose();
            Stoped?.Invoke(null, null);
            if (_device != null)
            {
                _device.AudioEndpointVolume.OnVolumeNotification -= OnVolumeChange;
            }
            if(device == null && sampleRate == 0)
            {
                _device?.Dispose();
            }
            _device = device;
            OnVolumeNotification?.Invoke(null, (int)((_device?.AudioEndpointVolume.MasterVolumeLevelScalar ?? 0) * 100));
            _sampleRate = sampleRate;
            if (_device == null)
            {
                _capture = null;
                _currentFormat = null;
                _sourceRoles = ChannelRoleHelper.GetDefaultRoles(2);
                return;
            }
            int channelCount = GetChannelCount(device);
            _capture = new WasapiLoopbackCapture(device);
            _capture.WaveFormat = new WaveFormat(sampleRate, 16, channelCount);
            _currentFormat = _capture.WaveFormat;
            _sourceRoles = ChannelRoleHelper.GetDefaultRoles(channelCount);
            _capture.DataAvailable += SendAudioData;
            if (AudioAvailable != null)
            {
                StartCapture();
            }
            _device.AudioEndpointVolume.OnVolumeNotification += OnVolumeChange;
            Logger.Info("set device end");
        }

        public static void StartCapture()
        {
            if (_capture == null) return;
            try
            {
                if (_capture.CaptureState == CaptureState.Stopped)
                {
                    _capture.StartRecording();
                }
            }
            catch (Exception)
            {

            }
        }

        public static void StopCapture()
        {
            try
            {
                _capture?.StopRecording();
            }
            catch (Exception)
            {

            }
        }

        private static CancellationTokenSource SetRemoteVolumeCancel = null;
        private static async void OnVolumeChange(AudioVolumeNotificationData data)
        {
            SetRemoteVolumeCancel?.Cancel();
            SetRemoteVolumeCancel = new CancellationTokenSource();
            CancellationToken token = SetRemoteVolumeCancel.Token;
            await Task.Delay(200);
            if (token.IsCancellationRequested) return;
            OnVolumeNotification?.Invoke(null, data.Muted ? 0 : (int)(data.MasterVolume * 100));
        }

        private static int GetChannelCount(MMDevice device)
        {
            try
            {
                int channels = device?.AudioClient?.MixFormat?.Channels ?? 2;
                if (channels <= 0) channels = 2;
                if (channels > 8) channels = 8;
                return channels;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        private static void SendAudioData(object sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded <= 0) return;
            Logger.Debug("set audio data start");
            var format = _currentFormat ?? _capture?.WaveFormat;
            if (format == null) return;
            var channelRoles = ChannelRoleHelper.EnsureLength(_sourceRoles, format?.Channels ?? 2);
            AudioAvailable?.Invoke(null, new AudioFrameEventArgs(e.Buffer, e.BytesRecorded, format, channelRoles));
            Logger.Debug("set audio data end");
        }
    }
}
