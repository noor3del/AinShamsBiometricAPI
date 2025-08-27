using Neurotec;
using Neurotec.Biometrics;
using Neurotec.Images;
using Neurotec.IO;
using Neurotec.Sound;
using System.Text;

namespace AinShamsBiometric.Application.Helpers
{
    public class EnrollDataSerializer
    {
        #region Private constructor

        private EnrollDataSerializer() { }

        #endregion

        #region Private fields

        private static readonly NVersion Version = new NVersion(1, 0);
        private static readonly string EnrollDataHeader = "NASPS";

        #endregion

        #region Private static methods

        private static void WriteImage(BinaryWriter writer, NImage image, NImageFormat format)
        {
            if (image != null)
            {
                using (NBuffer buffer = image.Save(format))
                {
                    WriteBuffer(writer, buffer);
                }
            }
            else
            {
                WriteBuffer(writer, NBuffer.Empty);
            }
        }

        private static void WriteBuffer(BinaryWriter writer, NBuffer buffer)
        {
            writer.Write(buffer.Size);
            writer.Write(buffer.ToArray());
        }

        private static bool CheckHeaderAndVersion(BinaryReader reader)
        {
            byte[] headerBytes = reader.ReadBytes(Encoding.ASCII.GetByteCount(EnrollDataHeader));
            string header = Encoding.ASCII.GetString(headerBytes);
            if (header != EnrollDataHeader) return false;
            NVersion ver = (NVersion)reader.ReadUInt16();
            if (ver > Version) return false;

            return true;
        }

        private static void WriteHeaderAndVersion(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes(EnrollDataHeader));
            writer.Write((ushort)Version);
        }

        #endregion

        #region Public static methods

        public static NBuffer Serialize(NSubject subject, bool useWsqForFingers)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                WriteHeaderAndVersion(writer);

                var fingers = SubjectUtils.GetTemplateCompositeFingers(subject).ToArray();
                writer.Write(fingers.Length);
                foreach (var finger in fingers)
                {
                    WriteImage(writer, finger.Image, useWsqForFingers ? NImageFormat.Wsq : NImageFormat.Png);
                }

                var faces = SubjectUtils.GetTemplateCompositeFaces(subject).ToArray();
                writer.Write(faces.Length);
                foreach (var face in faces)
                {
                    NLAttributes attributes = face.Objects.First();
                    NLTemplate template = attributes.Template;
                    writer.Write(template.Records.Count);
                    WriteImage(writer, face.Image, NImageFormat.Png);
                }

                var irises = SubjectUtils.GetTemplateCompositeIrises(subject).ToArray();
                writer.Write(irises.Length);
                foreach (var iris in irises)
                {
                    WriteImage(writer, iris.Image, NImageFormat.Png);
                }

                var palms = SubjectUtils.GetTemplateCompositePalms(subject).ToArray();
                writer.Write(palms.Length);
                foreach (var palm in palms)
                {
                    WriteImage(writer, palm.Image, NImageFormat.Png);
                }

                var voices = SubjectUtils.GetTemplateCompositeVoices(subject).ToArray();
                writer.Write(voices.Length);
                foreach (var voice in voices)
                {
                    WriteBuffer(writer, voice.SoundBuffer != null ? voice.SoundBuffer.Save() : NBuffer.Empty);
                }

                writer.Write(0); // fin

                return new NBuffer(stream.ToArray());
            }
        }

        public static NBuffer Serialize(NSubject subject)
        {
            return Serialize(subject, false);
        }

        public static NSubject Deserialize(NBuffer templateData, NBuffer serializeData, out int[] faceRecordCounts)
        {
            List<int> recrodCounts = new List<int>();

            using (NTemplate template = new NTemplate(templateData))
            using (MemoryStream stream = new MemoryStream(serializeData.ToArray()))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                NSubject subject = new NSubject();
                int length;
                int bufferSize;
                byte[]? bytes = null;
                int recordIndex = 0;

                if (!CheckHeaderAndVersion(reader)) throw new FormatException();

                // fingers
                length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    bufferSize = reader.ReadInt32();
                    bytes = reader.ReadBytes(bufferSize);
                    using (NImage? image = bufferSize > 0 ? NImage.FromMemory(bytes) : null)
                    {
                        NFinger finger = (NFinger)NFrictionRidge.FromImageAndTemplate(image, template.Fingers.Records[i]);
                        subject.Fingers.Add(finger);
                    }
                }

                // allFaces
                length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    int recordCount = reader.ReadInt32();
                    recrodCounts.Add(recordCount);

                    bufferSize = reader.ReadInt32();
                    bytes = reader.ReadBytes(bufferSize);
                    using (NImage? image = bufferSize > 0 ? NImage.FromMemory(bytes) : null)
                    {
                        NLTemplate actualTemplate = new NLTemplate();
                        NLAttributes attributes = new NLAttributes();
                        for (int j = 0; j < recordCount; j++)
                        {
                            actualTemplate.Records.Add(template.Faces.Records[recordIndex++]);
                        }

                        attributes.Template = actualTemplate;
                        NFace face = NFace.FromImageAndAttributes(image, attributes);
                        subject.Faces.Add(face);
                    }
                }

                // irises
                length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    bufferSize = reader.ReadInt32();
                    bytes = reader.ReadBytes(bufferSize);
                    using (NImage? image = bufferSize > 0 ? NImage.FromMemory(bytes) : null)
                    {
                        NIris iris = NIris.FromImageAndTemplate(image, template.Irises.Records[i]);
                        subject.Irises.Add(iris);
                    }
                }

                // palms
                length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    bufferSize = reader.ReadInt32();
                    bytes = reader.ReadBytes(bufferSize);
                    using (NImage? image = bufferSize > 0 ? NImage.FromMemory(bytes) : null)
                    {
                        NPalm palm = (NPalm)NFrictionRidge.FromImageAndTemplate(image, template.Palms.Records[i]);
                        subject.Palms.Add(palm);
                    }
                }

                // voices
                length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    bufferSize = reader.ReadInt32();
                    bytes = reader.ReadBytes(bufferSize);
                    using (NSoundBuffer? buffer = bufferSize > 0 ? NSoundBuffer.FromMemory(bytes) : null)
                    {
                        NVoice? voice = NVoice.FromSoundBufferAndTemplate(buffer, template.Voices.Records[i]);
                        subject.Voices.Add(voice);
                    }
                }

                faceRecordCounts = recrodCounts.ToArray();
                return subject;
            }
        }

        public static NSubject Deserialize(NBuffer templateData, NBuffer serializeData)
        {
            int[] recordCounts;
            return Deserialize(templateData, serializeData, out recordCounts);
        }

        #endregion
    }
}
