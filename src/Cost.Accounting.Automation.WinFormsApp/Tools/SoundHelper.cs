using System.IO;
using System.Media;
using System.Text;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

/// <summary>
/// Mesajlaşma sesli bildirimlerini tek yerden yönetir.
/// </summary>
/// <remarks>
/// <para>
/// Sesler kurulum klasörüne bir dosya koymadan, çalışma anında sentezlenir:
/// kısa bir WAV tamponu üretilir ve <see cref="SoundPlayer"/> ile çalınır.
/// Böylece projeye medya eklenmez, kurulum kopyalaması yapılmaz ve ses
/// her makinede aynı çıkardı.
/// </para>
/// <para>
/// Yeni mesaj sesi yükselen üç notalı bir "bip" (mesajlaşma uygulamalarındaki
/// bildirim gibi), oturum açma sesi ise daha yumuşak iki notadır. Ses
/// kartı ya da codec hatası durumunda sessizce sistem sesine düşülür;
/// uygulama hiçbir koşulda bu nedenle düşmez.
/// </para>
/// <para>
/// Bildirim sesi kullanıcı tarafından kapatılabilir (<see cref="IsEnabled"/>).
/// </para>
/// </remarks>
public static class SoundHelper
{
    private static int _muted;

    private static readonly object Sync = new();

    private static SoundPlayer? _messagePlayer;
    private static SoundPlayer? _presencePlayer;

    /// <summary>Sesli bildirim açık mı? Varsayılan olarak açıktır.</summary>
    public static bool IsEnabled
    {
        get => Volatile.Read(ref _muted) == 0;
        set => Interlocked.Exchange(ref _muted, value ? 0 : 1);
    }

    /// <summary>Birisi oturum açtığında çalar.</summary>
    public static void PlaySignedIn()
    {
        if (!IsEnabled)
        {
            return;
        }

        try
        {
            PresencePlayer.Play();
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("SoundHelper.PlaySignedIn", ex);
            PlayFallback(SystemSounds.Asterisk);
        }
    }

    /// <summary>Yeni mesaj geldiğinde çalar.</summary>
    public static void PlayNewMessage()
    {
        if (!IsEnabled)
        {
            return;
        }

        try
        {
            MessagePlayer.Play();
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("SoundHelper.PlayNewMessage", ex);
            PlayFallback(SystemSounds.Exclamation);
        }
    }

    private static SoundPlayer MessagePlayer
    {
        get
        {
            lock (Sync)
            {
                // Yükselen üç nota: kısa - kısa - biraz uzun. Mesajlaşma
                // uygulamalarının bildirim ritmine en yakın olan budur.
                return _messagePlayer ??= new SoundPlayer(new MemoryStream(
                    BuildChime([(880d, 70), (1174.66d, 70), (1567.98d, 150)])));
            }
        }
    }

    private static SoundPlayer PresencePlayer
    {
        get
        {
            lock (Sync)
            {
                return _presencePlayer ??= new SoundPlayer(new MemoryStream(
                    BuildChime([(783.99d, 80), (1046.5d, 160)])));
            }
        }
    }

    private static void PlayFallback(SystemSound sound)
    {
        try
        {
            sound.Play();
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("SoundHelper.PlayFallback", ex);
        }
    }

    /// <summary>
    /// Verilen notalardan 16 bitlik mono bir WAV tamponu üretir.
    /// </summary>
    /// <remarks>
    /// Her nota, kısa bir artış (attack) ve azalma (release) zarfıyla
    /// sönümlenir; aksi hâlde keskin tıklamalar duyulur. Ses düzeyi
    /// bilinçli olarak düşüktür (≈ %32 genlik): masaüstü bildirimi
    /// rahatsız etmemelidir.
    /// </remarks>
    private static byte[] BuildChime(IReadOnlyList<(double Frequency, int Milliseconds)> tones)
    {
        const int SampleRate = 44100;
        const short BitsPerSample = 16;
        const short Channels = 1;

        int totalSamples = 0;

        foreach (var tone in tones)
        {
            totalSamples += SampleRate * tone.Milliseconds / 1000;
        }

        int dataLength = totalSamples * Channels * (BitsPerSample / 8);

        using var stream = new MemoryStream(44 + dataLength);

        void WriteAscii(string value) => stream.Write(Encoding.ASCII.GetBytes(value));
        void WriteInt32(int value) => stream.Write(BitConverter.GetBytes(value));
        void WriteInt16(short value) => stream.Write(BitConverter.GetBytes(value));

        // WAV / RIFF başlığı.
        WriteAscii("RIFF");
        WriteInt32(36 + dataLength);
        WriteAscii("WAVE");
        WriteAscii("fmt ");
        WriteInt32(16);
        WriteInt16(1);                                    // PCM
        WriteInt16(Channels);
        WriteInt32(SampleRate);
        WriteInt32(SampleRate * Channels * (BitsPerSample / 8));
        WriteInt16((short)(Channels * (BitsPerSample / 8)));
        WriteInt16(BitsPerSample);
        WriteAscii("data");
        WriteInt32(dataLength);

        int attackSamples = Math.Max(1, SampleRate / 200);   // ~5 ms
        int releaseSamples = Math.Max(1, SampleRate / 25);   // ~40 ms

        foreach (var tone in tones)
        {
            double frequency = tone.Frequency;
            int count = SampleRate * tone.Milliseconds / 1000;

            for (int i = 0; i < count; i++)
            {
                double attack = i < attackSamples
                    ? (double)i / attackSamples
                    : 1d;

                double release = count - i < releaseSamples
                    ? (double)(count - i) / releaseSamples
                    : 1d;

                double envelope = attack * release;
                double sample = Math.Sin(2d * Math.PI * frequency * i / SampleRate)
                    * envelope
                    * 0.32d;

                stream.Write(BitConverter.GetBytes((short)(sample * short.MaxValue)));
            }
        }

        return stream.ToArray();
    }
}
