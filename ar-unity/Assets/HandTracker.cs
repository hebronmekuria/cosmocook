using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System.Collections.Generic;

public class HandTracker : MonoBehaviour
{
    private bool flipped_up_left = true;
    private bool flipped_up_right = true;
    private float threshold = 0.6f;

    private void Update()
    {
        var handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();
        if (handJointService != null)
        {
            Transform jointTransformLeft = handJointService.RequestJointTransform(TrackedHandJoint.Palm, Handedness.Left);
            Transform jointTransformRight = handJointService.RequestJointTransform(TrackedHandJoint.Palm, Handedness.Right);
            bool is_palm_up = 1 - jointTransformLeft.up.y > threshold;
            if (is_palm_up && !flipped_up_left)
            {
                EventBus.Publish<FlipHandUp_Event>(new FlipHandUp_Event());
            }
            flipped_up_left = is_palm_up;
            is_palm_up = 1 - jointTransformRight.up.y > threshold;
            if (is_palm_up && !flipped_up_right)
            {
                EventBus.Publish<FlipHandUp_Event>(new FlipHandUp_Event());
            }
            flipped_up_right = is_palm_up;
        }
        
    }
}
