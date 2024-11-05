#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SmoothShakeFree
{
    internal class WindowLinks : Editor
    {
        [MenuItem("Window/SmoothShakeFree/Upgrade to Pro version")]
        public static void ProLink()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/smooth-shake-pro-271080");
        }

        [MenuItem("Window/SmoothShakeFree/Showcase and Tutorial")]
        public static void TutorialLink()
        {
            Application.OpenURL("https://youtu.be/SFpfRgB9yh0?si=8H1EVeIFZ1tNjTdt");
        }

        //Leave a review
        [MenuItem("Window/SmoothShakeFree/Leave a review")]
        public static void ReviewLink()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/smooth-shake-free-271263#reviews");
        }
    }
}
#endif
