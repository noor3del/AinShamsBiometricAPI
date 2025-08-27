using Neurotec.Biometrics;

namespace AinShamsBiometric.Application.Helpers
{
    public class SubjectUtils
    {
        #region Private constructor

        private SubjectUtils() { }

        #endregion

        #region Helper functions

        public static bool IsMultipleNodeResult(NBiometric biometric)
        {
            if (biometric.SessionId == -1)
            {
                NBiometricAttributes parentObject = biometric.ParentObject;
                if (parentObject != null)
                {
                    NBiometric owner = (NBiometric)parentObject.Owner;
                    return owner != null && owner.SessionId != -1;
                }
            }
            return false;
        }

        public static bool IsMultipleNodeSource(NBiometric biometric)
        {
            return biometric.SessionId != -1;
        }

        public static IEnumerable<NFace[]> GetFaceGroups(NSubject subject)
        {
            return GetFaceGroups(subject.Faces.ToArray());
        }

        public static IEnumerable<NFace[]> GetFaceGroups(NFace[] allFaces)
        {
            foreach (var item in allFaces.Where(x => x.SessionId == -1 && x.ParentObject == null))
            {
                yield return new[] { item };
            }

            var ids = allFaces.Select(x => x.SessionId).Distinct().Except(new[] { -1 }).ToArray();
            foreach (var id in ids)
            {
                NFace[] faces = allFaces.Where(x => x.SessionId == id).ToArray();
                if (faces.Any(x => x.ParentObject != null))
                    yield return faces.Where(x => x.ParentObject != null).ToArray();
                if (faces.Any(x => x.ParentObject == null))
                    yield return faces.Where(x => x.ParentObject == null).ToArray();
            }
        }

        public static IEnumerable<NFinger[]> GetFingersGroups(NSubject subject)
        {
            return GetFingersGroups(subject.Fingers.ToArray());
        }

        public static IEnumerable<NFinger[]> GetFingersGroups(NFinger[] fingers)
        {
            return GetFrictionRidgeGroups(fingers);
        }

        public static IEnumerable<NPalm[]> GetPalmsGroups(NSubject subject)
        {
            return GetPalmsGroups(subject.Palms.ToArray());
        }

        public static IEnumerable<NPalm[]> GetPalmsGroups(NPalm[] palms)
        {
            return GetFrictionRidgeGroups(palms);
        }

        public static IEnumerable<NFinger> GetFingersInSameGroup(NFinger[] fingers, NFinger finger)
        {
            return GetFrictionRidgesInSameGroup(fingers, finger);
        }

        public static IEnumerable<NPalm> GetPalmsInSameGroup(NPalm[] palms, NPalm palm)
        {
            return GetFrictionRidgesInSameGroup(palms, palm);
        }

        public static IEnumerable<NFace> GetFacesInSameGroup(NFace[] faces, NFace face)
        {
            if (IsMultipleNodeSource(face))
            {
                bool hasParent = face.ParentObject != null;

                var result = faces
                    .Where(x => x.SessionId == face.SessionId && (x.ParentObject != null) == hasParent)
                    .ToList();

                // use null-conditional + safe cast
                var attributes = face.Objects.FirstOrDefault();
                var child = attributes?.Child as NFace;

                if (child is not null && child.SessionId == -1)
                    result.Add(child);

                if (result.Count > 0)
                    return result;
            }
            else if (IsMultipleNodeResult(face))
            {
                if (face.ParentObject is NLAttributes parentObject)
                {
                    var owner = parentObject.Owner;
                    return GetFacesInSameGroup(faces, owner);
                }
            }

            return new[] { face };
        }


        #endregion

        #region Flatten fingers

        public static IEnumerable<NFinger> FlattenFingers(NFinger[] fingers)
        {
            List<NFinger> result = new List<NFinger>();
            foreach (var item in fingers)
            {
                result.Add(item);
                NFinger[] children = item.Objects.ToArray().Select(x => (NFinger)x.Child).Where(x => x != null).ToArray();
                result.AddRange(FlattenFingers(children));
            }
            return result.Distinct();
        }

