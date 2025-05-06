using UnityEngine;
using System;
using System.IO;

public static class SavWav
{
    const int HEADER_SIZE = 44;

    public static byte[] GetWavBytes(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] bytesData = ConvertAndWrite(samples, clip.frequency, clip.channels);
        WriteHeader(bytesData, clip);

        return bytesData;
    }

    static byte[] ConvertAndWrite(float[] samples, int hz, int channels)
    {
        MemoryStream stream = new MemoryStream();
        int length = samples.Length;
        int byteLength = length * sizeof(short);
        byte[] bytesData = new byte[byteLength + HEADER_SIZE];

        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(value);
            byteArr.CopyTo(bytesData, HEADER_SIZE + i * 2);
        }

        return bytesData;
    }

    static void WriteHeader(byte[] stream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        MemoryStream headerStream = new MemoryStream(stream);
        BinaryWriter writer = new BinaryWriter(headerStream);

        writer.Seek(0, SeekOrigin.Begin);
        writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
        writer.Write(stream.Length - 8);
        writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
        writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((ushort)1);
        writer.Write((ushort)channels);
        writer.Write(hz);
        writer.Write(hz * channels * 2);
        writer.Write((ushort)(channels * 2));
        writer.Write((ushort)16);
        writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
        writer.Write(stream.Length - HEADER_SIZE);
    }
}
