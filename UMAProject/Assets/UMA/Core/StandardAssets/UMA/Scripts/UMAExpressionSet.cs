//	============================================================
//	Name:		UMAExpressionSet
//	Author: 	Eli Curtz
//	Copyright:	(c) 2013 Eli Curtz
//	============================================================

using UnityEngine;
using System.Collections.Generic;

namespace UMA.PoseTools
{
    /// <summary>
    /// UMA expression set. Groups poses for expression player channels.
    /// </summary>
    [System.Serializable]
    public class UMAExpressionSet : ScriptableObject
    {
        /// <summary>
        /// Pair of mutually exclusive expressions which can share a curve.
        /// </summary>
        [System.Serializable]
        public class PosePair
        {
            public UMABonePose primary = null;
            public UMABonePose inverse = null;
        }
        /// <summary>
        /// The pose pairs for each expression channel.
        /// </summary>
        public PosePair[] posePairs = new PosePair[UMAExpressionPlayer.PoseCount];

        [System.NonSerialized]
        private int[] boneHashes = null;

        private void ValidateBoneHashes()
        {
            if (boneHashes == null)
            {
                List<int> boneHashList = new List<int>();
                for (int i = 0; i < posePairs.Length; i++)
                {
                    PosePair pair = posePairs[i];
                    if (pair.primary != null)
                    {
                        for (int i1 = 0; i1 < pair.primary.poses.Length; i1++)
                        {
                            UMABonePose.PoseBone bone = pair.primary.poses[i1];
                            if (!boneHashList.Contains(bone.hash))
                            {
                                boneHashList.Add(bone.hash);
                            }
                        }
                    }
                    if (pair.inverse != null)
                    {
                        for (int i1 = 0; i1 < pair.inverse.poses.Length; i1++)
                        {
                            UMABonePose.PoseBone bone = pair.inverse.poses[i1];
                            if (!boneHashList.Contains(bone.hash))
                            {
                                boneHashList.Add(bone.hash);
                            }
                        }
                    }
                }

                boneHashes = boneHashList.ToArray();
            }
        }

        /// <summary>
        /// Restores all the bones used by poses in the set to default (post DNA) position.
        /// </summary>
        /// <param name="umaSkeleton">Skeleton to be reset.</param>
        /// <param name="logErrors"></param>
        public void RestoreBones(UMASkeleton umaSkeleton, bool logErrors = false)
        {
            if (umaSkeleton == null)
            {
                return;
            }

            ValidateBoneHashes();

            for (int i = 0; i < boneHashes.Length; i++)
            {
                int hash = boneHashes[i];
                if (!umaSkeleton.Restore(hash))
                {
					if (logErrors)
					{
						//Since this generally logs like crazy which screws everything anyway, it might be nice to provide some useful information?
						var umaname = umaSkeleton.GetBoneGameObject(umaSkeleton.rootBoneHash).GetComponentInParent<UMAAvatarBase>().gameObject.name;
						string boneName = "";
                        for (int i1 = 0; i1 < posePairs.Length; i1++)
						{
                            PosePair pair = posePairs[i1];
                            if (pair.primary != null)
							{
                                for (int i2 = 0; i2 < pair.primary.poses.Length; i2++)
								{
                                    UMABonePose.PoseBone bone = pair.primary.poses[i2];
                                    if (bone.hash == hash)
									{
										boneName = bone.bone;
									}
								}
							}
							if (pair.inverse != null)
							{
                                for (int i2 = 0; i2 < pair.inverse.poses.Length; i2++)
								{
                                    UMABonePose.PoseBone bone = pair.inverse.poses[i2];
                                    if (bone.hash == hash)
									{
										boneName = bone.bone;
									}
								}
							}
						}
						if (Debug.isDebugBuild)
                        {
                            Debug.LogWarning("Couldn't reset bone! " + boneName + " on " + umaname);
                        }
                    }
                }
            }
        }

        public int[] GetAnimatedBoneHashes()
        {
            ValidateBoneHashes();
            return boneHashes;
        }

        /// <summary>
        /// Gets the transforms for all animated bones.
        /// </summary>
        /// <returns>Array of transforms.</returns>
        /// <param name="umaSkeleton">Skeleton containing transforms.</param>
        public Transform[] GetAnimatedBones(UMASkeleton umaSkeleton)
        {
            if (umaSkeleton == null)
            {
                return null;
            }

            ValidateBoneHashes();

            var res = new Transform[boneHashes.Length];
            for (int i = 0; i < boneHashes.Length; i++)
            {
                res[i] = umaSkeleton.GetBoneGameObject(boneHashes[i]).transform;
            }
            return res;
        }
    }
}
