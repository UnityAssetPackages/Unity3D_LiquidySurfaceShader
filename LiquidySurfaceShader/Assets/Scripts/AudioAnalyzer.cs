using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAnalyzer : MonoBehaviour {
    [Range(0f, 1f)]
    public float mBassScaleMin, mBassScaleMax;
    [Range(0f, 1f)]
    public float mTrebScaleMin, mTrebScaleMax;
    [Range(0f, 1f)]
    public float mVolScaleMin, mVolScaleMax;

    [Header("Debug - just for visualizing")]
    [Range(0f, 1f)]
    public float mBass;
    [Range(0f, 1f)]
    public float mTreb;
    [Range(0f, 1f)]
    public float mVol;
    public bool isBassHit = false, isTrebHit = false, isVolHit = false;
    float pBass = 0f, pTreb = 0f, pVol = 0f;
    float ppBass = 0f, ppTreb = 0f, ppVol = 0f;

    bool isInit = false;

    public float bass
    {
        get { return mBass; }
    }

    public float treb
    {
        get { return mTreb; }
    }

    public float vol
    {
        get { return mVol; }
    }

    public bool bassHit
    {
        get { return isBassHit; }
    }

    public bool trebHit
    {
        get { return isTrebHit; }
    }

    public bool volHit
    {
        get { return isVolHit; }
    }

	void Start ()
    {

    }

    float normalizeRange(float range, float min, float max)
    {
        return (Mathf.Clamp(range, min, max)-min)/(max-min);
    }
    void Update()
    {
        float cBass = ((256f + Lasp.AudioInput.CalculateRMSDecibel(Lasp.FilterType.LowPass )) / 256f - .5f) * 2f;
        float cTreb = ((256f + Lasp.AudioInput.CalculateRMSDecibel(Lasp.FilterType.HighPass)) / 256f - .5f) * 2f;
        float cVol  = ((256f + Lasp.AudioInput.CalculateRMSDecibel(Lasp.FilterType.Bypass  )) / 256f - .5f) * 2f;
        cBass = normalizeRange(cBass, mBassScaleMin, mBassScaleMax);
        cTreb = normalizeRange(cTreb, mTrebScaleMin, mTrebScaleMax);
        cVol  = normalizeRange(cVol,  mVolScaleMin,  mVolScaleMax);

        if ((cBass > pBass && pBass <= ppBass) || !isInit)
        {
            mBass = cBass;
            isBassHit = true;
        }
        else
            isBassHit = false;

        if ((cTreb > pTreb && pTreb <= ppTreb) || !isInit)
        {
            mTreb = cTreb;
            isTrebHit = true;
        }
        else
            isTrebHit = false;

        if ((cVol > pVol && pVol <= ppVol) || !isInit)
        {
            mVol = cVol;
            isVolHit = true;
        }
        else
            isVolHit = false;

        if (mBass > 0.01f) mBass *= 0.96f;
        else mBass = 0f;

        if (mTreb > 0.01f) mTreb *= 0.96f;
        else mTreb = 0f;

        if (mVol > 0.01f) mVol *= 0.96f;
        else mVol = 0f;

        ppBass = pBass;
        ppTreb = pTreb;
        ppVol = pVol;

        pBass = mBass;
        pTreb = mTreb;
        pVol = mVol;

        if (!isInit) isInit = true;
    }
}
