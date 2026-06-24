using MeltySynth;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

namespace ITransitionTask5.Services
{
    public static class AudioService
    {

        private static readonly string _sfPath = Path.Combine(AppContext.BaseDirectory, "TimGM6mb.sf2");


        public static byte[] Generate(int seed)
        {
            var midiBytes = GenerateMidi(seed);
            return ConvertToWav(midiBytes);
        }

        private static byte[] GenerateMidi(int seed)
        {
            var rng = new Random(seed);

            var rootNotes = new[] { NoteName.C, NoteName.D, NoteName.E, NoteName.F, NoteName.G, NoteName.A };
            var durations = new[] { MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter, MusicalTimeSpan.Half };
            var programs = new[] {
                GeneralMidiProgram.AcousticGrandPiano,
                GeneralMidiProgram.AcousticGuitar1,
                GeneralMidiProgram.Flute,
                GeneralMidiProgram.Violin
            };

            var root = rootNotes[rng.Next(rootNotes.Length)];
            int octave = rng.Next(3, 6);
            int bpm = rng.Next(70, 150);

            var availableNotes = new[]
            {
                Melanchall.DryWetMidi.MusicTheory.Note.Get(root, octave),
                Melanchall.DryWetMidi.MusicTheory.Note.Get(rootNotes[(Array.IndexOf(rootNotes, root) + 2) % rootNotes.Length], octave),
                Melanchall.DryWetMidi.MusicTheory.Note.Get(rootNotes[(Array.IndexOf(rootNotes, root) + 4) % rootNotes.Length], octave),
                Melanchall.DryWetMidi.MusicTheory.Note.Get(rootNotes[(Array.IndexOf(rootNotes, root) + 5) % rootNotes.Length], octave),
                Melanchall.DryWetMidi.MusicTheory.Note.Get(root, octave + 1),
            };

            var builder = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Quarter)
                .SetOctave(Octave.Get(octave))
                .ProgramChange(programs[rng.Next(programs.Length)]);

            for (int i = 0; i < 32; i++)
                builder.Note(availableNotes[rng.Next(availableNotes.Length)], durations[rng.Next(durations.Length)]);

            var midiFile = builder.Build()
                .ToFile(TempoMap.Create(Tempo.FromBeatsPerMinute(bpm)));

            using var ms = new MemoryStream();
            midiFile.Write(ms, MidiFileFormat.SingleTrack);
            return ms.ToArray();
        }

        private static byte[] ConvertToWav(byte[] midiBytes)
        {
            const int SampleRate = 44100;

            using var midiStream = new MemoryStream(midiBytes);
            var midiFile = new MeltySynth.MidiFile(midiStream);
            var synthesizer = new Synthesizer(_sfPath, SampleRate);
            var sequencer = new MidiFileSequencer(synthesizer);
            sequencer.Play(midiFile, false);

            var left = new float[(int)(SampleRate * midiFile.Length.TotalSeconds)];
            var right = new float[(int)(SampleRate * midiFile.Length.TotalSeconds)];
            sequencer.Render(left, right);

            return WriteWav(left, right, SampleRate);
        }

        private static byte[] WriteWav(float[] left, float[] right, int sampleRate)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            int byteCount = left.Length * 4;

            w.Write("RIFF"u8.ToArray()); w.Write(36 + byteCount);
            w.Write("WAVE"u8.ToArray());
            w.Write("fmt "u8.ToArray()); w.Write(16);
            w.Write((short)1); w.Write((short)2);
            w.Write(sampleRate); w.Write(sampleRate * 4);
            w.Write((short)4); w.Write((short)16);
            w.Write("data"u8.ToArray()); w.Write(byteCount);

            for (int i = 0; i < left.Length; i++)
            {
                w.Write((short)(Math.Clamp(left[i], -1f, 1f) * short.MaxValue));
                w.Write((short)(Math.Clamp(right[i], -1f, 1f) * short.MaxValue));
            }

            return ms.ToArray();
        }
    }
}