        public static IEnumerable<NPalm> FlattenPalms(NPalm[] palms)
        {
            List<NPalm> result = new List<NPalm>();
            foreach (var item in palms)
            {
                result.Add(item);
                NPalm[] children = item.Objects.ToArray().Select(x => (NPalm)x.Child).Where(x => x != null).ToArray();
                result.AddRange(FlattenPalms(children));
            }
            return result.Distinct();
        }

        #endregion

        #region Get template composites

        public static IEnumerable<NFinger> GetTemplateCompositeFingers(NSubject subject)
        {
            var allFingers = subject.Fingers.ToArray();
            foreach (var finger in allFingers.Where(x => x.SessionId == -1))
            {
                var attributes = finger.Objects.ToArray();
                if (attributes.Length == 1 && attributes[0].Template != null)
                {
                    yield return finger;
                }
            }
        }

        public static IEnumerable<NFace> GetTemplateCompositeFaces(NSubject subject)
        {
            var allFaces = subject.Faces.ToArray();

            foreach (var face in allFaces.Where(x => x.SessionId == -1))
            {
                var attributes = face.Objects.FirstOrDefault(); // NLAttributes? (nullable)
                if (attributes?.Template != null)
                {
                    yield return face;
                }
            }
        }


        public static IEnumerable<NIris> GetTemplateCompositeIrises(NSubject subject)
        {
            var allIrises = subject.Irises.ToArray();
            foreach (var iris in allIrises.Where(x => x.SessionId == -1))
            {
                var attributes = iris.Objects.ToArray();
                if (attributes.Length == 1 && attributes[0].Template != null)
                {
                    yield return iris;
                }
            }
        }

        public static IEnumerable<NPalm> GetTemplateCompositePalms(NSubject subject)
        {
            var allPalms = subject.Palms.ToArray();
            foreach (var palm in allPalms.Where(x => x.SessionId == -1))
            {
                var attributes = palm.Objects.ToArray();
                if (attributes.Length == 1 && attributes[0].Template != null)
                {
                    yield return palm;
                }
            }
        }

        public static IEnumerable<NVoice> GetTemplateCompositeVoices(NSubject subject)
        {
            var allVoices = subject.Voices.ToArray();
            foreach (var voice in allVoices.Where(x => x.SessionId == -1))
            {
                var attributes = voice.Objects.ToArray();
                if (attributes.Length == 1 && attributes[0].Template != null)
                {
                    yield return voice;
                }
            }
        }

        #endregion

        #region Private static methods

        private static IEnumerable<T> GetFrictionRidgesInSameGroup<T>(T[] allFingers, T finger) where T : NFrictionRidge
        {
            if (IsMultipleNodeSource(finger))
            {
                var result = new List<T>();

                foreach (var item in allFingers)
                {
                    if (item.Position == finger.Position &&
                        item.ImpressionType == finger.ImpressionType &&
                        item.SessionId == finger.SessionId)
                    {
                        result.Add(item);
                    }
                }

                var attributes = finger.Objects.FirstOrDefault(); // NFAttributes?
                if (attributes?.Child is T child && child.SessionId == -1)
                {
                    result.Add(child);
                }

                return result;
            }
            else if (IsMultipleNodeResult(finger))
            {
                if (finger.ParentObject?.Owner is T owner && owner.SessionId != -1)
                {
                    return GetFrictionRidgesInSameGroup(allFingers, owner);
                }
            }

            return new[] { finger };
        }


        private static IEnumerable<T[]> GetFrictionRidgeGroups<T>(T[] fingers) where T : NFrictionRidge
        {
            foreach (var item in fingers.Where(x => x.SessionId == -1 && x.ParentObject == null))
            {
                yield return new[] { item };
            }

            var ids = fingers.Where(x => x.SessionId != -1).Select(x => new { Id = x.SessionId, Pos = x.Position, Impr = x.ImpressionType }).Distinct();
            foreach (var id in ids)
            {
                var result = fingers.Where(x => x.SessionId == id.Id && x.Position == id.Pos && x.ImpressionType == id.Impr).ToList();
                var first = result.FirstOrDefault();
                if (first != null)
                {
                    var attributes = first.Objects.ToArray().FirstOrDefault();
                    var child = attributes != null ? (T)attributes.Child : null;
                    if (child != null && child.SessionId == -1 && child.Position == first.Position)
                    {
                        result.Add(child);
                    }
                    yield return result.ToArray();
                }
            }
        }

        #endregion
    }
}
