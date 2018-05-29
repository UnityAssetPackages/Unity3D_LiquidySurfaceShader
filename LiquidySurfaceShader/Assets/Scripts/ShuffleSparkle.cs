using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleSparkle : MonoBehaviour {
    private Vector3 mLoc = new Vector3(0f, 0f, -1f);
    private Vector3 mLoc_target = new Vector3(0f, 0f, -1f);
    public float mOrbitRadiusMin = 1f;
    public float mOrbitRadiusMax = 10f;
    [Range(0.02f, 0.5f)]
    public float mOrbitSpeed = 0.2f;
    [Range(10f, 200f)]
    public float mOrbitFreqRandSeed = 140f;

    void Start () {
		
	}

    void Update() {
        if (Time.frameCount % (int)mOrbitFreqRandSeed 
            == (Mathf.Floor(Random.Range(0f, 1f) * mOrbitFreqRandSeed)))
        {
            ShuffleCam();
        }

        updateCamera();
    }

    private void ShuffleCam()
    {
        mLoc_target.x = Random.Range(-1f, 1f);
        mLoc_target.y = Random.Range(-1f, 1f);
        mLoc_target.z = Random.Range(-1f, 1f);

        mLoc_target.Normalize();
        mLoc_target *= 
            (Random.Range(0f, 1f) * 
            (mOrbitRadiusMax-mOrbitRadiusMin) + mOrbitRadiusMin);
    }

    private void updateCamera()
    {
        Vector3 dir = mLoc_target - mLoc;
        float dist = dir.magnitude;
        dir.Normalize();

        if (dist < 0.1f)
        {
            mLoc = mLoc_target;

            ShuffleCam();
        }
        else
        {
            mLoc += dir * dist * mOrbitSpeed;
        }

        transform.position = mLoc;
        transform.LookAt(Vector3.zero, Vector3.up);
    }
}
