using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidSurface : MonoBehaviour {
    public ComputeShader mCs;
    public Shader mSurfaceShader;
    public Texture2D mTex_noise;
    public Cubemap mCube;
    [Range(0f, 1f)]
    public float mGlossiness, mMetallic;
    [Range(0f, 100f)]
    public float mTesselEdge;
    [Range(0f, 1f)]
    public float mNormalInverse;
    private float mNormalInvere_target = 0f;

    private float mRotRad = 0f;
    private float mRotRad_target = 0f;
    private float mRotRad_vel = 0f;
    private Vector3 mRotAxis = new Vector3(0f, 1f, 0f);
    private Vector3 mRotAxis_target = new Vector3(0f, 1f, 0f);
    private float mRotAxis_vel = 0f;

    private Material mSurfaceMat;

    private RenderTexture[] mRt_vel;
    private RenderTexture mRt_color;
    private RenderTexture mRt_normal;

    private const int mCs_outSize = 2048;
    private const int mCs_numThread = 8;
    private const int mCs_workGroupSize = mCs_outSize / mCs_numThread;

    private int curFrame = 0;

    void Start ()
    {
        initMaterial();
        initCsRenderTexture();
    }
	
	void Update ()
    {
        updateAudioEvent();
        updateCsRenderTexture();
        updateSurfaceMaterial();

        curFrame ^= 1;
	}

    private void OnDestroy()
    {
        destroyRenderTexture();
        destroyMaterial();
    }

    private void updateAudioEvent()
    {
        AudioAnalyzer aa = GetComponent<AudioAnalyzer>();
        float bass = aa.bass;
        float treb = aa.treb;
        float vol = aa.vol;
        bool bassHit = aa.bassHit;
        bool trebHit = aa.trebHit;
        bool volHit = aa.volHit;

        mCs.SetFloat("uBass", bass);
        mCs.SetFloat("uTreb", treb);
        mCs.SetFloat("uVol", vol);
        mCs.SetBool("uBassHit", bassHit);
        mCs.SetBool("uTrebHit", trebHit);
        mCs.SetBool("uVolHit", volHit);

        mSurfaceMat.SetFloat("uVol", vol);

        // normal event
        shuffleNormal(bassHit);

        // rotation event
        shuffleRot(bassHit);
    }

    private void shuffleNormal(bool trigger)
    {
        if (Random.Range(0f, 1f) > 0.99f && trigger)
            mNormalInvere_target = Random.Range(0f, 1f) > 0.5f ? 1f : 0f;

        if (Mathf.Abs(mNormalInvere_target - mNormalInverse) > 0.01f)
            mNormalInverse += (mNormalInvere_target - mNormalInverse) * 0.02f;
        else
            mNormalInverse = mNormalInvere_target;
    }

    private void shuffleRot(bool trigger)
    {
        if (Random.Range(0f, 1f) > 0.65f && trigger)
        {
            mRotRad_target = Random.Range(-360f, 360f) * Mathf.PI / 180f;

            mRotAxis_target = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            mRotAxis_target.Normalize();
        }

        if (Mathf.Abs(mRotRad_target - mRotRad) > 0.01f)
        {
            // update angle
            float d = mRotRad_target - mRotRad;

            if(d > 1f)
            {
                mRotRad_vel += 0.002f / (d * d);
            }
            mRotRad += mRotRad_vel;
            mRotRad_vel *= 0.93f;

            // update axis
            //Vector3 dir = mRotAxis_target - mRotAxis;
            //float dist = dir.magnitude;
            //dir.Normalize();

            //if (dist > 1f)
            //{
            //    mRotAxis_vel += 0.0001f / (dist * dist);
            //    mRotAxis_vel = Mathf.Clamp(mRotAxis_vel, 0f, 0.0009f);
            //}

            //mRotAxis_vel *= 0.93f;

            //if (dist > 0.01f)
            //{
            //    mRotAxis += dir * mRotAxis_vel;
            //}
        }
        else
        {
            mRotRad_target = Random.Range(-360f, 360f) * Mathf.PI / 180f;

            mRotAxis_target = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            mRotAxis_target.Normalize();
        }

        mSurfaceMat.SetFloat("uRotRad", -mRotRad);
        mSurfaceMat.SetVector("uRotAxis", mRotAxis);
    }

    private void updateSurfaceMaterial()
    {
        mSurfaceMat.SetTexture("_Cube", mCube);
        mSurfaceMat.SetTexture("_MainTex", mRt_color);
        mSurfaceMat.SetTexture("_BumpMap", mRt_normal);
        mSurfaceMat.SetFloat("_Metallic", mMetallic);
        mSurfaceMat.SetFloat("_Glossiness", mGlossiness);
        mSurfaceMat.SetFloat("uTesselEdge", mTesselEdge);
        mSurfaceMat.SetFloat("uTime", Time.frameCount);
        mSurfaceMat.SetFloat("uNormalInverse", mNormalInverse);
    }

    private void updateCsRenderTexture()
    {
        mCs.SetFloat("uTime", Time.frameCount);
        mCs.SetFloat("uNormalInverse", mNormalInverse);
        mCs.SetVector("uRes", new Vector2(mCs_outSize, mCs_outSize));

        int _kernel = mCs.FindKernel("CalcDispersion");
        {
            mCs.SetTexture(_kernel, "out_vel", mRt_vel[curFrame]);
            mCs.SetTexture(_kernel, "uPVel", mRt_vel[curFrame ^ 1]);
            mCs.SetTexture(_kernel, "uTexNoise", mTex_noise);

            mCs.Dispatch(_kernel,
                mCs_workGroupSize, mCs_workGroupSize, 1);
        }

            
        _kernel = mCs.FindKernel("CalcLiquidySurface");
        {
            mCs.SetTexture(_kernel, "out_color", mRt_color);
            mCs.SetTexture(_kernel, "out_normal", mRt_normal);
            mCs.SetTexture(_kernel, "uPVel", mRt_vel[curFrame ^ 1]);
            mCs.SetTexture(_kernel, "uTexNoise", mTex_noise);

            mCs.Dispatch(_kernel,
                mCs_workGroupSize, mCs_workGroupSize, 1);
        }
    }

    private void initCsRenderTexture()
    {
        mRt_vel = new RenderTexture[2];
        for(int i = 0; i < 2; i++)
        {
            mRt_vel[i] = new RenderTexture(mCs_outSize, mCs_outSize, 0);
            mRt_vel[i].format = RenderTextureFormat.ARGBFloat;
            mRt_vel[i].filterMode = FilterMode.Bilinear;
            mRt_vel[i].wrapMode = TextureWrapMode.Mirror;
            mRt_vel[i].enableRandomWrite = true;
            mRt_vel[i].Create();
        }

        mRt_color = new RenderTexture(mCs_outSize, mCs_outSize, 0);
        mRt_color.format = RenderTextureFormat.ARGBHalf;
        mRt_color.filterMode = FilterMode.Trilinear;
        mRt_color.wrapMode = TextureWrapMode.Clamp;
        mRt_color.enableRandomWrite = true;
        mRt_color.Create();

        mRt_normal = new RenderTexture(mCs_outSize, mCs_outSize, 0);
        mRt_normal.format = RenderTextureFormat.ARGBFloat;
        mRt_normal.filterMode = FilterMode.Bilinear;
        mRt_normal.wrapMode = TextureWrapMode.Clamp;
        mRt_normal.enableRandomWrite = true;
        mRt_normal.Create();
    }

    private void destroyRenderTexture()
    {
        if (mRt_color != null)
            mRt_color.Release();
        mRt_color = null;

        if (mRt_normal != null)
            mRt_normal.Release();
        mRt_normal = null;

        for (int i = 0; i < 2; i++)
        {
            if (mRt_vel[i] != null)
                mRt_vel[i].Release();
            mRt_vel[i] = null;
        }
    }

    private void initMaterial()
    {
        mSurfaceMat = new Material(mSurfaceShader);
        GetComponent<MeshRenderer>().sharedMaterial = mSurfaceMat;
    }

    private void destroyMaterial()
    {
        if (mSurfaceMat)
            Destroy(mSurfaceMat);
    }
}
