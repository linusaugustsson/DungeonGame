using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitDirectionalUI : MonoBehaviour
{
    public float hitIndicatorFadeTime = 1f;
    [Space]
    public Image hitIndicatorBehind;
    public Image hitIndicatorFront;
    public Image hitIndicatorLeft;
    public Image hitIndicatorRight;
    private Image hitIndicator;

    public void triggerHitIndicator(Vector3 hitOrigin)
    {
        hitIndicator = GetDirectonalImage(hitOrigin);
        var color = hitIndicator.color;
        color.a = 0.5f;

        hitIndicator.color = color;
    }

    private Image GetDirectonalImage(Vector3 origin)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toOther = origin - transform.position;

        if (Vector3.Dot(forward, toOther) < 0)
        {
            print("The other transform is behind me!");
            return hitIndicatorBehind;
        }
        if (Vector3.Dot(forward, toOther) > 0)
        {
            print("The other transform is infront of me!");
            return hitIndicatorFront;
        }

        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 toOtherR = origin - transform.position;

        if (Vector3.Dot(right, toOtherR) < 0)
        {
            print("The other transform is left of me!");
            return hitIndicatorLeft;
        }
        if (Vector3.Dot(right, toOtherR) > 0)
        {
            print("The other transform is right of me!");
            return hitIndicatorRight;
        }

        return hitIndicatorFront;
    }

    void Update()
    {
        /// Directional Hit Indicator: Behind
        if (hitIndicatorBehind != null && hitIndicatorBehind.color.a > 0f)
        {
            var color = hitIndicatorBehind.color;
            color.a -= Time.deltaTime * hitIndicatorFadeTime;
            hitIndicatorBehind.color = color;
        }
        /// Directional Hit Indicators: Front
        if (hitIndicatorFront != null && hitIndicatorFront.color.a > 0f)
        {
            var color = hitIndicatorFront.color;
            color.a -= Time.deltaTime * hitIndicatorFadeTime;
            hitIndicatorFront.color = color;
        }
        /// Directional Hit Indicators: Left
        if (hitIndicatorLeft != null && hitIndicatorLeft.color.a > 0f)
        {
            var color = hitIndicatorLeft.color;
            color.a -= Time.deltaTime * hitIndicatorFadeTime;
            hitIndicatorLeft.color = color;
        }
        /// Directional Hit Indicators: Right
        if (hitIndicatorRight != null && hitIndicatorRight.color.a > 0f)
        {
            var color = hitIndicatorRight.color;
            color.a -= Time.deltaTime * hitIndicatorFadeTime;
            hitIndicatorRight.color = color;
        }
    }
}
